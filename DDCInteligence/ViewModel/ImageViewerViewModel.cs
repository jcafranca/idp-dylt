using DDCInteligence.DataTypes;
using DDCInteligence.Models;
using DDCInteligence.View;
using DotNetCS.ImageViewer;
using GalaSoft.MvvmLight.Command;
using SweetAlertSharp.Enums;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using static DDCInteligence.DataTypes.ImageViewerTypes;

namespace DDCInteligence.ViewModel
{
    public class ImageViewerViewModel : ImageViewerModel
    {

        public ImageViewerViewModel(IProcess process) : base(process) 
        {
            this.pictureBox = new PictureBox();
            this.OpenImageCommand = new RelayCommand(OpenImage);
            this.ZoomInCommand = new RelayCommand<Zooming>(Zoom);
            this.ZoomOutCommand = new RelayCommand<Zooming>(Zoom);
           
            this.ArrowLeftCommand = new RelayCommand<Arrows>(Arrow);
            this.ArrowUpCommand = new RelayCommand<Arrows>(Arrow);
            this.ArrowRightCommand = new RelayCommand<Arrows>(Arrow);
            this.ArrowDownCommand = new RelayCommand<Arrows>(Arrow);
            this.NextPageCommand = new RelayCommand<Paging>(Page);
            this.PreviousPageCommand = new RelayCommand<Paging>(Page);
            this.SetImageCommand = new RelayCommand(SetImage);
            this.BestFitCommand = new RelayCommand(BestFit);
        }
        void Arrow(Arrows arrows)
        {
            if (this.pictureBox.HasImage())
            {
                switch (arrows)
                {
                    case Arrows.LEFT:
                        this.pictureBox.ScrollX(-35);
                        break;
                    case Arrows.UP:
                        this.pictureBox.ScrollY(-35);
                        break;
                    case Arrows.RIGHT:
                        this.pictureBox.ScrollX(35);
                        break;
                    case Arrows.DOWN:
                        this.pictureBox.ScrollY(35);
                        break; ;
                }
            }
        }
        void Page(Paging paging) 
        {
            if (this.pictureBox.HasImage())
            {
                switch (paging)
                {
                    case Paging.NEXT:
                        if (this.pictureBox.PageNumber ==this.pictureBox.GetTotalPage()) break;
                        this.pictureBox.PageNumber += 1;
                        break;
                    case Paging.PREVIOUS:
                        if (this.pictureBox.PageNumber == 1) break;
                        this.pictureBox.PageNumber -= 1;
                        break;
                }
                this.CurrentPage = this.pictureBox.PageNumber;
            }
        }
        void Zoom(Zooming zoom) 
        {
            try
            {
                if (this.pictureBox.HasImage())
                {
                    switch (zoom)
                    {
                        case (Zooming.IN):
                            this.pictureBox.ZoomFactor += 0.05d;
                            break;
                        case (Zooming.OUT):
                            this.pictureBox.ZoomFactor -= 0.05d;
                            break;
                    }
                }
            } catch { }
        }
        void OpenImage() 
        {
            var image_path = ViewModelLocator.Instance.Process.ImagePath;
            if (!string.IsNullOrEmpty(image_path)  && System.IO.File.Exists(image_path))
            {
                Process.Start("explorer.exe", "/select," + ViewModelLocator.Instance.Process.ImagePath);
            }
        }
        void SetImage() 
        {
            if (this.pictureBox.HasImage())
            {
                var input = new InputBoxContent(Application.Current.MainWindow);
                if (input.ShowDialog() == SweetAlertResult.OK)
                {
                    Task.Factory.StartNew(() => {
                        int.TryParse(this.InputValue, out int value);
                        if (this.pictureBox.GetTotalPage() >= value && this.pictureBox.PageNumber != value && value > 0)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                this.pictureBox.PageNumber = (short)value;
                                this.pictureBox.ZoomToFit(ZoomFit.BestFit);
                                this.CurrentPage = value;
                            });
                        }
                    }).ConfigureAwait(true);
                }
            }
        }
        void BestFit() 
        {
            if (this.pictureBox.HasImage())
            {
                this.pictureBox.ZoomToFit(ZoomFit.BestFit);
            }
        }
    }
}
