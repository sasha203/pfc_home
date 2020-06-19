using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Google.Cloud.Storage.V1;
using Google.Apis.Storage.v1.Data;
using pfc_Home.DataAccess;
using pfc_Home.Models;

namespace pfc_Home.Controllers
{
    [Authorize]
    public class FileController : Controller
    {

        //when download button is clicked this method is called, the file link passed is used the to download the file and log the details.
        public ActionResult DownloadFile(string link) {
           
            try {
                string user = User.Identity.Name;
                string filename = new FileRepository().GetFileName(link); //gets the filename of the link provided.
                new LogRepository().WriteLogEntry(filename, link, user); //Log that the file is being downloaded
                return Redirect(link);
            }
            catch (Exception e)
            {
                new LogRepository().LogError(e);
                return RedirectToAction("Index", "File");
            }
 
        }


        [HttpGet]
        public ActionResult Index()
        {
            UserRepository ur = new UserRepository();
            FileRepository fr = new FileRepository();

            try
            {

                string email = User.Identity.Name;

                if (email != null || email != "")
                {
                    //check if email exists
                    if (ur.DoesEmailExist(email))
                    {

                        CacheRepository cr = new CacheRepository();
                        List<File> cacheResult = cr.GetFilesFromCache(email); //Get files from Cache

                      
                        // If items in db and cache are not the same amount get all data from db.
                        if (fr.GetFiles(email).Count != cacheResult.Count) {
                        
                            cr.UpdateCache(fr.GetFiles(email), email); //get items from db and update cache
                            return View(cr.GetFilesFromCache(email)); //get files from cache using the updated version.
                        }
                        else
                        {
                            //return from cache if has latest update.
                            return View(cacheResult);
                        }

                    }
                }

            }
            catch (Exception e)
            {
                new LogRepository().LogError(e);
            }


            return View();
        }


        [HttpGet]
        public ActionResult Create() {
            return View();
        }

        [HttpPost]
        public ActionResult Create(File f, HttpPostedFileBase file)
        {
            FileRepository fr = new FileRepository();

            try {

                if (file != null)
                {
                    var storage = StorageClient.Create();
                    string link, filename = "";
                    string ownerEmail = User.Identity.Name;

                    using (var _f = file.InputStream) {

                        if (f.FileTitle != null)
                        {
                            string rand = Guid.NewGuid().ToString().Substring(0, 4); //stores small part of a guid.
                            filename = f.FileTitle + "_" + rand + System.IO.Path.GetExtension(file.FileName);
                        }
                        else {
                            filename = Guid.NewGuid() + System.IO.Path.GetExtension(file.FileName);
                        }

                        filename = filename.Trim().Replace(" ", "-").Replace(",", "-").Replace("/", "").Replace("\\", "").Replace("&", ""); //Removes any spaces/slashes and & symbol.
                        var storageObject = storage.UploadObject("pfc-file-bucket", filename, null, _f); //Upload file on cloud.

                        
                        link = "https://storage.cloud.google.com/pfc-file-bucket/" + filename;  

                        if (null == storageObject.Acl)
                        {
                            storageObject.Acl = new List<ObjectAccessControl>();
                        }

                        storageObject.Acl.Add(new ObjectAccessControl()
                        {
                            Bucket = "pfc-file-bucket",
                            Entity = $"user-" + ownerEmail,
                            Role = "OWNER", //READER
                        });

                        var updatedObject = storage.UpdateObject(storageObject, new UpdateObjectOptions()
                        {
                            // Avoid race conditions.
                            IfMetagenerationMatch = storageObject.Metageneration,
                        });

                    }

                    // Store data in db (Create add method in fileRepo)
                    f.FileOwner = ownerEmail;
                    f.Link = link;
                    f.FileTitle = filename;


                    try
                    {
                        // Adds file to db / Initilize transaction.
                        fr.AddFile(f);
                    }
                    catch(Exception e) {

                        if (fr.MyConnection.State == System.Data.ConnectionState.Closed)
                        {
                            fr.MyConnection.Open();
                        }

                        ViewBag.Error = "File not created. " + e.Message;
                        fr.MyTransaction.Rollback();
                        fr.MyConnection.Close();
                        new LogRepository().LogError(e);
                    }

                    //Cache Update with latest files on DB.
                    CacheRepository cr = new CacheRepository();
                    cr.UpdateCache(fr.GetFiles(ownerEmail), ownerEmail);

                    //PubSub Add to email Queue
                    //PubSubRepository psr = new PubSubRepository();
                    //psr.AddToEmailQueue(f);

                    ViewBag.UploadSucc = "File Uploaded Successfully!";
                }
                else {
                    ViewBag.Error = "No File Inputted!";
                }
            }
            catch(Exception e) {

                ViewBag.Error = "File not created. " + e.Message;
                new LogRepository().LogError(e);
            }

            // If form is valid clears the entries
            if (ModelState.IsValid)
            {
                ModelState.Clear();
                return View();
            }
            // else entries are not removed so that invalid items can be changed accordingly.
            else {
                return View(f);
            }
           
        }



        [HttpGet]
        public ActionResult Share(string ftitle)
        {
            TempData["filename"] = ftitle;
            return View();
        }



        [HttpPost]
        public ActionResult Share(string ftitle, string recepient)
        {

            try
            {
                FileRepository ff = new FileRepository();

                if (TempData["filename"] != null)
                {
                    ftitle = TempData["filename"].ToString();

                    //if email is valid
                    if (ff.IsValidEmail(recepient))
                    {

                        //Actual file retreived using the file title.
                        File file = ff.GetFile(ftitle);


                        //PubSub - publish topic
                        PubSubRepository psr = new PubSubRepository();
                        psr.AddToEmailQueue(file, recepient);


                        //grants Read premissions to recepient on file.
                        psr.AddReadPermOnFile("pfc-file-bucket", ftitle, recepient);


                        //DownloadEmailFromQueueAndSend() should be called using third party cronJobs website.
                        //psr.DownloadEmailFromQueueAndSend(); //this will be called using cronJobs (ideal to be in another application but for this assignment was created in another controller)



                        //ViewBag.Error = "Invalid Email address";
                        ViewBag.ShareSucc = "File was shared successfully.";
                        TempData["filename"] = null;

                    }

                }
               
            }

            catch(Exception e) {
                ViewBag.Error = "File was not shared. " + e.Message;
                TempData["filename"] = null;
                new LogRepository().LogError(e);
            }

            return RedirectToAction("Index");
   


        }







        #region ShowFiles(string z) -  Not using IT
        //Shows files of specified user from db.
        //z is the encrypted Email
        /*
        public ActionResult ShowFiles(string z) {

           
            string encryptedEmail = z;
            UserRepository ur = new UserRepository();
            string email = null;

            try
            {
                //Checks if an email was passed as a parameter.
                if (encryptedEmail != null)
                {
                    //get email using encryption passed.
                    email = ur.GetEmail(encryptedEmail);

                    //check if email exists
                    if (ur.DoesEmailExist(email))
                    {
                        FileRepository fr = new FileRepository();
                        return View(fr.GetFiles(email));
                    }
                    //if it does not exists
                    else
                    {
                        TempData["Error"] = "Incorrect Email or password.";
                        //Log error
                    }

                }
                else if (encryptedEmail == null)
                {
                    //Can add this to a log for an attempt login
                    TempData["Error"] = "No Email passed"; //TempData is used to pass data between different methods (opposite to viewbag)

                    //Log error.
                }

            }
            catch (Exception e) {
                //Log error. (e.msg)
            }

            return RedirectToAction("Index", "Home");
        }
        */
        #endregion


    }
}