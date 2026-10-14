using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Helpers.Validation;
using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using static DDCInteligence.DataTypes.DatabaseTypes;

namespace DDCInteligence.Models
{
    public class BillingInfoModel : ValidatableBase, INotifyPropertyChanged
    {
        private readonly IProcess process;
        // Dictionary to store the mapping between controller instances and TextBox elements
        private Dictionary<ITextBoxController, TextBox> textBoxesByController = new Dictionary<ITextBoxController, TextBox>();

        public PONumberController PONumberController { get; } = new PONumberController();
        public BOLNumberController BOLNumberController { get; } = new BOLNumberController();
        public ProNumberController ProNumberController { get; } = new ProNumberController();
        protected BillingInfo BillData
        {
            get => ViewModelLocator.Instance.Process.CurrentDocument?.BillInfo;
        }
        public string Pronumber
        {
            get => ViewModelLocator.Instance.Process.CurrentDocument?.pro_number;
            set
            {
                if (BillData != null)
                {
                    process.CurrentDocument.pro_number = value.ToUpper();
                }
                RaisePropertyChanged();
            }
        }
        public string Terms
        {
            get => BillData?.terms;
            set
            {
                if (BillData != null)
                {
                    process.CurrentDocument.BillInfo.terms = value.ToUpper();
                }
                RaisePropertyChanged(nameof(Terms));
            }
        }
        public string ServiceLevel
        {
            get => BillData?.serviceLevel;
            set
            {
                if (BillData != null)
                {
                    process.CurrentDocument.BillInfo.serviceLevel = value.ToUpper();
                }
                RaisePropertyChanged(nameof(ServiceLevel));
            }
        }
        public string ExcessCoverageAmount
        {
            get => BillData?.excessCoverageAmount;
            set
            {
                if (BillData != null)
                {
                    process.CurrentDocument.BillInfo.excessCoverageAmount = value.ToUpper();
                }
                RaisePropertyChanged(nameof(ExcessCoverageAmount));
            }
        }
        public string Notes
        {

           get => BillData?.notes;
           set
           {
               if (BillData != null)
               {
                   process.CurrentDocument.BillInfo.notes = value.ToUpper();
               }
               RaisePropertyChanged(nameof(Notes));
           }
        }
        public void Focus(string name)
        {
            switch (name)
            {
                case "BOL":
                    BOLNumberController.OnSelectAll();
                    break;
                case "PO":
                    PONumberController.OnSelectAll();
                    break;
                case "PRO":
                    ProNumberController.OnSelectAll();
                    break;
            }
        }
        public BillingInfoModel(IProcess process)
        {
            this.process = process;
            this.process.PropertyChanged += (_, __) => RaisePropertyChanged(null);
            this.process.RegisterErrorSource(this);
            //RegisterValidator(nameof(Pronumber), ValidationHelper.ValidateEmpty);
            //RegisterValidator(nameof(Terms), ValidationHelper.ValidateTerms);
        }
    }
}
