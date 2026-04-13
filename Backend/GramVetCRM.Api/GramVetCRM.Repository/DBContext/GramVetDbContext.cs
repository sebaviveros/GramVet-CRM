using Microsoft.EntityFrameworkCore;
using GramVetCRM.Model;

namespace GramVetCRM.Repository.Context
{
    public class GramVetDbContext : DbContext
    {
        public GramVetDbContext(DbContextOptions<GramVetDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuario { get; set; }
    }
}