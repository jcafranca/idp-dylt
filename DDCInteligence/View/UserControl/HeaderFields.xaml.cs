using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using DotNetCS.ImageViewer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace DDCInteligence.View.UserControl
{
    /// <summary>
    /// Interaction logic for HeaderFields.xaml
    /// </summary>
    public partial class HeaderFields : INotifyPropertyChanged
    {
        private IRubberBand rubberBand;        
        public HeaderFields()
        {
            InitializeComponent();
            GotKeyboardFocus += (sender, e) =>
            {
                if (e.OriginalSource is TextBox textBox)
                {

                    rubberBand = new RubberBandServices();
                    var binding = BindingOperations.GetBinding(textBox, TextBox.TextProperty);
                    if (binding?.Path?.Path != null)
                    {
                        rubberBand.SetTarget(textBox, textBox.DataContext, binding.Path.Path);
                    }
                    else
                    {
                        rubberBand.SetTarget(null, null, null);
                    }

                    ViewModelLocator.Instance.Process.SetRubberBand(rubberBand);
                }
            };
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
