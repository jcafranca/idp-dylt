using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using WindowsInput.Native;
using WindowsInput;
using System;
using DDCInteligence.Utilities.Events;

namespace DDCInteligence.View.UserControl
{
    /// <summary>
    /// Interaction logic for ItemsContent.xaml
    /// </summary>
    public partial class ItemsContent 
    {
        private IRubberBand rubberBand;
        private SharedEvent _sharedEvent;
        private readonly LogServices logServices = new LogServices();
        public ItemsContent()
        {
            InitializeComponent();
            GotKeyboardFocus += (sender, e) =>
            {
                if (e.OriginalSource is TextBox)
                {
                    rubberBand = new RubberBandServices();
                    rubberBand.SetTarget(e.OriginalSource, ((TextBox)e.OriginalSource).DataContext, BindingOperations.GetBinding((TextBox)e.OriginalSource, TextBox.TextProperty).Path.Path);
                    ViewModelLocator.Instance.Process.SetRubberBand(rubberBand);
                }
            };
            
            // Subscribe to the event from the ViewModel
            var viewModel = (ItemsViewModel)this.DataContext;
            viewModel.NewItemAdded += FocusNewItem;
            _sharedEvent = ViewModelLocator.Instance.SharedEvent;
        }

        private void DgvItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _sharedEvent.SelectedIndex = DgvItems.SelectedIndex;
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void DataGridTextColumn_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Back)
            {
                e.Handled = !IsTextAllowed("");
            }
        }
        public void FocusNewItem()
        {
            if (this.DgvItems.Items.Count > 0)
            {
                this.DgvItems.Focus();   // Focus the DataGrid
                this.DgvItems.CancelEdit();  // Cancel any pending edits
                this.DgvItems.SelectedIndex = this.DgvItems.Items.Count - 1;  // Select the newly added row

                object nextItem = this.DgvItems.Items[this.DgvItems.Items.Count - 1];
                this.DgvItems.CurrentCell = new DataGridCellInfo(nextItem, this.DgvItems.Columns[1]);
                this.DgvItems.SelectedItem = nextItem;
                this.DgvItems.BeginEdit();
                // Ensure TextBox inside the new cell gets focus
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var newCell = this.DgvItems.CurrentCell;
                    if (newCell.Column?.GetCellContent(newCell.Item) is TextBox newTextBox)
                    {
                        newTextBox.Focus();
                        newTextBox.SelectAll();
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);

            }
        }

        bool IsTextAllowed(string text)
        {
            // Regular expression to match only digits
            return Regex.IsMatch(text, "^[0-9]+$");
        }
        private void DataGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            _sharedEvent.SelectedDataGrid = (DataGrid)sender;
        }
        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridCell cell && !cell.IsEditing && cell.Content is TextBlock)
            {
                cell.Focus();
                DgvItems.SelectedItem = cell.DataContext;
                DgvItems.BeginEdit();
            }
        }
    }
}
