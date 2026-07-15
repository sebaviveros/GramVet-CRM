using GramVetCRM.Model;

namespace GramVetCRM.Repository.Repositories
{
    public interface IMensajeRepository
    {
        Task<List<Mensaje>> GetByConversacion(int conversacionId, int page, int pageSize);
        // Fecha del último mensaje ENTRANTE (inbound) por conversación → alimenta la ventana de 24h.
        Task<Dictionary<int, DateTime>> GetUltimoEntrantePorConversaciones(List<int> conversacionIds);
        Task<Mensaje?> GetById(int id);
        Task<Mensaje?> GetByExternalId(string externalId);
        Task Add(Mensaje mensaje);
        Task Save();
    }
}