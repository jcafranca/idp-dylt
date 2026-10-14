using Dapper;
using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Helpers;
using DDCInteligence.Utilities.Helpers.Validation;
using DDCInteligence.Utilities.Services;
using DDCInteligence.View;
using HandyControl.Controls;
using HandyControl.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using SweetAlertSharp;
using SweetAlertSharp.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using static DDCInteligence.DataTypes.DatabaseTypes;
using static DDCInteligence.DataTypes.DocumentTypes;
using static DDCInteligence.DataTypes.LookupDatabaseTypes;

namespace DDCInteligence.ViewModel
{
    public class ProcessViewModel : INotifyPropertyChanged, IProcess
    {
        #region Constants and Static Data
        private readonly PromptHelperService _promptHelper = new PromptHelperService();
        private static readonly Dictionary<string, string> AddressAbbreviations = new Dictionary<string, string>
        {
            { "AVE", "AVENUE" },
            { "BLVD", "BOULEVARD" },
            { "FLR", "FLOOR" },
            { "RD", "ROAD" },
            { "ST", "STREET" },
            { "STE", "SUITE" },
            { "E", "EAST" },
            { "N", "NORTH" },
            { "S", "SOUTH" },
            { "W", "WEST" }
        };
        private static HttpClient httpClient = new HttpClient();
        private static readonly int[] RequiredAccountTypes = { 0, 1, 2, 3 };
        private const int TimerIntervalSeconds = 1;
        #endregion

        #region Services
        private readonly UtilityServices _utilityServices = new UtilityServices();
        private readonly MySQLServices _mySQLServices = new MySQLServices();
        private readonly LogServices _logServices = new LogServices();
        #endregion

        #region Private Fields
        private readonly DispatcherTimer _requestTimer;
        private readonly DispatcherTimer _elapsedTimer;
        private readonly ConcurrentBag<ValidatableBase> _errorSources = new ConcurrentBag<ValidatableBase>();
        private readonly SemaphoreSlim _databaseSemaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private FrameworkElement _billingEntry;
        private string _jsonFilePath;
        #endregion

        #region Backing Fields
        private BillerInfo _currentBiller;
        private Document _currentDocument;
        private RequestInfo _requestInfo;
        private ERole _currentRole;
        private Settings _currentSettings;
        private List<ChargesCodesInfo> _lookupChargeCodes;
        private string _connectionString;
        private string _lookUpConnectionString;
        private EState _currentState;
        private SkinType _currentSkin;
        private IRubberBand _rubberBandService;
        private string _imagePath;
        private string _ocrPath;
        private string _xmlPath;
        private string _jsonPath;
        private string _startTime;
        private string _elapsed;
        private string _statusMessage;
        private string _preloaderImageMessage;
        private string _ocrMessage;
        private string _serverText;
        private string _qcDetails;
        private string _keyedBy;
        private string _prompt;
        private CustomerPrompt _customerPrompt;
        private object _currentShortbill;
        // Cached regex for dimensions parsing
        private static readonly Regex DimensionRegex = new Regex(
            @"^(\d+(\.\d{1,2})?)x(\d+(\.\d{1,2})?)x(\d+(\.\d{1,2})?)\s([a-zA-Z]+)$",
            RegexOptions.Compiled);

        #endregion

        #region Public Properties
        public event PropertyChangedEventHandler PropertyChanged;
        public FrameworkElement BillingEntry
        {
            get => _billingEntry;
            set
            {
                _billingEntry = value;
                RaisePropertyChanged();
            }
        }
        public BillerInfo CurrentBiller
        {
            get => _currentBiller;
            set
            {
                if (_currentBiller != value)
                {
                    _currentBiller = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Document CurrentDocument
        {
            get => _currentDocument;
            set
            {
                if (_currentDocument != value)
                {
                    _currentDocument = value;
                    RaisePropertyChanged();
                }
            }
        }
        public object CurrentShortbill
        {
            get => _currentShortbill;
            set
            {
                if (_currentShortbill != value)
                {
                    _currentShortbill = value;
                    RaisePropertyChanged();
                }
            }
        }

        public RequestInfo RequestInfo
        {
            get => _requestInfo;
            private set
            {
                if (_requestInfo != value)
                {
                    _requestInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ERole CurrentRole
        {
            get => _currentRole;
            set
            {
                if (_currentRole != value)
                {
                    _currentRole = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Settings CurrentSettings
        {
            get => _currentSettings;
            set
            {
                if (_currentSettings != value)
                {
                    _currentSettings = value;
                    RaisePropertyChanged();
                }
            }
        }
        public List<ChargesCodesInfo> LookupChargeCodes => _lookupChargeCodes;

        public string ConnectionString
        {
            get => _connectionString;
            set
            {
                if (_connectionString != value)
                {
                    _connectionString = value;
                    RaisePropertyChanged();
                }
            }
        }
        public string LookUpConnectionString
        {
            get => _lookUpConnectionString;
            set
            {
                if (_lookUpConnectionString != value)
                {
                    _lookUpConnectionString = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EState CurrentState
        {
            get => _currentState;
            set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SkinType CurrentSkin
        {
            get => _currentSkin;
            set
            {
                if (_currentSkin != value)
                {
                    _currentSkin = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IRubberBand rubberBandService
        {
            get => _rubberBandService;
            set
            {
                if (_rubberBandService != value)
                {
                    _rubberBandService = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string ImagePath
        {
            get => _imagePath;
            set
            {
                if (_imagePath != value)
                {
                    _imagePath = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string OCRPath
        {
            get => _ocrPath;
            set
            {
                if (_ocrPath != value)
                {
                    _ocrPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string XMLPath
        {
            get => _xmlPath;
            set
            {
                if (_xmlPath != value)
                {
                    _xmlPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string JSONPath
        {
            get => _jsonPath;
            set
            {
                if (_jsonPath != value)
                {
                    _jsonPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string StartTime
        {
            get => _startTime;
            set
            {
                if (_startTime != value)
                {
                    _startTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string Elapsed
        {
            get => _elapsed;
            set
            {
                if (_elapsed != value)
                {
                    _elapsed = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string PreloaderImageMessage
        {
            get => _preloaderImageMessage;
            set
            {
                if (_preloaderImageMessage != value)
                {
                    _preloaderImageMessage = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string OcrMessage
        {
            get => _ocrMessage;
            set
            {
                if (_ocrMessage != value)
                {
                    _ocrMessage = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string ServerText
        {
            get => _serverText;
            private set
            {
                if (_serverText != value)
                {
                    _serverText = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string QcDetails
        {
            get => _qcDetails;
            set
            {
                if (_qcDetails != value)
                {
                    _qcDetails = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string KeyedBy
        {
            get => _keyedBy;
            set
            {
                if (_keyedBy != value)
                {
                    _keyedBy = value;
                    RaisePropertyChanged();
                }
            }
        }
        public string Prompt
        {
            get => _prompt;
            set
            {
                if (_prompt != value)
                {
                    _prompt = value;
                    RaisePropertyChanged();
                }
            }
        }
        public CustomerPrompt CustomerPrompt 
        {
            get => _customerPrompt;
            set
            {
                if (_customerPrompt != value)
                {
                    _customerPrompt = value;
                    RaisePropertyChanged();
                }
            }
        }


        #endregion

        #region Constructor
        public ProcessViewModel()
        {
            _rubberBandService = new RubberBandServices();
            CurrentDocument = new Document();
            CurrentSettings = _logServices.Deserialize<Settings>(_logServices.SettingPath);

            _requestTimer = new DispatcherTimer(
                TimeSpan.FromSeconds(TimerIntervalSeconds),
                DispatcherPriority.Background,
                RequestTimer_Tick,
                Application.Current.Dispatcher)
            {
                IsEnabled = false
            };

            _elapsedTimer = new DispatcherTimer(
                TimeSpan.FromSeconds(TimerIntervalSeconds),
                DispatcherPriority.Background,
                ElapsedTimer_Tick,
                Application.Current.Dispatcher)
            {
                IsEnabled = true
            };

            CurrentState = EState.IDLE;
            Initialize();
        }
        #endregion

        #region Initialization Methods
        private void Initialize()
        {
            InitializeConnectionString();
            InitializeLookupChargesCodes();
        }

        private void InitializeConnectionString()
        {
            if (CurrentSettings == null)
            {
                var tempSettings = new Settings
                {
                    Server = new List<string> { "localhost" },
                    Port = 3306,
                    Databases = new List<string> { "db_dylt", "db_dylt_lookup"},
                    Username = "Programmer",
                    Password = "prog@123",
                };
                _logServices.Serialize(_logServices.SettingPath, tempSettings);
            }

            ConnectionString = _mySQLServices.GetConnectionString(CurrentSettings);
            LookUpConnectionString = _mySQLServices.GetConnectionString(CurrentSettings, true);

            if (!string.IsNullOrEmpty(ConnectionString))
            {
                var server = ConnectionString.Split(';')[0].Split('=')[1];
                ServerText = $"{server} : {CurrentSettings.Port}";
            }
        }

        public void InitializeLookupChargesCodes()
        {
            try
            {

                using (var connection = new MySqlConnection(LookUpConnectionString))
                {
                    _lookupChargeCodes = connection.GetList<ChargesCodesInfo>().ToList();
                }

                //if (File.Exists(DefaultSettingsPath))
                //{
                //    var jsonContent = File.ReadAllText(DefaultSettingsPath);
                //    _lookupCodes = JsonConvert.DeserializeObject<List<LookupInfo>>(jsonContent) ?? new List<LookupInfo>();
                //}
                //else
                //    _lookupCodes = new List<LookupInfo>();
            }
            catch (Exception ex)
            {
                _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(InitializeLookupChargesCodes));
                _lookupChargeCodes = new List<ChargesCodesInfo>();
            }
        }
        #endregion

        #region Timer Event Handlers
        private void ElapsedTimer_Tick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(StartTime) && DateTime.TryParse(StartTime, out var startDateTime))
            {
                var elapsed = DateTime.Now - startDateTime;
                Elapsed = elapsed.ToString(@"hh\:mm\:ss");

                // Auto-save during entry state
                if (CurrentState == EState.ENTRY && CurrentDocument != null && !string.IsNullOrEmpty(_jsonFilePath))
                {
                    _ = Task.Run(() => SaveJsonDocumentAsync(CurrentDocument, _jsonFilePath));
                }
            }
            else
            {
                Elapsed = null;
            }
        }

        private async void RequestTimer_Tick(object sender, EventArgs e)
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested)
                return;

            try
            {
                await ProcessRequestAsync();
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            catch (Exception ex)
            {
                _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(RequestTimer_Tick));
            }
        }

        private async Task ProcessRequestAsync()
        {

            var requestType = CurrentRole == ERole.QC ? ERequestType.QC : ERequestType.IMAGEVIEWER;

            if (CurrentState != EState.REQUEST) return;

            await _databaseSemaphore.WaitAsync(_cancellationTokenSource.Token);
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    // Get or create request
                    if (RequestInfo?.id == 0 || RequestInfo == null)
                    {
                        var conditions = new Dictionary<string, object>
                        {
                            { "user_id", CurrentBiller.id },
                            { "request_type", (int)requestType }
                        };
                        RequestInfo = _mySQLServices.GetRequestInfo(ConnectionString, conditions);

                        if (RequestInfo == null)
                        {
                            var tempRequest = new RequestInfo
                            {
                                user_id = CurrentBiller.id,
                                doc_id = 0,
                                image_path = "",
                                request_type = 2
                            };
                            tempRequest.id = _mySQLServices.CreateRequest(ConnectionString, tempRequest);
                            if (tempRequest.id > 0) RequestInfo = tempRequest;
                        }
                    }
                    else
                        RequestInfo = _mySQLServices.GetRequestInfoById(ConnectionString, RequestInfo.id);

                    if (RequestInfo?.doc_id > 0)
                        await SetupDocumentPathsAndLoad(connection);
                }
            }
            finally
            {
                _databaseSemaphore.Release();
            }

            if (CurrentDocument?.id > 0)
            {
                StartEntry();
            }
        }

        private async Task SetupDocumentPathsAndLoad(IDbConnection connection)
        {
            var fileInfo = new FileInfo(RequestInfo.image_path);
            var baseDirectory = fileInfo.Directory.Parent.FullName;
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(RequestInfo.image_path);

            ImagePath = fileInfo.FullName;
            OCRPath = Path.Combine(baseDirectory, "OCRDOC", $"{fileNameWithoutExt}.ocrdoc");
            XMLPath = Path.Combine(baseDirectory, "EXPORT", $"{fileNameWithoutExt}.xml");
            JSONPath = Path.Combine(baseDirectory, "EXPORT", $"{fileNameWithoutExt}.json");
            _jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp", $"{RequestInfo.doc_id}.json");

            if (!File.Exists(_jsonFilePath))
                await LoadDocumentFromDatabase(connection);
            else
                LoadDocumentFromJson();
        }

        private async Task LoadDocumentFromDatabase(IDbConnection connection)
        {
            const string command = @"SELECT 
                                        d.*, dt.*, dct.*, p.*, u.*
                                    FROM
                                        document_tbl d
                                            LEFT JOIN
                                        data_tbl dt ON dt.id = (CASE
                                            WHEN d.is_qced = '1' THEN d.data_qced1_id
                                            WHEN d.is_qced = '2' THEN d.data_qced2_id
                                            ELSE d.data_id
                                        END)
                                            LEFT JOIN 
                                        doc_type dct ON dct.id = d.bill_type
                                            LEFT JOIN
                                        process_tbl p ON p.id = d.billed_reject_process_id
                                            LEFT JOIN
                                        user_tbl u ON u.id = p.process_by
                                    WHERE
                                        d.id = @docId AND d.archive = 0;";

            CurrentDocument = (await connection.QueryAsync<Document, BillingInfo, DocTypeInfo, ProcessInfo, BillerInfo, Document>(
                command,
                (d, dt, dct, p, u) =>
                {
                    d.BillInfo = dt ?? new BillingInfo();
                    d.bill_type_name = dct?.name;
                    if (u != null) KeyedBy = u.fullname;
                    return d;
                },
                new { docId = RequestInfo.doc_id })).FirstOrDefault();

            if (CurrentDocument == null) return;

            // Initialize collections
            InitializeDocumentCollections();

            if (CurrentDocument.data_id > 0)
                await LoadDocumentDetails(connection);
            else
                LoadDocumentFromJsonOrDefault();
        }

        private void InitializeDocumentCollections()
        {
            CurrentDocument.BillInfo = new BillingInfo();
            CurrentDocument.AccountInfos = new ObservableCollection<AccountInfo>();
            CurrentDocument.InstructionInfos = new ObservableCollection<InstructionInfo>();
            CurrentDocument.ReferenceInfos = new ObservableCollection<ReferenceInfo>();
            CurrentDocument.ItemsInfos = new ObservableCollection<ItemsInfo>();
            CurrentDocument.AccessorialInfos = new ObservableCollection<AccessorialInfo>();
            CurrentDocument.ReferenceInfos.CollectionChanged += ViewModelLocator.Instance.Reference.ItemsInfos_CollectionChanged;
            CurrentDocument.ItemsInfos.CollectionChanged += ViewModelLocator.Instance.Items.ItemsInfos_CollectionChanged;
            CurrentDocument.AccessorialInfos.CollectionChanged += ViewModelLocator.Instance.Accessorials.ItemsInfos_CollectionChanged;
        }

        private async Task LoadDocumentDetails(IDbConnection connection)
        {
            var tasks = new[]
            {
                LoadAccountInfosAsync(connection),
                LoadInstructionInfosAsync(connection),
                LoadReferenceInfosAsync(connection),
                LoadItemsInfosAsync(connection),
                LoadAccessorialsAsync(connection)
            };

            await Task.WhenAll(tasks);
        }

        private async Task LoadAccountInfosAsync(IDbConnection connection)
        {
            var accounts = await connection.GetListAsync<AccountInfo>(new { data_id = CurrentDocument.data_id });
            CurrentDocument.AccountInfos = new ObservableCollection<AccountInfo>(accounts);
        }

        private async Task LoadInstructionInfosAsync(IDbConnection connection)
        {
            var instructions = await connection.GetListAsync<InstructionInfo>(new { data_id = CurrentDocument.data_id });
            CurrentDocument.InstructionInfos = new ObservableCollection<InstructionInfo>(instructions);
        }

        private async Task LoadReferenceInfosAsync(IDbConnection connection)
        {
            var references = await connection.GetListAsync<ReferenceInfo>(new { data_id = CurrentDocument.data_id });
            foreach (var @ref in references)
            {
                CurrentDocument.ReferenceInfos.Add(@ref);
            }            
        }

        private async Task LoadItemsInfosAsync(IDbConnection connection)
        {
            var items = await connection.GetListAsync<ItemsInfo>(new { data_id = CurrentDocument.data_id });
            foreach (var item in items)
            {
                CurrentDocument.ItemsInfos.Add(item);
            }
        }

        private async Task LoadAccessorialsAsync(IDbConnection connection)
        {
            var items = await connection.GetListAsync<AccessorialInfo>(new { data_id = CurrentDocument.data_id });
            foreach (var item in items)
            {
                CurrentDocument.AccessorialInfos.Add(item);
            }
        }
        private Dimensions ParseDimensions(string dimString)
        {
            if (string.IsNullOrEmpty(dimString))
                return new Dimensions();

            var match = DimensionRegex.Match(dimString);
            if (!match.Success)
                return new Dimensions();

            return new Dimensions
            {
                length = match.Groups[0].Value,
                width = match.Groups[1].Value,
                height = match.Groups[2].Value,
                uom = match.Groups[3].Success ? match.Groups[3].Value : ""
            };
        }
        private void LoadDocumentFromJsonOrDefault()
        {
            if (File.Exists(JSONPath))
            {
                ParseJSONFile(JSONPath);
                OcrMessage = "";
            }
            else
            {
                CurrentDocument.AccountInfos = new ObservableCollection<AccountInfo>(SupplyMissingAccounts(CurrentDocument.AccountInfos));
                OcrMessage = "OCR data not available";
            }
        }

        private void LoadDocumentFromJson()
        {
            try
            {
                var tempDoc = LoadJsonDocument<Document>(_jsonFilePath);
                if (tempDoc != null)
                {
                    var tempItems = tempDoc.ItemsInfos?.ToList();
                    var tempReferences = tempDoc.ReferenceInfos?.ToList();
                    var tempAccessorials = tempDoc.AccessorialInfos?.ToList();
                    CurrentDocument = tempDoc;
                    CurrentDocument.ItemsInfos = new ObservableCollection<ItemsInfo>();
                    CurrentDocument.ItemsInfos.CollectionChanged += ViewModelLocator.Instance.Items.ItemsInfos_CollectionChanged;
                    if (tempItems != null)
                    {
                        foreach (var item in tempItems)
                        {
                            CurrentDocument.ItemsInfos.Add(item);
                        }
                    }

                    CurrentDocument.ReferenceInfos = new ObservableCollection<ReferenceInfo>();
                    CurrentDocument.ReferenceInfos.CollectionChanged += ViewModelLocator.Instance.Reference.ItemsInfos_CollectionChanged;
                    if (tempReferences != null)
                    {  
                        for (var i = 0; i < tempReferences.Count(); i++)
                        {
                            var @ref = tempReferences[i];
                            CurrentDocument.ReferenceInfos.Add(@ref);
                        }
                    }

                    CurrentDocument.AccessorialInfos = new ObservableCollection<AccessorialInfo>();
                    CurrentDocument.AccessorialInfos.CollectionChanged += ViewModelLocator.Instance.Accessorials.ItemsInfos_CollectionChanged;
                    if (tempAccessorials != null)
                    {
                        foreach (var item in tempAccessorials)
                        {
                            CurrentDocument.AccessorialInfos.Add(item);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(LoadDocumentFromJson));
            }
        }

        #endregion

        #region JSON Operations
        private static readonly object _fileLock = new object();
        private void SaveJsonDocumentAsync(Document document, string filePath)
        {
            try
            {
                if (document == null || string.IsNullOrEmpty(filePath)) return;
                var jsonContent = JsonConvert.SerializeObject(document, Formatting.Indented);

                lock (_fileLock)
                {
                    File.WriteAllText(filePath, jsonContent);
                }
            }
            catch (Exception ex)
            {
                _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(SaveJsonDocumentAsync));
            }
        }


        private T LoadJsonDocument<T>(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return default(T);
                var jsonContent = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<T>(jsonContent);
            }
            catch (Exception ex)
            {
                _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(LoadJsonDocument));
                return default(T);
            }
        }

        private void ParseJSONFile(string path)
        {
            try
            {
                var jsonDocument = LoadJsonDocument<OCRResponse>(path);

                // Initialize billing info with empty values
                InitializeBillingInfo(jsonDocument);

                // Parse account information
                ParseAccountInformation(jsonDocument);

                // Parse items (currently empty implementation in original)
                ParseItems(jsonDocument.items);

                // Parse references (currently empty implementation in original)
                ParseReferences(jsonDocument.referenceNumbers);

                // Parse accessorials (currently empty implementation in original)
                ParseAccessorials(jsonDocument.accessorials);

                // Parse instructions (currently empty implementation in original)
                ParseInstructions(jsonDocument.specialInstructions);
            }
            catch (Exception ex)
            {
                _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(ParseJSONFile));
            }
        }

        private void InitializeBillingInfo(OCRResponse invoice)
        {
            var billingInfo = CurrentDocument.BillInfo;
            CurrentDocument.pro_number = invoice.proNumber;

            billingInfo.serviceLevel = invoice.serviceLevel;
            billingInfo.excessCoverageAmount = invoice.excessCoverageAmount;
        }
        private void ParseAccountInformation(OCRResponse invoice)
        {
            var accounts = new List<AccountInfo>
            {
                CreateAccountInfo(0, invoice.shipper),
                CreateAccountInfo(1, invoice.consignee),
                CreateAccountInfo(2, invoice.billTo),
                new AccountInfo { type = 3 } // Empty account type 3
            };

            CurrentDocument.AccountInfos = new ObservableCollection<AccountInfo>(accounts);
        }

        private static AccountInfo CreateAccountInfo(int type, OCRAccount accountData)
        {
            if (accountData == null) return new AccountInfo { type = type };
            return new AccountInfo
            {
                type = type,
                name = accountData.name,
                address = accountData.address1,
                address1 = accountData.address2,
                city = accountData.city,
                state = accountData.state,
                zip = accountData.zip,
                contact =!string.IsNullOrEmpty(accountData.contactNumber) ? accountData.contactNumber : accountData.contactName,
            };
        }

        private void ParseItems(IEnumerable<OCRItem> items)
        {
            if (items == null) return;
            foreach (var item in items)
            {
                var itemInfo = new ItemsInfo
                {
                    nmfc = item.nmfc,
                    pcs = item.pcs,
                    pcsUnit = item.pcsUnit,
                    plt = item.plt,
                    pltUnits = item.pltUnits,
                    stc = item.stc,
                    description = item.description,
                    weight = item.weight,
                    @class = item.@class,
                };
                CurrentDocument.ItemsInfos.Add(itemInfo);
            }
        }
        private void ParseAccessorials(IEnumerable<OCRAccessorials> accessorials)
        {
            if (accessorials == null) return;
            foreach (var item in accessorials)
            {
                var itemInfo = new AccessorialInfo
                {
                    charge = item.charge,
                    description = item.description,
                    manualAmt = item.manualAmt,
                    quantity = item.quantity,
                    rate = item.rate,
                    amount = item.amount,
                    notation = item.notation,
                };
                CurrentDocument.AccessorialInfos.Add(itemInfo);
            }
        }

        private void ParseInstructions(string instructions)
        {
            // Original implementation was checking for instructions but not parsing
            // This can be expanded based on actual JSON structure
            if (!string.IsNullOrEmpty(instructions))
            {
                var tempInstruction = instructions.Split(',');
                tempInstruction?.ToList().ForEach(instr =>
                {
                    CurrentDocument.InstructionInfos.Add(new InstructionInfo { Instruction = instr });
                });
            }
        }

        private void ParseReferences(IEnumerable<OCRReferenceNumber> references)
        {
            if (references == null) return;

            foreach (var reference in references)
            {
                // refName is invalid — just add the original one
                AddNewReference(reference.id, reference);
            }
        }

        private void UpdateReference(ReferenceInfo existingRef, OCRReferenceNumber newData)
        {
            existingRef.description = newData.value;
        }

        private void AddNewReference(string code, OCRReferenceNumber reference)
        {
            CurrentDocument.ReferenceInfos.Add(new ReferenceInfo
            {
                code = code,
                description = reference.value,
            });
        }

        private string GetRefName(string refId)
        {
            // Use HashSet for O(1) lookup
            var validRefs = new HashSet<string> { "bl", "po", "email" };
            return validRefs.Contains(refId.ToLower()) ? refId.ToUpper() : string.Empty;
        }

        #endregion

        #region Utility Methods
        private IEnumerable<AccountInfo> SupplyMissingAccounts(IEnumerable<AccountInfo> accountInfos)
        {
            var existingTypes = new HashSet<int>(accountInfos.Select(a => a.type));
            var result = new List<AccountInfo>();

            foreach (var type in RequiredAccountTypes)
            {
                if (existingTypes.Contains(type))
                {
                    result.Add(accountInfos.First(item => item.type == type));
                }
                else
                {
                    result.Add(new AccountInfo { type = type });
                }
            }

            return result;
        }

        private static int TryParseInt(string value)
        {
            return int.TryParse(value, out var result) ? result : 0;
        }

        private static decimal TryParseDecimal(string value)
        {
            return decimal.TryParse(value, out var result) ? result : 0m;
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Public Methods
        public async Task<IEnumerable<string>> ValidateDocumentAsync()
        {
            var validationTasks = _errorSources.Select(source => source.ValidateAllAsync());
            var errors = await Task.WhenAll(validationTasks);
            return errors.SelectMany(errorList => errorList);
        }
        public void RegisterErrorSource(ValidatableBase errorSource)
        {
            if (errorSource != null && !_errorSources.Contains(errorSource))
                _errorSources.Add(errorSource);
        }
        public void UnregisterErrorSource(ValidatableBase errorSource)
        {
            // Note: ConcurrentBag doesn't have a direct remove method
            // Consider using ConcurrentDictionary if frequent removal is needed
        }
        public void SetRubberBand(IRubberBand rubberBand) => rubberBandService = rubberBand;
        public void SetCurrentSkin(SkinType skin) => CurrentSkin = skin;
        public void SetLookUpChargeCodes(List<ChargesCodesInfo> infos)
        {
            _lookupChargeCodes = infos ?? new List<ChargesCodesInfo>();
            ViewModelLocator.Instance?.Items?.InitializeLookupCode();
        }

        public void RemoveInstruction(InstructionInfo info)
        {
            if (info == null || CurrentDocument?.InstructionInfos == null) return;

            var index = CurrentDocument.InstructionInfos.IndexOf(info);
            if (index >= 0)
            {
                CurrentDocument.InstructionInfos.RemoveAt(index);
                if (info.id > 0)
                {
                    CurrentDocument.RemoveInstructions?.Add(info);
                }
            }
        }

        public async Task<IEnumerable<T>> GetLookupAccountDetailsByColumnAsync<T>(string columnName, string value)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(columnName))
                return default;

            using (var connection = new MySqlConnection(LookUpConnectionString))
            {
                var tableName = DDCInteligence.Utilities.Helpers.SimpleCRUD.GetTableName(typeof(T));
                if (string.IsNullOrEmpty(tableName))
                    return default;

                string sql = $"SELECT * FROM {tableName} WHERE {columnName} LIKE @Value";
                var result = await connection.QueryAsync<T>(sql, new { Value = $"{value}%" });
                return result;
            }
        }

        public async Task<IEnumerable<T>> GetLookupByCommand<T>(string command)
        {
            if (string.IsNullOrEmpty(command))
                return default;

            using (var connection = new MySqlConnection(LookUpConnectionString))
            {
                var tableName = DDCInteligence.Utilities.Helpers.SimpleCRUD.GetTableName(typeof(T));
                if (string.IsNullOrEmpty(tableName))
                    return default;

                var result = await connection.QueryAsync<T>(command);
                return result;
            }
        }

        /// <summary>
        /// Handles prompt setting for terms textbox input
        /// </summary>
        public async Task HandleTermsPromptAsync(string inputText, Control dataContext, string textBoxName)
        {
            try
            {
                var prompt = await _promptHelper.GetPromptAsync(dataContext, textBoxName, inputText);
                SetPrompt(prompt);
            }
            catch (Exception ex)
            {
                ClearPrompt();
                // Handle exception silently or log if needed
                _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(HandleTermsPromptAsync));
            }
        }
        /// <summary>
        /// Sets the prompt in the ViewModelLocator
        /// </summary>
        public void SetPrompt(string prompt)
        {
            ViewModelLocator.Instance.Process.Prompt = prompt;
        }
        /// <summary>
        /// Clears the current prompt
        /// </summary>
        public void ClearPrompt()
        {
            ViewModelLocator.Instance.Process.Prompt = string.Empty;
        }

        #endregion

        #region Interface Implementation
        void IProcess.Request()
        {
            if (CurrentState == EState.IDLE)
            {
                ViewModelLocator.Instance.Main.IsRequesting = true;
                StatusMessage = "Requesting, please wait...";
                PreloaderImageMessage = "Waiting for your image...";
                CurrentState = EState.REQUEST;
                StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                _requestTimer.Start();
            }
        }

        void IProcess.Save(EStatus status)
        {
            if (CurrentState != EState.ENTRY) return;

            _ = Task.Run(async () =>
            {
                try
                {
                    var errors = await ValidateDocumentAsync();
                    if (errors.Any())
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ShowValidationErrors(errors);
                        });
                        return;
                    }

                    var saveParameters = await GetSaveParametersAsync(status);
                    if (saveParameters.IsCancel) return;

                    await SaveDocumentAsync(saveParameters);
                }
                catch (Exception ex)
                {
                    _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(IProcess.Save));
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SweetAlert.Show("Save failed", "An unexpected error occurred during saving.", SweetAlertButton.OK, SweetAlertImage.ERROR);
                    });
                }
            });
        }

        async void IProcess.SetCurrentBiller(BillerInfo billerInfo, int role)
        {
            try
            {
                await _databaseSemaphore.WaitAsync();

                using (var connection = new MySqlConnection(ConnectionString))
                {

                    // Update previous biller login status
                    if (CurrentBiller != null)
                    {
                        await connection.QueryAsync("UPDATE user_tbl SET is_login = 0 WHERE id = @id",
                            new { id = CurrentBiller.id });
                    }

                    // Set current biller login status
                    await connection.QueryAsync("UPDATE user_tbl SET is_login = 1 WHERE id = @id",
                        new { id = billerInfo.id });

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CurrentRole = (ERole)role;
                        CurrentBiller = billerInfo;
                        GetCurrentProfile();
                    });
                }
            }
            catch (Exception ex)
            {
                _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(IProcess.SetCurrentBiller));
            }
            finally
            {
                _databaseSemaphore.Release();
            }
        }

        void IProcess.SetState(EState state) =>  CurrentState = state;
        #endregion

        #region Save Operations
        private void ShowValidationErrors(IEnumerable<string> errors)
        {
            var alert = new SweetAlert
            {
                Caption = "Errors",
                Message = $"Failed validation:\n{string.Join("\n", errors)}",
                MsgImage = SweetAlertImage.ERROR,
                MsgButton = SweetAlertButton.OK,
                OkText = "Okay! I'll check."
            };
            alert.ShowDialog();
        }

        private async Task<SaveParameters> GetSaveParametersAsync(EStatus status)
        {
            var parameters = new SaveParameters();

            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                switch (status)
                {
                    case EStatus.BILLED:
                        var confirmAlert = new SweetAlert
                        {
                            Caption = "Confirmation",
                            Message = "Do you want to save this document ?",
                            MsgImage = SweetAlertImage.QUESTION,
                            MsgButton = SweetAlertButton.YesNo,
                            OkText = "Yes",
                            CancelText = "Cancel"
                        };

                        if (confirmAlert.ShowDialog() == SweetAlertResult.CANCEL)
                        {
                            parameters.IsCancel = true;
                        }
                        else
                        {
                            parameters.DocumentState = (int)status;
                            parameters.Status = status;
                        }
                        break;

                    default:
                        var remarksView = new RemarksView { Owner = Application.Current.MainWindow };
                        if (remarksView.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            parameters.Shipper = remarksView.Shipper;
                            parameters.Consignee = remarksView.Consignee;
                            parameters.Remarks = remarksView.Remarks;
                            parameters.Status = remarksView.eStatus;
                            parameters.DocumentState = parameters.Status == EStatus.REJECT ? 8 : parameters.Status == EStatus.QUERY ? 5 : 7;

                        }
                        else
                        {
                            parameters.IsCancel = true;
                        }
                        break;
                }

                return parameters;
            });
        }

        private async Task SaveDocumentAsync(SaveParameters parameters)
        {
            var startTime = GetStartTimeOrNow(StartTime);

            Application.Current.Dispatcher.Invoke(() =>
            {
                CurrentState = EState.SAVING;
                StatusMessage = "Saving your changes...";
                PreloaderImageMessage = "Saving your image...";
                StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            });

            await _databaseSemaphore.WaitAsync();
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    var success = await ExecuteSaveOperation(connection, parameters);
                    if (success)
                    {
                        var endTime = DateTime.Now;
                        await SaveProcessInfo(connection, parameters, startTime, endTime);
                        await TakeScreenshotAndCleanup(endTime);
                    }
                    else
                    {
                        RestoreEntryState(startTime);
                    }
                }
            }
            finally
            {
                _databaseSemaphore.Release();
            }
        }
        DateTime GetStartTimeOrNow(string StartTime)
        {
            if (string.IsNullOrEmpty(StartTime))
            {
                return DateTime.Now;
            }

            if (DateTime.TryParse(StartTime, out DateTime parsedTime))
            {
                return parsedTime;
            }

            // If parsing fails, return current time as fallback
            return DateTime.Now;
        }

        private async Task<bool> ExecuteSaveOperation(MySqlConnection connection, SaveParameters parameters)
        {
            try
            {
                string idType = CurrentRole == ERole.QC ? "data_qced1_id" :
                             CurrentRole == ERole.QC2 ? "data_qced2_id" :
                             "data_id";

                CurrentDocument.state = parameters.DocumentState;
                return await RunSaveAsync(connection, idType);
            }
            catch (Exception ex)
            {
                _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(ExecuteSaveOperation));
                return false;
            }
        }

        private async Task<bool> RunSaveAsync(MySqlConnection connection, string idType)
        {
            try
            {
                var property = CurrentDocument.GetType().GetProperty(idType);
                var idValue = int.Parse(property.GetValue(CurrentDocument)?.ToString() ?? "0");

                if (idValue > 0)
                {
                    await UpdateExistingDocument(connection, idValue);
                }
                else
                {
                    idValue = await CreateNewDocument(connection);
                    property.SetValue(CurrentDocument, idValue);
                    await InsertNewDocumentData(connection, idValue);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(RunSaveAsync));
                return false;
            }
        }

        private async Task UpdateExistingDocument(MySqlConnection connection, int dataId)
        {
            await connection.UpdateAsync(CurrentDocument.BillInfo);
            FinalizeDocumentData(dataId);

            var updateTasks = new[]
            {
                BeginTransactionAsync<AccountInfo>(connection, CurrentDocument.AccountInfos, "UPDATE"),
                BeginTransactionAsync<InstructionInfo>(connection, CurrentDocument.InstructionInfos, "UPDATE"),
                BeginTransactionAsync<ItemsInfo>(connection, GetValidItems(), "UPDATE"),
                BeginTransactionAsync<AccessorialInfo>(connection, GetValidAccessorials(), "UPDATE"),
                BeginTransactionAsync<ReferenceInfo>(connection, GetValidReferences(), "UPDATE")
            };

            await Task.WhenAll(updateTasks);

            // Handle removals
            await Task.WhenAll(
                BeginTransactionAsync<InstructionInfo>(connection, CurrentDocument.RemoveInstructions, "DELETE"),
                BeginTransactionAsync<ItemsInfo>(connection, CurrentDocument.RemoveItems, "DELETE"),
                BeginTransactionAsync<AccessorialInfo>(connection, CurrentDocument.RemoveAccessorials, "DELETE"),
                BeginTransactionAsync<ReferenceInfo>(connection, CurrentDocument.RemoveReference, "DELETE")
            );
        }

        private async Task<int> CreateNewDocument(IDbConnection connection)
        {
            var billingInfo = CurrentDocument.BillInfo;
            billingInfo.id = 0;
            return (await connection.InsertAsync(billingInfo)).Value;
        }

        private async Task InsertNewDocumentData(MySqlConnection connection, int dataId)
        {
            FinalizeDocumentData(dataId);

            await Task.WhenAll(
                BeginTransactionAsync<AccountInfo>(connection, CurrentDocument.AccountInfos, "INSERT"),
                BeginTransactionAsync<InstructionInfo>(connection, CurrentDocument.InstructionInfos, "INSERT"),
                BeginTransactionAsync<ItemsInfo>(connection, GetValidItems(), "INSERT"),
                BeginTransactionAsync<AccessorialInfo>(connection, GetValidAccessorials(), "INSERT"),
                BeginTransactionAsync<ReferenceInfo>(connection, GetValidReferences(), "INSERT")
            );
        }

        private IEnumerable<ItemsInfo> GetValidItems()
        {
            List<ItemsInfo> result = new List<ItemsInfo>();
            CurrentDocument.ItemsInfos?.ToList().ForEach(i => {
                bool allBlank = string.IsNullOrEmpty(i.nmfc) &&
                                string.IsNullOrEmpty(i.plt) &&
                                string.IsNullOrEmpty(i.pcs) &&
                                string.IsNullOrEmpty(i.weight);
                if (!allBlank)
                    result.Add(i);

            });

            return result;
        }
        private IEnumerable<AccessorialInfo> GetValidAccessorials()
        {
            List<AccessorialInfo> result = new List<AccessorialInfo>();
            CurrentDocument.AccessorialInfos?.ToList().ForEach(a => {
                bool allBlank = string.IsNullOrEmpty(a.charge) &&
                                string.IsNullOrEmpty(a.description) &&
                                string.IsNullOrEmpty(a.manualAmt) &&
                                string.IsNullOrEmpty(a.quantity) &&
                                string.IsNullOrEmpty(a.rate) &&
                                string.IsNullOrEmpty(a.amount) &&
                                string.IsNullOrEmpty(a.notation);
                if (!allBlank)
                    result.Add(a);
            });

            return result;
        }

        private IEnumerable<ReferenceInfo> GetValidReferences()
        {
            List<ReferenceInfo> result = new List<ReferenceInfo>();
            CurrentDocument.ReferenceInfos?.ToList().ForEach(r =>
            {
                bool allBlank = string.IsNullOrEmpty(r.code) &&
                                string.IsNullOrEmpty(r.description);
                if(!allBlank)
                    result.Add(r);
            });

            return result;
        }

        private void FinalizeDocumentData(int dataId)
        {
            // Update all collections with the data_id
            foreach (var account in CurrentDocument.AccountInfos)
            {
                account.data_id = dataId;
                NormalizeAccountAddress(account);
            }

            foreach (var instruction in CurrentDocument.InstructionInfos)
            {
                instruction.data_id = dataId;
            }

            foreach (var item in CurrentDocument.ItemsInfos)
            {
                item.data_id = dataId;

                if (!string.IsNullOrEmpty(item.pcs) && int.TryParse(item.pcs, out var p))
                {
                    item.pcs = p.ToString();
                } else
                {
                    item.pcs = "0";
                }
                if (!string.IsNullOrEmpty(item.weight) && int.TryParse(item.weight, out var w))
                {
                    item.weight = w.ToString();
                }
                else
                {
                    item.weight = "0";
                }
            }
            foreach (var reference in CurrentDocument.ReferenceInfos)
            {
                reference.data_id = dataId;
            }
            foreach (var accessorial in CurrentDocument.AccessorialInfos)
            {
                accessorial.data_id = dataId;
            }
        }

        private static void NormalizeAccountAddress(AccountInfo account)
        {
            // Handle empty addresses
            if (string.IsNullOrEmpty(account.address) && !string.IsNullOrEmpty(account.address1))
            {
                account.address = account.address1;
                account.address1 = string.Empty;
            }

            // Expand abbreviations
            account.address = ExpandAddressAbbreviations(account.address);
            account.address1 = ExpandAddressAbbreviations(account.address1);
        }

        private static string ExpandAddressAbbreviations(string address)
        {
            if (string.IsNullOrEmpty(address)) return address;

            var words = address.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (AddressAbbreviations.TryGetValue(words[i], out var expansion))
                {
                    words[i] = expansion;
                }
            }
            return string.Join(" ", words);
        }

        private async Task BeginTransactionAsync<T>(MySqlConnection connection, IEnumerable<T> data, string operation)
        {
            if (data == null || !data.Any()) return;
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    foreach (var item in data)
                    {
                        if (item == null) continue;

                        switch (operation)
                        {
                            case "INSERT":
                                await connection.InsertAsync(item, transaction);
                                break;
                            case "UPDATE":
                                if (ShouldInsertInsteadOfUpdate(item))
                                    await connection.InsertAsync(item, transaction);
                                else
                                    await connection.UpdateAsync(item, transaction);
                                break;
                            case "DELETE":
                                await connection.DeleteAsync(item, transaction);
                                break;
                        }
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, $"BeginTransactionAsync<{typeof(T).Name}>");
                    throw;
                }
            }  
        }

        private static bool ShouldInsertInsteadOfUpdate<T>(T item)
        {
            var id = item is ItemsInfo ? ((ItemsInfo)(object)item).id :
                     item is InstructionInfo ? ((InstructionInfo)(object)item).id :
                     item is ReferenceInfo ? ((ReferenceInfo)(object)item).id :
                     item is AccessorialInfo ? ((AccessorialInfo)(object)item).id :
                     item is AccountInfo ? ((AccountInfo)(object)item).id :
                     -1;
            return id == 0;
        }


        private async Task SaveProcessInfo(IDbConnection connection, SaveParameters parameters, DateTime startTime, DateTime endTime)
        {
            if (parameters.Status == EStatus.QUERY)
            {
                await SaveQueryInfo(connection, parameters, startTime, endTime);
            }
            else
            {
                await SaveRegularProcessInfo(connection, parameters, startTime, endTime);
            }

            await connection.UpdateAsync(CurrentDocument);
        }

        private async Task SaveQueryInfo(IDbConnection connection, SaveParameters parameters, DateTime startTime, DateTime endTime)
        {
            var queryInfo = new QueryInfo
            {
                doc_id = CurrentDocument.id,
                query = parameters.Remarks,
                time_start = startTime,
                time_end = endTime,
                processed_by = 1,
                shipper = parameters.Shipper,
                consignee = parameters.Consignee
            };

            await connection.InsertAsync(queryInfo);

            var queryProcess = new ProcessInfo
            {
                doc_id = CurrentDocument.id,
                start_datetime = startTime,
                end_datetime = endTime,
                message1 = "ENTRY",
                message2 = "Document is QUERY",
                message3 = "This document is processed on DDCI",
                process_by = CurrentBiller.id
            };

            CurrentDocument.query_process_id = (await connection.InsertAsync(queryProcess)).Value;
        }

        private async Task SaveRegularProcessInfo(IDbConnection connection, SaveParameters parameters, DateTime startTime, DateTime endTime)
        {
            var remarksWithSeparator = string.IsNullOrEmpty(parameters.Remarks) ? "" : $"|{parameters.Remarks}";

            var process = new ProcessInfo
            {
                doc_id = CurrentDocument.id,
                start_datetime = startTime,
                end_datetime = endTime,
                message3 = $"This document is processed on DDCI{remarksWithSeparator}",
                process_by = CurrentBiller.id
            };

            switch (CurrentRole)
            {
                case ERole.BILLER:
                    process.message1 = "ENTRY";
                    process.message2 = $"Document is {parameters.Status}";
                    CurrentDocument.billed_reject_process_id = (await connection.InsertAsync(process)).Value;
                    break;
                case ERole.QC:
                    process.message1 = "QCED1";
                    process.message2 = "Document is QCED1";
                    CurrentDocument.is_qced = 1;
                    CurrentDocument.qced1_process_id = (await connection.InsertAsync(process)).Value;
                    break;
                case ERole.QC2:
                    process.message1 = "QCED2";
                    process.message2 = "Document is QCED2";
                    CurrentDocument.is_qced = 2;
                    CurrentDocument.qced2_process_id = (await connection.InsertAsync(process)).Value;
                    break;
            }
        }

        private async Task TakeScreenshotAndCleanup(DateTime endTime)
        {
            await Task.Run(() =>
            {
                var screenshotPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PRINTSCREEN", endTime.ToString("yyyyMMdd"));
                _utilityServices.TakeScreenShot(screenshotPath, CurrentDocument.pro_number, endTime, CurrentBiller.username);
            });

            DeAllocated();

            Application.Current.Dispatcher.Invoke(() =>
            {
                Growl.SuccessGlobal(new GrowlInfo { Message = "Success! Document has been saved.", WaitTime = 1 });

                var requestAgainAlert = new SweetAlert
                {
                    Caption = "Confirmation",
                    Message = "Do you want to request again?",
                    MsgImage = SweetAlertImage.QUESTION,
                    MsgButton = SweetAlertButton.YesNo,
                    OkText = "Yes",
                    CancelText = "Cancel"
                };

                if (requestAgainAlert.ShowDialog() == SweetAlertResult.OK)
                {
                    ViewModelLocator.Instance.NonClientArea.RequestCommand.Execute(true);
                }
            });
        }

        private void RestoreEntryState(DateTime startTime)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StartTime = startTime.ToString("yyyy-MM-dd HH:mm:ss");
                CurrentState = EState.ENTRY;
                SweetAlert.Show("Unable to save, please try again.", "Unexpected error occurred during saving.", SweetAlertButton.OK, SweetAlertImage.ERROR);
            });
        }
        #endregion

        #region Cleanup and State Management
        public void DeAllocated()
        {
            // Cancel any ongoing operations
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            // Clear UI state
            ViewModelLocator.Instance.Main.IsRequesting = false;
            ClearValidationErrors();

            // Clear document-related properties
            ResetDocumentState();

            // Stop timers
            _requestTimer.Stop();

            // Clean up resources
            CleanupResources();
        }

        private void ClearValidationErrors()
        {
            ViewModelLocator.Instance.Consignee.ClearErrors();
            ViewModelLocator.Instance.Shipper.ClearErrors();
            ViewModelLocator.Instance.BillTo.ClearErrors();
            ViewModelLocator.Instance.RateTo.ClearErrors();
            ViewModelLocator.Instance.BillingInfo.ClearErrors();
            ViewModelLocator.Instance.Items.ClearErrors();

            CurrentDocument?.ItemsInfos?.ToList()?.ForEach(item =>
            {
                item.ClearErrors();
                UnregisterErrorSource(item);
            });
        }

        private void ResetDocumentState()
        {
            StatusMessage = null;
            PreloaderImageMessage = null;
            CurrentDocument = null;
            CurrentState = EState.IDLE;
            ImagePath = "";
            OCRPath = "";
            XMLPath = "";
            StartTime = null;
            OcrMessage = null;
            KeyedBy = null;
            Prompt = null;
            CustomerPrompt = null;
        }

        private void CleanupResources()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _databaseSemaphore.WaitAsync();

                    using (var connection = new MySqlConnection(ConnectionString))
                        await connection.DeleteAsync<RequestInfo>(RequestInfo?.id);

                    RequestInfo = null;
                }
                catch (Exception ex)
                {
                    _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(CleanupResources));
                }
                finally
                {
                    _databaseSemaphore.Release();
                }
            });

            if (!string.IsNullOrEmpty(_jsonFilePath) && File.Exists(_jsonFilePath))
            {
                try
                {
                    File.Delete(_jsonFilePath);
                    _jsonFilePath = null;
                }
                catch (Exception ex)
                {
                    _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, "CleanupResources - File Deletion");
                }
            }
        }

        private void StartEntry()
        {
            StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _ = Task.Run(ValidateDocumentAsync);
            _requestTimer.Stop();
            CurrentState = EState.ENTRY;
            ViewModelLocator.Instance.NonClientArea.GotoProCommand.Execute(null);
        }

        void GetCurrentProfile()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CurrentBiller?.username) || string.IsNullOrWhiteSpace(CurrentBiller.employee_id_number))
                    return;

                string searchFilename = $"{CurrentBiller.username}_{CurrentBiller.employee_id_number}";
                string tempDirectory = Path.Combine(Path.GetTempPath(), "ProfileImages");

                // Ensure directory exists
                Directory.CreateDirectory(tempDirectory); // Safe even if it already exists

                // Efficient file lookup
                var profilePicturePath = Directory
                    .EnumerateFiles(tempDirectory, $"*{searchFilename}*")
                    .FirstOrDefault();

                CurrentBiller.profilePicture = profilePicturePath;
            }
            catch { }
        }
        #endregion

        #region Dispose Pattern
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _requestTimer?.Stop();
                _elapsedTimer?.Stop();
                _databaseSemaphore?.Dispose();
            }
        }
        #endregion

        #region Helper Classes
        private class SaveParameters
        {
            public bool IsCancel { get; set; }
            public string Remarks { get; set; } = "";
            public string Shipper { get; set; } = "";
            public string Consignee { get; set; } = "";
            public EStatus Status { get; set; }
            public int DocumentState { get; set; }
        }
        #endregion
    }
}