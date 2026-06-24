using GramVetCRM.Model;
using GramVetCRM.Model.DTOs.Mascota;
using GramVetCRM.Repository.Repositories;

namespace GramVetCRM.Service
{
    public class MascotaFotoService : IMascotaFotoService
    {
        private readonly IMascotaFotoRepository _repo;
        private readonly IR2StorageService _r2;

        public MascotaFotoService(IMascotaFotoRepository repo, IR2StorageService r2)
        {
            _repo = repo;
            _r2 = r2;
        }

        public async Task<List<MascotaFotoDto>> GetByMascota(int mascotaId)
        {
            var fotos = await _repo.GetByMascota(mascotaId);
            return fotos.Select(ToDto).ToList();
        }

        public async Task<MascotaFotoDto?> Subir(int mascotaId, Stream archivo, string nombreOriginal, string contentType, string? descripcion, int usuarioId)
        {
            var ext = Path.GetExtension(nombreOriginal);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";
            var fileName = $"mascota/{Guid.NewGuid()}{ext}";

            var url = await _r2.SubirArchivo(archivo, fileName, contentType);
            if (url == null) return null;

            var foto = new MascotaFoto
            {
                MascotaId = mascotaId,
                Url = url,
                Descripcion = descripcion,
                Usercr = usuarioId.ToString(),
                Fechacr = DateTime.Now,
                Active = true
            };

            await _repo.Add(foto);
            await _repo.Save();
            return ToDto(foto);
        }

        public async Task Eliminar(int id, int usuarioId)
        {
            var foto = await _repo.GetById(id)
                ?? throw new Exception($"Foto {id} no encontrada");

            // Borrar el objeto en R2 (la key es la ruta de la URL pública)
            try
            {
                var key = new Uri(foto.Url).AbsolutePath.TrimStart('/');
                await _r2.EliminarArchivo(key);
            }
            catch { /* si falla el borrado en R2, igual hacemos el soft delete */ }

            foto.Userup = usuarioId.ToString();
            await _repo.Delete(foto);
            await _repo.Save();
        }

        private static MascotaFotoDto ToDto(MascotaFoto f) => new MascotaFotoDto
        {
            Id = f.Id,
            MascotaId = f.MascotaId,
            Url = f.Url,
            Descripcion = f.Descripcion
        };
    }
}
