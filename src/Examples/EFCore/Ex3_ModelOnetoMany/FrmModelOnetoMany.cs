using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;



namespace Ex3_ModelOnetoMany
{
    public partial class FrmModelOnetoMany : Form
    {
        public FrmModelOnetoMany()
        {
            InitializeComponent();
        }

        private void btnMany2Many_Click(object sender, EventArgs e)
        {
            //https://entityframework.net/one-to-many-relationship
            TestOne2Many();
        }

        void TestOne2Many()
        {
            txtDebug.Text = "TestOne2Many()\r\n";

            using (EFModelOnetoMany context = new EFModelOnetoMany())
            {
                var result = MessageBox.Show("Do you really want to delete the existing database?", "Confirm Delete", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    txtDebug.Text += "Deleting DB...\r\n";
                    context.Database.EnsureDeleted(); // <- this doesnt do anything 
                    context.DeleteOldStore(); //<- my implementation, would be nice to implement Database.EnsureDeleted(); above

                    txtDebug.Text += "Deleted Ok\r\n";
                }

                txtDebug.Text += "Creating DB...\r\n";
                context.Database.EnsureCreated(); // <- this does nothing either
                txtDebug.Text += "Created DB\r\n";


                txtDebug.Text += "\r\nEach Author can have many Books\r\n----------------\r\n";
                Author AuthorMT = new Author()
                {
                    Firstname = "Mark",
                    Lastname = "Twain"

                };


                Author AuthorIA = new Author()
                {
                    Firstname = "Izzac",
                    Lastname = "Azimov"

                };


                Book book = Book.Create(AuthorIA);
                book.Title = "The Complete Robot";
                book.ISBN = "65445635";
                context.Books.Add(book);

                Book book1 = Book.Create(AuthorIA);
                book1.Title = "Youth";
                book1.ISBN = "43252345243";
                context.Books.Add(book1);

                Book book2 = Book.Create(AuthorMT);
                book2.Title = "The Adventures of Huckleberry Finn";
                book2.ISBN = "6436546345";
                context.Books.Add(book2);

                Book book3 = Book.Create(AuthorMT);
                book3.Title = "The Prince and the Pauper";
                book3.ISBN = "34523452";
                context.Books.Add(book3);

                context.Authors.Add(AuthorMT);
                context.Authors.Add(AuthorIA);

                try
                {
                    context.SaveChanges();
                }
                catch (DbUpdateException dbEx)
                {
                    // This Exception handler helps to describe what went wrong with the EF database save inner exception detail.
                    // It decodes why the data did not comply with defined database field structure. e.g too long or wrong type.
                    Exception raise = dbEx;
                    foreach (var entry in dbEx.Entries)
                    {
                        string message = $"Entity of type {entry.Entity.GetType().Name} in state {entry.State} could not be updated";
                        raise = new InvalidOperationException(message, raise);
                    }
                    throw raise;
                }

            }

        }

        void ReadDb()
        {
            txtDebug.Text += "\r\n--------- READ -------\r\n";
            using (EFModelOnetoMany readback = new EFModelOnetoMany())
            {
                var AllAuthors = readback.Authors.ToList();

                foreach (Author author in readback.Authors)
                {
                    txtDebug.Text += "---------\r\n";
                    txtDebug.Text += $"Author: {author.Firstname} {author.Lastname}\r\n";


                    foreach (Book thisbook in author.Books)
                    {
                        txtDebug.Text += $"Book Title:{thisbook.Title} - {thisbook.ISBN}\r\n";
                    }
                }
            }

            var openresult = MessageBox.Show("Do you want to view the database filesystem?", "Open Folder", MessageBoxButtons.YesNo);
            if (openresult == DialogResult.Yes)
            {
                // Open the database folder
                string dbFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, EFModelOnetoMany.DatabaseName);
                Process.Start("explorer.exe", dbFolderPath);
            }
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            txtDebug.Text = "Reading Database...\r\n";
            ReadDb();
        }
    }
}