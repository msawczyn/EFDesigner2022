using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ex1_Person
{



   // ---- Objective: Easy GUI Database create and use ---
   // Uses models / classes to setup data that is then stored in a Database.

   // ---------  BACKGROUND  --------------
   // The no ORM (Object Relationship Model) of doing Basic Database Operations is using C# hand crafted SQL CRUD
   // Works for simple database work but does not scale and is hard to maintiain.
   //https://www.geeksforgeeks.org/basic-database-operations-using-c-sharp/


   // ---------  Introducing ENTITY FRAMEWORK   --------------
   // This is a modern ORM way of using DB access, robust and solid using Code First & Fluent API  (
   // Unfortunatley the 'Code first' and 'Fluent API' are console like commands that require a deep dive to utilise and apply to your requirements.

   // ---------  Problem --------------
   // The Entity framework Microsoft ADO.Net modeller GUI wrapper for 'Code first' and 'Fluent API' modelling tool .edmx is fragile and incomplete.

   // Microsofts .EDMX visual tools DO NOT WORK - EDMX Entitfy framework gui tools from Microsoft suck!
   // DO NOT USE ->https://docs.microsoft.com/en-us/visualstudio/data-tools/entity-data-model-tools-in-visual-studio?view=vs-2019


   // ----------- Solution: Entity Framework Visual Editor  ------------------
   // An open source EF GUI tool works, here are some examples of how you get setup to use it..

   // It makes it easy to visualise, create and maintain complex relational databases


   // ---------  HOW TO GET SETUP --------------
   // Entity Framework Visual Editor - creates .efmodel etc from a GUI tool
   //https://marketplace.visualstudio.com/items?itemName=michaelsawczyn.EFDesigner

   //NOTE: Microsofts .EDMX visual tools DO NOT WORK - EDMX Entitfy framework gui tools from Microsoft suck!
   // DO NOT USE ->https://docs.microsoft.com/en-us/visualstudio/data-tools/entity-data-model-tools-in-visual-studio?view=vs-2019
   public class Logger
   {
      public static void Log(string message)
      {
         Console.WriteLine("EF Message: {0} ", message);
      }
   }

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

         Application.Run(new frmPerson());

      }
      static void InitaliseDb()
      {
         using (var db = new PersonDb())
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