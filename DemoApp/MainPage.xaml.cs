using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Acr.UserDialogs;
using DemoApp.Interfaces;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace DemoApp
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage,INotifyPropertyChanged
    {
        public String ScannedText { get; set; }
        MediaFile _file;
        public MainPage()
        {
            InitializeComponent();
            this.BindingContext = this;
        }

        async void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "Sample",
                Name = "test.jpg"
            });
            
            if (file == null)
                return;

            await DisplayAlert("File Location", file.Path, "OK");

            image.Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                return stream;
            });
            _file = file;
            _fileInfo = new FileInfo(_file.Path);
            
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        FileInfo _fileInfo;
        async void Button_Clicked_1(System.Object sender, System.EventArgs e)
        {
            try
            {
                // rename here
                if (ScannedText == "")
                {
                    await DisplayAlert("Error", "Please write something to rename.", "OK");
                    return;
                }
                if (_file == null || String.IsNullOrEmpty(_file.Path))
                {
                    await DisplayAlert("Error", "No image found to rename.", "OK");
                    return;
                }
                _fileInfo = new FileInfo(_file.Path);
                _fileInfo.Rename(ScannedText);
                await DisplayAlert("File Location", _fileInfo.FullName, "OK");
            }
            catch(Exception ex)
            {
                await DisplayAlert("File Location", ex.Message, "OK");
            }
            
        }

        async void Button_Clicked_2(System.Object sender, System.EventArgs e)
        {
            if(_fileInfo == null || string.IsNullOrEmpty(_fileInfo.FullName))
            {
                await DisplayAlert("Error", "No image found to upload.", "OK");
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    UserDialogs.Instance.ShowLoading("Uploading");
                });
                await Task.Run(() =>  DependencyService.Get<IFtpUploader>().upload(Constants.SERVER_URL, _fileInfo.FullName, Constants.SERVER_USERNAME, Constants.SERVER_PASSWORD, ""));
                Device.BeginInvokeOnMainThread(() =>
                {
                    UserDialogs.Instance.HideLoading();
                });
            }                
        }

        async void barcode_Clicked(System.Object sender, System.EventArgs e)
        {
            var ScannerPage = new ZXingScannerPage();
            

            ScannerPage.OnScanResult +=  (result) =>
            {
                 // Stop scanning
                //ScannerPage.IsScanning = false;
                var url = result.Text;
                var QRId = Regex.Match(url, @"\d+").Value;

                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PopAsync();
                });
                
                ScannedText = QRId;
                NotifyPropertyChanged("ScannedText");
                _fileInfo = new FileInfo(_file.Path);
                
            };

            await Navigation.PushAsync(ScannerPage);
        }

        async void delete_Clicked(System.Object sender, System.EventArgs e)
        {
            if(_file != null && _file.Path != "")
            {
                _fileInfo = new FileInfo(_file.Path);
                _fileInfo.Delete();
                await DisplayAlert("File Delete", _fileInfo.FullName, "OK");
                image.Source = null;
            }
            else
            {
                await DisplayAlert("Stop", "No image to delete.", "OK");
            }
            
        }
    }

    public static class ExtendedMethod
    {
        public static void Rename(this FileInfo fileInfo, string newName)
        {
            
                fileInfo.MoveTo(fileInfo.Directory.FullName + "/" + newName + ".jpg");
            
            
           
        }
    }
}
