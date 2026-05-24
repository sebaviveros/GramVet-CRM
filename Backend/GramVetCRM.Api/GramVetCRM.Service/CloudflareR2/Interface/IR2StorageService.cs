namespace GramVetCRM.Service
{
    public interface IR2StorageService
    {
        Task<string?> SubirArchivo(Stream fileStream, string fileName, string contentType);
        Task EliminarArchivo(string fileName);
    }
}