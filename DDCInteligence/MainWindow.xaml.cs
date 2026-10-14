using AvalonDock;
using AvalonDock.Layout;
using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Helpers;
using DDCInteligence.Utilities.Services;
using DDCInteligence.View;
using DDCInteligence.View.UserControl;
using DDCInteligence.ViewModel;
using DotNetCS.ImageViewer;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SweetAlertSharp;
using SweetAlertSharp.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static DDCInteligence.DataTypes.DocumentTypes;
using static DDCInteligence.DataTypes.ImageViewerTypes;
using static DDCInteligence.DataTypes.KeysInfo;

namespace DDCInteligence
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        private readonly DispatcherTimer _dispatcherTimer;
        private readonly DispatcherTimer _permanentPromptTimer;
        private readonly LogServices _logServices = new LogServices();

        private LayoutAnchorable _anchorable;
        private ImageViewerContent _imageViewerContent;
        private HashSet<Key> disabledKeys;
        public bool IsLogout { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            permanentPromptContainer.Visibility = Visibility.Collapsed;
            permanentPromptMessage.Text = "";

            Messenger.Default.Register<WindowRequestMessage>(this, msg => msg.Callback?.Invoke(this));
            Messenger.Default.Register<DisableKeysMessage>(this, OnDisableKeys);
            Messenger.Default.Register<EnableKeysMessage>(this, OnEnableKeys);
            Messenger.Default.Register<ShowCustomerPromptsMessage>(this, async _ =>
            {

                var customerPromptsAnchorable = dockManager.Layout.Descendents()
                    .OfType<LayoutAnchorable>()
                    .FirstOrDefault(a => a.ContentId == "CustomerPromptsPropertiesWindow");

                if (customerPromptsAnchorable == null || customerPromptsAnchorable.IsVisible)
                    return; // Already visible — do nothing

                await ToggleCustomerPrompts();
            });

            NonClientAreaContent = new NonClientAreaContent();
            Closing += OnClosing;
            PreviewKeyDown += MainWindow_PreviewKeyDown;
            dockManager.SizeChanged += Holder_SizeChanged;
            dockManager.LayoutFloatingWindowControlCreated += DockManager_LayoutFloatingWindowControlCreated;
            dockManager.LayoutFloatingWindowControlClosed += (_, __) => _dispatcherTimer.Stop();

            _dispatcherTimer = CreateTimer(DispatcherTimer_Tick);
            _permanentPromptTimer = CreateTimer(PermanentPromptTimer_Tick, start: true);
            disabledKeys = new HashSet<Key>();

            ViewModelLocator.Instance.Process.BillingEntry = Wrapper;
        }

        #region Timers
        private DispatcherTimer CreateTimer(EventHandler tick, bool start = false) =>
            new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(200) }
                .Also(t => { t.Tick += tick; if (start) t.Start(); });

        private void PermanentPromptTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                var prompt = ViewModelLocator.Instance.Process.CustomerPrompt;
                if (prompt == null)
                {
                    HidePrompt();
                    ViewModelLocator.Instance.Process.ClearPrompt();
                    return;
                }
            }
            catch (Exception ex)
            {
                _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(PermanentPromptTimer_Tick));
                HidePrompt();
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (_anchorable?.IsActive == true &&
                _imageViewerContent?.ImageViewer.Child?.Focused == true)
            {
                _imageViewerContent.ImageViewer.Focus();
            }
        }

        private void ShowPrompt(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                permanentPromptContainer.Visibility = Visibility.Visible;
                permanentPromptMessage.Text = text;
            }
        }

        private void HidePrompt()
        {
            permanentPromptContainer.Visibility = Visibility.Collapsed;
            permanentPromptMessage.Text = "";
        }
        #endregion

        #region Key Handling
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            InitializeMainWindowKeyBindings();
            HookCustomerPromptsVisibility();
        }

        private void InitializeMainWindowKeyBindings()
        {
            var keyBindings = new List<KeyBinding>
            {
                // Keys that should respect the disabledKeys collection
                new SelectiveKeyBinding
                {
                    Key = Key.F1,
                    Modifiers = ModifierKeys.None,
                    Command = ViewModelLocator.Instance.NonClientArea.RequestCommand,
                    CheckDisabledKeys = true,
                    IsKeyDisabled = key => disabledKeys.Contains(key)
                },
                new SelectiveKeyBinding
                {
                    Key = Key.F2,
                    Modifiers = ModifierKeys.None,
                    Command = ViewModelLocator.Instance.NonClientArea.DeAllocateCommand,
                    CheckDisabledKeys = true,
                    IsKeyDisabled = key => disabledKeys.Contains(key)
                },
                new SelectiveKeyBinding
                {
                    Key = Key.F3,
                    Modifiers = ModifierKeys.None,
                    Command = ViewModelLocator.Instance.NonClientArea.SaveCommand,
                    CheckDisabledKeys = true,
                    IsKeyDisabled = key => disabledKeys.Contains(key),
                    CommandParameter = EStatus.BILLED
                },
                new SelectiveKeyBinding
                {
                    Key = Key.F5,
                    Modifiers = ModifierKeys.None,
                    Command = ViewModelLocator.Instance.NonClientArea.SaveCommand,
                    CheckDisabledKeys = true,
                    IsKeyDisabled = key => disabledKeys.Contains(key),
                },
                new SelectiveKeyBinding
                {
                    Key = Key.F11,
                    Modifiers = ModifierKeys.None,
                    Command = new RelayCommand(async () =>
                    {
                       await ToggleCustomerPrompts();
                    }),
                    CheckDisabledKeys = true,
                    IsKeyDisabled = key => disabledKeys.Contains(key),
                },
                new SelectiveKeyBinding
                {
                    Key = Key.F6,
                    Modifiers = ModifierKeys.None,
                    Command = null,
                    CheckDisabledKeys = true,
                    IsKeyDisabled = key => disabledKeys.Contains(key),
                },
                new SelectiveKeyBinding
                {
                    Key = Key.F11,
                    Modifiers = ModifierKeys.Control,
                    Command = ViewModelLocator.Instance.NonClientArea.LogoutCommand,
                    CheckDisabledKeys = true,
                    IsKeyDisabled = key => disabledKeys.Contains(key),
                },
                new KeyBinding { Key = Key.J, Modifiers = ModifierKeys.Control, Command = ViewModelLocator.Instance.NonClientArea.GotoConsigneeCommand },
                new KeyBinding { Key = Key.K, Modifiers = ModifierKeys.Control, Command = ViewModelLocator.Instance.NonClientArea.GotoShipperCommand },
                new KeyBinding { Key = Key.L, Modifiers = ModifierKeys.Control, Command = ViewModelLocator.Instance.NonClientArea.GotoBillToCommand },
                new KeyBinding { Key = Key.M, Modifiers = ModifierKeys.Control, Command = ViewModelLocator.Instance.NonClientArea.GotoRateToCommand },
                new KeyBinding { Key = Key.X, Modifiers = ModifierKeys.Control | ModifierKeys.Alt, Command = ViewModelLocator.Instance.NonClientArea.GotoPOCommand },
                new KeyBinding { Key = Key.Y, Modifiers = ModifierKeys.Control | ModifierKeys.Alt, Command = ViewModelLocator.Instance.NonClientArea.GotoBOLCommand },
                new KeyBinding { Key = Key.Z, Modifiers = ModifierKeys.Control | ModifierKeys.Alt, Command = ViewModelLocator.Instance.NonClientArea.GotoProCommand },
                new KeyBinding { Key = Key.Space, Modifiers = ModifierKeys.Control | ModifierKeys.Control, Command = new RelayCommand(() => ViewModelLocator.Instance.Process.rubberBandService?.GetRubberBand(null)) },
                
                // Image Controls
                new KeyBinding { Command = ViewModelLocator.Instance.ImageViewer.ZoomInCommand, Key = Key.Add, Modifiers = ModifierKeys.Control, CommandParameter = Zooming.IN },
                new KeyBinding { Command = ViewModelLocator.Instance.ImageViewer.ZoomOutCommand, Key = Key.Subtract, Modifiers = ModifierKeys.Control, CommandParameter = Zooming.OUT },
                new KeyBinding { Command = ViewModelLocator.Instance.ImageViewer.ArrowLeftCommand, Key = Key.Left, CommandParameter = Arrows.LEFT, Modifiers = ModifierKeys.Alt },
                new KeyBinding { Command = ViewModelLocator.Instance.ImageViewer.ArrowUpCommand, Key = Key.Up, CommandParameter = Arrows.UP, Modifiers = ModifierKeys.Alt },
                new KeyBinding { Command = ViewModelLocator.Instance.ImageViewer.ArrowRightCommand, Key = Key.Right, CommandParameter = Arrows.RIGHT, Modifiers = ModifierKeys.Alt },
                new KeyBinding { Command = ViewModelLocator.Instance.ImageViewer.ArrowDownCommand, Key = Key.Down, CommandParameter = Arrows.DOWN, Modifiers = ModifierKeys.Alt },
                new KeyBinding { Command = ViewModelLocator.Instance.ImageViewer.NextPageCommand, Key = Key.Right, Modifiers = ModifierKeys.Control, CommandParameter = Paging.NEXT },
                new KeyBinding { Command = ViewModelLocator.Instance.ImageViewer.PreviousPageCommand, Key = Key.Left, Modifiers = ModifierKeys.Control, CommandParameter = Paging.PREVIOUS },
                new KeyBinding { Command = ViewModelLocator.Instance.ImageViewer.SetImageCommand, Key = Key.S, Modifiers = ModifierKeys.Control },
                new KeyBinding { Command = ViewModelLocator.Instance.ImageViewer.BestFitCommand, Key = Key.B, Modifiers = ModifierKeys.Control },

                //new KeyBinding { Command = ViewModelLocator.Instance.ImageViewer.RotateCCommand, Key = Key.PageDown, Modifiers = ModifierKeys.None, CommandParameter = ImageViewerTypes.Rotation.CLOCKWISE },
                //new KeyBinding { Command = ViewModelLocator.Instance.ImageViewer.RotateCCCommand, Key = Key.PageUp, Modifiers = ModifierKeys.None, CommandParameter = ImageViewerTypes.Rotation.COUNTER_CLOCKWISE},

                // Insert new datagrid row
                new KeyBinding { Command = ViewModelLocator.Instance.Items.AddItemCommand, Key = Key.I, Modifiers = ModifierKeys.Control },
                new KeyBinding { Command = ViewModelLocator.Instance.Reference.AddItemCommand, Key = Key.R, Modifiers = ModifierKeys.Control },

                // Hot Keys Menu
                new KeyBinding { Command = ViewModelLocator.Instance.NonClientArea.GotoConsigneeCommand, Key = Key.J, Modifiers = ModifierKeys.Control },
                new KeyBinding { Command = ViewModelLocator.Instance.NonClientArea.GotoShipperCommand, Key = Key.K, Modifiers = ModifierKeys.Control },
                new KeyBinding { Command = ViewModelLocator.Instance.NonClientArea.GotoBillToCommand, Key = Key.L, Modifiers = ModifierKeys.Control },
                new KeyBinding { Command = ViewModelLocator.Instance.NonClientArea.GotoProCommand, Key = Key.Z, Modifiers = ModifierKeys.Control | ModifierKeys.Alt },
                new KeyBinding { Command = ViewModelLocator.Instance.NonClientArea.GotoSpecialInstructionCommand, Key = Key.X, Modifiers = ModifierKeys.Control | ModifierKeys.Alt },
             
                // Open dimensions window
                new KeyBinding { Command = new RelayCommand(() =>
                {
                    ViewModelLocator.Instance.Main.DimensionForm();
                }), Key = Key.D, Modifiers = ModifierKeys.Control },

                new KeyBinding { Command = null, Key = Key.H, Modifiers = ModifierKeys.Control },

                // Breaks and Idle
                new SelectiveKeyBinding
                {
                    Key = Key.F7,
                    Modifiers = ModifierKeys.None,
                    Command = ViewModelLocator.Instance.NonClientArea.IdleBreakCommand,
                    CheckDisabledKeys = true,
                    IsKeyDisabled = key => disabledKeys.Contains(key),
                },
                new SelectiveKeyBinding
                {
                    Key = Key.F8,
                    Modifiers = ModifierKeys.None,
                    Command = ViewModelLocator.Instance.NonClientArea.MealBreakCommand,
                    CheckDisabledKeys = true,
                    IsKeyDisabled = key => disabledKeys.Contains(key),
                },
                new SelectiveKeyBinding
                {
                    Key = Key.F9,
                    Modifiers = ModifierKeys.None,
                    Command = ViewModelLocator.Instance.NonClientArea.NatureBreakCommand,
                    CheckDisabledKeys = true,
                    IsKeyDisabled = key => disabledKeys.Contains(key),
                },
            };
            GlobalShortcut.Init(keyBindings);
        }

        private void OnDisableKeys(DisableKeysMessage message)
        {
            foreach (var key in message.Keys)
                disabledKeys.Add(key);

            InitializeMainWindowKeyBindings();
        }

        private void OnEnableKeys(EnableKeysMessage message)
        {
            foreach (var key in message.Keys)
                disabledKeys.Remove(key);

            InitializeMainWindowKeyBindings();
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Shift)
            {
                e.Handled = true;
                MoveFocusToNextTextBox();
            }
        }

        private void MoveFocusToNextTextBox()
        {
            var current = Keyboard.FocusedElement as UIElement;
            if (current == null) return;

            if (header.IsAncestorOf(current)) FocusControl(shipper);
            else if (shipper.IsAncestorOf(current)) FocusControl(consignee);
            else if (consignee.IsAncestorOf(current)) FocusControl(billto);
            else if (billto.IsAncestorOf(current)) FocusControl(reference, "billto", header);
            else if (reference.IsAncestorOf(current)) FocusControl(billinfo, "reference", header);
            else if (billinfo.IsAncestorOf(current)) FocusControl(reference, "instruction", header, items);
            else if (items.IsAncestorOf(current)) FocusControl(header, "items");
        }

        private void FocusControl(UserControl control, string name = "", UserControl fallback = null, UserControl fallback2 = null)
        {
            try
            {
                UIElement next = null;

                switch (name)
                {
                    case "billto":
                        next = HandleReference(control, fallback);
                        break;

                    case "billinfo":
                        next = control != null ? control.FindName("TextBoxInstruction") as UIElement : null;
                        break;

                    case "items":
                        next = control != null ? control.FindName("Pronumber") as UIElement : null;
                        break;

                    case "instruction":
                        next = HandleInstruction(control, fallback, fallback2);
                        break;

                    case "reference":
                        next = control != null ? control.FindName("txtMasterPro") as UIElement : null;
                        break;

                    default:
                        next = control != null ? control.FindName("TextBoxName") as UIElement : null;
                        break;
                }

                if (next != null)
                {
                    next.Focus();
                    ScrollIntoView(next);
                }
            }
            catch (Exception ex)
            {
                _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(FocusControl));
            }
        }

        /// <summary>
        /// Scrolls the given UIElement into view if it is inside a ScrollViewer.
        /// </summary>
        private void ScrollIntoView(UIElement element)
        {
            try
            {
                var scrollViewer = entry_container;

                if (scrollViewer == null || element == null)
                    return;

                GeneralTransform transform = element.TransformToAncestor(scrollViewer);
                Point elementPosition = transform.Transform(new Point(0, 0));

                double elementTop = elementPosition.Y;
                double elementBottom = elementTop + ((FrameworkElement)element).ActualHeight;

                double viewerHeight = scrollViewer.ViewportHeight;
                double offset = scrollViewer.VerticalOffset;

                scrollViewer.ScrollToVerticalOffset(elementTop - 50);
            }
            catch (Exception ex)
            {
                _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(ScrollIntoView));
            }
        }

        private UIElement HandleInstruction(UserControl control, UserControl fb1, UserControl fb2)
        {
            if (TryFocusGrid(control?.FindName("DgvReference") as DataGrid)) return null;
            if (TryFocusGrid(fb2?.FindName("DgvItems") as DataGrid)) return null;
            return fb1?.FindName("Pronumber") as UIElement;
        }

        private UIElement HandleReference(UserControl control, UserControl fb1)
        {
            if (TryFocusGrid(control?.FindName("DgvItems") as DataGrid)) return null;
            return fb1?.FindName("Pronumber") as UIElement;
        }

        private bool TryFocusGrid(DataGrid grid)
        {
            try
            {
                if (grid == null || grid.Items.Count == 0) return false;

                grid.Focus();
                grid.SelectedIndex = 0;
                grid.CurrentCell = new DataGridCellInfo(grid.Items[0], grid.Columns[0]);
                grid.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        grid.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    }),
                DispatcherPriority.Input);
            }
            catch (Exception ex)
            {
                _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(TryFocusGrid));
            }

            return true;
        }
        #endregion

        #region Docking
        private void DockManager_LayoutFloatingWindowControlCreated(object sender, AvalonDock.LayoutFloatingWindowControlCreatedEventArgs e)
        {
            _anchorable = dockManager.Layout.Descendents()
                .OfType<LayoutAnchorable>()
                .FirstOrDefault(a => a.ContentId == "ImageViewerWindow");

            if (_anchorable?.Content is ContentControl cc &&
                cc.Content is Grid grid &&
                grid.Children.OfType<ImageViewerContent>().FirstOrDefault() is ImageViewerContent ivc)
            {
                _imageViewerContent = _imageViewerContent ?? ivc;
                _dispatcherTimer.Start();
            }

            // Intercept the floating window close button
            e.LayoutFloatingWindowControl.Closing += (s, args) =>
            {
                // Check if this floating window contains Customer Prompts
                var floatingPane = e.LayoutFloatingWindowControl.Model as LayoutAnchorableFloatingWindow;
                if (floatingPane == null) return;

                bool containsCustomerPrompts = floatingPane.Descendents()
                    .OfType<LayoutAnchorable>()
                    .Any(a => a.ContentId == "CustomerPromptsPropertiesWindow");

                if (containsCustomerPrompts)
                {
                    args.Cancel = true; // Block the actual window close

                    // Hide all anchorables inside and dock them back
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var customerPromptsAnchorable = dockManager.Layout.Descendents()
                            .OfType<LayoutAnchorable>()
                            .FirstOrDefault(a => a.ContentId == "CustomerPromptsPropertiesWindow");

                        if (customerPromptsAnchorable != null)
                        {
                            customerPromptsAnchorable.Hide();
                            AdjustDockWidths(false);
                        }
                    }), DispatcherPriority.Background);
                }
            };
        }

        private void HookCustomerPromptsVisibility()
        {
            var customerPromptsAnchorable = dockManager.Layout.Descendents()
                .OfType<LayoutAnchorable>()
                .FirstOrDefault(a => a.ContentId == "CustomerPromptsPropertiesWindow");

            if (customerPromptsAnchorable == null) return;

            customerPromptsAnchorable.IsVisibleChanged += (s, e) =>
            {
                AdjustDockWidths(customerPromptsAnchorable.IsVisible);
            };

            customerPromptsAnchorable.Closed += (s, e) =>
            {
                AdjustDockWidths(false);
            };

            dockManager.LayoutChanged += (s, e) =>
            {
                // Skip during startup — only react after window is fully rendered
                if (!IsLoaded) return;

                var anchorable = dockManager.Layout.Descendents()
                    .OfType<LayoutAnchorable>()
                    .FirstOrDefault(a => a.ContentId == "CustomerPromptsPropertiesWindow");

                bool isVisible = anchorable?.IsVisible ?? false;
                AdjustDockWidths(isVisible);
            };

            // Do NOT call AdjustDockWidths here — let XAML defaults render first
        }

        private void AdjustDockWidths(bool customerPromptsVisible)
        {
            var allPanes = dockManager.Layout.Descendents()
                .OfType<LayoutAnchorablePane>()
                .ToList();

            var documentDetailsPane = allPanes
                .FirstOrDefault(p => p.Children.OfType<LayoutAnchorable>()
                    .Any(a => a.ContentId == "DocumentDetailsWindow"));

            var imageViewerPane = allPanes
                .FirstOrDefault(p => p.Children.OfType<LayoutAnchorable>()
                    .Any(a => a.ContentId == "ImageViewerWindow"));

            if (customerPromptsVisible)
            {
                if (documentDetailsPane != null)
                    documentDetailsPane.DockWidth = new GridLength(48, GridUnitType.Star);
                if (imageViewerPane != null)
                    imageViewerPane.DockWidth = new GridLength(34, GridUnitType.Star);
            }
            else
            {
                if (documentDetailsPane != null)
                    documentDetailsPane.DockWidth = new GridLength(48, GridUnitType.Star);
                if (imageViewerPane != null)
                    imageViewerPane.DockWidth = new GridLength(50, GridUnitType.Star);
            }
        }
        public async Task ToggleCustomerPrompts()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var customerPromptsAnchorable = dockManager.Layout.Descendents()
                    .OfType<LayoutAnchorable>()
                    .FirstOrDefault(a => a.ContentId == "CustomerPromptsPropertiesWindow");

                if (customerPromptsAnchorable == null) return;

                if (customerPromptsAnchorable.IsVisible)
                {
                    customerPromptsAnchorable.Hide();
                    AdjustDockWidths(false);
                }
                else
                {
                    customerPromptsAnchorable.Show();
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => AdjustDockWidths(true)));
                }
            });
        }
        #endregion

        #region Closing
        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (IsLogout || ViewModelLocator.Instance.Process.CurrentState != EState.IDLE)
            {
                e.Cancel = !IsLogout;
                return;
            }

            var alert = new SweetAlert
            {
                Caption = "Exit Application?",
                Message = "You won't be able to revert this!",
                MsgButton = SweetAlertButton.YesNo,
                MsgImage = SweetAlertImage.QUESTION,
                OkText = "Yes, logout it!",
                CancelText = "Cancel",
                ShowInTaskbar = false
            };

            if (alert.ShowDialog() != SweetAlertResult.OK)
                e.Cancel = true;
            else
                System.Windows.Forms.Application.ExitThread();
        }
        #endregion

        #region Resizing
        private double _shipperWidth, _consigneeWidth, _billtoWidth, _expanderWidth2;
        public double ShipperWidth { get => _shipperWidth; set => Set(ref _shipperWidth, value); }
        public double ConsigneeWidth { get => _consigneeWidth; set => Set(ref _consigneeWidth, value); }
        public double BillToWidth { get => _billtoWidth; set => Set(ref _billtoWidth, value); }
        public double ExpanderWidth2 { get => _expanderWidth2; set => Set(ref _expanderWidth2, value); }

        private void Holder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                double totalAvailableWidth = dockManager.ActualWidth - 100;
                double minControlWidth = totalAvailableWidth / 6;
                double minControlWidth2 = (dockManager.ActualWidth - 300) / 3;

                int numberOfColumns = Math.Max(6, Math.Min(4, (int)(totalAvailableWidth / minControlWidth)));
                int numberOfColumns2 = Math.Min(2, (int)(totalAvailableWidth / minControlWidth2));

                ExpanderWidth2 = (totalAvailableWidth / numberOfColumns2) + 5;
                if (e.NewSize.Width >= totalAvailableWidth)
                {
                    numberOfColumns = Math.Min(3, (int)(totalAvailableWidth / (totalAvailableWidth / 3)));
                }
                double calculatedWidth = totalAvailableWidth / numberOfColumns;
                if (numberOfColumns == 3)
                {
                    ExpanderWidth2 = (totalAvailableWidth / numberOfColumns2) + 3;
                }

                ShipperWidth = calculatedWidth;
                ConsigneeWidth = calculatedWidth;
                BillToWidth = calculatedWidth;
            }
            catch (Exception ex)
            {
                _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(Holder_SizeChanged));
            }
        }
        #endregion

        #region Helpers
        public event PropertyChangedEventHandler PropertyChanged;
        private void Set<T>(ref T field, T value, string name = null)
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name ?? ""));
        }
        #endregion
    }

    internal static class Ext
    {
        public static T Also<T>(this T obj, Action<T> act) { act(obj); return obj; }
    }
}