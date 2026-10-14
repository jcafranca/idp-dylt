using DDCInteligence.DataTypes;
using DDCInteligence.Models;
using DDCInteligence.Utilities.Events;
using DDCInteligence.Utilities.Helpers;
using DDCInteligence.Utilities.Services;
using DDCInteligence.View;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using HandyControl.Controls;
using SweetAlertSharp;
using SweetAlertSharp.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static DDCInteligence.DataTypes.DocumentTypes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DDCInteligence.ViewModel
{
    public class NonClientAreaViewModel : NonClientAreaModel
    {
        protected LogServices logServices = new LogServices();
        public NonClientAreaViewModel() 
        {
            var app = System.Windows.Application.ResourceAssembly.GetName();
            var assembly = Assembly.GetExecutingAssembly();
            var titleAttribute = (AssemblyTitleAttribute)assembly.GetCustomAttribute(typeof(AssemblyTitleAttribute));
            this.AppName = String.Format("{0}", titleAttribute.Title);
            this.AppVersion = String.Format("v{0}.{1}", app.Version.Major, app.Version.Minor);
            this.RequestCommand = new RelayCommand(Request);
            this.DeAllocateCommand = new RelayCommand(Deallocated);
            this.SaveCommand = new RelayCommand<EStatus>(ViewModelLocator.Instance.Process.Save);
            this.LogoutCommand = new RelayCommand(Logout);
            this.RestartApplicationCommand = new RelayCommand(Restart);
            this.AccountSettingCommand = new RelayCommand(() =>
            {
                ProfileView accountSettings = new ProfileView();
                accountSettings.Owner = Application.Current.MainWindow;
                accountSettings.ShowDialog();
            });
            this.GotoConsigneeCommand = new RelayCommand(() => ViewModelLocator.Instance.Consignee.Focus());
            this.GotoShipperCommand = new RelayCommand(() => ViewModelLocator.Instance.Shipper.Focus());
            this.GotoBillToCommand = new RelayCommand(() => ViewModelLocator.Instance.BillTo.Focus());
            this.GotoRateToCommand = new RelayCommand(() => ViewModelLocator.Instance.RateTo.Focus());
            this.GotoBOLCommand = new RelayCommand(() => ViewModelLocator.Instance.BillingInfo.Focus("BOL"));
            this.GotoPOCommand = new RelayCommand(() => ViewModelLocator.Instance.BillingInfo.Focus("PO"));
            this.GotoProCommand = new RelayCommand(() => ViewModelLocator.Instance.BillingInfo.Focus("PRO"));
            this.GotoSpecialInstructionCommand = new RelayCommand(() => ViewModelLocator.Instance.Instructions.Focus());
            this.NatureBreakCommand = new RelayCommand(new Action(() =>
            {
                if (ViewModelLocator.Instance.Process.CurrentState == EState.IDLE)
                {
                    ViewModelLocator.Instance.Process.SetState(EState.BREAK);
                    BreakView breakContent = new BreakView("Nature BREAK");
                    breakContent.Owner = Application.Current.MainWindow;
                    breakContent.ShowDialog();
                }
                else
                {
                    SweetAlert sweetAlert = new SweetAlert
                    {
                        Caption = "BREAK alert",
                        Message = "Taposin mo muna yung ginagawa mo bago ka mag break.",
                        OkText = "Okay, got it!",
                        MsgImage = SweetAlertSharp.Enums.SweetAlertImage.WARNING,
                    };
                    sweetAlert.Show();
                }
            }));
            this.MealBreakCommand = new RelayCommand(new Action(() =>
            {
                if (ViewModelLocator.Instance.Process.CurrentState == EState.IDLE)
                {
                    ViewModelLocator.Instance.Process.SetState(EState.BREAK);
                    BreakView breakContent = new BreakView("Meal BREAK");
                    breakContent.Owner = Application.Current.MainWindow;
                    breakContent.ShowDialog();
                }
                else
                {
                    SweetAlert sweetAlert = new SweetAlert
                    {
                        Caption = "BREAK alert",
                        Message = "Taposin mo muna yung ginagawa mo bago ka mag break.",
                        OkText = "Okay, got it!",
                        MsgImage = SweetAlertSharp.Enums.SweetAlertImage.WARNING,
                    };
                    sweetAlert.Show();
                }
            }));
            this.IdleBreakCommand = new RelayCommand(new Action(() =>
            {
                ViewModelLocator.Instance.Process.SetState(EState.BREAK);
                IdleView idle = new IdleView();
                idle.Owner = Application.Current.MainWindow;
                idle.ShowDialog();
            }));
            this.ChangePasswordCommand = new RelayCommand(() =>
            {
                ChangePasswordView changePassword = new ChangePasswordView();
                changePassword.Owner = Application.Current.MainWindow;
                changePassword.ShowDialog();
            });
            this.DimensionCommand = new RelayCommand(() =>
            {
                ViewModelLocator.Instance.Main.DimensionForm();
            });
            this.ViewDocumentsCommand = new RelayCommand(() =>
            {
                //ProcessedDocumentsView processed = new ProcessedDocumentsView();
                //processed.Owner = Application.Current.MainWindow;
                //processed.ShowDialog();
            });
        }
        void Request() { ViewModelLocator.Instance.Process.Request(); }
        void Deallocated () { ViewModelLocator.Instance.Process.DeAllocated(); }
        void Logout() 
        {
            if (ViewModelLocator.Instance.Process.CurrentState == EState.IDLE)
            {
                var alert_info = new SweetAlert
                {
                    Caption = "Ready to Leave?",
                    Message = $@"Select 'Logout' below if you are ready to end your current session.",
                    MsgButton = SweetAlertButton.YesNo,
                    MsgImage = SweetAlertImage.QUESTION,
                    OkText = "Yes, logout it!",
                    CancelText = "Cancel",
                    ShowInTaskbar = false
                };
                if (alert_info.ShowDialog() == SweetAlertResult.OK)
                {
                    try
                    {
                        ViewModelLocator.Instance.Process.DeAllocated();
                        ViewModelLocator.Instance.Login.Password = null;
                        LoginView login = new LoginView();
                        Messenger.Default.Send(new WindowRequestMessage(window =>
                        {
                            window.IsLogout = true;
                            window.Close();
                            Application.Current.MainWindow = login;
                        }));
                        login.Show();
                        //Application.Current.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        Growl.ErrorGlobal(ex.Message);
                    }
                }
            }
        }
        void Restart() 
        {
            // Restart the application
            Process.Start(Process.GetCurrentProcess().MainModule.FileName);
            Application.Current.Shutdown();
        }
    }
}
