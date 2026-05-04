// Service/Abstract/IAmasPhotoService.cs

namespace ForQab.Service.Abstract
{
    public interface IAmasPhotoService
    {
        /// <summary>
        /// AMAS SOAP servisindən FIN, SerialPrefix və Serial əsasında şəkil alır.
        /// Şəkil tapılmadıqda null qaytarır.
        /// </summary>
        Task<string?> FetchPhotoAsBase64Async(string fin, string serialPrefix, string serial);
    }
}
