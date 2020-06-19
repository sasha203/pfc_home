using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.Cloud.PubSub.V1;
using System.Text.Json;
using Google.Protobuf;
using pfc_Home.Models;
using System.Net.Mail;
using System.Net;
using Google.Cloud.Storage.V1;
using Google.Apis.Storage.v1.Data;

namespace pfc_Home.DataAccess
{
    public class PubSubRepository
    {
        TopicName tn;
        SubscriptionName sn;

        //string topicName = "DemoTopic";
        //string subName = "DemoSubscription";

        string topicName = "EmailsTopic";
        string subName = "EmailsSubscription";


        public PubSubRepository()
        {
            // a Queue/Topic will be created to hold the emails to be sent.  It will always have the same name DemoTopic (but can be changed)
            //tn = new TopicName("pfc-home", "DemoTopic");
            tn = new TopicName("pfc-home", topicName);

            //A Subscription will be created to hold which messages were read or not.  It will always have the same name Emails-Subscription (but can be changed)
            sn = new SubscriptionName("pfc-home", subName); //Emails-Subscription
        }



        //Creates or gets a topic.
        private Topic CreateGetTopic() {

            PublisherServiceApiClient client = PublisherServiceApiClient.Create();
            TopicName tn = new TopicName("pfc-home", topicName);


            try {
                //Get topic if already exists
                return client.GetTopic(tn);
            }
            catch {
                //Create new topic if it does not exist.
                return client.CreateTopic(tn);
            }
        }


        //Publish method: uploads a message to the queue.
        public void AddToEmailQueue(File f, string recepient)  {

            PublisherServiceApiClient client = PublisherServiceApiClient.Create();
            Topic t = CreateGetTopic();
            KeyRepository kr = new KeyRepository();

            string serialized = JsonSerializer.Serialize(f, typeof(File));

            // this list is used so that the method can take more than 1 message/item/file at a time.
            List <PubsubMessage> messagesToAddToQueue = new List<PubsubMessage>();

            PubsubMessage msg = new PubsubMessage();

            //Encripting data and recepient values
            string encryptedData = kr.Encrypt(serialized);
            string encryptedRecepient = kr.Encrypt(recepient);

            msg.Data = ByteString.CopyFromUtf8(encryptedData); //Stores email content.
            msg.Attributes["recepient"] = encryptedRecepient; //stores recepient as a key-value attribute.
            messagesToAddToQueue.Add(msg);

            client.Publish(t.TopicName, messagesToAddToQueue); //committing to queue
        }



        //Creates or gets a Subscription
        private Subscription CreateGetSubscription()
        {
            SubscriberServiceApiClient client = SubscriberServiceApiClient.Create();

            //if subscription exist get it else create it and return it.
            try {
                return client.GetSubscription(sn);
            }
            catch {
                return client.CreateSubscription(sn, tn, null, 30);
            }


        }

        //subscription pulls from the topic(queue) and send email with info gathered from the topic to the recepient.
        public void DownloadEmailFromQueueAndSend() {

            SubscriberServiceApiClient client = SubscriberServiceApiClient.Create();
            Subscription s = CreateGetSubscription(); //you must getSubscription before being able to read messages from Topic/Queue
            PullResponse pullResponse = client.Pull(s.SubscriptionName, true, 1); //Reading the message on top (You can read more than just 1 at a time)
            KeyRepository kr = new KeyRepository();

            if (pullResponse != null) {

                if (pullResponse.ReceivedMessages.Count > 0)
                {

                    //extracting the first message since in the previous line it was specified to read one at a time. (Loop required to read more than 1 item(The amount set in maxMessages from the Pull method))
                    string encryptedData = pullResponse.ReceivedMessages[0].Message.Data.ToStringUtf8();
                    string decryptedData = kr.Decrypt(encryptedData);
               

                    File deserialized = JsonSerializer.Deserialize<File>(decryptedData); //Deserializing since it was serialized when added to the queue
                    string htmlString = $"<html><b>{deserialized.FileOwner}</b> is sharing the file <b>{deserialized.FileTitle}</b> with you.<br/> <a href=\"{deserialized.Link}\">Click me to access file</a> </html>";

                    string encryptedRecepient = pullResponse.ReceivedMessages[0].Message.Attributes["recepient"];
                    string recepient = kr.Decrypt(encryptedRecepient);

                    //Dummy email was created to act as a "Company email which will send emails to users."
                    //Send Email with deserialized. Documentation: https://docs.microsoft.com/en-us/dotnet/api/system.net.mail.smtpclient?view=netframework-4.8
                    MailMessage message = new MailMessage();  
                    SmtpClient smtp = new SmtpClient();  
                    message.From = new MailAddress("noreply.pfc.sa@gmail.com");  
                    message.To.Add(new MailAddress(recepient));  
                    message.Subject = "File";  
                    message.IsBodyHtml = true; //to make message body as html  
                    message.Body = htmlString;
                    smtp.Port = 587;  
                    smtp.Host = "smtp.gmail.com"; //for gmail host  
                    smtp.EnableSsl = true;  
                    smtp.UseDefaultCredentials = false;  
                    smtp.Credentials = new NetworkCredential("noreply.pfc.sa@gmail.com", "passForpfc1234");

                    //go on googleaccount > Security > search for lesssecureapps > turn it to on
                    //Note:  Google will automatically turn this setting OFF lesssecureapps if it’s not being used
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;  
                    smtp.Send(message);

                  
                    List<string> acksIds = new List<string>();
                    //after the email is sent successfully you acknolwedge the message so it is confirmed that it was processed. (Message will be removed after acknowledge.)
                    acksIds.Add(pullResponse.ReceivedMessages[0].AckId); 

                    client.Acknowledge(s.SubscriptionName, acksIds.AsEnumerable());
                   
                }

            }


        }


        //It grants user permission to the relevant file.
        public void AddReadPermOnFile(string bucketName, string objectName, string recepient)
        {

            var storage = StorageClient.Create();
            var storageObject = storage.GetObject(bucketName, objectName,
                new GetObjectOptions() { Projection = Projection.Full });

            if (null == storageObject.Acl)
            {
                storageObject.Acl = new List<ObjectAccessControl>();
            }

            storageObject.Acl.Add(new ObjectAccessControl()
            {
                Bucket = bucketName,
                Entity = $"user-{recepient}",
                Role = "READER"
            });

            var updatedObject = storage.UpdateObject(storageObject, new UpdateObjectOptions()
            {
                // Avoid race conditions.
                IfMetagenerationMatch = storageObject.Metageneration,
            });
        }





    }
}