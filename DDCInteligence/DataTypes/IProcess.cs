using DDCInteligence.Utilities.Helpers.Validation;
using HandyControl.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static DDCInteligence.DataTypes.DatabaseTypes;
using static DDCInteligence.DataTypes.DocumentTypes;
using static DDCInteligence.DataTypes.LookupDatabaseTypes;

namespace DDCInteligence.DataTypes
{
    public interface IProcess : INotifyPropertyChanged
    {
        BillerInfo CurrentBiller { get; }
        RequestInfo RequestInfo { get; }
        Settings CurrentSettings { get; }
        Document CurrentDocument { get; }
        object CurrentShortbill { get; }
        List<ChargesCodesInfo> LookupChargeCodes { get; }
        IRubberBand rubberBandService { get; }
        EState CurrentState { get; }
        SkinType CurrentSkin { get; }
        ERole CurrentRole { get; }
        string ConnectionString { get; }
        string LookUpConnectionString { get; }
        string ImagePath { get; }
        string OCRPath { get; }
        string XMLPath { get; }
        string StartTime { get; }
        string Elapsed { get; }
        string StatusMessage { get; }
        string PreloaderImageMessage { get; }
        string ServerText { get; }
        string QcDetails { get; }
        string OcrMessage { get; }
        string KeyedBy { get; }
        string Prompt { get; set; }
        CustomerPrompt CustomerPrompt { get; set; }
        FrameworkElement BillingEntry { get; set; }
        Task<IEnumerable<string>> ValidateDocumentAsync();
        void Request();
        void DeAllocated();
        void Save(EStatus status);
        void SetState(EState state);
        void SetCurrentBiller(BillerInfo billerInfo, int role);
        void SetRubberBand(IRubberBand rubberBand);
        void RemoveInstruction(InstructionInfo info);
        void SetCurrentSkin(SkinType skin);
        void RegisterErrorSource(ValidatableBase errorSource);
        void UnregisterErrorSource(ValidatableBase errorSource);
        void InitializeLookupChargesCodes();
        void SetLookUpChargeCodes(List<ChargesCodesInfo> infos);
        void SetPrompt(string prompt);
        void ClearPrompt();
        Task<IEnumerable<T>> GetLookupAccountDetailsByColumnAsync<T>(string columnName, string value);
        Task<IEnumerable<T>> GetLookupByCommand<T>(string command);
        Task HandleTermsPromptAsync(string inputText, Control dataContext, string textBoxName);
        
    }
}
