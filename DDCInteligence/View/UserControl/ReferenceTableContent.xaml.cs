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
using DDCInteligence.Models;
using DDCInteligence.Utilities.Events;
using System.Windows.Media;
using System.Windows.Threading;
using static DDCInteligence.DataTypes.DatabaseTypes;

namespace DDCInteligence.View.UserControl
{
    /// <summary>
    /// Interaction logic for ReferenceTableContent.xaml
    /// </summary>
    public partial class ReferenceTableContent
    {
        private IRubberBand rubberBand;
        private SharedEvent _sharedEvent;
        private readonly LogServices logServices = new LogServices();
        private TextBox _textBox;
        public ReferenceTableContent()
        {
            InitializeComponent();
            GotKeyboardFocus += (sender, e) =>
            {                
                if (e.OriginalSource is TextBox textBox)
                {
                    _textBox = textBox;
                    rubberBand = new RubberBandServices();
                    rubberBand.SetTarget(e.OriginalSource, textBox.DataContext, BindingOperations.GetBinding(textBox, TextBox.TextProperty).Path.Path);
                    ViewModelLocator.Instance.Process.SetRubberBand(rubberBand);
                }
            };
            var viewModel = (ReferenceModel)this.DataContext;
            // Subscribe to the event from the ViewModel
            viewModel.NewItemAdded += FocusNewItem;
            _sharedEvent = ViewModelLocator.Instance.SharedEvent;
            //DgvReference.BeginningEdit += DgvReference_BeginningEdit;
        }

        private void DgvReference_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            DgvReference.Columns[1].IsReadOnly = false;
            if (DgvReference.Items.Count > 0 && _textBox != null)
            {
                // Get the index of the row being edited
                int rowIndex = DgvReference.Items.IndexOf(DgvReference?.SelectedItem);
                int columnIndex = e.Column.DisplayIndex;
                if (rowIndex >= 0 && rowIndex < 3 && columnIndex >= 0 && columnIndex == 1)
                {
                    e.Cancel = true; // Cancel the edit operation
                }
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }
        private void DgvReference_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _sharedEvent.SelectedIndex = DgvReference.SelectedIndex;
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
            if (this.DgvReference.Items.Count > 0)
            {
                int index = this.DgvReference.Items.Count - 1;
                this.DgvReference.Focus();   // Focus the DataGrid
                this.DgvReference.CancelEdit();  // Cancel any pending edits
                this.DgvReference.SelectedIndex = index;  // Select the newly added row

                object nextItem = this.DgvReference.Items[index];
                this.DgvReference.CurrentCell = new DataGridCellInfo(nextItem, this.DgvReference.Columns[1]);
                this.DgvReference.SelectedItem = nextItem;
                this.DgvReference.BeginEdit();
                // Ensure TextBox inside the new cell gets focus
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var newCell = this.DgvReference.CurrentCell;
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
            // This sets the currently focused DataGrid in the shared service.
            _sharedEvent.SelectedDataGrid = (DataGrid)sender;
        }
        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridCell cell && cell != null && !cell.IsEditing && cell.Content is TextBlock)
            {
                cell.Focus();
                DgvReference.SelectedItem = cell.DataContext;
                DgvReference.BeginEdit();

                //if (DgvReference.CurrentCell.Column != null && DgvReference.CurrentCell.Column.DisplayIndex == 2)
                //{
                //    var selectedItem = DgvReference.SelectedItem as ReferenceInfo;
                //    _ = ViewModelLocator.Instance.Process.HandleTermsPromptAsync(cell.Content.ToString(), this, $"reference{selectedItem.code}");
                //} else
                //    ViewModelLocator.Instance.Process.ClearPrompt();
            }
        }
    }
}
