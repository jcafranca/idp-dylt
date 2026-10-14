using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WindowsInput.Native;
using WindowsInput;
using System.ComponentModel;
using static DDCInteligence.DataTypes.DatabaseTypes;
using static DDCInteligence.DataTypes.DocumentTypes;
using DDCInteligence.Utilities.Events;
using System.Windows.Data;
using SweetAlertSharp;
using SweetAlertSharp.Enums;
using static DDCInteligence.DataTypes.LookupDatabaseTypes;

namespace DDCInteligence.View.UserControl
{
    /// <summary>
    /// Interaction logic for InstructionContent.xaml
    /// </summary>
    public partial class InstructionContent : INotifyPropertyChanged
    {
        private readonly ObservableCollection<SpecHDInfo> _filteredSuggestions = new ObservableCollection<SpecHDInfo>();
        private readonly DispatcherTimer _hidePopupTimer;

        private IRubberBand rubberBand;
        private SharedEvent _sharedEvent;
        private readonly LogServices logServices = new LogServices();
        public Point last_textbox_position { get; private set; }
        private double textbox_width;
        public double TextBoxWidth
        {
            get => textbox_width;
            set
            {
                textbox_width = value;
                OnPropertyChanged(nameof(TextBoxWidth));
            }
        }
        public InstructionContent()
        {
            InitializeComponent();

            GotKeyboardFocus += (sender, e) =>
            {
                if (e.OriginalSource is TextBox)
                {
                    rubberBand = new RubberBandServices();
                    rubberBand.SetTarget(e.OriginalSource, ((TextBox)e.OriginalSource).DataContext, BindingOperations.GetBinding((TextBox)e.OriginalSource, TextBox.TextProperty).Path.Path);
                    ViewModelLocator.Instance.Process.SetRubberBand(rubberBand);
                }
            };

            SuggestionList.ItemsSource = _filteredSuggestions;
            _hidePopupTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _hidePopupTimer.Tick += _hidePopupTimer_Tick;
            _hidePopupTimer.Start();

        }
        private void TextBoxInstruction_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = TextBoxInstruction.Text.ToUpper();
            _filteredSuggestions.Clear();

            if (!string.IsNullOrEmpty(query))
            {
                var filteredList = ViewModelLocator.Instance.Instructions.LookupInstructions
                    .Where(s => s.DESCR.ToUpper().Contains(query))
                    .ToList();

                foreach (var suggestion in filteredList)
                {
                    _filteredSuggestions.Add(suggestion);
                }

                SuggestionPopup.IsOpen = _filteredSuggestions.Count > 0;
            }
            else
            {
                SuggestionPopup.IsOpen = false;
            }
        }
        private void Control_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_filteredSuggestions.Count > 0)
            {
                SuggestionPopup.IsOpen = true;
            }
        }
        private void _hidePopupTimer_Tick(object sender, EventArgs e)
        {
            var focusedElement = Keyboard.FocusedElement;
            if (focusedElement != TextBoxInstruction && !SuggestionList.IsKeyboardFocusWithin)
            {
                TextBoxInstruction.Clear();
                SuggestionPopup.IsOpen = false;
            }
            UpdatePopupPosition();
        }
        private void TextBoxInstruction_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                if (SuggestionPopup.IsOpen && SuggestionList.Items.Count > 0)
                {
                    e.Handled = true;
                    SuggestionPopup.IsOpen = true;
                    SuggestionList.Focus();

                    if (SuggestionList.SelectedIndex == -1)
                    {
                        SuggestionList.SelectedIndex = 0;
                    }
                } else
                {
                    e.Handled = true;
                    if (dgvInstructions.Items.Count > 0)
                    {
                        dgvInstructions.Focus();
                        dgvInstructions.SelectedIndex = 0;

                        DataGridRow row = (DataGridRow)dgvInstructions.ItemContainerGenerator.ContainerFromIndex(0);
                        if (row != null)
                        {
                            row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        }
                    }
                }
            }
            else if (e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(TextBoxInstruction.Text))
                {
                    InputSimulator simulator = new InputSimulator();
                    simulator.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    return;
                }
                ApplySelectedItem();
                e.Handled = true;
            }
        }
        private void SuggestionList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && SuggestionList.SelectedIndex == 0)
            {
                e.Handled = true;
                TextBoxInstruction.Focus();
            }
            else if (e.Key == Key.Down)
            {
                e.Handled = true;
                if (SuggestionList.SelectedIndex < SuggestionList.Items.Count - 1)
                {
                    SuggestionList.SelectedIndex++;
                }
            }
            else if (e.Key == Key.Up)
            {
                e.Handled = true;
                if (SuggestionList.SelectedIndex > -1)
                {
                    SuggestionList.SelectedIndex--;
                }
            }
            else if (e.Key == Key.Enter && SuggestionList.SelectedItem != null)
            {
                ApplySelectedItem();
            }
            SuggestionList.ScrollIntoView(SuggestionList.SelectedItem);
        }
        private void SuggestionList_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (SuggestionList.SelectedItem != null)
            {
                ApplySelectedItem();
            }
        }
        private void ApplySelectedItem()
        {
            if (SuggestionList.SelectedItem != null)
            {
                SpecHDInfo info = SuggestionList.SelectedItem as SpecHDInfo;
                ValidateSelectedItem(info);
            } else
            {
                var temp_selected = SuggestionList.Items.Cast<SpecHDInfo>().ToArray()
                    .ToList().Where(item => item.CODE.ToUpper().Contains(TextBoxInstruction.Text.ToUpper()))
                    .FirstOrDefault();
                string temp_description = TextBoxInstruction.Text?.ToUpper();
                if (temp_selected != null)
                {
                    temp_description = temp_selected.DESCR;
                }
                //else
                //{
                //    SweetAlert.Show("Invalid Special Intruction Code.", "Please select a valid instruction code from the list.", SweetAlertButton.OK, SweetAlertImage.ERROR);
                //    return;
                //}
                ValidateSelectedItem(null, temp_description);
            }

            SuggestionPopup.IsOpen = false;
            TextBoxInstruction.Clear();
            TextBoxInstruction.Focus();
        }
        void ValidateSelectedItem(SpecHDInfo info, string value = "")
        {
            if (ViewModelLocator.Instance.Process.CurrentState == EState.ENTRY)
            {
                var newInstruction = new InstructionInfo() { Instruction =  value};
                if (info != null)
                {  
                    newInstruction = new InstructionInfo
                    {
                        Instruction = info.DESCR,
                    };
                }
                ViewModelLocator.Instance.Process.CurrentDocument.InstructionInfos.Add(newInstruction);
            }
        }      
        private void UpdatePopupPosition()
        {
            if (SuggestionPopup.IsOpen)
            {
                // Get the absolute position of the TextBox
                var textBoxLocation = TextBoxInstruction.PointToScreen(new Point(0, TextBoxInstruction.ActualHeight));
                if (textBoxLocation != last_textbox_position)
                {
                    last_textbox_position = textBoxLocation;
                }
            }
        }
        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void StackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double totalAvailableWidth = container.ActualWidth;
            TextBoxWidth = totalAvailableWidth - 200;
        }

        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
}
