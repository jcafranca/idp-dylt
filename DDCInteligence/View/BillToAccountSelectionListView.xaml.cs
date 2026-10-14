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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static DDCInteligence.DataTypes.LookupDatabaseTypes;

namespace DDCInteligence.View
{
    /// <summary>
    /// Interaction logic for BillToAccountSelectionListView.xaml
    /// </summary>
    public partial class BillToAccountSelectionListView : Window
    {
        private readonly UtilityServices _utilityServices = new UtilityServices();
        public ThirdPartyInfo SelectedBillTo => dgvSelectionList?.SelectedItem as ThirdPartyInfo;
        public BillToAccountSelectionListView(Window owner, IEnumerable<ThirdPartyInfo> billToLookupList)
        {
            InitializeComponent();
            if (owner != null) this.Owner = owner;
            Loaded += (s, e) =>
            {
                if (billToLookupList != null)
                {
                    dgvSelectionList.ItemsSource = billToLookupList;
                    if (dgvSelectionList.Items.Count > 0)
                    {
                        dgvSelectionList.Focus();
                        dgvSelectionList.SelectedIndex = 0;
                        dgvSelectionList.CurrentCell = new DataGridCellInfo(dgvSelectionList.Items[0], dgvSelectionList.Columns[0]);
                        dgvSelectionList.Dispatcher.BeginInvoke(
                        new Action(() =>
                        {
                            dgvSelectionList.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        }),
                        DispatcherPriority.Input);
                    }
                }
                _utilityServices.SetWindowOnScreen(this, 20);
            }; 
            PreviewKeyDown += (s, e) => { 
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
            dgvSelectionList.MouseDoubleClick += (s, e) =>
            {
                this.DialogResult = true;
                this.Close();
            };
        }
    }
}
