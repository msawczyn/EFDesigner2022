using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;

namespace Ex4_ModelInvoice
{
    public partial class FrmInvoice : Form
    {
      
        public FrmInvoice()
        {
            InitializeComponent();
           
        }


        private void button1_Click(object sender, EventArgs e)
        {

            txtDebug.Text = "TestModelInvoice()\r\n";

            using (AccountingSystemModel db = new AccountingSystemModel())
            {
              
                txtDebug.Text += "\r\nEx 4: Invoice - Header and Detail\r\n----------------\r\n";

                var result = MessageBox.Show("Do you really want to delete the existing database?", "Confirm Delete", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    txtDebug.Text += "Deleting DB...\r\n";
                    db.DeleteOldStore(); //<- my implementation, would be nice to implement Database.EnsureDeleted(); above
                    txtDebug.Text += "Deleted Ok\r\n";
                }

                InvoiceHeaders invHeader = new InvoiceHeaders();

                InvoiceDetails invDetails_Line1 = new InvoiceDetails(invHeader);
                invDetails_Line1.ItemDescription = "Item 1";
                invDetails_Line1.Price = 11.25M;
                invDetails_Line1.Quantity = 2570;
                invDetails_Line1.Total = invDetails_Line1.Price * invDetails_Line1.Quantity;

                InvoiceDetails invDetails_Line2 = new InvoiceDetails(invHeader);
                invDetails_Line2.ItemDescription = "Item 2";
                invDetails_Line2.Price = 5.25M;
                invDetails_Line2.Quantity = 1520;
                invDetails_Line2.Total = invDetails_Line2.Price * invDetails_Line2.Quantity; 

                //Associate Header and Details
                invHeader.InvoiceDetails.Add(invDetails_Line1);

                //Associate Header and Details
                invHeader.InvoiceDetails.Add(invDetails_Line2);

                invHeader.Total = invDetails_Line1.Total + invDetails_Line2.Total;

                //Save rows to Db
                db.InvoiceHeaders.Add(invHeader);
                db.InvoiceDetails.Add(invDetails_Line1); // Necessary?
                db.InvoiceDetails.Add(invDetails_Line2); // Necessary?

                try
                {
                    db.SaveChanges();
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

            using (AccountingSystemModel db = new AccountingSystemModel())
            {
                //Read it back
                DbSet<InvoiceHeaders> invoices = db.InvoiceHeaders;

                foreach (InvoiceHeaders invoice in invoices)
                {
                    txtDebug.Text += $"Invoice Id {invoice.Id} - Total: {invoice.Total} \r\n"; 

                    txtDebug.Text += "--- Detail --\r\n";

                    foreach (InvoiceDetails LineItem in invoice.InvoiceDetails)
                    {
                        txtDebug.Text += $"Line Id {LineItem.Id} Details Desc:{LineItem.ItemDescription}\t Qty:{LineItem.Quantity} x Price:{LineItem.Price} = Sub Total:\t{LineItem.Total}\r\n";
                    }
                }
            }

            var openresult = MessageBox.Show("Do you want to view the database filesystem?", "Open Folder", MessageBoxButtons.YesNo);
            if (openresult == DialogResult.Yes)
            {
                // Open the database folder
                string dbFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AccountingSystemModel.DatabaseName);
                Process.Start("explorer.exe", dbFolderPath);
            }
        }
      
    }
}