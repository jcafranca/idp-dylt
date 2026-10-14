using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Helpers;
using DDCInteligence.ViewModel;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DDCInteligence.View
{
    /// <summary>
    /// Interaction logic for References.xaml
    /// </summary>
    public partial class ReferencesView : Window
    {
        private DispatcherTimer timer;
        public string RETURN { get; set; } 
        public ReferencesView()
        {
            InitializeComponent();
            this.InputBindings.AddRange(new InputBindingCollection {
                new KeyBinding { Command = new RelayCommand(Save), Key = Key.F2},
                new KeyBinding { Command = new RelayCommand(() => this.Close()), Key = Key.Escape },
            });
            this.timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            this.timer.Tick += Timer_Tick;
            this.timer.Start();
        }
        private  readonly Regex _regex = new Regex("^[a-zA-Z0-9]*$"); // Regex for alphanumeric characters
        private void TextBoxTag_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
        private bool IsTextAllowed(string text)
        {
            return _regex.IsMatch(text);
        }
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        void Save()
        {
            var tags = ViewModelLocator.Instance.RefNumberVM.TagList;
            RETURN = string.Join(",", tags);
            this.Close();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            var openWindow = Application.Current.Windows.OfType<ReferencesView>().FirstOrDefault();
            if (GlobalShortcut.ApplicationIsActivated())
            {
                if (openWindow != null)
                {
                    openWindow.Topmost = true;
                    openWindow.Topmost = false;
                }
            }
        }
    }
}
