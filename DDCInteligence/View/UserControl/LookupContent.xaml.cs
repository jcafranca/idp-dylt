using DDCInteligence.DataTypes;
using DDCInteligence.ViewModel;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static DDCInteligence.DataTypes.DatabaseTypes;
using static DDCInteligence.DataTypes.DocumentTypes;
using static DDCInteligence.DataTypes.LookupDatabaseTypes;

namespace DDCInteligence.View.UserControl
{
    /// <summary>
    /// Interaction logic for LookupContent.xaml
    /// </summary>
    public partial class LookupContent 
    {
        // Selected Index of the datagrid in parent
        public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register("SelectedItem", typeof(object), typeof(LookupContent), new PropertyMetadata(null));

        public object SelectedItem
        {
            get => (object)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }
        // Title for the header
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(LookupContent), new PropertyMetadata(""));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        // Subtitle for additional description
        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register("Subtitle", typeof(string), typeof(LookupContent), new PropertyMetadata(""));

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        // Placeholder for the search bar
        public static readonly DependencyProperty SearchPlaceholderProperty =
            DependencyProperty.Register("SearchPlaceholder", typeof(string), typeof(LookupContent), new PropertyMetadata("Search code..."));

        public string SearchPlaceholder
        {
            get => (string)GetValue(SearchPlaceholderProperty);
            set => SetValue(SearchPlaceholderProperty, value);
        }

        // Bound to the search bar text
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("SearchText", typeof(string), typeof(LookupContent), new PropertyMetadata(string.Empty));

        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        // ItemsSource for the DataGrid
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(LookupContent), new PropertyMetadata(null));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        public ICommand SelectedItemCommand { get; set; }

        // Text for the reference label in the description column
        public static readonly DependencyProperty ReferenceTextProperty =
            DependencyProperty.Register("ReferenceText", typeof(string), typeof(LookupContent), new PropertyMetadata("Reference"));

        public string ReferenceText
        {
            get => (string)GetValue(ReferenceTextProperty);
            set => SetValue(ReferenceTextProperty, value);
        }
        public LookupContent()
        {
            InitializeComponent();
            this.Loaded += LookupContent_Loaded;
            TextBoxSearch.PreviewKeyDown += TextBoxSearch_PreviewKeyDown;
            dgvLookup.PreviewKeyDown += DgvLookup_PreviewKeyDown;
            dgvLookup.PreviewMouseDoubleClick += DgvLookup_PreviewMouseDoubleClick;
            SelectedItemCommand = new RelayCommand(() =>
            {
                if (dgvLookup.Items.Count > 0)
                {
                    ApplySelectedItem();
                }
            });
        }
        private void LookupContent_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                TextBoxSearch.Focus();
            }));
        }
        private void DgvLookup_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvLookup.Items.Count > 0)
            {
                ApplySelectedItem();
            }
        }
        private void DgvLookup_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && dgvLookup.SelectedIndex <= 0)
            {
                e.Handled = true;
                TextBoxSearch.Focus();
            }
            else if (e.Key == Key.Enter && dgvLookup.SelectedItem != null)
            {
                e.Handled = true;
                ApplySelectedItem();
            }
        }
        private void TextBoxSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                e.Handled = true;
                if (dgvLookup.Items.Count > 0)
                {
                    dgvLookup.Focus();
                    dgvLookup.SelectedIndex = 0;

                    DataGridRow row = (DataGridRow)dgvLookup.ItemContainerGenerator.ContainerFromIndex(0);
                    if (row != null)
                    {
                        row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    }
                }
            }
            else if (e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(TextBoxSearch.Text))
                {
                    return;
                }
                ApplySelectedItem();
            }
        }
        private void ApplySelectedItem()
        {
            if (dgvLookup.SelectedItem != null)
            {
                if (ViewModelLocator.Instance.Process.CurrentState == EState.ENTRY)
                {
                    var dgv_item = (ChargesCodesInfo)dgvLookup.SelectedItem;
                    NewItem(dgv_item);
                }
            }
            else
            {
                var temp_selected = dgvLookup.Items.Cast<ChargesCodesInfo>().ToArray()
                    .ToList().Where(item => item.CODE.ToUpper().Contains(TextBoxSearch.Text.ToUpper()))
                    .FirstOrDefault();

                if (temp_selected != null && ViewModelLocator.Instance.Process.CurrentState == EState.ENTRY)
                {
                    NewItem(temp_selected);
                }
            }
            TextBoxSearch.Clear();
            CloseWindow();

            ViewModelLocator.Instance.Process.InitializeLookupChargesCodes();
            ViewModelLocator.Instance.Items.InitializeLookupCode();
        }
        void NewItem(ChargesCodesInfo info)
        {
            //if (info != null)
            //{
            //    if (this.SelectedItem is ItemsInfo i)
            //    {
            //        i.charge = info.CODE;
            //        //i.description = info.description;
            //    }
            //}
        }
        void CloseWindow()
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                if (IsWindowModal(parentWindow) && parentWindow.DialogResult != null)
                {
                    parentWindow.DialogResult = true;
                }
                parentWindow.Close();
            }
        }
        public bool IsWindowModal(Window wind)
        {
            return wind.Owner != null;
        }
    }
}
