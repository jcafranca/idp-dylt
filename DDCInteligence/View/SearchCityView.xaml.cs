using DDCInteligence.Utilities.Services;
using System.Windows;
using System.Globalization;
using System.Windows.Input;
using System.Data.Linq;
using DDCInteligence.Models;
using GalaSoft.MvvmLight.Command;
using System.Windows.Documents;
using System.Collections.Generic;
using System;
using DDCInteligence.ViewModel;
using static DDCInteligence.DataTypes.LookupDatabaseTypes;
using System.Windows.Controls;

namespace DDCInteligence.View
{
    /// <summary>
    /// Interaction logic for SearchCityView.xaml
    /// </summary>
    public partial class SearchCityView : Window
    {
        private readonly UtilityServices _utilityServices = new UtilityServices();
        private readonly MySQLServices _mySQLServices = new MySQLServices();
        private readonly LogServices _logServices = new LogServices();

        private AccountsModel _dataContext;
        public SearchCityView(Window owner, AccountsModel data)
        {
            InitializeComponent();

            if (data != null) _dataContext = data;
            if (owner != null) Owner = owner;

            Title += _dataContext.IdentifierName;
            Loaded += SearchCityView_Loaded;
            InputBindings.AddRange(
                new List<KeyBinding>
                {
                    new KeyBinding { Command = new RelayCommand(this.Close), Key = Key.Escape },
                    new KeyBinding { Command = new RelayCommand(() =>
                    {
                        TextBoxCity.ClearValue(TextBox.TextProperty);
                    }), Key = Key.D, Modifiers = ModifierKeys.Control}
                }
            );
            dgvPoints.PreviewKeyDown += DgvPoints_PreviewKeyDown;
        }


        private  void SearchCityView_Loaded(object sender, RoutedEventArgs e)
        {
            _utilityServices.SetWindowOnScreen(this, 100);
            TextBoxCity.Text = _dataContext.City;
            ApplyLookupData(_dataContext.City);
        }

        private void DgvPoints_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                e.Handled = true;
                if (dgvPoints.Items.Count > 1)
                {
                    int currentRowIndex = dgvPoints.Items.IndexOf(dgvPoints.CurrentItem);
                    if (currentRowIndex == 0 && e.Key == Key.Up)
                        TextBoxCity.Focus();
                    else
                        MoveFocusCell(0, currentRowIndex, dgvPoints, e);
                }
                else
                    TextBoxCity.Focus();
            } else if (e.Key == Key.Enter)
            {
                e.Handled = true;
                _dataContext.City = _logServices.RemoveDoubleSpace(((PointInfo)dgvPoints.CurrentItem).POINT);
                this.Close();
            }
        }

        private async void ApplyLookupData(string City)
        {
            string TerminalCode = "";
            switch (_dataContext.IdentifierName.ToLower())
            {
                case "shipper":
                    break;
                case "consignee":
                    break;
            }

            // Get data lookup points
            var points = await _mySQLServices.GetQueryAsync<PointInfo>(ViewModelLocator.Instance.Process.LookUpConnectionString,
                 new Dictionary<string, object>() {
                     { "POINT", $"{City}"},
                 }, new List<string>() {"POINT"});

            dgvPoints.ItemsSource = points;
        }
        void MoveFocusCell(int currentColumnIndex, int currentRowIndex, DataGrid dataGrid, KeyEventArgs e)
        {
            // Determine the next cell to focus on
            int nextColumnIndex = currentColumnIndex;
            int nextRowIndex = currentRowIndex;

            if (nextRowIndex < 0) return;
            if (e.Key == Key.Left) nextColumnIndex--;
            else if (e.Key == Key.Right) nextColumnIndex++;
            else if (e.Key == Key.Up) nextRowIndex--;
            else if (e.Key == Key.Down) nextRowIndex++;

            // Ensure indices are within bounds
            if (nextColumnIndex < 0)
            {
                nextColumnIndex = dataGrid.Columns.Count - 1;
            }
            else if (nextColumnIndex >= dataGrid.Columns.Count)
            {
                nextColumnIndex = 0;
            }
            if (nextRowIndex < 0)
            {
                nextRowIndex = dataGrid.Items.Count - 1;
            }
            else if (nextRowIndex >= dataGrid.Items.Count)
            {
                nextRowIndex = 0;
                nextColumnIndex = currentColumnIndex - 1;
            }

            object nextItem = dataGrid.Items[nextRowIndex];
            dataGrid.SelectedItem = nextItem;
            if (nextColumnIndex < 0) nextColumnIndex = 0;
            dataGrid.CurrentCell = new DataGridCellInfo(nextItem, dataGrid.Columns[nextColumnIndex]);
            dataGrid.ScrollIntoView(nextItem, dataGrid.Columns[nextColumnIndex]);
            dataGrid.BeginEdit();

            // Ensure TextBox inside the new cell gets focus
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var newCell = dataGrid.CurrentCell;
                if (newCell.Column?.GetCellContent(newCell.Item) is TextBox newTextBox)
                {
                    newTextBox.Focus();
                    newTextBox.SelectAll();
                    try
                    {
                        ViewModelLocator.Instance.ImageViewer.pictureBox.SetRubberBand(newTextBox.Text);
                    }
                    catch { }
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void TextBoxCity_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = TextBoxCity.Text?.Trim();
            if (string.IsNullOrEmpty(text))
            {
                dgvPoints.ItemsSource = null;
                return;
            }
            ApplyLookupData(text);
        }
    }
}
