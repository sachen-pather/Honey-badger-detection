namespace Honey_badger_detection.Services
{
    public interface IDropboxService
    {
        Task<string> UploadImageAsync(Stream imageStream, string fileName);
    }
}
