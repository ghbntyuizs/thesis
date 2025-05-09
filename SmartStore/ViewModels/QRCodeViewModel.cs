using Microsoft.Win32;
using SmartStorePOS.Helpers;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace SmartStorePOS.ViewModels
{
    public class QRCodeViewModel : ViewModelBase
    {
        private string _title = "Mã QR Thanh toán";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _qrCodeText = "";
        public string QRCodeText
        {
            get => _qrCodeText;
            set => SetProperty(ref _qrCodeText, value);
        }

        private ImageSource _qrCodeImage;
        public ImageSource QRCodeImage
        {
            get => _qrCodeImage;
            set => SetProperty(ref _qrCodeImage, value);
        }

        private bool _isImageGenerated;
        public bool IsImageGenerated
        {
            get => _isImageGenerated;
            set => SetProperty(ref _isImageGenerated, value);
        }

        public ICommand GenerateQRCodeCommand { get; }
        public ICommand SaveImageCommand { get; }
        public ICommand CloseCommand { get; }

        public Action CloseAction { get; set; }

        public QRCodeViewModel()
        {
            GenerateQRCodeCommand = new RelayCommand(_ => GenerateQRCode(), _ => !string.IsNullOrWhiteSpace(QRCodeText));
            SaveImageCommand = new RelayCommand(_ => SaveQRCodeImage(), _ => IsImageGenerated);
            CloseCommand = new RelayCommand(_ => CloseAction?.Invoke());
        }

        public QRCodeViewModel(string initialText) : this()
        {
            QRCodeText = initialText;
            if (!string.IsNullOrWhiteSpace(initialText))
            {
                GenerateQRCode();
            }
        }

        private void GenerateQRCode()
        {
            try
            {
                var barcodeWriter = new BarcodeWriterPixelData
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new QrCodeEncodingOptions
                    {
                        Width = 300,
                        Height = 300,
                        Margin = 10,
                        ErrorCorrection = ErrorCorrectionLevel.M
                    }
                };

                var pixelData = barcodeWriter.Write(QRCodeText);

                // Create a bitmap from the pixel data
                var bitmap = new WriteableBitmap(
                    pixelData.Width,
                    pixelData.Height,
                    96, // DPI X
                    96, // DPI Y
                    PixelFormats.Bgr32,
                    null);

                // Copy the pixel data to the bitmap
                bitmap.WritePixels(
                    new Int32Rect(0, 0, pixelData.Width, pixelData.Height),
                    pixelData.Pixels,
                    pixelData.Width * 4, // stride
                    0);

                QRCodeImage = bitmap;
                IsImageGenerated = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating QR code: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                IsImageGenerated = false;
            }
        }

        private void SaveQRCodeImage()
        {
            if (QRCodeImage == null) return;

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp",
                FileName = "QRCode"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var encoder = new PngBitmapEncoder();
                    var bitmap = QRCodeImage as BitmapSource;

                    encoder.Frames.Add(BitmapFrame.Create(bitmap));

                    using (var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }

                    MessageBox.Show("QR Code saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving QR code: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
