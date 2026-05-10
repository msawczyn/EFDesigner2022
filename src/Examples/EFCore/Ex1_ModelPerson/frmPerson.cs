using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

using Bogus; // for generating fake data

namespace Ex1_ModelPerson
{
    public partial class FrmPerson : Form
    {
        DbContextOptionsBuilder<PersonModel> optionsBuilder;
        public FrmPerson()
        {
            InitializeComponent();
        }

        private void btnTestPerson_Click(object sender, EventArgs e)
        {
            TestPeople();
        }

        private void TestPeople()
        {
            txtDebug.Text = "TestPerson()\r\n";

            using (PersonModel context = new PersonModel())
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

                List<Person> people = GeneratePeople(50);

                foreach(var p in people)
                {
                    context.People.Add(p);
                }

                try
                {
                    txtDebug.Text += "Saved filesystem based DB as people.json\r\n";
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

                // Read it back
                List<Person> dbpeople = context.People.ToList();

                foreach (Person p in dbpeople)
                    txtDebug.Text += String.Format("{0} {1} {2} {3} {4}", p.Id, p.FirstName, p.MiddleName, p.LastName, p.Phone) + "\r\n";

                result = MessageBox.Show("Do you want to view the database filesystem?", "Open Folder", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    // Open the database folder
                    string dbFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PersonModel");
                    Process.Start("explorer.exe", dbFolderPath);
                }
            }
        }

        public static List<Person> GeneratePeople(int count)
        {
            var personFaker = new Faker<Person>()
                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                .RuleFor(p => p.LastName, f => f.Name.LastName())
                // Make Mobile phone number format (eg. 04## ### ###)
                .RuleFor(p => p.Phone, f => f.Phone.PhoneNumber("04## ### ###"));
                
            return personFaker.Generate(count);
        }
    }
}
