using GramVetCRM.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace GramVetCRM.Repository.Context
{
    public class GramVetDbContext : DbContext
    {
        public GramVetDbContext(DbContextOptions<GramVetDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Rol> Rol { get; set; }
        public DbSet<Contacto> Contacto { get; set; }
        public DbSet<Mascota> Mascota { get; set; }
        public DbSet<MascotaBitacora> MascotaBitacora { get; set; }
        public DbSet<MascotaFoto> MascotaFoto { get; set; }
        public DbSet<Canal> Canal { get; set; }
        public DbSet<Conversacion> Conversacion { get; set; }
        public DbSet<Mensaje> Mensaje { get; set; }
        public DbSet<Etiqueta> Etiqueta { get; set; }
        public DbSet<ContactoEtiqueta> ContactoEtiqueta { get; set; }
        public DbSet<EtiquetaUsuario> EtiquetaUsuario { get; set; }
        public DbSet<RespuestaRapida> RespuestaRapida { get; set; }
        public DbSet<RespuestaRapidaUsuario> RespuestaRapidaUsuario { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Mascota → Contacto
            modelBuilder.Entity<Mascota>()
                .HasOne(m => m.Contacto)
                .WithMany(c => c.Mascotas)
                .HasForeignKey(m => m.ContactoId)
                .OnDelete(DeleteBehavior.Restrict);

            // MascotaBitacora → Mascota
            modelBuilder.Entity<MascotaBitacora>()
                .HasOne(b => b.Mascota)
                .WithMany()
                .HasForeignKey(b => b.MascotaId)
                .OnDelete(DeleteBehavior.Restrict);

            // MascotaFoto → MascotaBitacora (la foto cuelga de la anotación)
            modelBuilder.Entity<MascotaFoto>()
                .HasOne(f => f.Bitacora)
                .WithMany()
                .HasForeignKey(f => f.BitacoraId)
                .OnDelete(DeleteBehavior.Restrict);

            // Conversacion → Contacto
            modelBuilder.Entity<Conversacion>()
                .HasOne(c => c.Contacto)
                .WithMany()
                .HasForeignKey(c => c.ContactoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Conversacion → Canal
            modelBuilder.Entity<Conversacion>()
                .HasOne(c => c.Canal)
                .WithMany()
                .HasForeignKey(c => c.CanalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Mensaje → Conversacion
            modelBuilder.Entity<Mensaje>()
                .HasOne(m => m.Conversacion)
                .WithMany()
                .HasForeignKey(m => m.ConversacionId)
                .OnDelete(DeleteBehavior.Restrict);

            // ContactoEtiqueta → Contacto
            modelBuilder.Entity<ContactoEtiqueta>()
                .HasOne(ce => ce.Contacto)
                .WithMany()
                .HasForeignKey(ce => ce.ContactoId)
                .OnDelete(DeleteBehavior.Restrict);

            // ContactoEtiqueta → Etiqueta
            modelBuilder.Entity<ContactoEtiqueta>()
                .HasOne(ce => ce.Etiqueta)
                .WithMany()
                .HasForeignKey(ce => ce.EtiquetaId)
                .OnDelete(DeleteBehavior.Restrict);

            // EtiquetaUsuario → Etiqueta / Usuario
            modelBuilder.Entity<EtiquetaUsuario>()
                .HasOne(eu => eu.Etiqueta)
                .WithMany()
                .HasForeignKey(eu => eu.EtiquetaId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EtiquetaUsuario>()
                .HasOne(eu => eu.Usuario)
                .WithMany()
                .HasForeignKey(eu => eu.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // RespuestaRapidaUsuario → RespuestaRapida / Usuario
            modelBuilder.Entity<RespuestaRapidaUsuario>()
                .HasOne(ru => ru.RespuestaRapida)
                .WithMany()
                .HasForeignKey(ru => ru.RespuestaRapidaId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RespuestaRapidaUsuario>()
                .HasOne(ru => ru.Usuario)
                .WithMany()
                .HasForeignKey(ru => ru.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}