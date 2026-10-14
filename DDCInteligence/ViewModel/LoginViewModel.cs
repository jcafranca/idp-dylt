using DDCInteligence.DataTypes;
using DDCInteligence.Models;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Linq;

namespace DDCInteligence.ViewModel
{
    public class LoginViewModel : LoginModel
    {
        private static readonly RoleInfo[] DefaultRoles = {
            new RoleInfo { Key = (int)ERole.ADMIN, Icon = "\uf508", Name = " Admin" },
            new RoleInfo { Key = (int)ERole.PROCESSOR, Icon = "\uf51c", Name = " Processor" },
            new RoleInfo { Key = (int)ERole.BILLER, Icon = "\uf084", Name = " Biller" },
            new RoleInfo { Key = (int)ERole.QC, Icon = "\ue5a0", Name = " QC" },
            new RoleInfo { Key = (int)ERole.QC2, Icon = "\ue561", Name = " QC2" }
        };

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(Login);
            ResetPassword = new RelayCommand(Reset);
            Loading = false;
            InitializeAccounts();
        }

        private void InitializeAccounts()
        {
            Accounts = new ObservableCollection<RoleInfo>(DefaultRoles);
            SelectedRole = Accounts.First(a => a.Key == (int)ERole.BILLER);
        }
    }
}