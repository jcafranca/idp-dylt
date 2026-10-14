using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Helpers;
using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using DotNetCS.ImageViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
using System.Windows.Threading;

namespace DDCInteligence.View.UserControl
{
    /// <summary>
    /// Interaction logic for ReferencesContent.xaml
    /// </summary>
    public partial class ReferencesContent
    {
        private IRubberBand rubberBand;
        public ReferencesContent()
        {
            InitializeComponent();
            GotKeyboardFocus += (sender, e) =>
            {
                if (e.OriginalSource is TextBox textBox)
                {
                    rubberBand = new RubberBandServices();
                    rubberBand.SetTarget(e.OriginalSource, ((TextBox)e.OriginalSource).DataContext, BindingOperations.GetBinding((TextBox)e.OriginalSource, TextBox.TextProperty).Path.Path);
                    ViewModelLocator.Instance.Process.SetRubberBand(rubberBand);
                }
            };           
        } 
    }
}
