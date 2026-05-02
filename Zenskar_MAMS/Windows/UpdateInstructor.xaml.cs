using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Zenskar_MAMS.Helpers;

namespace Zenskar_MAMS.Windows
{
    public partial class UpdateInstructor : Window
    {
        public UpdateInstructor()
        {
            InitializeComponent();
            LoadInstructors();
        }

        private void LoadInstructors()
        {
            try
            {
                // Fetch all Instructors from Users collection (User_Type = "Instructor" and Status = "Active")
                var filterBuilder = Builders<UserTable>.Filter;
                var filter = filterBuilder.Eq(x => x.User_Type, "Instructor") 
                    & filterBuilder.Eq(x => x.Status, "Active");

                var instructors = CommonItems._mongoDBContext.Users
                    .Find(filter)
                    .ToList();

                if (instructors.Count == 0)
                {
                    MessageBox.Show("No active instructors found in the system.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                CmbInstructor.ItemsSource = instructors;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading instructors: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(TxtBatchId.Text))
                {
                    MessageBox.Show("Please enter a Batch ID.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (CmbInstructor.SelectedItem == null)
                {
                    MessageBox.Show("Please select an instructor.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string batchId = TxtBatchId.Text.Trim();
                var selectedInstructor = CmbInstructor.SelectedItem as UserTable;
                string newInstructorName = selectedInstructor.User_Name;

                // Update all students with the given Batch ID
                var filterBuilder = Builders<StudentTable>.Filter;
                var filter = filterBuilder.Eq(x => x.Batch_ID, batchId);

                var updateDefinition = Builders<StudentTable>.Update
                    .Set(x => x.InstructorName, newInstructorName);

                var result = CommonItems._mongoDBContext.Students.UpdateMany(filter, updateDefinition);

                if (result.MatchedCount == 0)
                {
                    MessageBox.Show($"No students found with Batch ID: {batchId}", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                MessageBox.Show($"Successfully updated instructor for {result.ModifiedCount} student(s) in Batch ID: {batchId}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating instructor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
