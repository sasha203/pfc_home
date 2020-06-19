using Npgsql;
using pfc_Home.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace pfc_Home.DataAccess
{
    public class FileRepository : ConnectionClass
    {
        public FileRepository() : base() {
        }

        //gets all files from db
        public List<File> GetFiles() {

            string sql = "Select * from userfiles";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            List<File> outputList = new List<File>();

            MyConnection.Open();


            using (NpgsqlDataReader reader = cmd.ExecuteReader()) {

                while (reader.Read()) {
                    outputList.Add(new File() {
                        Id = reader.GetInt32(0),
                        FileOwner = reader.GetString(1),
                        FileTitle = reader.GetString(2),
                        Link = reader.GetString(3)
                    });
                }
            }

            MyConnection.Close();
            return outputList;
        }


        //Get files of a specified email from db
        public List<File> GetFiles(string email) {

            string sql = "Select * from userfiles where file_owner = @email";
            NpgsqlCommand cmd = new NpgsqlCommand(sql,MyConnection);
            cmd.Parameters.AddWithValue("@email", email);
            List<File> outputList = new List<File>();
            MyConnection.Open();

            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    outputList.Add(new File()
                    {
                        Id = reader.GetInt32(0),
                        FileOwner = reader.GetString(1),
                        FileTitle = reader.GetString(2),
                        Link = reader.GetString(3)
                    });
                }
            }

            MyConnection.Close();
            return outputList;
        }

        //Adds a new file.
        public void AddFile(File file) {

            bool isConnOpen = false;
            string sql = "INSERT INTO userfiles (file_owner, file_title, link) VALUES(@fileOwner, @fileTitle, @link)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@fileOwner", file.FileOwner);
            cmd.Parameters.AddWithValue("@fileTitle", file.FileTitle);
            cmd.Parameters.AddWithValue("@link", file.Link);

            if (MyConnection.State == System.Data.ConnectionState.Closed)
            {
                MyConnection.Open();
                isConnOpen = true;
                MyTransaction = MyConnection.BeginTransaction();
            }

            if (MyTransaction != null)
            {
                //used to participate in an opened trasaction happening somewhere else
                //assign the Transaction property to the opened transaction
                cmd.Transaction = MyTransaction; 
            }

            cmd.ExecuteNonQuery();
            MyTransaction.Commit();

            if (isConnOpen)
            {
                MyConnection.Close();
                isConnOpen = false;
            }

        }

        //gets file name using link provided
        public string GetFileName(string link)
        {

            string sql = "select file_title from userfiles where link = @link";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@link", link);
            string filename = "";
            MyConnection.Open();

            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {

                while (reader.Read())
                {
                    filename = reader.GetString(0);
                }


            }

            MyConnection.Close();
            return filename;

        }



        //Returns true if email format is valid.
        public bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }

        }


        public File GetFile(string fileTitle) {
            
            string sql = "select * from userfiles where file_title = @fTitle";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@fTitle", fileTitle);

            File output = new File();

            MyConnection.Open();
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {

                while (reader.Read())
                {
                    output.Id = reader.GetInt32(0);
                    output.FileOwner = reader.GetString(1);
                    output.FileTitle = reader.GetString(2);
                    output.Link = reader.GetString(3);
                }
            }

            MyConnection.Close();
            return output;

        }





    }
}