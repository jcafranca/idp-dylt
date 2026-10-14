using DDCInteligence.Utilities.Helpers;
using DDCInteligence.Utilities.Helpers.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using static DDCInteligence.DataTypes.DocumentTypes;

namespace DDCInteligence.DataTypes
{
    public class DatabaseTypes
    {
        [Table("document_tbl")]
        public class Document : INotifyPropertyChanged
        {
            public int id { get; set; }
            public int batch_id { get; set; }
            public int prod_id { get; set; }
            public int data_id { get; set; }
            public int data_qced1_id { get; set; }
            public int data_qced2_id { get; set; }
            public int prod_reference { get; set; }
            public DateTime ftp_datetime { get; set; }
            public string batch_name { get; set; }
            private string _filename;
            public string filename
            {
                get => _filename;
                set
                {
                    _filename = value;
                    OnPropertyChanged(nameof(filename));
                }
            }
            private string _pro_number;
            public string pro_number
            {
                get => _pro_number;
                set
                {
                    _pro_number = value;
                    OnPropertyChanged(nameof(pro_number));
                }
            }
            public int pages { set; get; }
            public int terminal { get; set; }
            public int bill_type { get; set; }
            [IgnoreInsert]
            [IgnoreUpdate]
            public string bill_type_name { get; set; }
            public int state { get; set; }
            public int is_qced { get; set; }
            public int download_process_id { get; set; }
            public int sort_process_id { get; set; }
            public int pr_process_id { get; set; }
            public int query_process_id { get; set; }
            public int query_tbl_id { get; set; }
            public int billed_reject_process_id { get; set; }
            public int qced1_process_id { get; set; }
            public int qced2_process_id { get; set; }
            public ObservableCollection<AccountInfo> AccountInfos { get; set; }
            public BillingInfo BillInfo { get; set; }
            public ObservableCollection<QueryInfo> QueryInfos { get; set; }
            public ObservableCollection<ReferenceInfo> ReferenceInfos { get; set; }
            public ObservableCollection<ItemsInfo> ItemsInfos { get; set; }
            public ObservableCollection<AccessorialInfo> AccessorialInfos { get; set; }
            public ObservableCollection<InstructionInfo> InstructionInfos { get; set; }
            public ObservableCollection<InstructionInfo> RemoveInstructions { get; set; }
            public ObservableCollection<ItemsInfo> RemoveItems { get; set; }
            public ObservableCollection<AccessorialInfo> RemoveAccessorials { get; set; }
            public ObservableCollection<ReferenceInfo> RemoveReference { get; set; }
            public Document()
            {
                this.AccountInfos = new ObservableCollection<AccountInfo>();
                this.BillInfo = new BillingInfo();
                this.ItemsInfos = new ObservableCollection<ItemsInfo>();
                this.InstructionInfos = new ObservableCollection<InstructionInfo>();
                this.AccessorialInfos = new ObservableCollection<AccessorialInfo>();
                this.QueryInfos = new ObservableCollection<QueryInfo>();
                this.RemoveItems = new ObservableCollection<ItemsInfo>();
                this.RemoveReference = new ObservableCollection<ReferenceInfo>();
                this.RemoveInstructions = new ObservableCollection<InstructionInfo>();
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [Table("data_tbl")]
        public class BillingInfo
        {
            public int id { get; set; }
            public string terms { get; set; }
            public string serviceLevel { get; set; }
            public string excessCoverageAmount { get; set; }
            public string notes { get; set; }
        }

        [Table("account_tbl")]
        public class AccountInfo
        {
            private int _id;
            public int id
            {
                get => _id;
                set => _id = value;
            }
            private int _type;
            public int type
            {
                get => _type;
                set => _type = value;
            }
            private int _data_id;
            public int data_id
            {
                get => _data_id;
                set => _data_id = value;
            }
            private string _code;
            public string code
            {
                get => _code;
                set => _code = value;
            }
            private string _name;
            public string name
            {
                get => _name;
                set => _name = value;
            }
            private string _address;
            public string address
            {
                get => _address;
                set => _address = value;
            }
            private string _address1;
            public string address1
            {
                get => _address1;
                set => _address1 = value;
            }
            private string _city;
            public string city
            {
                get => _city;
                set => _city = value;
            }
            private string _state;
            public string state
            {
                get => _state;
                set => _state = value;
            }
            private string _zip;
            public string zip
            {
                get => _zip;
                set => _zip = value;
            }
            private string _country_code;
            [IgnoreInsert]
            [IgnoreUpdate]
            [IgnoreSelect]
            public string country_code
            {
                get => _country_code;
                set => _country_code = value;
            }
            private string _phone;
            public string phone
            {
                get => _phone;
                set => _phone = value;
            }
            private string _contact;
            public string contact
            {
                get => _contact;
                set => _contact = value;
            }
           
            private List<string> _addresses;
            [IgnoreInsert]
            [IgnoreUpdate]
            [IgnoreSelect]
            public List<string> addresses
            {
                get => _addresses;
                set => _addresses = value;
            }
        }

        [Table("reference_tbl")]
        public class ReferenceInfo : ValidatableBase, INotifyPropertyChanged
        {
            protected override string Identifier => "Reference";
            public int id { get; set; }
            public int data_id { get; set; }

            private string _code;
            public string code
            {
                get => _code;
                set
                {
                    _code = value?.ToUpper();
                    RaisePropertyChanged(nameof(code));
                }
            }
            private string _description;
            public string description
            {
                get => _description;
                set
                {
                    _description = value?.ToUpper();
                    RaisePropertyChanged(nameof(description));
                }
            }
        }

        [Table("instruction_tbl")]
        public class InstructionInfo
        {
            public int id { get; set; }
            public int data_id { get; set; }
            public string Instruction { get; set; }
        }

        [Table("doc_type")]
        public class DocTypeInfo
        {
            public int id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string key { get; set; }
            public string reference_to { get; set; }
            public int priority { get; set; }
            public int ignore_entry { get; set; }
            public int archive { get; set; }
            public DateTime modification_datetime { get; set; }
        }

        [Table("items_tbl")]
        public class ItemsInfo : ValidatableBase, INotifyPropertyChanged
        {
            protected override string Identifier => "Item";
            public int id { get; set; }
            public int data_id { get; set; }
            private string _plt;
            private string _pltUnits;
            private string _pcs;
            private string _pcsUnit;
            private string _stc;
            private string _weight;
            private string _description;
            private string _class;
            private string _nmfc;
            public string nmfc
            {
                get => _nmfc;
                set
                {
                    _nmfc = value?.ToUpper();
                    RaisePropertyChanged(nameof(nmfc));
                }
            }          
            public string plt
            {
                get => _plt;
                set
                {
                    _plt = value;
                    RaisePropertyChanged(nameof(plt));
                }
            }
            public string pltUnits
            {
                get => _pltUnits;
                set
                {
                    _pltUnits = value;
                    RaisePropertyChanged(nameof(pltUnits));
                }
            }
            public string pcs
            {
                get => _pcs;
                set
                {
                    _pcs = value;
                    RaisePropertyChanged(nameof(pcs));
                }
            }
            public string pcsUnit
            {
                get => _pcsUnit;
                set
                {
                    _pcsUnit = value;
                    RaisePropertyChanged(nameof(pcsUnit));
                }
            }
            public string stc
            {
                get => _stc;
                set
                {
                    _stc = value;
                    RaisePropertyChanged(nameof(stc));
                }
            }
            public string weight
            {
                get => _weight;
                set
                {
                    _weight = value;
                    RaisePropertyChanged(nameof(weight));
                }
            }
            public string description
            {
                get => _description;
                set
                {
                    _description = value;
                    RaisePropertyChanged(nameof(description));
                }
            }
            public string @class
            {
                get => _class;
                set
                {
                    _class = value;
                    RaisePropertyChanged(nameof(@class));
                }
            }
        }

        [Table("accessorials_tbl")]
        public class AccessorialInfo : ValidatableBase, INotifyPropertyChanged
        {
            protected override string Identifier => "Accessorial";
            public int id { get; set; }
            public int data_id { get; set; }
            private string _charge;
            private string _description;
            private string _quantity;
            private string _rate;
            private string _amount;
            private string _manualAmt;
            private string _notation;

            public string charge
            {
                get => _charge;
                set
                {
                    _charge = value?.ToUpper();
                    RaisePropertyChanged(nameof(charge));
                }
            }
            public string description
            {
                get => _description;
                set
                {
                    _description = value;
                    RaisePropertyChanged(nameof(description));
                }
            }
            public string quantity
            {
                get => _quantity;
                set
                {
                    _quantity = value;
                    RaisePropertyChanged(nameof(quantity));
                }
            }
            public string rate
            {
                get => _rate;
                set
                {
                    _rate = value;
                    RaisePropertyChanged(nameof(rate));
                }
            }
            public string amount
            {
                get => _amount;
                set
                {
                    _amount = value;
                    RaisePropertyChanged(nameof(amount));
                }
            }
            public string manualAmt
            {
                get => _manualAmt;
                set
                {
                    _manualAmt = value;
                    RaisePropertyChanged(nameof(manualAmt));
                }
            }
            public string notation
            {
                get => _notation;
                set
                {
                    _notation = value;
                    RaisePropertyChanged(nameof(notation));
                }
            }
        }

        [Table("process_tbl")]
        public class ProcessInfo
        {
            public int id { get; set; }
            public int doc_id { get; set; }
            public DateTime start_datetime { get; set; }
            public DateTime end_datetime { get; set; }
            public string message1 { get; set; }
            public string message2 { get; set; }
            public string message3 { get; set; }
            public int process_by { get; set; }
        }

        [Table("query_tbl")]
        public class QueryInfo
        {
            public int id { get; set; }
            public int doc_id { get; set; }
            public string answer { get; set; }
            public string query { get; set; }
            public DateTime time_start { get; set; }
            public DateTime time_end { get; set; }
            public string shipper { get; set; }
            public string consignee { get; set; }
            public int processed_by { get; set; }
            public DateTime answered_start_time { get; set; }
            public DateTime answered_end_time { get; set; }
            public int answered_by { get; set; }

        }

        [Table("request_tbl")]
        public class RequestInfo
        {
            public int id { get; set; }
            public int user_id { get; set; }
            public int doc_id { get; set; }
            public string image_path { get; set; }
            public int request_type { get; set; }
        }

        [Table("activity_tbl")]
        public class ActivityInfo
        {
            public int id { get; set; }
            public int user_id { get; set; }
            public DateTime start_datetime { get; set; }
            public DateTime end_datetime { get; set; }
            public string message1 { get; set; }
            public string message2 { get; set; }
            public string message3 { get; set; }
        }

        [Table("user_tbl")]
        public class BillerInfo : ValidatableBase, INotifyPropertyChanged
        {
            public int id { get; set; }
            public string username { get; set; }
            public string employee_id_number { get; set; }
            public string fullname { get; set; }
            public string password { get; set; }
            [IgnoreInsert]
            [IgnoreUpdate]
            public int account { get; set; }
            [IgnoreInsert]
            [IgnoreUpdate]
            public int password_length { get; set; }
            public int is_login { get; set; }
            private string _expiration;
            [IgnoreInsert]
            [IgnoreUpdate]
            public string expiration
            {
                get => _expiration;
                set
                {
                    if (value != null)
                    {
                        _expiration = $"Expires on {DateTime.Parse(value).ToString("F")}";
                    }
                }
            }
            private string _profilePicture;
            [IgnoreSelect]
            [IgnoreInsert]
            [IgnoreUpdate]
            public string profilePicture
            {
                get => _profilePicture;
                set
                {
                    _profilePicture = value;
                    RaiseErrorsChanged(nameof(profilePicture));
                }
            }
        }

    }
}
