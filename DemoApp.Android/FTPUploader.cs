using System;
using System.IO;
using System.Net;
using Acr.UserDialogs;
using DemoApp.Droid;
using DemoApp.Interfaces;
[assembly: Xamarin.Forms.Dependency(typeof(FTPUploader))]
namespace DemoApp.Droid
{
    public class FTPUploader: IFtpUploader
    {
        public FTPUploader()
        {
        }

        public string upload(string FtpUrl, string fileName, string userName, string password, string UploadDirectory = "")
        {
            try
            {
              
                
                string PureFileName = new FileInfo(fileName).Name;
                String uploadUrl = String.Format("{0}{1}/{2}", FtpUrl, UploadDirectory, PureFileName);
                FtpWebRequest req = (FtpWebRequest)FtpWebRequest.Create(uploadUrl);
                req.Proxy = null;
                req.Method = WebRequestMethods.Ftp.UploadFile;
                req.Credentials = new NetworkCredential(userName, password);
                req.UseBinary = true;
                req.UsePassive = true;
                byte[] data = File.ReadAllBytes(fileName);
                req.ContentLength = data.Length;
                Stream stream = req.GetRequestStream();
                stream.Write(data, 0, data.Length);
                stream.Close();
                FtpWebResponse res = (FtpWebResponse)req.GetResponse();
                
                return res.StatusDescription;

            }
            catch (Exception err)
            {
                UserDialogs.Instance.HideLoading();
                return err.ToString();
            }
        }
    }
}
