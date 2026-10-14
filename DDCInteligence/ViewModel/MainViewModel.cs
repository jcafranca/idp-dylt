using AvalonDock.Themes;
using DDCInteligence.DataTypes;
using DDCInteligence.Models;
using DDCInteligence.Utilities.Events;
using DDCInteligence.View;
using DDCInteligence.View.UserControl;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using static DDCInteligence.DataTypes.DatabaseTypes;
using static DDCInteligence.DataTypes.KeysInfo;
using static DotNetCS.OCR;

namespace DDCInteligence.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : MainModel
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        private readonly DispatcherTimer dispatcher;
        private SharedEvent sharedEvent;
        public ICommand InsertCommand { get; }
        public ICommand DeleteCommand { get; }

        public MainViewModel()
        {
            this.AvalonDockTheme = new Vs2013LightTheme();
            this.IsRequesting = false;
            this.dispatcher = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, Dispatcher_Tick, Application.Current.Dispatcher) { IsEnabled = true };
            sharedEvent = ViewModelLocator.Instance.SharedEvent;
            InsertCommand = new RelayCommand(new Action(() => {
                if (sharedEvent.SelectedDataGrid != null)
                {
                    var selectedDataGrid = sharedEvent.SelectedDataGrid;
                    switch (selectedDataGrid.Name)
                    {
                        case "DgvItems":
                            ViewModelLocator.Instance.Items.AddItemCommand.Execute(null);
                            break;
                        case "DgvReference":
                            ViewModelLocator.Instance.Reference.AddItemCommand.Execute(null);
                            break;
                    }
                }
            }));
            DeleteCommand = new RelayCommand(new Action(() => {
                if (sharedEvent.SelectedDataGrid != null)
                {
                    var selectedDataGrid = sharedEvent.SelectedDataGrid;
                    int selected_index = selectedDataGrid.SelectedIndex;
                    object selected_item = selectedDataGrid.SelectedItem;
                    switch (selectedDataGrid.Name)
                    {
                        case "DgvItems":
                            ViewModelLocator.Instance.Items.RemoveItemCommand.Execute((ItemsInfo)selectedDataGrid.SelectedItem);                            
                            break;
                        case "DgvReference":
                            ViewModelLocator.Instance.Reference.RemoveItemCommand.Execute((ItemsInfo)selectedDataGrid.SelectedItem);
                            break;
                    }
                    ValidateItemDelete(selectedDataGrid, selected_index, selected_item);
                }
            }));
        }
        void ValidateItemDelete(DataGrid dataGrid, int currentRowIndex, object currentItem)
        {
            dataGrid.Focus();
            if (currentRowIndex > dataGrid.Items.Count - 1)
            {
                dataGrid.SelectedIndex = dataGrid.Items.Count - 1;
                if (dataGrid.Items.Count > 0)
                    dataGrid.CurrentCell = new DataGridCellInfo(dataGrid.Items[dataGrid.Items.Count - 1], dataGrid.Columns[0]);
            }
            else
            {
                dataGrid.SelectedIndex = currentRowIndex;
                dataGrid.CurrentCell = new DataGridCellInfo(dataGrid.Items[currentRowIndex], dataGrid.Columns[0]);
            }
            return;
        }
        public void DimensionForm()
        {

            Messenger.Default.Send(new DisableKeysMessage(Key.F1, Key.F2, Key.F3, Key.F5, Key.F11));
            DimensionsView openWindow = System.Windows.Application.Current.Windows
                                                           .OfType<DimensionsView>()
                                                           .FirstOrDefault();

            if (openWindow == null)  // If the window is not already open
            {
                DimensionsView dimensions = new DimensionsView(Application.Current.MainWindow);
                dimensions.Closed += (s, e) =>
                {
                    // Re-enable keys when window closes
                    Messenger.Default.Send(new EnableKeysMessage(Key.F1, Key.F2, Key.F3, Key.F5, Key.F11));
                };
                dimensions.Show();  // Show the window
            }
            else
            {
                // If the window is already open, bring it to the front
                if (openWindow.WindowState == WindowState.Minimized)
                    openWindow.WindowState = WindowState.Normal;  // Restore if minimized
                openWindow.Activate();  // Bring the window to the front
            }
        }
        private void Dispatcher_Tick(object sender, EventArgs e)
        {
            this.ComputerTime = DateTime.Now;
        }
    }
}