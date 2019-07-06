using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TATA_Production_Status
{
    public class ConnectionFactory : IConnectionFactory
    {
        static String ServerName = @"SRKS-TECH3-PC\\SQLEXPRESS";
        static String username = "sa";
        static String password = "srks4$";  
        static String DB = "i_facility_tsal";
        public readonly string connectionString = @"Data Source = " + ServerName + "; User ID = " + username + "; Password = " + password + ";Initial Catalog = " + DB + "; Persist Security Info=True";

        public IDbConnection GetConnection
        {

            get
            {
                var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
                var conn = factory.CreateConnection();
                conn.ConnectionString = connectionString;
                conn.Open();
                return conn;
            }


        }

    }
}
