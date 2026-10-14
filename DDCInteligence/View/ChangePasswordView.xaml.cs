using Dapper;
using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using GalaSoft.MvvmLight.Command;
using HandyControl.Controls;
using MySql.Data.MySqlClient;
using SweetAlertSharp;
using SweetAlertSharp.Enums;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WindowsInput.Native;
using WindowsInput;
using static DDCInteligence.DataTypes.DatabaseTypes;

namespace DDCInteligence.View
{
    /// <summary>
    /// Interaction logic for ChangePasswordView.xaml
    /// </summary>
    public partial class ChangePasswordView 
    {
        private LogServices logServices = new LogServices();
        public ChangePasswordView()
        {
            InitializeComponent();
            this.InputBindings.Clear();
            this.InputBindings.AddRange(new List<KeyBinding>
            {
                new KeyBinding {Key = Key.F2, Command = new RelayCommand(UpdatePassword) },
                new KeyBinding {Key = Key.Escape, Command = new RelayCommand(() => this.Close()) },
            });
            this.Submit.Click += Submit_Click;
            this.Exit.Click += Exit_Click;
        }


        private void NewPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                e.Handled = true;
                UpdatePassword();
            }
        }
        void UpdatePassword()
        {
            try
            {
                if (string.IsNullOrEmpty(OldPassword.Password) || string.IsNullOrEmpty(NewPassword.Password))
                {
                    SweetAlert.Show("Invalid Password", "One or more field is empty, please check.", msgImage: SweetAlertSharp.Enums.SweetAlertImage.ERROR);
                    return;
                }
                using (IDbConnection mysql = new MySqlConnection(ViewModelLocator.Instance.Process.ConnectionString))
                {
                    string query = $"SELECT * FROM user_tbl WHERE username = '{Username.Text}' AND password = sha1('{OldPassword.Password}')";
                    var temp_user = mysql.QueryAsync<BillerInfo>(query).Result.FirstOrDefault();
                    if (temp_user != null)
                    {
                        string update_query = $"UPDATE user_tbl SET password = '{NewPassword.Password}' WHERE id = {temp_user.id}";
                        mysql.Execute(update_query);

                        this.Close();
                        SweetAlert.Show("Password successfully changed.", "", SweetAlertButton.OK, SweetAlertImage.SUCCESS);
                    }
                    else
                    {
                        SweetAlert.Show("Invalid Password", "The old password you inputted is not match, please check.", msgImage: SweetAlertSharp.Enums.SweetAlertImage.ERROR);
                    }
                }
            } catch (System.Exception ex)
            {
                this.logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(UpdatePassword));
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            this.UpdatePassword();
        }
        private void OldPassword_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                NewPassword.Focus();
            }
        }
    }
}
