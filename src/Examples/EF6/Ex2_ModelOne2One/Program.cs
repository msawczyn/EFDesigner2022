using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using Ex1_Person;

namespace Ex2_ModelOne2One
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
         static void Main()
         {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            InitaliseDb();
            Application.Run(new FrmOne2One());

         }
         static void InitaliseDb()
         {
            using (var db = new EFModelOne2One())
            {
               db.Database.Log = Logger.Log;

               //{"Unable to complete operation.
               //The supplied SqlConnection does not specify an initial catalog or AttachDBFileName



               db.Database.CreateIfNotExists();

               if (db.Database.Exists())
               {
                  db.Database.Delete();
                  Console.WriteLine("Deleted DB\r\n");
               }

               db.Database.CreateIfNotExists();
               Console.WriteLine("Created DB\r\n");


               string connectionString = ConfigurationManager.ConnectionStrings["MyLocalDb"].ConnectionString;
               SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

               string DbName = connectionStringBuilder.InitialCatalog;
               Console.WriteLine($"Db: {DbName}");
             }
         }
    }
}