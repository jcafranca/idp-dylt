using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Helpers.Validation;
using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static DDCInteligence.DataTypes.DatabaseTypes;
using static DDCInteligence.DataTypes.DocumentTypes;

namespace DDCInteligence.Models
{
    public class AccessorialModel : ValidatableBase
    {
        private readonly LogServices logServices = new LogServices();
        private readonly IProcess process;
        private AccessorialInfo _selectedItem;
        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }

        // Events to notify the view for UI updates
        public event Action NewItemAdded;
        public event Action<int> ItemInserted; // New event for insertion with index
        public event Action ItemsRefreshRequired; // New event for general refresh

        public ObservableCollection<AccessorialInfo> Items
        {
            get => process?.CurrentDocument?.AccessorialInfos;
        }
        public AccessorialInfo SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                RaisePropertyChanged(nameof(SelectedItem));
            }
        }

        public AccessorialModel(IProcess process)
        {
            this.process = process;
            this.process.PropertyChanged += (_, __) => RaisePropertyChanged(null);
            this.process.RegisterErrorSource(this);

            AddItemCommand = new RelayCommand(NewItem);
            RemoveItemCommand = new RelayCommand<AccessorialInfo>(RemoveItem);
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
                if (ViewModelLocator.Instance.Process.CurrentDocument?.AccessorialInfos == null) return;
                process.CurrentDocument?.AccessorialInfos.Add(new AccessorialInfo { });

                // Force refresh of UI properties
                RaisePropertyChanged(nameof(Items));

                OnNewItemAdded();
                OnItemsRefreshRequired();
            }
        }
        void RemoveItem(AccessorialInfo info)
        {
            if (info != null && process.CurrentState == EState.ENTRY)
            {
                if (process.CurrentDocument.RemoveAccessorials is null)
                    process.CurrentDocument.RemoveAccessorials = new ObservableCollection<AccessorialInfo>();

                process.CurrentDocument.AccessorialInfos.Remove(info);
                process.CurrentDocument.RemoveAccessorials.Add(info);

                // Force refresh of UI properties
                RaisePropertyChanged(nameof(Items));

                // Clear selection if removed item was selected
                if (SelectedItem == info)
                {
                    SelectedItem = null;
                }

                OnItemsRefreshRequired();
            }
        }
        public void InsertNewItemAt(int index)
        {
            if (process.CurrentState == EState.ENTRY)
            {
                if (process.CurrentDocument?.AccessorialInfos == null) return;

                var newItem = new AccessorialInfo { };

                if (index < 0 || index > ViewModelLocator.Instance.Process.CurrentDocument?.AccessorialInfos.Count)
                {
                    process.CurrentDocument?.AccessorialInfos.Add(newItem);
                    index = ViewModelLocator.Instance.Process.CurrentDocument.AccessorialInfos.Count - 1;
                }
                else
                {
                    process.CurrentDocument?.AccessorialInfos.Insert(index, newItem);
                }

                // Force refresh of UI properties
                RaisePropertyChanged(nameof(Items));

                // Update selected item to the newly inserted item
                SelectedItem = newItem;

                // Notify view about the insertion
                OnItemInserted(index);
                OnItemsRefreshRequired();
            }
        }
        public void ItemsInfos_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (AccessorialInfo n in e.NewItems)
                    {
                        n.PropertyChanged += DataItemsPropertyChanged;
                        this.process.RegisterErrorSource(n);
                        RegisterValidators(n);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (AccessorialInfo o in e.OldItems)
                    {
                        o.PropertyChanged -= DataItemsPropertyChanged;
                        this.process.UnregisterErrorSource(o);
                    }
                    break;
            }

            // Force refresh after any collection change
            RaisePropertyChanged(nameof(Items));
            OnItemsRefreshRequired();
        }
        private void DataItemsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(null);
        }
        public void RegisterValidators(AccessorialInfo item) { }
        public void RefreshItemsDisplay()
        {
            RaisePropertyChanged(nameof(Items));
            OnItemsRefreshRequired();
        }
    }
}
