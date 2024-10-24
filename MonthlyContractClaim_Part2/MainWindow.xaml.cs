using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Configuration;


namespace MonthlyClaimContractSystem_Part2
{
    public partial class MainWindow : Window
    {
        private List<Claim> pendingClaims = new List<Claim>();

        public MainWindow()
        {
            InitializeComponent();
            LoadPendingClaims();
        }

        private void SubmitClaim(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(LecturerName.Text) || string.IsNullOrEmpty(HoursWorked.Text) ||
                string.IsNullOrEmpty(HourlyRate.Text) || string.IsNullOrEmpty(UploadedFileName.Text))
            {
                MessageBox.Show("Please fill in all required fields and upload a supporting document.");
                return;
            }

            try
            {
                double hoursWorked = Convert.ToDouble(HoursWorked.Text);
                double hourlyRate = Convert.ToDouble(HourlyRate.Text);
                double totalAmount = hoursWorked * hourlyRate;

                var claim = new Claim
                {
                    LecturerName = LecturerName.Text,
                    HoursWorked = hoursWorked,
                    HourlyRate = hourlyRate,
                    TotalAmount = totalAmount,
                    Notes = AdditionalNotes.Text,
                    Status = "Pending",
                    DocumentPath = UploadedFileName.Text
                };

                using (var context = new AppDbContext())
                {
                    context.Claims.Add(claim);
                    context.SaveChanges(); // Save the claim to the database
                }

                // After the claim is successfully saved, reset the form fields
                MessageBox.Show("Claim Submitted Successfully!");
                ClaimStatus.Text = "Pending";

                LecturerName.Text = "";
                HoursWorked.Text = "";
                HourlyRate.Text = "";
                AdditionalNotes.Text = "";
                UploadedFileName.Text = "";

                // **Number 2: Update the pending claims list** 
                pendingClaims.Add(claim); // Add the new claim to the pending claims list
                LoadPendingClaims(); // Refresh the list to show the new claim
            }
            catch (DbUpdateException dbEx)
            {
                MessageBox.Show($"A database error occurred: {dbEx.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void UploadDocument(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PDF Files|*.pdf|Word Documents|*.docx|Excel Files|*.xlsx",
                Title = "Upload Supporting Document"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FileInfo fileInfo = new FileInfo(openFileDialog.FileName);

                // Limit file size to 5MB
                if (fileInfo.Length > 5 * 1024 * 1024)
                {
                    MessageBox.Show("File size exceeds the 5MB limit.");
                    return;
                }

                UploadedFileName.Text = fileInfo.FullName;
                MessageBox.Show("Document Uploaded Successfully!");
            }
        }

        private void ApproveClaim(object sender, RoutedEventArgs e)
        {
            var selectedClaim = (Claim)PendingClaims.SelectedItem;
            if (selectedClaim != null)
            {
                using (var context = new AppDbContext())
                {
                    var claim = context.Claims.Find(selectedClaim.ClaimID);
                    if (claim != null)
                    {
                        claim.Status = "Approved";
                        context.SaveChanges();
                    }
                }

                MessageBox.Show("Claim Approved!");
                LoadPendingClaims();
            }
        }

        private void RejectClaim(object sender, RoutedEventArgs e)
        {
            var selectedClaim = (Claim)PendingClaims.SelectedItem;
            if (selectedClaim != null)
            {
                using (var context = new AppDbContext())
                {
                    var claim = context.Claims.Find(selectedClaim.ClaimID);
                    if (claim != null)
                    {
                        claim.Status = "Rejected";
                        context.SaveChanges();
                    }
                }

                MessageBox.Show("Claim Rejected!");
                LoadPendingClaims();
            }
        }

        private void LoadPendingClaims()
        {
            using (var context = new AppDbContext())
            {
                // Fetch all pending claims from the database
                var pendingClaims = context.Claims.Where(c => c.Status == "Pending").ToList();

                // Refresh the PendingClaims ListBox with updated list
                PendingClaims.ItemsSource = null;  // Clear the old items
                PendingClaims.ItemsSource = pendingClaims;  // Load the updated list of claims
            }

        }

        private void OnClaimSelected(object sender, RoutedEventArgs e)
        {
            var selectedClaim = (Claim)PendingClaims.SelectedItem;
            if (selectedClaim != null)
            {
                LecturerNameDetails.Text = selectedClaim.LecturerName;
                HoursWorkedDetails.Text = selectedClaim.HoursWorked.ToString();
                HourlyRateDetails.Text = selectedClaim.HourlyRate.ToString();
                TotalAmountDetails.Text = selectedClaim.TotalAmount.ToString("C");
                AdditionalNotesDetails.Text = selectedClaim.Notes;
                DocumentPathDetails.Text = selectedClaim.DocumentPath;

                // Bind the status of the selected claim to the ClaimStatus TextBox
                ClaimStatus.DataContext = selectedClaim;
                ClaimStatus.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("Status"));
            }
        }
    }

    public class Claim : INotifyPropertyChanged
    {
        private string _status;

        public int ClaimID { get; set; } // Add ClaimID for primary key
        public string LecturerName { get; set; }
        public double HoursWorked { get; set; }
        public double HourlyRate { get; set; }
        public double TotalAmount { get; set; }
        public string Notes { get; set; }
        public string DocumentPath { get; set; }


        // Property for Claim Status with INotifyPropertyChanged implementation
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
