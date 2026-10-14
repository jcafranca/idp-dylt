using Dapper;
using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using HandyControl.Data;
using MySql.Data.MySqlClient;
using SweetAlertSharp;
using SweetAlertSharp.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static DDCInteligence.DataTypes.DatabaseTypes;

namespace DDCInteligence.Models
{
    public class LoginModel : INotifyPropertyChanged
    {
        private const int LOGIN_DELAY_MS = 100;
        private readonly UtilityServices _utilityServices = new UtilityServices();
        private readonly LogServices _logServices = new LogServices();
        private readonly MySQLServices _mySQLServices = new MySQLServices();

        private ObservableCollection<RoleInfo> _accounts;
        public ObservableCollection<RoleInfo> Accounts
        {
            get => _accounts;
            set => SetProperty(ref _accounts, value);
        }

        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private bool _loading;
        public bool Loading
        {
            get => _loading;
            set => SetProperty(ref _loading, value);
        }

        private RoleInfo _selectedRole;
        public RoleInfo SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
        }

        public ICommand LoginCommand { get; set; }
        public ICommand ResetPassword { get; set; }

        public async void Login()
        {
            if (!ValidateInput())
                return;

            try
            {
                Loading = true;
                await Task.Delay(LOGIN_DELAY_MS);

                var settings = GetOrCreateSettingsAsync();
                var connectionString = _mySQLServices.GetConnectionString(settings);
                var billerInfo = await AuthenticateUserAsync(connectionString);

                if (billerInfo != null)
                    await HandleSuccessfulLoginAsync(billerInfo);
                else
                    ShowError("Invalid Credentials", "Invalid username or password. Please try again.");
            }
            catch (Exception ex)
            {
                ShowError("Login Error", "An error occurred during login. Please try again.");
                _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(Login));
            }
            finally
            {
                Loading = false;
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                ShowError("Validation Error", "Please enter a username.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ShowError("Validation Error", "Please enter a password.");
                return false;
            }

            if (SelectedRole == null)
            {
                ShowError("Validation Error", "Please select a role.");
                return false;
            }

            if (SelectedRole == null)
            {
                ShowError("Role Not Available", "The selected account type is not available. Please choose a different role.");
                return false;
            }

            return true;
        }

        private Settings GetOrCreateSettingsAsync()
        {
            var settings = _logServices.Deserialize<Settings>(_logServices.SettingPath);

            if (settings == null)
            {
                settings = new Settings
                {
                    Server = new List<string> { "localhost" },
                    Port = 3306,
                    Databases = new List<string> { "db_dylt", "db_dylt_lookup" },
                    Username = "alluser",
                    Password = "alluser"
                };

                _logServices.Serialize(_logServices.SettingPath, settings);
            }

            return settings;
        }

        private async Task<BillerInfo> AuthenticateUserAsync(string connectionString)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                // Use parameterized query to prevent SQL injection
                const string query = @"
                    SELECT * FROM user_tbl 
                    WHERE username = @username 
                    AND password = SHA1(@password)
                    AND account = @account
                    AND archive = 0 
                    LIMIT 1"
                ;

                var parameters = new { username = Username, password = Password, account = SelectedRole.Key };
                var result = await connection.QueryAsync<BillerInfo>(query, parameters);
                return result.SingleOrDefault();
            }
        }

        private async Task HandleSuccessfulLoginAsync(BillerInfo billerInfo)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ViewModelLocator.Instance.Process.SetCurrentBiller(billerInfo, SelectedRole.Key);
                ViewModelLocator.Instance.Process.DeAllocated();

                var mainWindow = new MainWindow
                {
                    IsLogout = false,
                    Name = "MainForm"
                };
                var currentMainWindow = Application.Current.MainWindow;
                Application.Current.MainWindow = mainWindow;
                LoadCurrentSkin(ViewModelLocator.Instance.Process.CurrentBiller);

                mainWindow.Show();
                currentMainWindow?.Close();
            });
        }
        void LoadCurrentSkin(BillerInfo b)
        {
            var folder = System.IO.Path.GetTempPath();
            var filename = System.IO.Directory.GetFiles(folder, $"*{b.id}_*")?.FirstOrDefault();
            if (filename != null)
            {
                var splt = System.IO.Path.GetFileName(filename).Split('_');
                if (splt.Length > 1 && Enum.TryParse(splt[1], out SkinType skin))
                {
                    if (skin == (SkinType)ViewModelLocator.Instance.Process.CurrentSkin) return;
                    ViewModelLocator.Instance.Process.SetCurrentSkin(skin);
                    ((App)Application.Current).UpdateTheme(skin);
                    Messenger.Default.Send(skin, "SkinUpdated");
                }
            }
        }

        private void ShowError(string title, string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                SweetAlert.Show(title, message, msgImage: SweetAlertImage.ERROR);
            });
        }

        public void Reset()
        {
            var alert = new SweetAlert
            {
                Caption = "Password Reset",
                Message = "Please contact your supervisor to reset your password.",
                MsgImage = SweetAlertImage.INFORMATION,
                MsgButton = SweetAlertButton.OK,
                OkText = "Understood"
            };
            alert.ShowDialog();
        }

        #region Property Changed Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
        #endregion
    }
}