
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace DDCInteligence.Utilities.Events
{
    public class SharedEvent : INotifyPropertyChanged
    {
        private DataGrid _selectedDataGrid;
        public DataGrid SelectedDataGrid
        {
            get => _selectedDataGrid;
            set
            {
                _selectedDataGrid = value;
                OnPropertyChanged();
            }
        }
        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
