using DDCInteligence.Models;
using DDCInteligence.Utilities.Helpers;
using DDCInteligence.Utilities.Services;
using DDCInteligence.View;
using DDCInteligence.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using HandyControl.Interactivity;
using Mysqlx.Expr;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static DDCInteligence.DataTypes.DocumentTypes;
using static DDCInteligence.DataTypes.KeysInfo;

namespace DDCInteligence.Utilities.Behaviors
{
    public class ButtonClickBehavior : Behavior<Button>
    {
        private readonly UtilityServices _services = new UtilityServices();
        private readonly LogServices _logService = new LogServices();
        private readonly PromptHelperService _promptHelper = new PromptHelperService();

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Click += AssociatedObject_Click;
        }

        private void AssociatedObject_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var keys = new Key[] { Key.F1, Key.F2, Key.F3, Key.F5, Key.F11 };
            if (sender is Button button)
            {
                if (button.Name.Equals("btnResetAccount", System.StringComparison.OrdinalIgnoreCase))
                {
                    AccountsModel accounts = (AccountsModel)button.DataContext;
                    //accounts.Clear();
                }
                else if (button.Name.Equals("btnAccountName", System.StringComparison.OrdinalIgnoreCase))
                {
                    Messenger.Default.Send(new DisableKeysMessage(keys));
                    AccountsModel accounts = (AccountsModel)button.DataContext;
                    var view = new ManualSearchAccountName(accounts);
                    view.Owner = Application.Current.MainWindow;
                    view.Closed += (s, args) =>
                    {
                        Messenger.Default.Send(new EnableKeysMessage(keys));

                        if (view.SelectedAccountData != null)
                        {
                            accounts.Code = view.SelectedAccountData.clientId;
                            accounts.Name = view.SelectedAccountData.name;
                            accounts.Address = view.SelectedAccountData.address1;
                            accounts.Address2 = view.SelectedAccountData.address2;
                            accounts.City = view.SelectedAccountData.city;
                            accounts.State = view.SelectedAccountData.provine;
                            accounts.Zip = view.SelectedAccountData.postalcode;
                        }
                    };
                    view.ShowDialog();
                }
                else
                {
                    var remarksWindow = Application.Current.Windows
                                      .OfType<RemarksView>()
                                      .FirstOrDefault();
                    if (remarksWindow != null)
                    {
                        List<string> remarks = new List<string>();
                        if ((bool)remarksWindow?.c1.IsChecked) remarks.Add(remarksWindow.c1.Content.ToString());
                        if ((bool)remarksWindow?.c2.IsChecked) remarks.Add(remarksWindow.c2.Content.ToString());
                        if ((bool)remarksWindow?.c3.IsChecked) remarks.Add(remarksWindow.c3.Content.ToString());
                        if ((bool)remarksWindow?.c4.IsChecked) remarks.Add(remarksWindow.c4.Content.ToString());
                        if ((bool)remarksWindow?.c5.IsChecked) remarks.Add(remarksWindow.c5.Content.ToString());
                        if (!string.IsNullOrEmpty(remarksWindow?.TextBoxOthers.Text)) remarks.Add(_services.FormatText(remarksWindow.TextBoxOthers.Text));

                        remarksWindow.Shipper = _services.FormatText(remarksWindow.TextBoxShipper.Text);
                        remarksWindow.Consignee = _services.FormatText(remarksWindow.TextBoxConsignee.Text);
                        remarksWindow.Remarks = string.Join("|", remarks);
                    }
                    switch (button.Name)
                    {
                        case "LoginButtonCancel":
                            Application.Current.Shutdown();
                            return;
                        case "SendQueryButton":
                            remarksWindow.eStatus = EStatus.QUERY;
                            break;
                        case "QueryBilledButton":
                            remarksWindow.eStatus = EStatus.QUERYBILLED;
                            break;
                        case "RejectButton":
                            remarksWindow.eStatus = EStatus.REJECT;
                            break;
                    }

                    remarksWindow.Result = System.Windows.Forms.DialogResult.OK;
                    remarksWindow.Close();
                }
            }
        }
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Click -= AssociatedObject_Click;
        }
    }
}
