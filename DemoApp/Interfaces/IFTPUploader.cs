using System;
namespace DemoApp.Interfaces
{
    public interface IFtpUploader
    {
        string upload(string FtpUrl, string fileName, string userName, string password, string UploadDirectory = "");
    }
}
