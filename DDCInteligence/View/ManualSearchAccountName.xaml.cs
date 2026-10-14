using DDCInteligence.Models;
using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using static DDCInteligence.DataTypes.LookupDatabaseTypes;

namespace DDCInteligence.View
{
    /// <summary>
    /// Interaction logic for ManualSearchAccountName.xaml
    /// </summary>
    public partial class ManualSearchAccountName : Window, INotifyPropertyChanged
    {
        private readonly LogServices logServices = new LogServices();
        private readonly AccountsModel accounts;
        private readonly bool isShownOnly;

        private DispatcherTimer _searchTimer;
        private DispatcherTimer _timerClock;
        private CancellationTokenSource _searchCts;

        private string _connectionString => ViewModelLocator.Instance.Process.LookUpConnectionString;

        public ClientInfo SelectedAccountData { get; private set; }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading == value) return;
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public ManualSearchAccountName(AccountsModel _accounts, bool isShownOnly = false)
        {
            InitializeComponent();

            if (DataContext == null)
                DataContext = this;

            accounts = _accounts;
            this.isShownOnly = isShownOnly;

            Title = $"Manual Search Account for {accounts.IdentifierName}";

            SetupSearchTimer();
            SetupKeyboardHandlers();
            SetupClock();

            TextBoxSearchName.Text = accounts.Name ?? string.Empty;
            TextBoxSearchAddress.Text = accounts.Address ?? string.Empty;

            PreviewKeyDown += ManualSearchAccountName_PreviewKeyDown;
            Loaded += ManualSearchAccountName_Loaded;
            Closed += ManualSearchAccountName_Closed;

            lblDatagridCount.Text = $"Selected {0:D6}";
        }

        private async void ManualSearchAccountName_Loaded(object sender, RoutedEventArgs e)
        {
            TextBoxSearchName.Focus();
            TextBoxSearchName.SelectAll();

            if (!string.IsNullOrWhiteSpace(TextBoxSearchName.Text) ||
                !string.IsNullOrWhiteSpace(TextBoxSearchAddress.Text))
            {
                await StartSearchAsync();
            }
        }

        private void ManualSearchAccountName_Closed(object sender, EventArgs e)
        {
            _timerClock?.Stop();
            _searchTimer?.Stop();

            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _searchCts = null;
        }

        private void ManualSearchAccountName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (!isShownOnly)
                    DialogResult = false;

                Close();
            }
        }

        private void SetupClock()
        {
            _timerClock = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timerClock.Tick += (s, e) =>
            {
                lblDate.Text = $"Date: {DateTime.Now:MM/dd/yy}";
                lblTime.Text = $"Time: {DateTime.Now:hh:mm:ss tt}";
            };

            lblDate.Text = $"Date: {DateTime.Now:MM/dd/yy}";
            lblTime.Text = $"Time: {DateTime.Now:hh:mm:ss tt}";
            _timerClock.Start();
        }

        private void SetupSearchTimer()
        {
            _searchTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };

            _searchTimer.Tick += async (s, e) =>
            {
                _searchTimer.Stop();
                await StartSearchAsync();
            };
        }

        private void RestartSearchTimer()
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void SetupKeyboardHandlers()
        {
            TextBoxSearchName.PreviewKeyDown += TextBox_PreviewKeyDown;
            TextBoxSearchAddress.PreviewKeyDown += TextBox_PreviewKeyDown;

            TextBoxSearchName.TextChanged += (s, e) => RestartSearchTimer();
            TextBoxSearchAddress.TextChanged += (s, e) => RestartSearchTimer();

            dgvSelectionList.PreviewKeyDown += DataGrid_PreviewKeyDown;
            dgvSelectionList.MouseDoubleClick += (s, e) => SelectAndClose();
        }

        private async void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _searchTimer.Stop();
                await StartSearchAsync();

                if (dgvSelectionList.Items.Count > 0)
                {
                    if (dgvSelectionList.SelectedItem == null)
                        dgvSelectionList.SelectedIndex = 0;

                    SelectAndClose();
                }

                e.Handled = true;
            }
            else if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ClearAllFields();
                e.Handled = true;
            }
            else if (e.Key == Key.Down && dgvSelectionList.Items.Count > 0)
            {
                dgvSelectionList.Focus();

                if (dgvSelectionList.SelectedItem == null)
                    dgvSelectionList.SelectedIndex = 0;

                e.Handled = true;
            }
        }

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SelectAndClose();
                e.Handled = true;
            }
            else if (e.Key == Key.Up && dgvSelectionList.SelectedIndex <= 0)
            {
                TextBoxSearchAddress.Focus();
                e.Handled = true;
            }
            else if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ClearAllFields();
                e.Handled = true;
            }
        }

        private async Task StartSearchAsync()
        {
            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _searchCts = new CancellationTokenSource();

            var token = _searchCts.Token;

            try
            {
                await PerformSearchAsync(token);
            }
            catch (OperationCanceledException)
            {
                // Ignore canceled searches
            }
        }

        private async Task PerformSearchAsync(CancellationToken cancellationToken)
        {
            string nameFilter = TextBoxSearchName.Text?.Trim() ?? string.Empty;
            string addressFilter = TextBoxSearchAddress.Text?.Trim() ?? string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(nameFilter) &&
                    string.IsNullOrWhiteSpace(addressFilter))
                {
                    dgvSelectionList.ItemsSource = null;
                    dgvSelectionList.SelectedItem = null;
                    lblDatagridCount.Text = $"Selected {0:D6}";
                    IsLoading = false;
                    return;
                }

                IsLoading = true;

                var results = await Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return SearchNameDataInMySQL(nameFilter, addressFilter);
                }, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                dgvSelectionList.ItemsSource = results;

                if (results.Count == 1)
                    dgvSelectionList.SelectedIndex = 0;
                else if (results.Count == 0)
                    dgvSelectionList.SelectedItem = null;

                lblDatagridCount.Text = $"Selected {results.Count:D6}";
            }
            catch (OperationCanceledException)
            {
                // Ignore canceled searches
            }
            catch (Exception ex)
            {
                logServices.ErrorLogToFile(
                    $"Error performing account search: {ex.Message}",
                    ex.StackTrace,
                    nameof(PerformSearchAsync));

                MessageBox.Show(
                    $"Error searching account name data: {ex.Message}",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                if (!cancellationToken.IsCancellationRequested)
                    IsLoading = false;
            }
        }

        private List<ClientInfo> SearchNameDataInMySQL(string name, string address)
        {
            var accountDataList = new List<ClientInfo>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    var queryParts = new List<string>();

                    string baseQuery = @"
                        SELECT 
                            clientid AS clientId,
                            name,
                            address1,
                            address2,
                            city,
                            provine,
                            postalcode
                        FROM client_tbl
                        WHERE clientid REGEXP '^[0-9]+$'";

                    using (var command = new MySqlCommand())
                    {
                        command.Connection = connection;

                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            queryParts.Add("TRIM(name) LIKE @name");
                            command.Parameters.AddWithValue("@name", $"{name}%");
                        }

                        if (!string.IsNullOrWhiteSpace(address))
                        {
                            queryParts.Add("(TRIM(address1) LIKE @address OR TRIM(address2) LIKE @address)");
                            command.Parameters.AddWithValue("@address", $"{address}%");
                        }

                        if (queryParts.Count > 0)
                        {
                            baseQuery += " AND " + string.Join(" AND ", queryParts);
                        }

                        baseQuery += " ORDER BY name, address1 LIMIT 100";
                        command.CommandText = baseQuery;

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                accountDataList.Add(new ClientInfo
                                {
                                    clientId = reader["clientId"]?.ToString() ?? string.Empty,
                                    name = reader["name"]?.ToString() ?? string.Empty,
                                    address1 = reader["address1"]?.ToString() ?? string.Empty,
                                    address2 = reader["address2"]?.ToString() ?? string.Empty,
                                    city = reader["city"]?.ToString() ?? string.Empty,
                                    provine = reader["provine"]?.ToString() ?? string.Empty,
                                    postalcode = reader["postalcode"]?.ToString() ?? string.Empty
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logServices.ErrorLogToFile(
                    $"Error searching account name data: {ex.Message}",
                    ex.StackTrace,
                    nameof(SearchNameDataInMySQL));
            }

            return accountDataList;
        }

        private void ClearAllFields()
        {
            _searchTimer?.Stop();

            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _searchCts = null;

            TextBoxSearchName.Clear();
            TextBoxSearchAddress.Clear();

            dgvSelectionList.SelectedItem = null;
            dgvSelectionList.ItemsSource = null;

            lblDatagridCount.Text = $"Selected {0:D6}";
            IsLoading = false;

            TextBoxSearchName.Focus();
        }

        private void SelectAndClose()
        {
            if (ViewModelLocator.Instance.Process.CurrentState == DataTypes.DocumentTypes.EState.ENTRY)
            {
                if (dgvSelectionList.SelectedItem is ClientInfo selectedData)
                {
                    SelectedAccountData = selectedData;

                    if (!isShownOnly)
                        DialogResult = true;

                    Close();
                }
                else
                {
                    MessageBox.Show(
                        "Please select a name from the list.",
                        "No Selection",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            else
            {
                if (!isShownOnly)
                    DialogResult = false;

                SelectedAccountData = null;
                Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}