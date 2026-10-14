using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Events;
using DDCInteligence.Utilities.Helpers;
using DDCInteligence.ViewModel;
using DotNetCS;
using DotNetCS.ImageViewer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DDCInteligence.Models
{
    public class ImageViewerModel : INotifyPropertyChanged
    {
        protected IProcess process;
        public event EventHandler<PathChangedEventArgs> ImagePathChanged;
        public event EventHandler<PathChangedEventArgs> OCRPathChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        private PictureBox _pictureBox;
        public PictureBox pictureBox
        {
            get => _pictureBox;
            set
            {
                _pictureBox = value;
                RaisePropertyChanged(nameof(PictureBox));
            }
        }
        public string ImageFullPath => ViewModelLocator.Instance.Process.ImagePath;
        public string OCRFullPath => ViewModelLocator.Instance.Process.OCRPath;

        private int _totalPage;
        public int TotalPage
        {
            get => _totalPage;
            set
            {
                _totalPage = value;
                RaisePropertyChanged(nameof(TotalPage));
            }
        }
        private int _currentPage;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                RaisePropertyChanged(nameof(CurrentPage));
            }
        }
        private string _input_value;
        public string InputValue
        {
            get => _input_value;
            set
            {
                _input_value = value;
                RaisePropertyChanged(nameof(InputValue));
            }
        }

        public ICommand OpenImageCommand { get; set; }
        public ICommand ZoomInCommand { get; set; }
        public ICommand ZoomOutCommand { get; set; }
        public ICommand ArrowLeftCommand { get; set; }
        public ICommand ArrowUpCommand { get; set; }
        public ICommand ArrowRightCommand { get; set; }
        public ICommand ArrowDownCommand { get; set; }
        public ICommand NextPageCommand { get; set; }
        public ICommand PreviousPageCommand { get; set; }
        public ICommand SetImageCommand { get; set; }
        public ICommand BestFitCommand { get; set; }
        public ImageViewerModel(IProcess process) 
        {
            this.process = process;
            this.process.PropertyChanged += Process_PropertyChanged;

            OnImagePathChanged(new PathChangedEventArgs(ImageFullPath));
            OnOcrPathChanged(new PathChangedEventArgs(OCRFullPath));
        }

        private void Process_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModelLocator.Instance.Process.ImagePath):
                    OnImagePathChanged(new PathChangedEventArgs(ViewModelLocator.Instance.Process.ImagePath));
                    break;
                case nameof(ViewModelLocator.Instance.Process.OCRPath):
                    OnOcrPathChanged(new PathChangedEventArgs(ViewModelLocator.Instance.Process.OCRPath));
                    break;
            }
            RaisePropertyChanged(null);
        }
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void OnImagePathChanged(PathChangedEventArgs e)
        {
            ImagePathChanged?.Invoke(this, e);
        }
        private void OnOcrPathChanged(PathChangedEventArgs e)
        {
            OCRPathChanged?.Invoke(this, e);
        }
        
    }
}
