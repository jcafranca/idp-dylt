using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using System;
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

namespace DDCInteligence.View.UserControl
{
    /// <summary>
    /// Interaction logic for ConsigneeContent.xaml
    /// </summary>
    public partial class ConsigneeContent 
    {
        private IRubberBand rubberBand;
        public ConsigneeContent()
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
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var textBlock = sender as TextBlock;
            if (textBlock != null && !string.IsNullOrEmpty(textBlock.Text))
            {
                Clipboard.SetText(textBlock.Text);
            }
        }
    }
}
