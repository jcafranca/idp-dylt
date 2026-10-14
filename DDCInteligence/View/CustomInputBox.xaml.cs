using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using SweetAlertSharp.Enums;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DDCInteligence.View
{
    /// <summary>
    /// Interaction logic for CustomInputBox.xaml
    /// </summary>
    public partial class CustomInputBox : Window
    {
        private readonly UtilityServices utilityServices = new UtilityServices();
        public CustomInputBox(Window owner)
        {
            InitializeComponent();
            if (owner != null)
            {
                this.Owner = owner;
            }
            _Content.Focus();
        }
        private Brush brush = null;
        #region Private Fields
        private List<CancelEventHandler> _preCloseEvents = new List<CancelEventHandler>();
        private bool _isCloseable = true;

        private string _caption;
        private string _message;

        #endregion
        #region Private Methods
        private bool RaiseCloseEvent()
        {
            var args = new CancelEventArgs();
            foreach (var preCloseEvent in _preCloseEvents)
            {
                preCloseEvent(this, args);

                if (args.Cancel)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion
        #region Private Events
        private void Event_HideAnimation_Completed(object sender, EventArgs e)
        {
            if (_isCloseable)
            {
                return;
            }

            Close();
        }
        private void Event_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void Event_Closing(object sender, CancelEventArgs e)
        {
            var hideAnimation = this.FindResource("HideAnimation") as Storyboard;
            if (hideAnimation == null || !_isCloseable)
            {
                return;
            }

            e.Cancel = true;
            if (!RaiseCloseEvent())
            {
                _isCloseable = true;
                return;
            }

            _isCloseable = false;

            hideAnimation.Completed += Event_HideAnimation_Completed;
            hideAnimation.Begin(_Dialog);
        }
        #endregion

        #region Public Properties
        public string Caption
        {
            get => _caption;
            set
            {
                _caption = value;

                _Title.Text = _caption;

                NotifyPropertyChanged("Caption");
            }
        }
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                _Subtitle.Text = _message;
                NotifyPropertyChanged("Message");
            }
        }
        public SweetAlertResult Result { get; private set; } = SweetAlertResult.CANCEL;
        public event CancelEventHandler PreClose
        {
            add => _preCloseEvents.Add(value);
            remove => _preCloseEvents.Remove(value);
        }
        #endregion

        #region Public Methods
        public new SweetAlertResult ShowDialog()
        {
            base.ShowDialog();
            return Result;
        }
        #endregion

        #region Public Events
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion

        private void _Content_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (brush is null) brush = _Content.BorderBrush;
                if (!string.IsNullOrEmpty(_Content.Text))
                {
                    Result = SweetAlertResult.OK;
                    Close();
                }
                else
                {
                    _Content.BorderBrush = Brushes.Red;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.utilityServices.SetWindowOnScreen(this);
            _Content.Text = String.Empty;
        }

        private void _Content_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (brush != null) _Content.BorderBrush = brush;
        }

        private void Background_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }


    }
}
