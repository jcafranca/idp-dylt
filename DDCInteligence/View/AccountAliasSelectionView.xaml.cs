using DDCInteligence.Utilities.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static DDCInteligence.DataTypes.LookupDatabaseTypes;

namespace DDCInteligence.View
{
    /// <summary>
    /// Interaction logic for AccountSelectionView.xaml
    /// </summary>
    public partial class AccountAliasSelectionView : Window
    {

        private readonly UtilityServices _utilityServices = new UtilityServices();
        public CustomerInfo SelectedAlias => aliasSuggestionAccountList?.SelectedItem as CustomerInfo;
        public AccountAliasSelectionView(Window owner, IEnumerable<CustomerInfo> childInfos)
        {
            InitializeComponent();
            if (owner != null)
                Owner = owner;

            Loaded += (s, e) => 
           {
               aliasSuggestionAccountList.ItemsSource = childInfos?.OrderBy(acct => acct.NAME);
               if (aliasSuggestionAccountList.Items.Count >= 0)
               {
                   aliasSuggestionAccountList.Focus();
                   aliasSuggestionAccountList.SelectedIndex = 0;
                   aliasSuggestionAccountList.Dispatcher.BeginInvoke(
                   new Action(() =>
                   {
                       aliasSuggestionAccountList.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                   }),
                   DispatcherPriority.Input);
               }

               _utilityServices.SetWindowOnScreen(this, 170);
           };

            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {

                    this.DialogResult = false;
                    this.Close();
                }
                else if (e.Key == Key.Enter)
                {
                    this.DialogResult = true;
                    this.Close();
                }
            };
            aliasSuggestionAccountList.MouseDoubleClick += (s, e) =>
            {
                this.DialogResult = true;
                this.Close();
            };
        }
    }
}
