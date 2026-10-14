using GalaSoft.MvvmLight;
using SweetAlertSharp.Enums;
using SweetAlertSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using DDCInteligence.DataTypes;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using DDCInteligence.Utilities.Services;
using MySql.Data.MySqlClient;
using DDCInteligence.Utilities.Helpers;
using Newtonsoft.Json;
using static Org.BouncyCastle.Math.EC.ECCurve;
using static DDCInteligence.DataTypes.DatabaseTypes;
using static DDCInteligence.DataTypes.DocumentTypes;

namespace DDCInteligence.ViewModel
{
    public class IdleViewModel : ViewModelBase
    {
        protected MySQLServices MySQLServices = new MySQLServices();
        private readonly string IdleReasonsFile = $"{AppDomain.CurrentDomain.BaseDirectory}{@"AppFiles\IdleReasons.json"}";

        public IdleViewModel()
        {
            _dispatcherTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            IsEnable = true;
            StartCommand = new RelayCommand(Start, false);
            StopCommand = new RelayCommand(Stop, false);
        }

        private readonly DispatcherTimer _dispatcherTimer;
        private DateTime breaktime;
        private bool _isEnable;
        public bool IsEnable
        {
            get => _isEnable;
            set
            {
                _isEnable = value;
                RaisePropertyChanged(nameof(IsEnable));
            }
        }
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        private ActivityInfo activity { get; set; }
        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                Set(ref _selectedIndex, value);
                RaisePropertyChanged(nameof(SelectedIndex));
            }
        }
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
        private ObservableCollection<string> _idlereasonsList;
        public ObservableCollection<string> IdleReasonsList
        {
            get => _idlereasonsList;
            set
            {
                Set(ref _idlereasonsList, value);
                RaisePropertyChanged(nameof(IdleReasonsList));
            }
        }
        private void Start()
        {
            if (SelectedIndex != -1 && IdleReasonsList != null)
            {
                if (IsEnable)
                {

                    IsEnable = false;
                    _dispatcherTimer.Tick += DispatcherTimer_Tick;
                    _dispatcherTimer.Start();
                    breaktime = DateTime.Now;

                    //Insert database
                    activity = new ActivityInfo();
                    ActivityInfo new_data = new ActivityInfo
                    {
                        user_id = ViewModelLocator.Instance.Process.CurrentBiller.id,
                        start_datetime = breaktime,
                        message1 = "IDLE",
                        message2 = IdleReasonsList[SelectedIndex],
                    };
                    activity = new_data;
                    do
                    {
                        activity.id = this.MySQLServices.Insert<ActivityInfo>(new_data, ViewModelLocator.Instance.Process.ConnectionString);
                    } while (activity.id == 0);

                }
            }
            else
            {
                var alert_info = new SweetAlert
                {
                    Caption = "Invalid IDLE Reason",
                    Message = "IDLE reason cannot be empty, please check!",
                    MsgButton = SweetAlertButton.OK,
                    MsgImage = SweetAlertImage.ERROR,
                    OkText = "Okay, got it!",
                    ShowInTaskbar = false
                };
                alert_info.ShowDialog();
            }
        }
        private void Stop()
        {

            if (activity != null)
            {
                IsEnable = true;
                _dispatcherTimer.Stop();
                _dispatcherTimer.Tick -= DispatcherTimer_Tick;

                //Update to database
                if (activity.id > 0)
                {
                    using (IDbConnection connection = new MySqlConnection(ViewModelLocator.Instance.Process.ConnectionString))
                    {
                        activity.end_datetime = DateTime.Now;
                        connection.Update(activity);
                    }

                    Display1stHour = "0";
                    Display1stMin = "0";
                    Display1stSec = "0";
                    Display2ndHour = "0";
                    Display2ndMin = "0";
                    Display2ndSec = "0";
                    SelectedIndex = -1;
                }
            }

            ViewModelLocator.Instance.Process.SetState(EState.BREAK);

        }
        public void InitializeIdleReasons()
        {
            var json = System.IO.File.ReadAllText(this.IdleReasonsFile);
            this.IdleReasonsList = (string.IsNullOrEmpty(json) ? new ObservableCollection<string>() : JsonConvert.DeserializeObject<ObservableCollection<string>>(json)) ?? new ObservableCollection<string>();
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
    }
}
