using DDCInteligence.Utilities.Helpers.Validation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DDCInteligence.DataTypes.DatabaseTypes;

namespace DDCInteligence.DataTypes
{
    public class DocumentTypes
    {
        public enum ERequestType
        {
            SORTER = 0,
            PR = 1,
            IMAGEVIEWER = 2,
            QUERY = 3,
            QC = 4,
        }
        public enum EStatus
        {
            ONGOING,
            BILLED = 7,
            REJECT = 8,
            QUERY = 5,
            ANSWERED,
            QUERYBILLED
        }
        public enum EState
        {
            IDLE,
            REQUEST,
            CANCELREQUEST,
            ENTRY,
            SAVING,
            BREAK
        }
        public class Dimensions : ValidatableBase, INotifyPropertyChanged
        {
            [JsonIgnore]
            public ItemsInfo keyword { get; set; }
            private int _hu_number;
            public int hu_number
            {
                get => _hu_number;
                set
                {
                    _hu_number = value;
                    RaisePropertyChanged(nameof(hu_number));
                }
            }
            private string _length;
            public string length
            {
                get => _length;
                set
                {
                    _length = value;
                    RaisePropertyChanged(nameof(length));
                }
            }
            private string _width;
            public string width
            {
                get => _width;
                set
                {
                    _width = value;
                    RaisePropertyChanged(nameof(width));
                }
            }
            private string _height;
            public string height
            {
                get => _height;
                set
                {
                    _height = value;
                    RaisePropertyChanged(nameof(height));
                }
            }
            private string _uom;
            public string uom
            {
                get => _uom;
                set
                {
                    _uom = value;
                    RaiseErrorsChanged(nameof(uom));
                }
            }
        }
        public class OCRResponse
        {
            public string proNumber { get; set; }
            public string serviceLevel { get; set; }
            public string excessCoverageAmount { get; set; }
            public OCRAccount consignee { get; set; }
            public OCRAccount shipper { get; set; }
            public OCRAccount billTo { get; set; }           
            public string specialInstructions { get; set; }
            public List<OCRItem> items { get; set; }
            public List<OCRReferenceNumber> referenceNumbers { get; set; }
            public List<OCRAccessorials> accessorials { get; set; }
        }

        public class OCRAccount
        {
            public string name { get; set; }
            public string address1 { get; set; }
            public string address2 { get; set; }
            public string contactName { get; set; }
            public string contactNumber { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string zip { get; set; }
        }

        public class OCRItem
        {
            public string nmfc { get; set; }
            public string pcs { get; set; }
            public string pcsUnit { get; set; }
            public string plt { get; set; }
            public string pltUnits { get; set; }
            public string stc { get; set; }
            public string description { get; set; }
            public string weight { get; set; }
            public string @class { get; set; }
        }
        public class OCRAccessorials
        {
            public string charge { get; set; }
            public string description { get; set; }
            public string manualAmt { get; set; }
            public string quantity { get; set; }
            public string rate { get; set; }
            public string amount { get; set; }
            public string notation { get; set; }
        }

        public class OCRReferenceNumber
        {
            public string id { get; set; }
            public string value { get; set; }
        }
    }
     public static class StatusHelper
     {
         public static DocumentTypes.EStatus Billed => DocumentTypes.EStatus.BILLED;
     }

}
