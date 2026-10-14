using DDCInteligence.DataTypes;
using DDCInteligence.ViewModel;
using GalaSoft.MvvmLight.Command;
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
using System.Windows.Shapes;
using static DDCInteligence.DataTypes.DocumentTypes;

namespace DDCInteligence.View
{
    /// <summary>
    /// Interaction logic for IdleView.xaml
    /// </summary>
    public partial class IdleView : Window
    {
        public IdleView()
        {
            InitializeComponent();
            this.InputBindings.Clear();
            this.InputBindings.AddRange(new List<KeyBinding> {
                new KeyBinding { Key = Key.S, Modifiers = ModifierKeys.None, Command = ViewModelLocator.Instance.IdleVM.StartCommand },
                new KeyBinding { Key = Key.C, Modifiers = ModifierKeys.None, Command = ViewModelLocator.Instance.IdleVM.StopCommand },
                new KeyBinding { Key = Key.Escape, Modifiers = ModifierKeys.None,  Command = new RelayCommand(this.Close) }
            });
            this.Loaded += IdleView_Loaded;
        }

        private void IdleView_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModelLocator.Instance.IdleVM.InitializeIdleReasons();
            ViewModelLocator.Instance.IdleVM.SelectedIndex = 0;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!StartButton.IsEnabled)
            {
                ViewModelLocator.Instance.IdleVM.StopCommand.Execute(true);
            }

            ViewModelLocator.Instance.Process.SetState(EState.IDLE);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModelLocator.Instance.IdleVM.StopCommand.Execute(true);
            this.Close();
        }
    }
}
