using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public partial class ApplicationDbContext
    {
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.SeedData();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.;DataBase=SoumerViewDb;User Id=sa;Password=Asd123zxc;TrustServerCertificate=True;MultipleActiveResultSets=true;");
        }

    }
}
