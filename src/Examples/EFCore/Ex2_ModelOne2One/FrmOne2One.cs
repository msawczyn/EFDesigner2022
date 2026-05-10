using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Ex2_ModelOne2One
{
    public partial class FrmOne2One : Form
    {
        public FrmOne2One()
        {
            InitializeComponent();
        }

        private void btnTestOne2One_Click(object sender, EventArgs e)
        {
            TestOne2One();
        }

        void TestOne2One()
        {
            txtDebug.Text = "TestOne2One()\r\n";

            using (EFModelOne2One context = new EFModelOne2One())
            {
                // Ask for confirmation before deleting the database
                var result = MessageBox.Show("Do you really want to delete the existing database?", "Confirm Delete", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    txtDebug.Text += "Deleting DB...\r\n";
                    context.Database.EnsureDeleted();
                    txtDebug.Text += "Deleted Ok\r\n";
                }

                txtDebug.Text += "Creating DB...\r\n";
                context.Database.EnsureCreated();
                txtDebug.Text += "Created DB\r\n";


                Person person1 = new Person();
                Address address1 = new Address();

                person1.FirstName = "Bob";
                person1.MiddleName = "James";
                person1.LastName = "Smith";
                person1.Phone = "555-123-321";
                CultureInfo culture = new CultureInfo("en-AU");
                person1.DOB = Convert.ToDateTime("6/12/70", culture);

                address1.Number = "1";
                address1.StreetLine1 = "High St";
                address1.City = "Perth";
                address1.Country = "Australia";

                txtDebug.Text += "Adding Address1...\r\n";
                context.Addresses.Add(address1);

                txtDebug.Text += "Linking Address1 to Person1...\r\n";
                person1.Address = address1;

                txtDebug.Text += "Adding Person1...\r\n";
                context.People.Add(person1);

                Person person2 = new Person();
                Address address2 = new Address();

                person2.FirstName = "Paul";
                person2.MiddleName = "Michael";
                person2.LastName = "Black";
                person2.Phone = "555-324-564";
                person2.DOB = Convert.ToDateTime("10/1/82", culture);

                address2.Number = "34";
                address2.StreetLine1 = "Murray St";
                address2.City = "Melbourne";
                address2.Country = "Australia";

                txtDebug.Text += "Adding Address2...\r\n";
                context.Addresses.Add(address2);

                txtDebug.Text += "Linking Address2 to Person2...\r\n";
                person2.Address = address2;

                txtDebug.Text += "Adding Person2...\r\n";
                context.People.Add(person2);

                try
                {
                    txtDebug.Text += "Saving changes...\r\n";
                    context.SaveChanges();
                    txtDebug.Text += "Changes saved successfully.\r\n";
                }
                catch (DbUpdateException dbEx)
                {
                    Exception raise = dbEx;
                    foreach (var entry in dbEx.Entries)
                    {
                        string message = $"Entity of type {entry.Entity.GetType().Name} in state {entry.State} could not be updated";
                        raise = new InvalidOperationException(message, raise);
                    }
                    txtDebug.Text += $"Error: {raise.Message}\r\n";
                    throw raise;
                }

                var items = context.People;

                txtDebug.Text += "\r\n\r\n------- READ  --------\r\n";
                txtDebug.Text += "Recalled from Db.\r\n";

                foreach (var x in items)
                {
                    txtDebug.Text += $"Person: [Pk {x.PersonId}]  {x.FirstName} {x.MiddleName} {x.LastName} {x.Phone}\r\n";
                    txtDebug.Text += $"Address: [Pk {x.Address.AddressId}] {x.Address.Number} {x.Address.StreetLine1} {x.Address.StreetLine2} {x.Address.StreetType} {x.Address.City} {x.Address.PostalCode}\r\n\r\n";
                }

                result = MessageBox.Show("Do you want to view the database filesystem?", "Open Folder", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    // Open the database folder
                    string dbFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, EFModelOne2One.DatabaseName);
                    Process.Start("explorer.exe", dbFolderPath);
                }
            }
        }


    }
}