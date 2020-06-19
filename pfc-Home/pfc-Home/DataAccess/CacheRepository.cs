using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using pfc_Home.Models;
using System.Text.Json;

namespace pfc_Home.DataAccess
{
    public class CacheRepository
    {
        private IDatabase db;

        public CacheRepository() {

            //Ask sir - tal gitHub biex jintuza locally flokk tal cloud ? 
            // var connection = ConnectionMultiplexer.Connect("localhost"); //localhost if cache server is installed locally after downloaded from https://github.com/rgl/redis/downloads 

            // connection to REDISLABS.com
            var connection = ConnectionMultiplexer.Connect("redis-19259.c114.us-east-1-4.ec2.cloud.redislabs.com:19259,password=VSyv9IIOvwvdEemwCZKHlWqmVSE5sJn3");
            db = connection.GetDatabase();
        }


        // Adds/Updates a list of files in cache
        public void UpdateCache(List<File> files, string user)
        {

            string key = user + "-files";

            //Will remove any existing files in cache.
            if (db.KeyExists(key) == true)
            {
                db.KeyDelete(key);
            }

            //Serilizes the files
            string jsonString = JsonSerializer.Serialize(files);

            // sets the serilized files to the inputted key.
            db.StringSet(key, jsonString);
        }


     
        // Gets a list of files from cache
        public List<File> GetFilesFromCache(string user)
        {

            string key = user + "-files";

            //Deserialize and return content if the key exist
            if (db.KeyExists(key) == true)
            {
                var files = JsonSerializer.Deserialize<List<File>>(db.StringGet(key));
                return files;
            }
            else
            {
                return new List<File>();
            }
        }




        //testing ONLY.
        public void Remove()
        {
            db.KeyDelete("sashaattard94@gmail.com-files");
            db.KeyDelete("enstafrace@gmail.com-files");
        }



        #region back up Cache methods (without user input)
        /*
        // Adds/Updates a list of files in cache
        public void UpdateCache(List<File> files)
        {
            //Will remove any existing files in cache.
            if (db.KeyExists("files") == true) {
                db.KeyDelete("files");
            }

            //Serilizes the files
            string jsonString = JsonSerializer.Serialize(files);

            // sets the serilized files to the inputted key.
            db.StringSet("files", jsonString);
        }

     

        // Gets a list of files from cache
        public List<File> GetFilesFromCache() {

            //Deserialize and return content if the key exist
            if (db.KeyExists("files") == true)
            {
                var files = JsonSerializer.Deserialize<List<File>>(db.StringGet("files"));
                return files;
            }
            else {
                return new List<File>();
            }
        }
     */
        #endregion
    }
}