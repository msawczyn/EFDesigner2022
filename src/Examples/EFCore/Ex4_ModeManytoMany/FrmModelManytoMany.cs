using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Ex4_ModelManytoMany
{
    public partial class FrmModelManytoMany : Form
    {
        public FrmModelManytoMany()
        {
            InitializeComponent();
        }

        private void btnMany2Many_Click(object sender, EventArgs e)
        {
            // Trigger the many-to-many relationship test
            TestMany2Many();
        }

        void TestMany2Many()
        {
            txtDebug.Text = "TestOne2Many()\r\n";

            using (EFModelManytoMany context = new EFModelManytoMany())
            {
                // Confirm deletion of the existing database
                var result = MessageBox.Show("Do you really want to delete the existing database?", "Confirm Delete", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    txtDebug.Text += "Deleting DB...\r\n";
                    context.Database.EnsureDeleted(); // Ensure the database is deleted
                    context.DeleteOldStore(); // Custom implementation to delete old store

                    txtDebug.Text += "Deleted Ok\r\n";
                }

                // Generate sample data
                List<Student> students = GenerateStudents(10);
                List<Course> courses = GenerateCourse(10);

                // Add generated data to the context
                context.Students.AddRange(students);
                context.Courses.AddRange(courses);

                context.SaveChanges();

                txtDebug.Text += "Added students and courses to the context and saved changes.\r\n";
            }

            using (EFModelManytoMany context = new EFModelManytoMany())
            {
                // Retrieve the saved students and courses from the database
                var dbStudents = context.Students.Include(s => s.Courses).ToList();
                var dbCourses = context.Courses.Include(c => c.Students).ToList();

                txtDebug.Text += $"[{dbStudents.Count}] students and [{dbCourses.Count}] courses.\r\n";

                // Establish many-to-many relationships
                for (int i = 0; i < dbStudents.Count; i++)
                {
                    var student = dbStudents[i];
                    var course = dbCourses[i % dbCourses.Count]; // Ensure we don't go out of bounds

                    // Relationship Management:
                    // Ensure that the many-to-many relationships are being managed correctly and that there are no duplicate entries in the enrollments join table.

                    // Check if the course is already in the student's courses collection
                    if (!student.Courses.Any(c => c.Id == course.Id))
                    {
                        // Add the course to the student's courses collection
                        student.Courses.Add(course);
                        txtDebug.Text += $"course {course.Title} to student {student.FirstName} {student.LastName}.\r\n";
                    }
                    else
                    {
                        txtDebug.Text += $"Course {course.Title} already exists for student {student.FirstName} {student.LastName} .\r\n";
                    }
                }

                try
                {
                    // Save changes to persist the relationships
                    context.SaveChanges();
                    txtDebug.Text += "Saved changesOk to persist the many-to-many relationships.\r\n";
                }
                catch (DbUpdateException dbEx)
                {
                    // Handle exceptions and provide detailed error messages
                    Exception raise = dbEx;
                    foreach (var entry in dbEx.Entries)
                    {
                        string message = $"Entity of type {entry.Entity.GetType().Name} in state {entry.State} could not be updated";
                        raise = new InvalidOperationException(message, raise);
                    }
                    txtDebug.Text += $"Error: {raise.Message}\r\n";
                    throw raise;
                }
            }
        }


        void ReadDbEnrolments()
        {
            txtDebug.Text += "\r\n--------- READ ENROLMENTS -------\r\n";
            using (EFModelManytoMany readback = new EFModelManytoMany())
            {
              
                // Read Joining Table enrollments
                // Retrieve the students and their courses from the database

                // To Do  : Fix System.ArgumentException: 'An item with the same key has already been added. Key: System.Object[]'
                var students = readback.Students.Include(s => s.Courses).ToList();

                // Display the relationships
                foreach (var student in students)
                {
                    txtDebug.Text += $"Student: {student.FirstName} {student.LastName} (ID: {student.Id})\r\n";
                    foreach (var course in student.Courses)
                    {
                        txtDebug.Text += $"\tEnrolled in: {course.Title} (ID: {course.Id})\r\n";
                    }
                }


            }

            // Prompt to open the database folder
            var openresult = MessageBox.Show("Do you want to view the database filesystem?", "Open Folder", MessageBoxButtons.YesNo);
            if (openresult == DialogResult.Yes)
            {
                // Open the database folder
                string dbFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, EFModelManytoMany.DatabaseName);
                Process.Start("explorer.exe", dbFolderPath);
            }
        }



        private void btnRead_Click(object sender, EventArgs e)
        {
            // Trigger the database read operation
            txtDebug.Text = "Reading Database...\r\n";
            ReadDbEnrolments();
        }
        public static List<Student> GenerateStudents(int count)
        {
            // Generate a list of students using Bogus
            var studentFaker = new Faker<Student>()
                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                .RuleFor(p => p.LastName, f => f.Name.LastName());

            return studentFaker.Generate(count);
        }

        public static List<Course> GenerateCourse(int count)
        {
            // List of sample educational course titles
            var courseTitles = new List<string>
            {
                "Introduction to Computer Science",
                "Data Structures and Algorithms",
                "Database Management Systems",
                "Operating Systems",
                "Software Engineering",
                "Artificial Intelligence",
                "Machine Learning",
                "Web Development",
                "Mobile App Development",
                "Cybersecurity"
            };

            // Generate a list of educational courses using Bogus
            var courseFaker = new Faker<Course>()
                .RuleFor(p => p.Title, f => f.PickRandom(courseTitles));

            return courseFaker.Generate(count);
        }

    }
}
