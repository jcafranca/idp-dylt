using DDCInteligence.Utilities.Events;
using GalaSoft.MvvmLight;
using System.Windows.Input;

namespace DDCInteligence.Models
{
    public class NonClientAreaModel : ViewModelBase
    {
        private string _appname;
        public string AppName
        {
            get => _appname;
            set => Set(nameof(AppName), ref _appname, value);
        }
        private string _appversion;
        public string AppVersion
        {
            get => _appversion;
            set => Set(nameof(AppVersion), ref _appversion, value);
        }

        #region Commands

        #region Image Controls
        public ICommand SetPageCommand { get; set; }
        public ICommand NextCommand { get; set; }
        public ICommand PreviousCommand { get; set; }
        public ICommand ZoomInCommand { get; set; }
        public ICommand ZoomOutCommand { get; set; }
        public ICommand RightCommand { get; set; }
        public ICommand LeftCommand { get; set; }
        public ICommand UpCommand { get; set; }
        public ICommand DownCommand { get; set; }
        public ICommand BestFitCommand { get; }
        #endregion
        public ICommand RequestCommand { get; set; }
        public ICommand DeAllocateCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand AccountSettingCommand { get; set; }
        public ICommand LogoutCommand { get; set; }
        public ICommand RestartApplicationCommand { get; set; }
        public ICommand GotoConsigneeCommand { get; set; }
        public ICommand GotoShipperCommand { get; set; }
        public ICommand GotoBillToCommand { get; set; }
        public ICommand GotoRateToCommand { get; set; }
        public ICommand GotoBOLCommand { get; set; }
        public ICommand GotoPOCommand { get; set; }
        public ICommand GotoProCommand { get; set; }
        public ICommand GotoSpecialInstructionCommand { get; set; }
        public ICommand NatureBreakCommand { get; set; }
        public ICommand MealBreakCommand { get; set; }
        public ICommand IdleBreakCommand { get; set; }
        public ICommand ChangePasswordCommand { get; set; }
        public ICommand DimensionCommand { get; set; }
        public ICommand ViewDocumentsCommand { get; set; }

        #endregion
    }
}
