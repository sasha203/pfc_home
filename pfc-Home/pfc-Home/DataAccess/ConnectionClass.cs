using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace pfc_Home.DataAccess
{
    public class ConnectionClass
    {

        public NpgsqlConnection MyConnection { get; set; }

        //Keeps track of all transactions. (biex tuzaha bhala rollback ez. deleting item 1,2 but 3 had an error and was not deleted. Transaction is used to do the rollback so that the deleted items are reverted.)
        public NpgsqlTransaction MyTransaction { get; set; }



        public ConnectionClass() {

            string connectionString = WebConfigurationManager.ConnectionStrings["postgresql"].ConnectionString;
            MyConnection = new NpgsqlConnection(connectionString);
        }


        

    }
}