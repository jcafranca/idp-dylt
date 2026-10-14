using DDCInteligence.DataTypes;
using DDCInteligence.ViewModel;
using GalaSoft.MvvmLight.Command;
using SweetAlertSharp.Enums;
using SweetAlertSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight.Messaging;
using static DDCInteligence.DataTypes.KeysInfo;
using DDCInteligence.Utilities.Services;
using static DDCInteligence.DataTypes.DocumentTypes;

namespace DDCInteligence.View
{
    /// <summary>
    /// Interaction logic for KeywordsContent.xaml
    /// </summary>
    public partial class DimensionsView : Window
    {
        private readonly UtilityServices _utilityServices = new UtilityServices();
        public ObservableCollection<Dimensions> DimensionsCollection { get; } = new ObservableCollection<Dimensions>();
        public DimensionsView(Window owner)
        {
            InitializeComponent();
            if (owner != null)
            {
                this.Owner = owner;
            }
            this.DataContext = this; // Set DataContext for binding
            this.Loaded += DimensionsView_Loaded;
            this.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.F2)
                {
                    SaveDimensions();
                }
                else if (e.Key == Key.F10 || e.SystemKey == Key.F10)
                {
                    ClearDimensions();
                }
                else if (e.Key == Key.Escape)
                {
                    this.Close();
                }
            };
        }

        private void DimensionsView_Loaded(object sender, RoutedEventArgs e)
        {
            _utilityServices.SetWindowOnScreen(this, 100);
            SetDimensionsData();
        }

        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridCell cell && !cell.IsEditing && cell.Content is TextBlock)
            {
                cell.Focus();
                DgvDimensions.SelectedItem = cell.DataContext;
                DgvDimensions.BeginEdit();
            }
        }

        public void UpdateShieldValue(HandyControl.Controls.Shield subject, string newValue)
        {
            subject.Status = newValue;
        }
        void SetDimensionsData()
        {
            //var items = ViewModelLocator.Instance.Items.Items;
            //if (items == null || items.Count == 0) return;
            //// Clear existing items just in case
            //DimensionsCollection.Clear();

            //for (int i = 0; i < items.Count; i++)
            //{
            //    DimensionsCollection.Add(new Dimensions
            //    {
            //        keyword = items[i],
            //        hu_number = i + 1,
            //        length = items[i].dimensions?.length ?? "0",
            //        width = items[i].dimensions?.width ?? "0",
            //        height = items[i].dimensions?.height ?? "0",
            //        uom = items[i].dimensions?.uom ?? ""
            //    });
            //}
            //if (DimensionsCollection.Count() > 0)
            //{
            //    focusElement();
            //}
        }

        void focusElement()
        {
            int index = this.DgvDimensions.Items.Count - 1;
            object nextItem = this.DgvDimensions.Items[index];

            this.DgvDimensions.Focus();   // FocusTargetElement the DataGrid
            this.DgvDimensions.CancelEdit();  // Cancel any pending edits
            this.DgvDimensions.CurrentCell = new DataGridCellInfo(nextItem, this.DgvDimensions.Columns[1]);
            this.DgvDimensions.SelectedItem = nextItem;
            this.DgvDimensions.BeginEdit();

            // Ensure TextBox inside the new cell gets focus
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var newCell = this.DgvDimensions.CurrentCell;
                if (newCell.Column?.GetCellContent(newCell.Item) is TextBox newTextBox)
                {
                    newTextBox.Focus();
                    newTextBox.SelectAll();
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void FrameworkElement_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Try to find the parent DataGridCell
                var cell = FindParent<DataGridCell>(textBox);
                if (cell != null && cell.Column is DataGridTextColumn column)
                {
                    // You can use the column.Header or any identifying logic here
                    var columnHeader = column.Header?.ToString();

                    if (columnHeader != "UOM") // match your column by header name
                    {
                        e.Handled = !IsTextNumeric(e.Text);
                    }
                }
            }
        }
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as T;
        }
        private bool IsTextNumeric(string text)
        {
            return text.All(char.IsDigit);
        }
        private void DimensionButtonDone_Click(object sender, RoutedEventArgs e)
        {
            SaveDimensions();
        }

        private void DimensionsButtonDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            ClearDimensions();
        }

        private void dimsButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        void SaveDimensions()
        {
            DimensionsCollection.ToList().ForEach(dims =>
            {
                var items = ViewModelLocator.Instance.Items.Items.SingleOrDefault(item => item == dims.keyword);
                //items.dimensions = dims;
            });
            HandyControl.Controls.Growl.Success("Dimensions saved successfully!");
        }

        void ClearDimensions()
        {
            var alert = new SweetAlert
            {
                Caption = "Confirmation",
                Message = "Do you want to clear all data?",
                MsgImage = SweetAlertImage.QUESTION,
                MsgButton = SweetAlertButton.YesNo,
                OkText = "Yes",
                CancelText = "Cancel"
            };
            if (alert.ShowDialog() == SweetAlertResult.OK)
            {
                DimensionsCollection.ToList().ForEach(dims =>
                {
                    dims.length = "0";
                    dims.width = "0";
                    dims.height = "0";
                    dims.uom = null;
                });
            }
        }
    }
}
