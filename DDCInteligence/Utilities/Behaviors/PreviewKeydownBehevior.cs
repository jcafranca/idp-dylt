using DDCInteligence.DataTypes;
using DDCInteligence.Models;
using DDCInteligence.Utilities.Events;
using DDCInteligence.Utilities.Helpers.Validation;
using DDCInteligence.Utilities.Services;
using DDCInteligence.View;
using DDCInteligence.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using HandyControl.Interactivity;
using SweetAlertSharp;
using SweetAlertSharp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WindowsInput;
using WindowsInput.Native;
using static DDCInteligence.DataTypes.DatabaseTypes;
using static DDCInteligence.DataTypes.DocumentTypes;
using static DDCInteligence.DataTypes.KeysInfo;
using static DDCInteligence.DataTypes.LookupDatabaseTypes;

namespace DDCInteligence.Utilities.Behaviors
{
    public class PreviewKeydownBehevior : Behavior<Control>
    {
        #region Private Fields
        private readonly UtilityServices _services = new UtilityServices();
        private readonly LogServices _logServices = new LogServices();
        private readonly IRubberBand rubber = new RubberBandServices();
        private Control CurrentControl { get; set; }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty TargetDataGridProperty =
            DependencyProperty.Register(nameof(TargetDataGrid), typeof(DataGrid), typeof(PreviewKeydownBehevior));

        public DataGrid TargetDataGrid
        {
            get => (DataGrid)GetValue(TargetDataGridProperty);
            set => SetValue(TargetDataGridProperty, value);
        }

        public static readonly DependencyProperty TargetTextBoxProperty =
            DependencyProperty.Register(nameof(TargetTextBox), typeof(TextBox), typeof(PreviewKeydownBehevior));

        public TextBox TargetTextBox
        {
            get => (TextBox)GetValue(TargetTextBoxProperty);
            set => SetValue(TargetTextBoxProperty, value);
        }
        #endregion

        #region Event Handlers
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
        }

        private void AssociatedObject_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                CurrentControl = (Control)sender;

                if (HandleSpaceControlKeyDown(e)) return;

                switch (CurrentControl)
                {
                    case DataGrid dataGrid:
                        HandleDataGridKeyDown(dataGrid, e);
                        break;
                    case TextBox textBox:
                        HandleTextBoxKeyDown(textBox, e);
                        break;
                    case HandyControl.Controls.PasswordBox passwordBox:
                        HandlePasswordBoxKeyDown(passwordBox, e);
                        break;
                    case HandyControl.Controls.AutoCompleteTextBox autoCompleteTextBox:
                        HandleAutoCompleteTextBoxKeyDown(autoCompleteTextBox, e);
                        break;
                    default:
                        HandleGenericControlKeyDown(e);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(AssociatedObject_PreviewKeyDown));
            }
        }
        #endregion

        #region Key Handling Methods
        private bool HandleSpaceControlKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                return true;
            }
            return false;
        }

        private void HandleDataGridKeyDown(DataGrid dataGrid, KeyEventArgs e)
        {
            if (HandleCommonDataGridShortcuts(dataGrid, e)) return;

            if (dataGrid.CurrentCell.Column == null) return;

            var context = new DataGridContext
            {
                ColumnIndex = dataGrid.CurrentCell.Column.DisplayIndex,
                RowIndex = dataGrid.SelectedIndex,
                CurrentItem = dataGrid.CurrentItem
            };

            var handlers = new Dictionary<string, Action<DataGrid, KeyEventArgs, DataGridContext>>
            {
                ["DgvItems"] = HandleItemsDataGrid,
                ["dgvInstructions"] = HandleInstructionsDataGrid,
                ["DgvReference"] = HandleReferenceDataGrid,
                ["dgvAccessorials"] = HandleAccessorialDataGrid,
            };

            if (handlers.TryGetValue(dataGrid.Name, out var handler))
            {
                handler(dataGrid, e, context);
            }
        }

        private bool HandleCommonDataGridShortcuts(DataGrid dataGrid, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F2:
                    e.Handled = true;
                    ViewModelLocator.Instance.NonClientArea.DeAllocateCommand.Execute(null);
                    return true;

                case Key.Up when TargetTextBox != null && dataGrid.SelectedIndex <= 0:
                    e.Handled = true;
                    TargetTextBox.Focus();
                    return true;

                case Key.Down when dataGrid.Items.Count > 0 && dataGrid.SelectedIndex == dataGrid.Items.Count - 1:
                    SimulateTabKey();
                    return true;
            }
            return false;
        }

        private void HandleItemsDataGrid(DataGrid dataGrid, KeyEventArgs e, DataGridContext context)
        {
            var isShiftNavigation = IsShiftNavigationKey(e.Key);

            switch (e.Key)
            {
                case Key.Enter when !Keyboard.Modifiers.HasFlag(ModifierKeys.Control):
                case Key.Tab:
                    e.Handled = true;
                    FocusNextCellOrRow(dataGrid, context.ColumnIndex, context.CurrentItem);
                    break;
                case Key.Delete:
                    e.Handled = true;
                    ViewModelLocator.Instance.Items.RemoveItemCommand.Execute((ItemsInfo)dataGrid.SelectedItem);
                    ValidateItemSelectedRow(dataGrid, context.RowIndex);
                    break;
                case Key.Insert:
                    e.Handled = true;

                    int nextIndex;

                    if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    {
                        // Ctrl + Insert → Insert *above* current index
                        nextIndex = context.RowIndex;
                    }
                    else
                    {
                        // Just Insert → Insert at current index
                        nextIndex = context.RowIndex + 1;
                    }

                    ViewModelLocator.Instance.Items.InsertNewItemAt(nextIndex);
                    int targetIndex = nextIndex >= dataGrid.Items.Count - 1
                       ? dataGrid.Items.Count - 1
                       : nextIndex;

                    if (dataGrid.Items.Count > 0)
                    {
                        dataGrid.SelectedIndex = targetIndex;
                        dataGrid.CurrentCell = new DataGridCellInfo(dataGrid.Items[targetIndex], dataGrid.CurrentColumn);
                    }
                    break;
                case Key.OemQuestion when context.ColumnIndex == 1:
                    e.Handled = true;
                    //ShowItemsLookupForm(dataGrid.SelectedItem as ItemsInfo);
                    break;
                case Key.Add:
                    e.Handled = true;
                    if (IsShiftNavigationKey(e.Key))
                    {
                        string custNumber = ViewModelLocator.Instance.Shipper.Code;
                        string terms = ViewModelLocator.Instance.BillingInfo.Terms;
                        if (terms != null)
                        {
                            if (terms.Equals("c", StringComparison.OrdinalIgnoreCase))
                                custNumber = ViewModelLocator.Instance.Consignee.Code;
                            ShowOrFocusWindow<ItemSelectionView>(() =>
                            {
                                Messenger.Default.Send(new DisableKeysMessage(Key.F1, Key.F2, Key.F3, Key.F5, Key.F11));
                                var view = new ItemSelectionView(Application.Current.MainWindow) { custNumber = custNumber };
                                view.Closed += (_, __) => Messenger.Default.Send(new EnableKeysMessage(Key.F1, Key.F2, Key.F3, Key.F5, Key.F11));
                                view.Show();
                                return view;
                            });
                        }
                    }
                    break;
                default:
                    if (isShiftNavigation)
                    {
                        e.Handled = true;
                        MoveFocusCell(context, dataGrid, e);
                    }
                    break;
            }
        }

        private void HandleInstructionsDataGrid(DataGrid dataGrid, KeyEventArgs e, DataGridContext context)
        {
            var isShiftNavigation = IsShiftNavigationKey(e.Key);

            switch (e.Key)
            {
                case Key.Insert when Keyboard.Modifiers == ModifierKeys.Control:
                    e.Handled = true;
                    ViewModelLocator.Instance.Reference.AddItemCommand.Execute(null);
                    break;

                case Key.Delete:
                    e.Handled = true;
                    ViewModelLocator.Instance.Instructions.RemoveInstructionCommand.Execute(dataGrid.SelectedItem);
                    ValidateItemSelectedRow(dataGrid, context.RowIndex);

                    if (dataGrid.Items.Count == 0)
                        TargetTextBox?.Focus();
                    break;

                default:
                    if (isShiftNavigation)
                    {
                        e.Handled = true;
                        MoveFocusCell(context, dataGrid, e);
                    }
                    break;
            }
        }

        private void HandleReferenceDataGrid(DataGrid dataGrid, KeyEventArgs e, DataGridContext context)
        {
            var isShiftNavigation = IsShiftNavigationKey(e.Key);

            switch (e.Key)
            {
                case Key.Enter:
                case Key.Tab:
                    e.Handled = true;
                    FocusNextCellOrRow(dataGrid, context.ColumnIndex, context.CurrentItem);
                    break;

                case Key.OemQuestion when context.ColumnIndex == 1:
                    e.Handled = true;
                    //ShowReferenceLookupForm(dataGrid.SelectedItem as ReferenceInfo);
                    break;

                case Key.Insert:
                    e.Handled = true;
                    ViewModelLocator.Instance.Reference.AddItemCommand.Execute(null);
                    break;

                case Key.Delete: //when context.RowIndex > 2:
                    e.Handled = true;
                    ViewModelLocator.Instance.Reference.RemoveItemCommand.Execute((ReferenceInfo)dataGrid.SelectedItem);
                    ValidateItemSelectedRow(dataGrid, context.RowIndex);
                    break;

                default:
                    if (isShiftNavigation)
                    {
                        e.Handled = true;
                        MoveFocusCell(context, dataGrid, e);

                    }
                    break;
            }
        }
        private void HandleAccessorialDataGrid(DataGrid dataGrid, KeyEventArgs e, DataGridContext context)
        {
            var isShiftNavigation = IsShiftNavigationKey(e.Key);

            switch (e.Key)
            {
                case Key.Enter when !Keyboard.Modifiers.HasFlag(ModifierKeys.Control):
                case Key.Tab:
                    e.Handled = true;
                    FocusNextCellOrRow(dataGrid, context.ColumnIndex, context.CurrentItem);
                    break;
                case Key.Delete:
                    e.Handled = true;
                    ViewModelLocator.Instance.Accessorials.RemoveItemCommand.Execute((AccessorialInfo)dataGrid.SelectedItem);
                    ValidateItemSelectedRow(dataGrid, context.RowIndex);
                    break;
                case Key.Insert:
                    e.Handled = true;

                    int nextIndex;

                    if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                        nextIndex = context.RowIndex; // Ctrl + Insert → Insert *above* current index
                    else
                        nextIndex = context.RowIndex + 1; // Just Insert → Insert at current index

                    ViewModelLocator.Instance.Accessorials.InsertNewItemAt(nextIndex);
                    int targetIndex = nextIndex >= dataGrid.Items.Count - 1
                       ? dataGrid.Items.Count - 1
                       : nextIndex;

                    if (dataGrid.Items.Count > 0)
                    {
                        dataGrid.SelectedIndex = targetIndex;
                        dataGrid.CurrentCell = new DataGridCellInfo(dataGrid.Items[targetIndex], dataGrid.CurrentColumn);
                    }
                    break;
                case Key.OemQuestion when context.ColumnIndex == 1:
                    e.Handled = true;
                    //ShowItemsLookupForm(dataGrid.SelectedItem as ItemsInfo);
                    break;
                default:
                    if (isShiftNavigation)
                    {
                        e.Handled = true;
                        MoveFocusCell(context, dataGrid, e);
                    }
                    break;
            }
        }
        private void HandleTextBoxKeyDown(TextBox textBox, KeyEventArgs e)
        {
            if (!IsNavigationOrConfirmKey(e.Key) && e.Key != Key.OemQuestion)
                return;

            var handlers = new Dictionary<string, Func<TextBox, KeyEventArgs, bool>>
            {
                ["TextBoxUsername"] = HandleUsernameTextBox,
                ["TextBoxTag"] = HandleTagTextBox,
                ["TextBoxInstruction"] = HandleInstructionTextBox,
                ["TextBoxTerms"] = HandleTermsTextBox,
                ["Name"] = HandleNameTextBox,
                ["City"] = HandleCityTextBox,
                ["Zip"] = HandleZipTextBox,
                ["TextBoxNotes"] = HandleNotesTextBox,
            };

            if (handlers.TryGetValue(textBox.Name, out var handler))
            {
                var result = handler(textBox, e);
                if (result == false) return;
            }

            HandleGenericNavigation(e);
        }

        private void HandlePasswordBoxKeyDown(HandyControl.Controls.PasswordBox passwordBox, KeyEventArgs e)
        {
            if (!IsNavigationOrConfirmKey(e.Key) || ViewModelLocator.Instance.Login.Loading)
                return;

            if (e.Key == Key.Enter)
            {
                if (passwordBox.Name == "TextBoxPassword")
                {
                    if (string.IsNullOrWhiteSpace(passwordBox.Password))
                    {
                        ShowValidationError("Invalid password", "Password cannot be blank, please check.");
                        e.Handled = true;
                        passwordBox.SelectAll();
                        return;
                    }

                    e.Handled = true;
                    ViewModelLocator.Instance.Login.LoginCommand.Execute(null);
                }
            } else
            {
                HandleGenericControlKeyDown(e);
            }
        }

        private void HandleAutoCompleteTextBoxKeyDown(HandyControl.Controls.AutoCompleteTextBox autoCompleteTextBox, KeyEventArgs e)
        {
            if (!IsNavigationOrConfirmKey(e.Key)) return;

            e.Handled = true;

            if (ViewModelLocator.Instance.Process.CurrentState == EState.ENTRY)
            {
                var spec = ViewModelLocator.Instance.Instructions.LookupInstructions;

                if (spec.Any(s => s.CODE.Equals(autoCompleteTextBox.Text, StringComparison.OrdinalIgnoreCase)))
                {
                    ViewModelLocator.Instance.Process.CurrentDocument.InstructionInfos.Add(
                        new InstructionInfo {  Instruction = autoCompleteTextBox.Text });
                }
                else
                {
                    ShowValidationError("Invalid instruction", "Please select a valid instruction code from the list.");
                }
            }
        }

        private void HandleGenericControlKeyDown(KeyEventArgs e)
        {
            if (IsNavigationOrConfirmKey(e.Key))
            {
                HandleGenericNavigation(e);
            }
        }
        #endregion

        #region TextBox Handlers
        private bool HandleUsernameTextBox(TextBox textBox, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return true;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                ShowValidationError("Invalid username", "Username cannot be blank, please check.");
                e.Handled = true;
                textBox.SelectAll();
                return false;
            }
            return true;
        }

        private bool HandleTagTextBox(TextBox textBox, KeyEventArgs e)
        {
            ViewModelLocator.Instance.RefNumberVM.TagName = textBox.Text;
            ViewModelLocator.Instance.RefNumberVM.AddItemCmd.Execute(true);
            return true;
        }

        private bool HandleInstructionTextBox(TextBox textBox, KeyEventArgs e)
        {
            if (e.Key == Key.Down && TargetDataGrid != null)
            {
                HandleInstructionTextBoxDown(textBox, e);
                return true;
            }
            return true;
        }

        private bool HandleTermsTextBox(TextBox textBox, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return true;

            e.Handled = true;
            if (ViewModelLocator.Instance.Process.CurrentState != EState.ENTRY) return true;
            //if (!string.IsNullOrEmpty(_logServices.RemoveDoubleSpace(textBox.Text)))
            //{
            //    var accountInfo = ViewModelLocator.Instance.Process.CurrentDocument.AccountInfos
            //        .FirstOrDefault(acct => acct.type == 0);
            //    ShowAccountSelectionForm(accountInfo.code, "shipper");
            //}
            return true;
        }

        private bool HandleNameTextBox(TextBox textBox, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return true;

            var account = GetParentAccountModel(textBox);
            if (ViewModelLocator.Instance.Process.CurrentState != EState.ENTRY || account == null) return true;

            if (string.IsNullOrEmpty(account.Code))
            {
                var result = HandleNameLookup(textBox, account).Result;
                if (!result) return false;
            }
            return true;
        }

        private bool HandleCityTextBox(TextBox textBox, KeyEventArgs e)
        {
            //if (e.Key == Key.OemQuestion)
            //{
            //    e.Handled = true;
            //    var account = GetParentAccountModel(textBox);
            //    ShowSearchCityForm(account);                
            //}
            return true;
        }

        private bool HandleZipTextBox(TextBox textBox, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return true;

            //var account = GetParentAccountModel(textBox);
            //if (account?.IdentifierName.Equals("consignee", StringComparison.OrdinalIgnoreCase) == true)
            //{
            //    var res = HandleConsigneeZipEntry().Result;
            //    if (!res) return false;
            //}
            return true;
        }
        private bool HandleNotesTextBox(TextBox textBox, KeyEventArgs e)
        {
            return false;
        }
        #endregion

        #region Navigation and Focus Methods
        private void HandleGenericNavigation(KeyEventArgs e)
        {
            if (!IsNavigationOrConfirmKey(e.Key)) return;

            var direction = e.Key == Key.Up ? FocusNavigationDirection.Previous : FocusNavigationDirection.Next;
            var request = new TraversalRequest(direction);
            var currentElement = Keyboard.FocusedElement as UIElement;

            if (currentElement?.MoveFocus(request) == true)
            {
                var nextElement = Keyboard.FocusedElement;
                if (nextElement is DataGridCell cell)
                {
                    var nextDataGrid = GetParentDataGrid(cell);
                    FocusOnItem(nextDataGrid, 0, 0);
                }
            }

            e.Handled = true;
        }

        private void FocusOnItem(DataGrid dataGrid, int rowIndex, int columnIndex)
        {
            if (dataGrid?.Items.Count > rowIndex)
            {
                var item = dataGrid.Items[rowIndex];
                dataGrid.SelectedItem = item;
                dataGrid.ScrollIntoView(item, dataGrid.Columns[columnIndex]);
                dataGrid.BeginEdit();

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var currentCell = dataGrid.CurrentCell;
                    if (currentCell.Column?.GetCellContent(currentCell.Item) is TextBox textBox)
                    {
                        textBox.Focus();
                        textBox.SelectAll();
                        TrySetRubberBand(textBox.Text);
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void FocusNextCellOrRow(DataGrid dataGrid, int currentColumnIndex, object currentItem)
        {
            if (!ValidateItemFields(dataGrid, currentColumnIndex, currentItem)) return;

            int nextColumnIndex = currentColumnIndex + 1;
            int currentRowIndex = dataGrid.Items.IndexOf(currentItem);

            if (nextColumnIndex < dataGrid.Columns.Count)
            {
                dataGrid.CurrentCell = new DataGridCellInfo(currentItem, dataGrid.Columns[nextColumnIndex]);
            }
            else if (currentRowIndex < dataGrid.Items.Count - 1)
            {
                var nextRowItem = dataGrid.Items[currentRowIndex + 1];
                dataGrid.CurrentCell = new DataGridCellInfo(nextRowItem, dataGrid.Columns[1]);
                dataGrid.SelectedIndex = currentRowIndex + 1;
                dataGrid.ScrollIntoView(nextRowItem, dataGrid.Columns[1]);
            }
            else
            {
                dataGrid.CancelEdit();
                return;
            }

            dataGrid.BeginEdit();
            FocusCurrentCell(dataGrid);
        }

        private void MoveFocusCell(DataGridContext context, DataGrid dataGrid, KeyEventArgs e)
        {
            var (nextColumnIndex, nextRowIndex) = CalculateNextCellPosition(context, dataGrid, e.Key);
            if (nextRowIndex < 0) return;

            var nextItem = dataGrid.Items[nextRowIndex];
            dataGrid.SelectedItem = nextItem;
            dataGrid.CurrentCell = new DataGridCellInfo(nextItem, dataGrid.Columns[Math.Max(0, nextColumnIndex)]);
            dataGrid.ScrollIntoView(nextItem, dataGrid.Columns[nextColumnIndex]);
            dataGrid.BeginEdit();

            FocusCurrentCell(dataGrid);
        }
        private (int columnIndex, int rowIndex) CalculateNextCellPosition(DataGridContext context, DataGrid dataGrid, Key key)
        {
            int nextColumnIndex = context.ColumnIndex;
            int nextRowIndex = context.RowIndex;

            switch (key)
            {
                case Key.Left:
                    nextColumnIndex--;
                    break;
                case Key.Right:
                    nextColumnIndex++;
                    break;
                case Key.Up:
                    nextRowIndex--;
                    break;
                case Key.Down:
                    nextRowIndex++;
                    break;
            }

            // Handle column wrapping
            if (nextColumnIndex < 0)
            {
                nextColumnIndex = dataGrid.Columns.Count - 1;
            }
            else if (nextColumnIndex >= dataGrid.Columns.Count)
            {
                nextColumnIndex = 0;
            }

            // Handle row wrapping
            if (nextRowIndex < 0)
            {
                nextRowIndex = dataGrid.Items.Count - 1;
            }
            else if (nextRowIndex >= dataGrid.Items.Count)
            {
                nextRowIndex = 0;
                nextColumnIndex = Math.Max(0, context.ColumnIndex - 1);
            }

            return (nextColumnIndex, nextRowIndex);
        }

        private void FocusCurrentCell(DataGrid dataGrid)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var currentCell = dataGrid.CurrentCell;
                if (currentCell.Column?.GetCellContent(currentCell.Item) is TextBox textBox)
                {
                    textBox.Focus();
                    textBox.SelectAll();
                    TrySetRubberBand(textBox.Text);

                    if (dataGrid.Name.Equals("dgvreference", StringComparison.OrdinalIgnoreCase))
                    {
                        if (dataGrid.CurrentCell.Column != null && dataGrid.CurrentCell.Column.DisplayIndex == 2)
                        {
                            //var selectedItem = dataGrid.SelectedItem as ReferenceInfo;
                            //_ = ViewModelLocator.Instance.Process.HandleTermsPromptAsync(textBox.Text, dataGrid, $"reference{selectedItem.code}");
                        } else
                            ViewModelLocator.Instance.Process.ClearPrompt();
                    } else
                    {
                        ViewModelLocator.Instance.Process.ClearPrompt();
                    }
                }
                else
                    ViewModelLocator.Instance.Process.ClearPrompt();



            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void HandleInstructionTextBoxDown(TextBox textBox, KeyEventArgs e)
        {
            textBox.Clear();
            e.Handled = true;

            if (TargetDataGrid?.Items.Count > 0)
            {
                TargetDataGrid.Focus();
                TargetDataGrid.SelectedIndex = 0;

                var row = (DataGridRow)TargetDataGrid.ItemContainerGenerator.ContainerFromIndex(0);
                row?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
        #endregion

        #region Validation Methods
        private bool ValidateItemFields(DataGrid dataGrid, int currentColumnIndex, object currentItem)
        {
            try
            {
                var currentColumn = dataGrid.Columns[currentColumnIndex];
                if (!(currentColumn is DataGridTextColumn textColumn)) return true;

                var bindingPath = (textColumn.Binding as Binding)?.Path?.Path;
                if (string.IsNullOrEmpty(bindingPath)) return true;

                var propertyValue = GetPropertyValue(currentItem, bindingPath);
                if (propertyValue == null) return true;

                return ValidateColumnValue(textColumn, currentItem, bindingPath, propertyValue);
            }
            catch (Exception ex)
            {
                _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(ValidateItemFields));
                ShowValidationError(ex.Message, ex.StackTrace);
                return false;
            }
        }

        private bool ValidateColumnValue(DataGridTextColumn column, object item, string bindingPath, object propertyValue)
        {
            string value = RemoveSpaces(propertyValue.ToString(), column.Header.ToString().Equals("Weight") ? "s" : "m");
            switch (column.Header.ToString())
            {
                case "Weight":
                    return ValidateWeight(item, bindingPath, value);
                case "Pallet":
                case "Pieces":
                    return ValidateNumericField(item, bindingPath, value, column.Header.ToString());
                default:
                    return true;
            }
        }

        private bool ValidateWeight(object item, string bindingPath, string value)
        {
            if (!string.IsNullOrEmpty(value) && value.StartsWith("#"))
            {
                var convertedValue = _services.ConvertKiloToPound(value.Replace("#", ""));
                SetPropertyValue(item, bindingPath, convertedValue);
            }
            return true;
        }

        private bool ValidateNumericField(object item, string bindingPath, string value, string fieldName)
        {
            if (string.IsNullOrEmpty(value))
            {
                SetPropertyValue(item, bindingPath, "0");
            }
            else if (!IsNumber(value))
            {
                ShowValidationError("Invalid characters!", $"{fieldName} must be numeric only.");
                return false;
            }
            return true;
        }
        private void ValidateItemSelectedRow(DataGrid dataGrid, int currentRowIndex)
        {
            dataGrid.Focus();

            int targetIndex = currentRowIndex > dataGrid.Items.Count - 1
                ? dataGrid.Items.Count - 1
                : currentRowIndex;

            if (dataGrid.Items.Count > 0)
            {
                dataGrid.SelectedIndex = targetIndex;
                dataGrid.CurrentCell = new DataGridCellInfo(dataGrid.Items[targetIndex], dataGrid.Columns[0]);
            }
        }
        #endregion

        #region Helper Methods
        private static bool IsNavigationKey(Key key) =>
            key == Key.Left || key == Key.Right || key == Key.Up || key == Key.Down || key == Key.Add;

        private static bool IsNavigationOrConfirmKey(Key key) =>
            key == Key.Enter || key == Key.Tab || key == Key.Up || key == Key.Down;

        private static bool IsShiftNavigationKey(Key key) =>
            Keyboard.Modifiers == ModifierKeys.Shift && IsNavigationKey(key);

        private void SimulateTabKey()
        {
            var simulator = new InputSimulator();
            simulator.Keyboard.KeyPress(VirtualKeyCode.TAB);
        }

        private void ShowValidationError(string title, string message)
        {
            SweetAlert.Show(title, message, SweetAlertButton.OK, SweetAlertImage.ERROR);
        }

        private static DataGrid GetParentDataGrid(DataGridCell cell)
        {
            DependencyObject parent = cell;
            while (parent != null)
            {
                if (parent is DataGrid dataGrid) return dataGrid;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        private AccountsModel GetParentAccountModel(TextBox textBox)
        {
            return GetParentUserControl(textBox)?.DataContext as AccountsModel;
        }

        private UserControl GetParentUserControl(TextBox textBox)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(textBox);
            while (parent != null)
            {
                if (parent is UserControl userControl) return userControl;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        private object GetPropertyValue(object obj, string propertyName)
        {
            return obj?.GetType().GetProperty(propertyName)?.GetValue(obj, null);
        }

        private void SetPropertyValue(object obj, string propertyName, object value)
        {
            obj?.GetType().GetProperty(propertyName)?.SetValue(obj, value);
        }

        private string RemoveSpaces(string value, string type = "m")
        {
            return type == "s" ? value.Replace(" ", "") : Regex.Replace(value, @"\s+", " ").Trim();
        }

        private bool IsNumber(string value) => int.TryParse(value, out _);

        private void TrySetRubberBand(string text)
        {

            try
            {
                ViewModelLocator.Instance.ImageViewer.pictureBox.SetRubberBand(text);
            }
            catch
            {
                // Silently handle rubber band errors
            }
        }
        #endregion

        #region Form Display Methods
        private void ShowItemsLookupForm(ItemsInfo currentItem)
        {
            ShowOrFocusWindow<ItemsLookupView>(() =>
            {
                Messenger.Default.Send(new DisableKeysMessage(Key.F1, Key.F2, Key.F3, Key.F5, Key.F11));
                var view = new ItemsLookupView(Application.Current.MainWindow) { SelectedItem = currentItem };
                view.Closed += (_, __) => Messenger.Default.Send(new DisableKeysMessage(Key.F1, Key.F2, Key.F3, Key.F5, Key.F11));
                view.Show();
                return view;
            });
        }

        private void ShowReferenceLookupForm(ReferenceInfo currentItem)
        {
            ShowOrFocusWindow<ReferenceLookupView>(() =>
            {
                Messenger.Default.Send(new DisableKeysMessage(Key.F1, Key.F2, Key.F3, Key.F5, Key.F11));
                var view = new ReferenceLookupView(Application.Current.MainWindow) { SelectedItem = currentItem };
                view.Closed += (_, __) => Messenger.Default.Send(new DisableKeysMessage(Key.F1, Key.F2, Key.F3, Key.F5, Key.F11));
                view.Show();
                return view;
            });
        }

        private void ShowSearchCityForm(AccountsModel account)
        {
            ShowOrFocusWindow<SearchCityView>(() =>
            {
                Messenger.Default.Send(new DisableKeysMessage(Key.F1, Key.F2, Key.F3, Key.F5, Key.F11));
                var view = new SearchCityView(Application.Current.MainWindow, account);
                view.Closed += (_, __) => Messenger.Default.Send(new DisableKeysMessage(Key.F1, Key.F2, Key.F3, Key.F5, Key.F11));
                view.Show();
                return view;
            });
        }

        private async void ShowAccountSelectionForm(string accountCode, string identifier = "")
        {
            if (ShowOrFocusWindowBoolean<AccountAliasSelectionView>(null)) return;

            try
            {
                var parentInfo = await ViewModelLocator.Instance.Process
                    .GetLookupAccountDetailsByColumnAsync<CustomerInfo>("ACCOUNT_CODE", accountCode);
                var parent = parentInfo?.FirstOrDefault();

                if (parent != null)
                {
                    var childAliasInfos = await GetChildAliasInfos(parent.PARENT_PROFIT_NUM);
                    if (childAliasInfos != null)
                    {
                        var view = new AccountAliasSelectionView(Application.Current.MainWindow, childAliasInfos);
                        if (view.ShowDialog() == true && view.SelectedAlias != null)
                        {
                            UpdateAccountInfo(view.SelectedAlias, identifier);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(ShowAccountSelectionForm));
            }
        }

        private void ShowOrFocusWindow<T>(Func<T> createWindow) where T : Window
        {
            var existingWindow = Application.Current.Windows.OfType<T>().FirstOrDefault();
          
            if (existingWindow == null && createWindow != null)
            {
                createWindow();
            }
            else if (existingWindow != null)
            {
                FocusExistingWindow(existingWindow);
            }
        }

        private bool ShowOrFocusWindowBoolean<T>(Func<T> createWindow) where T : Window
        {
            var existingWindow = Application.Current.Windows.OfType<T>().FirstOrDefault();
            if (existingWindow != null)
            {
                FocusExistingWindow(existingWindow);
                return true;
            }
            return false;
        }

        private void FocusExistingWindow(Window window)
        {
            if (window.WindowState == WindowState.Minimized)
                window.WindowState = WindowState.Normal;
            window.Activate();
        }
        #endregion

        #region Async Helpers
        private async Task<IEnumerable<CustomerInfo>> GetChildAliasInfos(string parentProfitNum)
        {
            string query = $@"
                SELECT c.ACCOUNT_CODE, c.NAME, c.ADDR_1 
                FROM customer_tbl c 
                WHERE c.PARENT_PROFIT_NUM = '{parentProfitNum}'
                UNION ALL
                SELECT a.ACCOUNT_CODE, a.ALIAS_NAME as NAME, a.ADDR_1
                FROM aliases_tbl a
                JOIN customer_tbl c ON a.ACCOUNT_CODE = c.ACCOUNT_CODE
                WHERE c.PARENT_PROFIT_NUM = '{parentProfitNum}'
                    AND a.FLAGGED_ALIAS_FLAG = 'Y'
                    AND a.ALIAS_NAME NOT LIKE 'ZZ%'";
            return await ViewModelLocator.Instance.Process.GetLookupByCommand<CustomerInfo>(query);
        }

        private async Task<bool> HandleNameLookup(TextBox textBox, AccountsModel account)
        {
            try
            {
                string accountToSearch = _logServices.RemoveDoubleSpace(textBox.Text.Trim());
                
                var accountDetails = await ViewModelLocator.Instance.Process.GetLookupAccountDetailsByColumnAsync<ClientInfo>("NAME", accountToSearch);

                if (accountDetails != null && !ShowOrFocusWindowBoolean<AccountSelectionListView>(null))
                {
                    Messenger.Default.Send(new DisableKeysMessage(Key.F1, Key.F2, Key.F3, Key.F5, Key.F11));
                    var view = new AccountSelectionListView(Application.Current.MainWindow, accountDetails, account.IdentifierName);                  
                    view.Closed += (_, __) =>
                    {
                        Messenger.Default.Send(new EnableKeysMessage(Key.F1, Key.F2, Key.F3, Key.F5, Key.F11));
                        if (view.SelectedCustomer != null)
                        {
                            UpdateAccountFromCustomer(account, view.SelectedCustomer);
                        }
                    };
                    view.Show();
                }
                return true;
            }
            catch (Exception ex)
            {
                _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(HandleNameLookup));
            }
            return true;
        }

        private async Task<bool> HandleConsigneeZipEntry()
        {
            //if (ShowOrFocusWindowBoolean<BillToAccountSelectionListView>(null)) return false;

            //try
            //{
            //    var suggestionList = await BuildBillToSuggestionList();
            //    var view = new BillToAccountSelectionListView(Application.Current.MainWindow, suggestionList);
            //    bool? result = view.ShowDialog();

            //    if (result == true && view.SelectedBillTo != null)
            //    {
            //        UpdateBillToInfo(view.SelectedBillTo);
            //    }
            //    else return false;
            //}
            //catch (Exception ex)
            //{
            //    _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(HandleConsigneeZipEntry));
            //}

            return true;
        }

      
    
        private void UpdateAccountInfo(CustomerInfo selected, string identifier)
        {
            dynamic target = ViewModelLocator.Instance.Shipper;
            if (identifier.Equals("consignee", StringComparison.OrdinalIgnoreCase))
            {
                target = ViewModelLocator.Instance.Consignee;
            }

            if (target != null)
            {
                target.Code = _logServices.RemoveDoubleSpace(selected.ACCOUNT_CODE);
                target.Name = _logServices.RemoveDoubleSpace(selected.NAME);
                target.Address = _logServices.RemoveDoubleSpace(selected.ADDR_1);
            }
        }

        private void UpdateAccountFromCustomer(AccountsModel account, ClientInfo selected)
        {
            account.Code = _logServices.RemoveDoubleSpace(selected.clientId.ToString());
            account.Name = _logServices.RemoveDoubleSpace(selected.name);
            account.Address = _logServices.RemoveDoubleSpace(selected.address1);
            account.Address2 = _logServices.RemoveDoubleSpace(selected.address2);
            account.City = _logServices.RemoveDoubleSpace(selected.city);
            account.State = _logServices.RemoveDoubleSpace(selected.provine);
            account.Zip = _logServices.RemoveDoubleSpace(selected.postalcode);
        }

        private void UpdateBillToInfo(ThirdPartyInfo selected)
        {
            var billTo = ViewModelLocator.Instance.BillTo;
            billTo.Code = selected.ACCOUNT_CODE;
            billTo.Name = _logServices.RemoveDoubleSpace(selected.TP_NAME);
            billTo.Address = _logServices.RemoveDoubleSpace(selected.ADDR_1);
            billTo.Address2 = _logServices.RemoveDoubleSpace(selected.ADDR_2);
            billTo.City = _logServices.RemoveDoubleSpace(selected.CITY);
            billTo.State = _logServices.RemoveDoubleSpace(selected.STATE);
            billTo.Zip = _logServices.RemoveDoubleSpace(selected.ZIP);
        }

        private void References_Closed(object sender, EventArgs e)
        {
            if (CurrentControl is TextBox textBox && sender is ReferencesView view)
            {
                textBox.Text = view.RETURN ?? textBox.Text.Replace("?", "");
                textBox.SelectAll();
            }
        }
        #endregion

        #region Data Context Class
        private class DataGridContext
        {
            public int ColumnIndex { get; set; }
            public int RowIndex { get; set; }
            public object CurrentItem { get; set; }
        }
        #endregion
    }
}