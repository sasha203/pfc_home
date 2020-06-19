using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace pfc_Home.DataAccess
{
    public class UserRepository : ConnectionClass
    {
        public UserRepository() : base() { }

        //Add user
        public void AddUser(string email, string name, string surname)
        {
            //To avoid sql injection we add the values using parameters such as @email.
            string sql = "INSERT INTO users (email, name, surname, lastloggedin) VALUES(@email, @name, @surname, @lastLoggedIn)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@surname", surname);
            cmd.Parameters.AddWithValue("@lastLoggedIn", DateTime.UtcNow);

            //Connection is Opend and closed after Adding the new user
            MyConnection.Open();
            cmd.ExecuteNonQuery();
            MyConnection.Close();
        }

        //Checks if email exists in db
        public bool DoesEmailExist(string email)
        {

            if (email != null)
            {
                string sql = "Select Count(*) from users where email = @email";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
                cmd.Parameters.AddWithValue("@email", email);

                MyConnection.Open();
                bool result = Convert.ToBoolean(cmd.ExecuteScalar());
                MyConnection.Close();

                return result;
            }
            else {
                return false;
            }

            
        }


        //Updates the LastLoggedIn to the current date/time.
        public void UpdateLastLoggedIn(string email)
        {
            string sql = "UPDATE users set lastloggedin = @lastLoggedIn WHERE email = @email";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@LastLoggedIn", DateTime.Now);
            cmd.Parameters.AddWithValue("@email", email);

            MyConnection.Open();
            cmd.ExecuteNonQuery();
            MyConnection.Close();
        }




        #region Add User - With RSA COMMENTED
        //Add user - also saving RSA keys + Email Encryption
        //Note: this will not work since the columns encryption, publicKey, privateKey where removed from users table (re-create them to work)
        /*
        public void AddUser(string email, string name, string surname)
        {
            RSAHash rsaHash = new RSAHash();
            string pubKey = rsaHash.GetPublicKey(); //XML version
            string privKey = rsaHash.GetPrivateKey(); //XML version
            string emailEnc = rsaHash.Encrypt(email, pubKey, 4096);

            //To avoid sql injection we add the values using parameters such as @email.
            string sql = "INSERT INTO users (email, name, surname, lastloggedin, encryption, publicKey, privateKey) VALUES(@email, @name, @surname, @lastLoggedIn, @enc, @pubKey, @privKey)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@surname", surname);
            cmd.Parameters.AddWithValue("@lastLoggedIn", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@enc", emailEnc);
            cmd.Parameters.AddWithValue("@pubKey", pubKey);
            cmd.Parameters.AddWithValue("@privKey", privKey);
     

            //Connection is Opend and closed after Adding the new user
            MyConnection.Open();
            cmd.ExecuteNonQuery();
            MyConnection.Close();
        }
        */
        #endregion

        #region Methods used for Email RSA enc handling
        //Returns Encryption of the email passed.
        public string GetEmailEnc(string email)
        {
            string sql = "select encryption from users where email = @email";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@email", email);
            string enc = null;
            MyConnection.Open();


            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    enc = reader.GetString(0);
                }
            }

            MyConnection.Close();
            return enc;
        }


        //Returns the email of the encryption passed.
        public string GetEmail(string enc)
        {
            string sql = "select email from users where encryption = @enc";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@enc", enc);
            string email = null;
            MyConnection.Open();
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    email = reader.GetString(0);
                }
            }

            MyConnection.Close();
            return email;
        }

        #endregion


    }
}