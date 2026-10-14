using AvalonDock.Themes;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.Models
{
    public class MainModel : ViewModelBase
    {
        private Theme _avalondock_theme;
        public Theme AvalonDockTheme
        {
            get => _avalondock_theme;
            set
            {
                Set(ref _avalondock_theme, value);
                RaisePropertyChanged(nameof(AvalonDockTheme));
            }
        }
        private string _servername;
        public string ServerName
        {
            get => _servername;
            set
            {
                Set(ref _servername, value);
                RaisePropertyChanged(nameof(ServerName));
            }
        }
        private DateTime _computer_time;
        public DateTime ComputerTime
        {
            get => _computer_time;
            set
            {
                Set(ref _computer_time, value);
                RaisePropertyChanged(nameof(ComputerTime));
            }
        }
        private bool _isRequesting;
        public bool IsRequesting
        {
            get => _isRequesting;
            set
            {
                Set(ref _isRequesting, value);
                RaisePropertyChanged(nameof(IsRequesting));
            }
        }
    }
}
