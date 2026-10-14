using DDCInteligence.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using HandyControl.Data;
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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DDCInteligence.View.UserControl
{
    /// <summary>
    /// Interaction logic for NonClientAreaContent.xaml
    /// </summary>
    public partial class NonClientAreaContent
    {
        public NonClientAreaContent()
        {
            InitializeComponent();
        }
        private void ButtonConfig_OnClick(object sender, RoutedEventArgs e) => PopupConfig.IsOpen = true;
        private void ButtonSkins_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button)
            {
                PopupConfig.IsOpen = false;
                SkinType skin = (SkinType)button.Tag;
                if (skin == (SkinType)ViewModelLocator.Instance.Process.CurrentSkin) return;
                ViewModelLocator.Instance.Process.SetCurrentSkin(skin);
                ((App)Application.Current).UpdateTheme(skin);
                Messenger.Default.Send(skin, "SkinUpdated");
            }
        }
    }
}
