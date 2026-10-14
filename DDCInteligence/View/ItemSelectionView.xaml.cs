using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using static DDCInteligence.DataTypes.DatabaseTypes;
using static DDCInteligence.DataTypes.DocumentTypes;
using static DDCInteligence.DataTypes.LookupDatabaseTypes;

namespace DDCInteligence.View
{
    /// <summary>
    /// Interaction logic for ItemSelectionView.xaml
    /// </summary>
    public partial class ItemSelectionView : Window, INotifyPropertyChanged
    {
        private readonly UtilityServices _utilityServices;
        // Bound to the search bar text
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                Filter();
            }
        }
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }
        private IEnumerable<ClientCommodityInfo> suggestionList { get; set; }
        public string custNumber { get; set; }
        public ItemsInfo SelectedItem => ViewModelLocator.Instance.Items.SelectedItem as ItemsInfo;
        public ItemSelectionView(Window owner)
        {
            _utilityServices = new UtilityServices();
            InitializeComponent();
            if (owner != null)
                Owner = owner;

            Loaded += async (_, __) =>
            {
                IsLoading = true; // Show loader

                suggestionList = await Task.Run(() => ViewModelLocator.Instance.Items.GetDTLSuggestions(custNumber));

                if (itemsSuggestionAccountList != null && suggestionList != null && suggestionList.Count() > 0)
                {
                    itemsSuggestionAccountList.ItemsSource = suggestionList;

                    if (itemsSuggestionAccountList.Items.Count >= 0)
                    {
                        itemsSuggestionAccountList.Focus();
                        itemsSuggestionAccountList.SelectedIndex = 0;

                        await itemsSuggestionAccountList.Dispatcher.InvokeAsync(
                            () =>
                            {
                                itemsSuggestionAccountList.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                            },
                            DispatcherPriority.Input);
                    }
                }

                IsLoading = false; // Hide loader
                _utilityServices.SetWindowOnScreen(this);
            };
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape || e.Key == Key.Enter)
                {
                    CloseWindow();
                }
                else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    e.Handled = true;
                    TextBoxSearch.Focus();
                }
            };
            TextBoxSearch.PreviewKeyDown += TextBoxSearch_PreviewKeyDown;
            itemsSuggestionAccountList.PreviewKeyDown += ItemsSuggestionAccountList_PreviewKeyDown;
            itemsSuggestionAccountList.PreviewMouseDoubleClick += ItemsSuggestionAccountList_PreviewMouseDoubleClick;
        }

        private void ItemsSuggestionAccountList_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (itemsSuggestionAccountList.Items.Count > 0)
            {
                ApplySelectedItem();
            }
        }

        private void ItemsSuggestionAccountList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && itemsSuggestionAccountList.SelectedIndex <= 0)
            {
                e.Handled = true;
                TextBoxSearch.Focus();
            }
            else if (e.Key == Key.Enter && itemsSuggestionAccountList.SelectedItem != null)
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
                if (itemsSuggestionAccountList.Items.Count > 0)
                {
                    itemsSuggestionAccountList.Focus();
                    itemsSuggestionAccountList.SelectedIndex = 0;
                    itemsSuggestionAccountList.Dispatcher.BeginInvoke(
                         new Action(() =>
                         {
                             itemsSuggestionAccountList.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                         }),
                         DispatcherPriority.Input);
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
            if (itemsSuggestionAccountList.SelectedItem != null)
            {
                if (ViewModelLocator.Instance.Process.CurrentState == EState.ENTRY)
                {
                    AddSelectedItem();
                }
            }
            else
            {
                if (suggestionList != null && ViewModelLocator.Instance.Process.CurrentState == EState.ENTRY)
                {
                    AddSelectedItem();
                }
            }
            TextBoxSearch.Clear();
            CloseWindow();
        }
        private void AddSelectedItem()
        {
            var selected = itemsSuggestionAccountList.SelectedItem as ClientCommodityInfo;
            if (SelectedItem != null)
            {
                SelectedItem.description = selected.altcommoditydesc;
                SelectedItem.nmfc = selected.commoditycode;
            }
            else
            {
                ViewModelLocator.Instance.Items.NewItemWithData(new ItemsInfo
                {
                    description = selected.altcommoditydesc,
                    nmfc = selected.commoditycode
                });
            }
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
        void Filter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                itemsSuggestionAccountList.ItemsSource = suggestionList;
            }
            else
            {
                string filterText = SearchText.ToUpperInvariant();
                var filteredList = suggestionList;
                itemsSuggestionAccountList.ItemsSource = filteredList;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
