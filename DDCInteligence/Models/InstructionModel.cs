using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using static DDCInteligence.DataTypes.DatabaseTypes;
using static DDCInteligence.DataTypes.LookupDatabaseTypes;

namespace DDCInteligence.Models
{
    public class InstructionModel : ViewModelBase, ITextBoxController
    {
        private readonly MySQLServices mySQLServices = new MySQLServices();
        private readonly IProcess process;

        public ICollection<InstructionInfo> Instructions => process?.CurrentDocument?.InstructionInfos;
        public ICommand RemoveInstructionCommand { get; }
        public event SelectAllEventHandler SelectAll;

        private ObservableCollection<SpecHDInfo> _lookupInstructions;
        public ObservableCollection<SpecHDInfo> LookupInstructions
        {
            get => _lookupInstructions;
            set
            {
                _lookupInstructions = value;
                OnPropertyChanged(nameof(LookupInstructions));
            }
        }

        private LookupInfo _selectedInstruction;
        public LookupInfo SelectedInstruction
        {
            get => _selectedInstruction;
            set
            {
                _selectedInstruction = value;
                OnPropertyChanged(nameof(SelectedInstruction));
            }
        }

        private string _instructionTextBox;
        public string InstructionTextBox
        {
            get => _instructionTextBox;
            set
            {
                _instructionTextBox = value;
                OnPropertyChanged(nameof(InstructionTextBox));
            }
        }

        public void Focus()
        {
            if (SelectAll != null) SelectAll(this);
        }

        public InstructionModel(IProcess process)
        {
            this.process = process;
            this.process.PropertyChanged += (_, __) => RaisePropertyChanged(null);

            RemoveInstructionCommand = new RelayCommand<object>(RemoveInstruction, CanRemoveRow);

            // Read the message list from the JSON file
            LoadInstructionsFromJson();
        }

        private void LoadInstructionsFromJson()
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppFiles", "instruction_rules.json");

                if (File.Exists(filePath))
                {
                    string jsonContent = File.ReadAllText(filePath);

                    // Reads array of message strings from JSON
                    List<string> messageList = JsonConvert.DeserializeObject<List<string>>(jsonContent);

                    if (messageList != null)
                    {
                        LookupInstructions = new ObservableCollection<SpecHDInfo>(
                            messageList.Select(msg => new SpecHDInfo { DESCR = msg })
                        );
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading instructions.json: {ex.Message}");
            }

            LookupInstructions = new ObservableCollection<SpecHDInfo>();
        }

        void RemoveInstruction(object parameter)
        {
            if (parameter is InstructionInfo instructionInfo)
            {
                process.RemoveInstruction(instructionInfo);
            }
        }

        private bool CanRemoveRow(object parameter)
        {
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}