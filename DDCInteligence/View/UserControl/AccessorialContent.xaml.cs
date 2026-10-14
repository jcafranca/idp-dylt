using DDCInteligence.DataTypes;
using DDCInteligence.Models;
using DDCInteligence.Utilities.Events;
using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DDCInteligence.View.UserControl
{
    /// <summary>
    /// Interaction logic for AccessorialContent.xaml
    /// </summary>
    public partial class AccessorialContent 
    {
        private IRubberBand rubberBand;
        private SharedEvent _sharedEvent;
        private readonly LogServices logServices = new LogServices();
        public AccessorialContent()
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
            var viewModel = (AccessorialModel)this.DataContext;
            viewModel.NewItemAdded += FocusNewItem;
            _sharedEvent = ViewModelLocator.Instance.SharedEvent;
        }
        private void DgvItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
            if (this.dgvAccessorials.Items.Count > 0)
            {
                this.dgvAccessorials.Focus();   // Focus the DataGrid
                this.dgvAccessorials.CancelEdit();  // Cancel any pending edits
                this.dgvAccessorials.SelectedIndex = this.dgvAccessorials.Items.Count - 1;  // Select the newly added row

                object nextItem = this.dgvAccessorials.Items[this.dgvAccessorials.Items.Count - 1];
                this.dgvAccessorials.CurrentCell = new DataGridCellInfo(nextItem, this.dgvAccessorials.Columns[1]);
                this.dgvAccessorials.SelectedItem = nextItem;
                this.dgvAccessorials.BeginEdit();
                // Ensure TextBox inside the new cell gets focus
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var newCell = this.dgvAccessorials.CurrentCell;
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
        }
        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is DataGridCell cell && !cell.IsEditing && cell.Content is TextBlock)
                {
                    cell.Focus();
                    dgvAccessorials.SelectedItem = cell.DataContext;
                    dgvAccessorials.BeginEdit();
                    ViewModelLocator.Instance.ImageViewer.pictureBox.SetRubberBand(cell.Content.ToString());
                    this.rubberBand.ScrollToText(cell.Content.ToString());
                }
            }
            catch (Exception ex)
            {
                logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(DataGridCell_PreviewMouseLeftButtonDown));
            }
        }
    }
}
