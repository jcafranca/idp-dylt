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
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using DDCInteligence.Utilities.Services;
using System.Windows.Forms;
using DDCInteligence.View.UserControl;
using System.Threading.Tasks;
using static DDCInteligence.DataTypes.DatabaseTypes;
using static DDCInteligence.DataTypes.DocumentTypes;
namespace DDCInteligence.Models
{
    public class ReferenceModel : ValidatableBase
    {
        private readonly LogServices logServices = new LogServices();
        private readonly IProcess process;
        public ObservableCollection<ReferenceInfo> Reference
        {
            get => process?.CurrentDocument?.ReferenceInfos;
        }
        private ObservableCollection<LookupInfo> _referenceLookup;
        public ObservableCollection<LookupInfo> ReferenceLookup
        {
            get => _referenceLookup;
            set
            {
                _referenceLookup = value;
                RaisePropertyChanged(nameof(ReferenceLookup));
            }
        }
        private ReferenceInfo _selectedItem;
        public ReferenceInfo SelectedItem
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
            }
        }
        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }
        // Event to notify the view to focus
        public event Action NewItemAdded;
        public ReferenceModel(IProcess process)
        {
            this.process = process;
            this.process.PropertyChanged += (_, __) => RaisePropertyChanged(null);
            this.process.RegisterErrorSource(this);

            AddItemCommand = new RelayCommand(NewItem);
            RemoveItemCommand = new RelayCommand<ReferenceInfo>(RemoveItem);
        }
        protected void OnNewItemAdded()
        {
            NewItemAdded?.Invoke();
        }
        void NewItem()
        {
            if (process.CurrentState == EState.ENTRY)
            {
                if (process.CurrentDocument?.ReferenceInfos == null) return;
                process.CurrentDocument?.ReferenceInfos.Add(new ReferenceInfo { description = "" });
                OnNewItemAdded();
            }
        }
        void RemoveItem(ReferenceInfo info)
        {
            if (info != null && ViewModelLocator.Instance.Process.CurrentState == EState.ENTRY)
            {
                if (process.CurrentDocument.RemoveReference is null)
                    process.CurrentDocument.RemoveReference = new ObservableCollection<ReferenceInfo>();
                process.CurrentDocument.ReferenceInfos.Remove(info);
                process.CurrentDocument.RemoveReference.Add(info);
            }
        }
        public void ItemsInfos_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ReferenceInfo n in e.NewItems)
                    {
                        n.PropertyChanged += DataItemsPropertyChanged;
                        process.RegisterErrorSource(n);
                        RegisterValidators(n);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ReferenceInfo o in e.OldItems)
                    {
                        o.PropertyChanged -= DataItemsPropertyChanged;
                        process.UnregisterErrorSource(o);
                        UnregisterValidators(o);
                    }
                    break;
            }
        }
        private void DataItemsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(null);
        }
        public void RegisterValidators(ReferenceInfo item)
        {
            //item.RegisterValidator(nameof(ReferenceInfo.description), ValidationHelper.ValidateReferenceDescription);
            //item.RegisterValidator(nameof(ReferenceInfo.piece), ValidationHelper.ValidateReferencePieces);
            //item.RegisterValidator(nameof(ReferenceInfo.weight), ValidationHelper.ValidateReferenceWeight);
        }
        public void UnregisterValidators(ReferenceInfo item)
        {
            //item.UnregisterValidator(nameof(ReferenceInfo.description));
            //item.UnregisterValidator(nameof(ReferenceInfo.piece));
            //item.UnregisterValidator(nameof(ReferenceInfo.weight));
        }
    }
}
