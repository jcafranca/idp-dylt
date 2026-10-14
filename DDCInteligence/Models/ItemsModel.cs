using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Helpers.Validation;
using DDCInteligence.ViewModel;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Input;
using DDCInteligence.Utilities.Services;
using static DDCInteligence.DataTypes.DatabaseTypes;
using static DDCInteligence.DataTypes.DocumentTypes;
using static DDCInteligence.DataTypes.LookupDatabaseTypes;
using MySql.Data.MySqlClient;
using DDCInteligence.Utilities.Helpers;
using System.Threading.Tasks;
using Dapper;
using System.Security.Cryptography;

namespace DDCInteligence.Models
{
    public class ItemsModel : ValidatableBase
    {
        private readonly LogServices logServices = new LogServices();
        private readonly IProcess process;
        private ObservableCollection<ChargesCodesInfo> _filteredSuggestions
        {
            get
            {
                return new ObservableCollection<ChargesCodesInfo>(
                    process.LookupChargeCodes);
            }
        }
        public ObservableCollection<ItemsInfo> Items
        {
            get => process?.CurrentDocument?.ItemsInfos;
        }
        private ObservableCollection<ChargesCodesInfo> _itemsLookup;
        public ObservableCollection<ChargesCodesInfo> ItemsLookup
        {
            get => _itemsLookup;
            set
            {
                _itemsLookup = value;
                RaisePropertyChanged(nameof(ItemsLookup));
            }
        }
        public ObservableCollection<string> AdditionalItemInformations {  get; set; }
        public int? TotalPieces
        {
            get
            {
                var t_items = process?.CurrentDocument?.ItemsInfos?
                                          .Select(tp => tp.pcs)
                                          .Where(tp => int.TryParse(tp, out int v))
                                          .Sum(tp => int.Parse(tp));
                if (t_items != null)
                {
                    return t_items;
                }
                else
                {
                    return 0;
                }
            }
        }
        public decimal? TotalWeight
        {
            get
            {
                var t_weight = process?.CurrentDocument?.ItemsInfos?
                                            .Select(tw => tw.weight)
                                            .Where(tw => decimal.TryParse(tw, out decimal v))
                                            .Sum(tw => decimal.Parse(tw));
                if (t_weight != null)
                    return t_weight;
                else
                    return 0;
            }
        }
        private ItemsInfo _selectedItem;
        public ItemsInfo SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                RaisePropertyChanged(nameof(SelectedItem));
            }
        }
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                RaisePropertyChanged(nameof(SearchText));
                Filter();
            }
        }
        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }

        // Events to notify the view for UI updates
        public event Action NewItemAdded;
        public event Action<int> ItemInserted; // New event for insertion with index
        public event Action ItemsRefreshRequired; // New event for general refresh

        public ItemsModel(IProcess process)
        {
            this.process = process;
            this.process.PropertyChanged += (_, __) => RaisePropertyChanged(null);
            this.process.RegisterErrorSource(this);

            AddItemCommand = new RelayCommand(NewItem);
            RemoveItemCommand = new RelayCommand<ItemsInfo>(RemoveItem);
            InitializeLookupCode();

            //RegisterValidator(nameof(Items), ValidationHelper.ValidateDataItems);
        }

        protected void OnNewItemAdded()
        {
            NewItemAdded?.Invoke();
        }

        protected void OnItemInserted(int index)
        {
            ItemInserted?.Invoke(index);
        }

        protected void OnItemsRefreshRequired()
        {
            ItemsRefreshRequired?.Invoke();
        }

        void NewItem()
        {
            if (ViewModelLocator.Instance.Process.CurrentState == EState.ENTRY)
            {
                if (ViewModelLocator.Instance.Process.CurrentDocument?.ItemsInfos == null) return;
                process.CurrentDocument?.ItemsInfos.Add(new ItemsInfo { });

                // Force refresh of UI properties
                RaisePropertyChanged(nameof(Items));
                RaisePropertyChanged(nameof(TotalPieces));
                RaisePropertyChanged(nameof(TotalWeight));

                OnNewItemAdded();
                OnItemsRefreshRequired();
            }
        }
        public async Task<IEnumerable<ClientCommodityInfo>> GetDTLSuggestions(string custNumber)
        {
            if (string.IsNullOrWhiteSpace(custNumber)) return Enumerable.Empty<ClientCommodityInfo>();

            using (var connection = new MySqlConnection(process.LookUpConnectionString))
            {
                // Use a single query with JOIN instead of two separate queries
                const string combinedQuery = @"SELECT * FROM client_commodity_tbl WHERE clientId = @custNumber";
                var results = await connection.QueryAsync<ClientCommodityInfo>(combinedQuery, new { custNumber = custNumber });
                if (!results?.Any() == true) return Enumerable.Empty<ClientCommodityInfo>();
                return results;
            }
        }

        private List<HDRInfo> ProcessDescriptions(IEnumerable<HDRInfo> results)
        {
            var processedResults = new List<HDRInfo>();
            var pendingDescriptions = new List<string>();

            foreach (var item in results)
            {
                if (item.DESCR?.StartsWith("      ") == true)
                {
                    // Process accumulated descriptions
                    if (pendingDescriptions.Count > 0)
                    {
                        var concatenatedDescr = string.Join(", ", pendingDescriptions.Distinct().Reverse());
                        item.DESCR = logServices.RemoveDoubleSpace($"{item.DESCR} {concatenatedDescr}");
                        pendingDescriptions.Clear();
                    } else
                        item.DESCR = logServices.RemoveDoubleSpace(item.DESCR);
                    processedResults.Add(item);
                }
                else if (!string.IsNullOrWhiteSpace(item.DESCR))
                {
                    pendingDescriptions.Add(item.DESCR);
                }
            }

            // Handle any remaining descriptions
            if (pendingDescriptions.Count > 0)
            {
                AdditionalItemInformations = new ObservableCollection<string>(pendingDescriptions.Distinct());
            }

            return processedResults;
        }

        private static IEnumerable<HDRInfo> GetLatestItemsByDescription(IEnumerable<HDRInfo> items)
        {
            return items
                .GroupBy(item => item.DESCR)
                .Select(group => group
                    .Where(item => int.TryParse(item.EFF_DATE, out _))
                    .OrderByDescending(item => int.Parse(item.EFF_DATE))
                    .FirstOrDefault())
                .Where(item => item != null);
        }

        public void InsertNewItemAt(int index)
        {
            if (process.CurrentState == EState.ENTRY)
            {
                if (process.CurrentDocument?.ItemsInfos == null) return;

                var newItem = new ItemsInfo {};

                if (index < 0 || index > ViewModelLocator.Instance.Process.CurrentDocument?.ItemsInfos.Count)
                {
                    process.CurrentDocument?.ItemsInfos.Add(newItem);
                    index = ViewModelLocator.Instance.Process.CurrentDocument.ItemsInfos.Count - 1;
                }
                else
                {
                    process.CurrentDocument?.ItemsInfos.Insert(index, newItem);
                }

                // Force refresh of UI properties
                RaisePropertyChanged(nameof(Items));
                RaisePropertyChanged(nameof(TotalPieces));
                RaisePropertyChanged(nameof(TotalWeight));

                // Update selected item to the newly inserted item
                SelectedItem = newItem;

                // Notify view about the insertion
                OnItemInserted(index);
                OnItemsRefreshRequired();
            }
        }
        public void NewItemWithData(ItemsInfo info)
        {
            if (process.CurrentState == EState.ENTRY)
            {
                if (process.CurrentDocument?.ItemsInfos == null) return;
                process.CurrentDocument?.ItemsInfos.Add(info);

                // Force refresh of UI properties
                RaisePropertyChanged(nameof(Items));
                RaisePropertyChanged(nameof(TotalPieces));
                RaisePropertyChanged(nameof(TotalWeight));

                // Update selected item
                SelectedItem = info;

                OnItemsRefreshRequired();
            }
        }
        void RemoveItem(ItemsInfo info)
        {
            if (info != null && process.CurrentState == EState.ENTRY)
            {
                if (process.CurrentDocument.RemoveItems is null)
                    process.CurrentDocument.RemoveItems = new ObservableCollection<ItemsInfo>();

                process.CurrentDocument.ItemsInfos.Remove(info);
                process.CurrentDocument.RemoveItems.Add(info);

                // Force refresh of UI properties
                RaisePropertyChanged(nameof(Items));
                RaisePropertyChanged(nameof(TotalPieces));
                RaisePropertyChanged(nameof(TotalWeight));

                // Clear selection if removed item was selected
                if (SelectedItem == info)
                {
                    SelectedItem = null;
                }

                OnItemsRefreshRequired();
            }
        }

        void Filter()
        {
            if (!string.IsNullOrEmpty(SearchText))
            {
                ItemsLookup.Clear();
                List<ChargesCodesInfo> filteredList = _filteredSuggestions
                    .Where(s => s.CODE.ToUpper().Contains(SearchText))
                    .ToList(); ;

                filteredList.ToArray().ToList().ForEach(item => ItemsLookup.Add(item));
            }
            else
            {
                ItemsLookup = _filteredSuggestions;
            }
        }

        public void ItemsInfos_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ItemsInfo n in e.NewItems)
                    {
                        n.PropertyChanged += DataItemsPropertyChanged;
                        this.process.RegisterErrorSource(n);
                        RegisterValidators(n);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ItemsInfo o in e.OldItems)
                    {
                        o.PropertyChanged -= DataItemsPropertyChanged;
                        this.process.UnregisterErrorSource(o);
                    }
                    break;
            }

            // Force refresh after any collection change
            RaisePropertyChanged(nameof(Items));
            RaisePropertyChanged(nameof(TotalPieces));
            RaisePropertyChanged(nameof(TotalWeight));
            OnItemsRefreshRequired();
        }

        private void DataItemsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(null);
        }

        public void InitializeLookupCode()
        {
            this.ItemsLookup = _filteredSuggestions;
        }

        public void RegisterValidators(ItemsInfo item)
        {
            //item.RegisterValidator(nameof(ItemsInfo.charge), ValidationHelper.ValidatePieces);
            //item.RegisterValidator(nameof(ItemsInfo.weight), ValidationHelper.ValidateWeight);
            //item.RegisterValidator(nameof(ItemsInfo.description), ValidationHelper.ValidateDescription);
        }

        // Helper method to refresh the entire items collection display
        public void RefreshItemsDisplay()
        {
            RaisePropertyChanged(nameof(Items));
            RaisePropertyChanged(nameof(TotalPieces));
            RaisePropertyChanged(nameof(TotalWeight));
            OnItemsRefreshRequired();
        }
    }
}