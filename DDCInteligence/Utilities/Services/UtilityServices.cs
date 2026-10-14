using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace DDCInteligence.Utilities.Services
{
    public class UtilityServices
    {
        public string SecureString2String(SecureString secure)
        {
            IntPtr temp_string = IntPtr.Zero;
            try
            {
                temp_string = Marshal.SecureStringToGlobalAllocUnicode(secure);
                return Marshal.PtrToStringUni(temp_string);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(temp_string);
            }
        }
        public SecureString String2SecureString(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            SecureString secureString = new SecureString();
            input.ToCharArray().ToList().ForEach(c => secureString.AppendChar(c));
            secureString.MakeReadOnly();
            return secureString;
        }
        public void TakeScreenShot(string directory_path, string pro, DateTime endtime, string username)
        {
            Task.Factory.StartNew(() =>
            {
                int screenLeft = System.Windows.Forms.SystemInformation.VirtualScreen.Left;
                int screenTop = System.Windows.Forms.SystemInformation.VirtualScreen.Top;
                int screenWidth = System.Windows.Forms.SystemInformation.VirtualScreen.Width;
                int screenHeight = System.Windows.Forms.SystemInformation.VirtualScreen.Height;

                using (Bitmap capture = new Bitmap(screenWidth, screenHeight))
                {
                    using (Graphics graphics = Graphics.FromImage(capture))
                    {
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                        graphics.CopyFromScreen(screenLeft, screenTop, 0, 0, capture.Size);
                    }

                    ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                    Encoder myEncoder = Encoder.Quality;
                    EncoderParameters myEncoderParameters = new EncoderParameters(1);
                    EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 75L);
                    myEncoderParameters.Param[0] = myEncoderParameter;

                    if (!Directory.Exists(directory_path)) Directory.CreateDirectory(directory_path);
                    if (endtime == null) endtime = DateTime.Now;
                    DateTime endt = DateTime.Parse(endtime.ToString());
                    string filename = Path.Combine(directory_path, $"{endt.ToString("yyyyMMdd")}_{endt.ToString("HHmmss")}_{pro}_{username}.jpeg");
                    capture.Save(filename, jpgEncoder, myEncoderParameters);
                }

            }).ConfigureAwait(true);
        }
        private ImageCodecInfo GetEncoder(ImageFormat imageFormat)
        {
            ImageCodecInfo[] imageCodecInfos = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in imageCodecInfos)
            {
                if (codec.FormatID == imageFormat.Guid)
                    return codec;
            }
            return null;
        }
        public string FormatText(string str)
        {
            if (!string.IsNullOrEmpty(str)) return Regex.Replace(str, "\\s{2,}|,{2,}|\\r\\n?|\\n", " ");
            return string.Empty;
        }
        public string ConvertKiloToPound(string kilo)
        {
            decimal ret = 0;
            decimal kilograms;
            decimal pounds;
            decimal value_;

            if (!string.IsNullOrEmpty(kilo) && decimal.TryParse(kilo, out value_) && value_ > 0)
            {
                kilograms = value_;
                pounds = kilograms * 2.20462262m;
                ret = Math.Round(pounds);
            }

            return ret.ToString();
        }
        public void SetWindowOnScreen(Window window, int left = 0, int top = 0)
        {
            // Default to primary screen
            System.Windows.Forms.Screen currentScreen;

            if (window.Owner != null)
            {
                // Get screen where owner is located
                var ownerInteropHelper = new System.Windows.Interop.WindowInteropHelper(window.Owner);
                var ownerScreenHandle = ownerInteropHelper.Handle;
                currentScreen = System.Windows.Forms.Screen.FromHandle(ownerScreenHandle);
            }
            else
            {
                // Fallback if no owner is set
                var thisInteropHelper = new System.Windows.Interop.WindowInteropHelper(window);
                currentScreen = System.Windows.Forms.Screen.FromHandle(thisInteropHelper.Handle);
            }

            var workingArea = currentScreen.WorkingArea;
            window.Left = workingArea.Left + (left > 0 ? left : (workingArea.Width - window.Width) / 2);
            window.Top = workingArea.Top + (top > 0 ? top : (workingArea.Height - window.ActualHeight) / 2);
        }
    }
}
