using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using static DDCInteligence.DataTypes.LookupDatabaseTypes;

namespace DDCInteligence.View
{
    /// <summary>
    /// Interaction logic for AccountSelectionListView.xaml
    /// </summary>
    public partial class AccountSelectionListView : Window
    {
        private readonly UtilityServices _utilityServices = new UtilityServices();
        private readonly LogServices _logServices = new LogServices();
        private DispatcherTimer _timerClock;
        public ClientInfo SelectedCustomer { get; set; }
        public ObservableCollection<ClientInfo> customerInfos { get; set; }
        public AccountSelectionListView(Window owner, IEnumerable<ClientInfo> customerList, string title)
        {
            InitializeComponent();
            if (owner != null)
            {
                Owner = owner;
            }
            Loaded += (s, e) =>
            {
                try
                {
                    Title = $"{title} {Title}";
                    customerInfos = new ObservableCollection<ClientInfo>(customerList);
                    if (customerList != null && customerList.Count() > 0)
                    {
                        dgvSelectionList.ItemsSource = customerList;
                        if (dgvSelectionList.Items.Count >= 0)
                        {
                            dgvSelectionList.Focus();
                            dgvSelectionList.Dispatcher.BeginInvoke(
                            new Action(() =>
                            {
                                dgvSelectionList.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                            }),
                            DispatcherPriority.Input);
                            dgvSelectionList.SelectedIndex = 0;
                            dgvSelectionList.CurrentCell = new DataGridCellInfo(dgvSelectionList.Items[0], dgvSelectionList.Columns[0]);
                        }
                    }
                    lblDatagridCount.Text = $"Selected {dgvSelectionList.Items.Count.ToString("D6")}";
                    _utilityServices.SetWindowOnScreen(this);
                    _timerClock.Start();
                }
                catch (Exception ex)
                {
                    _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(AccountSelectionListView));
                }
            };
            Closed += (s, e) => _timerClock.Stop();
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    SelectedCustomer = null;
                    this.Close();
                }
                else if (e.Key == Key.Enter)
                {
                    SelectedCustomer = dgvSelectionList?.SelectedItem as ClientInfo;
                    this.Close();
                }
                else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    e.Handled = true;
                    TextBoxAddress.Focus();
                }
            };
            dgvSelectionList.PreviewKeyDown += (_, e) =>
            {
                if (e.Key == Key.Up && dgvSelectionList.SelectedIndex <= 0)
                {
                    e.Handled = true;
                    TextBoxAddress.Focus();
                }
            };
            dgvSelectionList.PreviewMouseDoubleClick += (_, __) =>
            {
                if (dgvSelectionList.SelectedIndex >= 0)
                {
                    SelectedCustomer = dgvSelectionList?.SelectedItem as ClientInfo;
                    this.Close();
                }
            };

            _timerClock = new DispatcherTimer { Interval = new System.TimeSpan(1000) };
            _timerClock.Tick += (s, e) => { lblDate.Text = $"Date: {DateTime.Now.ToString("MM/dd/yy")}"; lblTime.Text = $"Time {DateTime.Now.ToString("hh:mm:ss tt")}"; };
        }

        private void TextBoxCity_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = TextBoxAddress.Text?.Trim();
            if (string.IsNullOrEmpty(text))
            {
                dgvSelectionList.ItemsSource = customerInfos;
                return;
            }
            ApplyLookupData(text);
        }
        private async void ApplyLookupData(string SearchText)
        {
            dgvSelectionList.ItemsSource = customerInfos.Where(ci => ci.address1.ToUpper().Contains(SearchText.ToUpper()) || ci.address2.ToUpper().Contains(SearchText.ToUpper()));
        }
    }
}
