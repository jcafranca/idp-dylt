using Dapper;
using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Helpers;
using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using HandyControl.Controls;
using HandyControl.Data;
using MySql.Data.MySqlClient;
using Mysqlx;
using SweetAlertSharp;
using SweetAlertSharp.Enums;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static DDCInteligence.DataTypes.DatabaseTypes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace DDCInteligence.View
{
    public partial class ProfileView
    {
        protected LogServices log = new LogServices();
        private string _selectedImage { get; set; }
        public ProfileView()
        {
            InitializeComponent();
            // Set Profile as the default active view
            SetActiveView("Profile");
            InitializeActiveTheme();
            this.Loaded += ProfileView_Loaded;
        }

        private void ProfileView_Loaded(object sender, RoutedEventArgs e)
        {
            // Get the current assembly
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Get the AssemblyVersion
            Version assemblyVersion = assembly.GetName().Version;

            lblTitle.Text = GetAssemblyAttribute<AssemblyTitleAttribute>(assembly)?.Title;
            lblDescription.Text = GetAssemblyAttribute<AssemblyDescriptionAttribute>(assembly)?.Description;
            lblCompany.Text = GetAssemblyAttribute<AssemblyCompanyAttribute>(assembly)?.Company;
            lblProduct.Text = GetAssemblyAttribute<AssemblyProductAttribute>(assembly)?.Product;
            lblCopyright.Text = GetAssemblyAttribute<AssemblyCopyrightAttribute>(assembly)?.Copyright;
            lblTrademark.Text = GetAssemblyAttribute<AssemblyTrademarkAttribute>(assembly)?.Trademark;

            // Set the AssemblyVersion labels
            lblAssemblyVesion1.Text = assemblyVersion.Major.ToString();
            lblAssemblyVesion2.Text = assemblyVersion.Minor.ToString();
            lblAssemblyVesion3.Text = assemblyVersion.Build.ToString();
            lblAssemblyVesion4.Text = assemblyVersion.Revision.ToString();

            // Get the FileVersion info from the assembly file
            var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            Version fileVersion = new Version(fileVersionInfo.FileVersion);

            // Set the FileVersion labels
            lblFileVesion1.Text = fileVersion.Major.ToString();
            lblFileVesion2.Text = fileVersion.Minor.ToString();
            lblFileVesion3.Text = fileVersion.Build.ToString();
            lblFileVesion4.Text = fileVersion.Revision.ToString();
        }
        private T GetAssemblyAttribute<T>(Assembly assembly) where T : Attribute
        {
            return assembly.GetCustomAttribute<T>();
        }
        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveView("Profile");
        }

        private void PasswordButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveView("Password");
        }

        private void AppearanceButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveView("Appearance");
        }
        private void ApplicationButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveView("Application");
        }

        private void HotkeysButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveView("Hotkeys");
        }
        private void SetActiveView(string viewName)
        {
            // Hide all views
            ProfileInfoView.Visibility = Visibility.Collapsed;
            PasswordView.Visibility = Visibility.Collapsed;
            AppearanceView.Visibility = Visibility.Collapsed;
            ApplicationView.Visibility = Visibility.Collapsed;
            HotkeysView.Visibility = Visibility.Collapsed;

            // Reset all button styles
            ResetButtonStyles();

            // Show the selected view and highlight the corresponding button
            switch (viewName)
            {
                case "Profile":
                    ProfileInfoView.Visibility = Visibility.Visible;
                    SetActiveButton(ProfileButton);
                    break;
                case "Password":
                    PasswordView.Visibility = Visibility.Visible;
                    SetActiveButton(PasswordButton);
                    break;
                case "Appearance":
                    // Appearance view is not implemented yet
                    AppearanceView.Visibility = Visibility.Visible;
                    SetActiveButton(AppearanceButton);
                    break;
                case "Application":
                    ApplicationView.Visibility = Visibility.Visible;
                    SetActiveButton(ApplicationButton);
                    break;
                case "Hotkeys":
                    HotkeysView.Visibility = Visibility.Visible;
                    SetActiveButton(HotkeysButton);
                    break;
            }
        }

        private void InitializeActiveTheme()
        {
            var currentSkin = ViewModelLocator.Instance.Process.CurrentSkin;
            switch (currentSkin)
            {
                case SkinType.Default:
                    Light.IsChecked = true;
                    break;
                case SkinType.Dark:
                    Dark.IsChecked = true;
                    break;
                case SkinType.Violet:
                    Other.IsChecked = true;
                    break;
            }
        }
        private void ResetButtonStyles()
        {
            // Reset all navigation buttons to default style
            ProfileButton.ClearValue(BackgroundProperty);
            ProfileButton.ClearValue(ForegroundProperty);
            ProfileButton.ClearValue(FontWeightProperty);

            PasswordButton.ClearValue(BackgroundProperty);
            PasswordButton.ClearValue(ForegroundProperty);
            PasswordButton.ClearValue(FontWeightProperty);

            AppearanceButton.ClearValue(BackgroundProperty);
            AppearanceButton.ClearValue(ForegroundProperty);
            AppearanceButton.ClearValue(FontWeightProperty);

            ApplicationButton.ClearValue(BackgroundProperty);
            ApplicationButton.ClearValue(ForegroundProperty);
            ApplicationButton.ClearValue(FontWeightProperty);

            HotkeysButton.ClearValue(BackgroundProperty);
            ApplicationButton.ClearValue(ForegroundProperty);
            HotkeysButton.ClearValue(FontWeightProperty);
        }

        private void SetActiveButton(Button activeButton)
        {
            // Set active button styling
            activeButton.Background = (Brush)new BrushConverter().ConvertFrom("#F3F4F6");
            activeButton.Foreground = (Brush)new BrushConverter().ConvertFrom("#111827");
            activeButton.FontWeight = FontWeights.SemiBold;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentBiller = ViewModel.ViewModelLocator.Instance.Process.CurrentBiller;
                currentBiller.fullname = NameTextBox.Text.Trim();
                currentBiller.username = UsernameTextBox.Text.Trim();
                currentBiller.employee_id_number = CompanyIdTextBox.Text.Trim();

                using (var connection = new MySqlConnection(ViewModelLocator.Instance.Process.ConnectionString))
                    await connection.UpdateAsync(currentBiller);

                SaveImage(currentBiller.username, currentBiller.employee_id_number);
                SweetAlert.Show("Success", "Profile updated successfully!", msgImage: SweetAlertImage.SUCCESS);
            }
            catch (Exception ex)
            {
                log.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(SaveButton_Click));
                SweetAlert.Show("Error", ex.Message, msgImage: SweetAlertImage.ERROR);
            }
        }
        void SaveImage(string username, string employeeId)
        {
            try
            {
                // Get file extension
                string extension = Path.GetExtension(_selectedImage);

                // Create custom filename: username_employeeId_timestamp.ext
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string filename = $"{username}_{employeeId}_{timestamp}{extension}";

                // Create temp directory path (in system temp folder)
                string tempDirectory = Path.Combine(Path.GetTempPath(), "ProfileImages");

                // Create directory if it doesn't exist
                if (!Directory.Exists(tempDirectory))
                    Directory.CreateDirectory(tempDirectory);

                // Get old temp files with same username and employeeId
                var tempFiles = Directory.GetFiles(tempDirectory, $"*{username}_{employeeId}*");

                // IMPORTANT: Clear the ProfileImage to release any file locks on old temp files
                var previousImage = ProfileImage.Source;
                ProfileImage.Source = null;

                // Force garbage collection to release file handles
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // Delete old temp files BEFORE creating new one
                if (tempFiles != null && tempFiles.Length > 0)
                {
                    foreach (var file in tempFiles)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception)
                        {
                            // Ignore deletion errors for old files that might still be locked
                        }
                    }
                }

                // Full path for the new temp file
                string fullName = Path.Combine(tempDirectory, filename);

                // Copy the file using FileStream to avoid file locking issues
                if (File.Exists(_selectedImage))
                {
                    using (var sourceStream = new FileStream(_selectedImage, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var destinationStream = new FileStream(fullName, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        sourceStream.CopyTo(destinationStream);
                    }
                }
                else return;

                // Update your view model with the saved temp image path
                ViewModelLocator.Instance.Process.CurrentBiller.profilePicture = fullName;

                // Reload the image from the new temp location
                var bitmap = new BitmapImage();
                using (var stream = new FileStream(fullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        memoryStream.Position = 0;

                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = memoryStream;
                        bitmap.EndInit();
                    }
                }
                bitmap.Freeze();
                ProfileImage.Source = bitmap;
            }
            catch (Exception ex)
            {
                log.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(SaveImage));
                throw; // Re-throw to let the caller handle it
            }
        }
        static string ValidatePasswords(MySqlConnection connection, int user_id, string currentPassword, string newPassword, string confirmPassword, string storedPassword)
        {
            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) ||
                    string.IsNullOrEmpty(confirmPassword))
                return "One or more fields is blank.";

            if (currentPassword != storedPassword)
            {
                string query = $"SELECT * FROM user_tbl WHERE id = '{user_id}' AND password = sha1('{currentPassword}')";
                var temp_user = connection.QueryAsync<BillerInfo>(query).Result?.ToList().FirstOrDefault();
                if (temp_user == null)
                {
                    return "Current password do not match.";
                }
            }

            if (newPassword == currentPassword)
                return "New password must be different from current password.";

            if (newPassword != confirmPassword)
                return "New password and confirm password do not match.";

            return null; // All validations passed
        }
        private async void SavePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentBiller = ViewModel.ViewModelLocator.Instance.Process.CurrentBiller;
                string currentPassword = CurrentPasswordBox.Password;
                string newPassword = NewPasswordBox.Password;
                string confirmPassword = ConfirmPasswordBox.Password;
                string validationResult = "";

                using (var connection = new MySqlConnection(ViewModelLocator.Instance.Process.ConnectionString))
                {
                    validationResult = ValidatePasswords(connection, currentBiller.id, currentPassword, newPassword, confirmPassword, currentBiller.password);
                    if (string.IsNullOrEmpty(validationResult))
                    {
                        currentBiller.password = newPassword;
                        await connection.UpdateAsync(currentBiller);
                    }
                }
                if (string.IsNullOrEmpty(validationResult))
                {
                    SweetAlert.Show("Success", "Password updated successfully!", msgImage: SweetAlertImage.SUCCESS);
                    CurrentPasswordBox.Clear();
                    NewPasswordBox.Clear();
                    ConfirmPasswordBox.Clear();
                }
                else
                {
                    SweetAlert.Show("Error", validationResult, msgImage: SweetAlertImage.ERROR);
                }
            }
            catch (Exception ex)
            {
                log.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(SavePasswordButton_Click));
                SweetAlert.Show("Error", ex.Message, msgImage: SweetAlertImage.ERROR);
            }
        }
        private void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            var deleteConfirmation = new DeleteAcountConfirmation(this);
            deleteConfirmation.ShowDialog();
        }
        private void ButtonSkins_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is RadioButton button)
            {
                SkinType skin = (SkinType)button.Tag;
                if (skin == (SkinType)ViewModelLocator.Instance.Process.CurrentSkin) return;
                ViewModelLocator.Instance.Process.SetCurrentSkin(skin);
                ((App)Application.Current).UpdateTheme(skin);
                Messenger.Default.Send(skin, "SkinUpdated");
            }
        }
        private void UploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png, *.gif)|*.jpg;*.jpeg;*.png;*.gif|All files (*.*)|*.*",
                Title = "Select Profile Picture",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    ClearImage();

                    // Load image through MemoryStream to completely release file handle
                    var bitmap = new BitmapImage();
                    using (var stream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            stream.CopyTo(memoryStream);
                            memoryStream.Position = 0;

                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = memoryStream;
                            bitmap.EndInit();
                        }
                    }
                    bitmap.Freeze();

                    ProfileImage.Source = bitmap;
                    UploadIconOverlay.Visibility = Visibility.Collapsed;
                    _selectedImage = openFileDialog.FileName;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error loading image: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            ClearImage();
        }
        void ClearImage()
        {
            ProfileImage.Source = null;
            UploadIconOverlay.Visibility = Visibility.Visible;
            _selectedImage = null;
        }
    }
}