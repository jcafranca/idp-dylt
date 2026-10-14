using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.ViewModel
{
    public class ReferenceNumberViewModel : ViewModelBase
    {
        private ObservableCollection<string> _tagList;
        public ObservableCollection<string> TagList
        {
            get => _tagList;
            set
            {
                _tagList = value;
                Set(ref _tagList, value);
                RaisePropertyChanged(nameof(TagList));
            }
        }
        private string _tagName;
        public string TagName
        {
            get => _tagName;
            set {
                Set(ref _tagName, value);
                RaisePropertyChanged(nameof(TagName));
            }
        }
        public ReferenceNumberViewModel()
        {
            TagList = new ObservableCollection<string>();
        }
        public RelayCommand AddItemCmd => new RelayCommand(() =>
        {
            if (string.IsNullOrEmpty(TagName))
            {
                return;
            }
            TagList.Insert(0, TagName);
            TagName = string.Empty;
        });

    }
}
