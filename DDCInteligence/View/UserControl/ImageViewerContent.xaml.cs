using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Helpers;
using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using DotNetCS;
using DotNetCS.ImageViewer;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Threading;

namespace DDCInteligence.View.UserControl
{
    /// <summary>
    /// Interaction logic for ImageViewerContent.xaml
    /// </summary>
    public partial class ImageViewerContent : IDisposable
    {
        private readonly Messenger _messenger;
        private delegate void SetPathDelegate(string path);
        private readonly LogServices logServices = new LogServices();
        private IRubberBand rubberBand => ViewModelLocator.Instance?.Process?.rubberBandService;
        public ICommand RubberBandCommand { get; }
        public Action FinishInteraction { get; set; }
        public ImageViewerContent()
        {
            InitializeComponent();
            var dataContext = (ImageViewerViewModel)DataContext;

            _messenger = new Messenger();
            _messenger.Register<RubberBandFetched>(this, rubberBand.GetRubberBand);
            RubberBandCommand = new RelayCommand<RubberBandFetched>(rubberBand.GetRubberBand);

            InitializePictureBox();
            ImageViewer.Child = ViewModelLocator.Instance.ImageViewer.pictureBox;
            dataContext.ImagePathChanged += DataContext_ImagePathChanged;
            dataContext.OCRPathChanged += DataContext_OCRPathChanged;

            this.KeyUp += ImageViewerContent_KeyUp;
            this.KeyDown += ImageViewerContent_KeyDown;
        }

        private void ImageViewerContent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl && e.IsDown)
            {
                if (ViewModelLocator.Instance.ImageViewer.pictureBox.MouseDragging == MouseDrag.ImagePosition)
                {
                    ViewModelLocator.Instance.ImageViewer.pictureBox.MouseDragging = MouseDrag.RubberBand;
                }
            }
        }
        private void ImageViewerContent_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl && e.IsUp)
            {
                if (ViewModelLocator.Instance.ImageViewer.pictureBox.MouseDragging == MouseDrag.RubberBand)
                {
                    ViewModelLocator.Instance.ImageViewer.pictureBox.MouseDragging = MouseDrag.ImagePosition;
                }
            }
        }
        private void UpdateImagePath()
        {
            if (!string.IsNullOrEmpty(ViewModelLocator.Instance.Process.ImagePath) && System.IO.File.Exists(ViewModelLocator.Instance.Process.ImagePath))
            {
                ViewModelLocator.Instance.ImageViewer.pictureBox.ImagePath = ViewModelLocator.Instance.Process.ImagePath;
                ViewModelLocator.Instance.ImageViewer.pictureBox.ZoomToFit(ZoomFit.Width);

                ViewModelLocator.Instance.ImageViewer.CurrentPage = ViewModelLocator.Instance.ImageViewer.pictureBox.PageNumber;
                ViewModelLocator.Instance.ImageViewer.TotalPage = ViewModelLocator.Instance.ImageViewer.pictureBox.GetTotalPage();
            }
            else
            {
                ViewModelLocator.Instance.ImageViewer.pictureBox.ClearImage();
                ViewModelLocator.Instance.ImageViewer.CurrentPage = 0;
                ViewModelLocator.Instance.ImageViewer.TotalPage = 0;
            }
        }
        private void DataContext_OCRPathChanged(object sender, Utilities.Events.PathChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Path) && System.IO.File.Exists(e.Path))
            {
                ViewModelLocator.Instance.ImageViewer.pictureBox.Document = DotNetCS.OCR.Open(e.Path);
            }
        }
        private void DataContext_ImagePathChanged(object sender, Utilities.Events.PathChangedEventArgs e)
        {
            var d = new SetPathDelegate(SetImagePath);
            Dispatcher.Invoke(d, e.Path);
        }
        void SetImagePath(string path)
        {
            UpdateImagePath();
        }

        public void InitializePictureBox()
        {
            OCRToolTip ocrToolTip = new OCRToolTip()
            {
                BackColor = System.Drawing.Color.FromArgb(255, 192, 128),
                Font = new System.Drawing.Font("Consolas", 8.25f),
                ForeColor = System.Drawing.Color.Black,
                ShowConfidence = true,
                Visible = true
            };
            ImageChange imageChange = new ImageChange();
            RubberBand rubberBand = new RubberBand()
            {
                BackColor = System.Drawing.Color.FromArgb(75, 255, 255, 1),
                BorderColor = System.Drawing.Color.Black,
                BorderStyle = System.Windows.Forms.ButtonBorderStyle.Dotted,
                ContextMenu = null
            };
            Ruler ruler = new Ruler()
            {
                BackColor = System.Drawing.Color.Empty,
                Height = 7,
                MoveFactor = 27,
                Visible = false
            };
            ViewModelLocator.Instance.ImageViewer.pictureBox = new PictureBox()
            {
                BackColor = System.Drawing.SystemColors.ControlDarkDark,
                Dock = System.Windows.Forms.DockStyle.Fill,
                HighlightColor = System.Drawing.Color.Empty,
                OCRToolTip = ocrToolTip,
                OnImageChange = imageChange,
                RubberBand = rubberBand,
                Ruler = ruler,
                Document = new OCR.Document()
            };

            //ViewModelLocator.Instance.ImageViewer.pictureBox.Init();
            ViewModelLocator.Instance.ImageViewer.pictureBox.RubberBand.BackColor = System.Drawing.Color.FromArgb(75, 255, 255, 1);
            ViewModelLocator.Instance.ImageViewer.pictureBox.RubberBandDoubleClick += PictureBoxRubberBand;
        }
        private void PictureBoxRubberBand(object sender, EventArgs e)
        {
            this.rubberBand.GetRubberBand(null);
        }
        public void Dispose()
        {
            _messenger.Unregister<RubberBandFetched>(this);
            ViewModelLocator.Instance.ImageViewer.pictureBox?.Dispose();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            string path = ViewModelLocator.Instance.Process.ImagePath;
            if (Directory.Exists(path))
            {
                System.Diagnostics.Process.Start("explorer.exe", path);
            }
        }
    }
}
