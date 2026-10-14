using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Helpers.Validation;
using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using GalaSoft.MvvmLight;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using static DDCInteligence.DataTypes.DatabaseTypes;

namespace DDCInteligence.Models
{
    public class AccountsModel : ValidatableBase, INotifyPropertyChanged, ITextBoxController
    {
        public string IdentifierName => this.Identifier;
        private readonly IProcess process;
        protected virtual int Type { get; }
        protected AccountInfo AccountInfo
        {
            get
            {
              return process.CurrentDocument?.AccountInfos.SingleOrDefault(t => t.type == Type);
            }
        }
        public event SelectAllEventHandler SelectAll;
        public string Code
        {
            get => AccountInfo?.code?.ToUpper();
            set
            {
                if (AccountInfo != null)
                {
                    process.CurrentDocument.AccountInfos.SingleOrDefault(t => t.type == Type).code = value?.ToUpper();
                }
                RaisePropertyChanged();
            }
        }
        public string Name
        {
            get => AccountInfo?.name?.ToUpper();
            set
            {
                if (AccountInfo != null)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        Code = null;
                        if (Type == 0) process.CustomerPrompt = null;
                    };
                    process.CurrentDocument.AccountInfos.SingleOrDefault(t => t.type == Type).name = value.ToUpper();
                }
                RaisePropertyChanged();
            }
        }
        public string Address
        {

            get => AccountInfo?.address?.ToUpper();
            set
            {
                if (AccountInfo != null)
                {
                    process.CurrentDocument.AccountInfos.SingleOrDefault(t => t.type == Type).address = value.ToUpper();
                }
                RaisePropertyChanged();
            }
        }
        public string Address2
        {
            get => AccountInfo?.address1?.ToUpper();
            set
            {
                if (AccountInfo != null)
                {
                    process.CurrentDocument.AccountInfos.SingleOrDefault(t => t.type == Type).address1 = value.ToUpper();
                }
                RaisePropertyChanged();
            }
        }
        public string City
        {
            get => AccountInfo?.city?.ToUpper();
            set
            {
                if (AccountInfo != null)
                {
                    process.CurrentDocument.AccountInfos.SingleOrDefault(t => t.type == Type).city = value.ToUpper();
                }
                RaisePropertyChanged();
            }
        }
        public string State
        {
            get => AccountInfo?.state?.ToUpper();
            set
            {
                if (AccountInfo != null)
                {
                    process.CurrentDocument.AccountInfos.SingleOrDefault(t => t.type == Type).state = value.ToUpper();
                }
                RaisePropertyChanged();
            }
        }
        public string Zip
        {
            get => AccountInfo?.zip?.ToUpper();
            set
            {
                if (AccountInfo != null)
                {
                    process.CurrentDocument.AccountInfos.SingleOrDefault(t => t.type == Type).zip = value.ToUpper();
                }
                RaisePropertyChanged();
            }
        }
        public string Phone
        {
            get => AccountInfo?.phone?.ToUpper();
            set
            {
                if (AccountInfo != null)
                {
                    process.CurrentDocument.AccountInfos.SingleOrDefault(t => t.type == Type).phone = value.ToUpper();
                }
                RaisePropertyChanged();
            }
        }
        public string Contact
        {
            get => AccountInfo?.contact?.ToUpper();
            set
            {
                if (AccountInfo != null)
                {
                    process.CurrentDocument.AccountInfos.SingleOrDefault(t => t.type == Type).contact = value.ToUpper();
                }
                RaisePropertyChanged();
            }
        }
        public void Focus()
        {
            if (SelectAll != null) SelectAll(this);
        }
        public AccountsModel(IProcess process)
        {
            this.process = process ?? throw new ArgumentNullException(nameof(process));

            process.PropertyChanged += (_, __) => RaisePropertyChanged(null);
            process.RegisterErrorSource(this);

            //RegisterValidator(nameof(Name), ValidationHelper.ValidateAccountName);
            //RegisterValidator(nameof(Address), ValidationHelper.ValidateAddress);
            //RegisterValidator(nameof(City), ValidationHelper.ValidateCity);
            //RegisterValidator(nameof(State), ValidationHelper.ValidateState);
            //RegisterValidator(nameof(Zip), ValidationHelper.ValidateZip);
        }
    }
}
