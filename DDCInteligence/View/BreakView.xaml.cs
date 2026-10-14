using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Helpers;
using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using GalaSoft.MvvmLight.Command;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Threading;
using static DDCInteligence.DataTypes.DatabaseTypes;
using static DDCInteligence.DataTypes.DocumentTypes;

namespace DDCInteligence.View
{
    /// <summary>
    /// Interaction logic for BreakView.xaml
    /// </summary>
    public partial class BreakView : INotifyPropertyChanged
    {
        private readonly DispatcherTimer _dispatcherTimer;
        private DateTime breaktime;
        private ActivityInfo activityProperties { get; set; }
        protected MySQLServices MySQLServices = new MySQLServices();
        protected string BreakMessage;

        private string _display1stHour;
        public string Display1stHour
        {
            get => _display1stHour;
            set
            {
                _display1stHour = value;
                RaisePropertyChanged(nameof(Display1stHour));
            }
        }
        private string _display1stMin;
        public string Display1stMin
        {
            get => _display1stMin;
            set
            {
                _display1stMin = value;
                RaisePropertyChanged(nameof(Display1stMin));
            }
        }
        private string _display1stSec;
        public string Display1stSec
        {
            get => _display1stSec;
            set
            {
                _display1stSec = value;
                RaisePropertyChanged(nameof(Display1stSec));
            }
        }

        private string _display2ndHour;
        public string Display2ndHour
        {
            get => _display2ndHour;
            set
            {
                _display2ndHour = value;
                RaisePropertyChanged(nameof(Display2ndHour));
            }
        }
        private string _display2ndMin;
        public string Display2ndMin
        {
            get => _display2ndMin;
            set
            {
                _display2ndMin = value;
                RaisePropertyChanged(nameof(Display2ndMin));
            }
        }
        private string _display2ndSec;
        public string Display2ndSec
        {
            get => _display2ndSec;
            set
            {
                _display2ndSec = value;
                RaisePropertyChanged(nameof(Display2ndSec));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public BreakView(string breakText)
        {
            InitializeComponent();
            _dispatcherTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            IsVisibleChanged += FlipClock_IsVisibleChanged;
            breaktime = DateTime.Now;
            BreakText.Text = breakText;
            BreakMessage = breakText;

            this.InputBindings.Add(new KeyBinding { Command = new RelayCommand(this.Close), Key = Key.Escape });
        }
        private async void FlipClock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                _dispatcherTimer.Tick += DispatcherTimer_Tick;
                _dispatcherTimer.Start();

                //Insert database
                activityProperties = new ActivityInfo();
                ActivityInfo new_data = new ActivityInfo
                {
                    user_id = ViewModelLocator.Instance.Process.CurrentBiller.id,
                    start_datetime = breaktime,
                    message1 = "Break",
                    message2 = BreakMessage,
                };
                new_data.id = this.MySQLServices.Insert<ActivityInfo>(new_data, ViewModelLocator.Instance.Process.ConnectionString);
                if (new_data.id > 0) activityProperties = new_data;
            }
            else
            {
                _dispatcherTimer.Stop();
                _dispatcherTimer.Tick -= DispatcherTimer_Tick;
                ViewModelLocator.Instance.Process.SetState(EState.IDLE);

                //Update to database
                if (activityProperties.id > 0)
                {
                    using (IDbConnection connection = new MySqlConnection(ViewModelLocator.Instance.Process.ConnectionString))
                    {
                        activityProperties.end_datetime = DateTime.Now;
                        await connection.UpdateAsync(activityProperties);
                    }
                }
            }
        }
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan ts = DateTime.Now.Subtract(breaktime);
            Display1stHour = ts.Hours.ToString("D2").Substring(0, 1);
            Display1stMin = ts.Minutes.ToString("D2").Substring(0, 1);
            Display1stSec = ts.Seconds.ToString("D2").Substring(0, 1);
            Display2ndHour = ts.Hours.ToString("D2").Substring(1, 1);
            Display2ndMin = ts.Minutes.ToString("D2").Substring(1, 1);
            Display2ndSec = ts.Seconds.ToString("D2").Substring(1, 1);
        }
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
