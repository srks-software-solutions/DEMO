using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TATA_Production_Status
{
    class TataSqlConnection : IDisposable
    {
        //local
        //static String ServerName = "SRKS-TECH3-PC\\SQLEXPRESS";
        //static String username = "sa";
        //static String password = "srks4$";
        //static String DB = "i_facility_tsal";

        //server
        static String ServerName = @"TCP:TSAL-DAS\TSALSQLEXPDAS";
        static String username = "sa";
        static String password = "srks4$tsal";
        static String DB = "i_facility_tsal";

        public SqlConnection msqlConnection = new SqlConnection(@"Data Source = " + ServerName + ";User ID = " + username + ";Password = " + password + ";Initial Catalog = " + DB + ";Persist Security Info=True");
        public void open()
        {
            if (msqlConnection.State != System.Data.ConnectionState.Open)
                msqlConnection.Open();
        }

        public void close()
        {
            msqlConnection.Close();
        }

        public void Dispose()
        {
            msqlConnection.Dispose();
            GC.SuppressFinalize(this);
        }

        void IDisposable.Dispose()
        {


        }
    }
}
