using Microsoft.EntityFrameworkCore;
using Producer.Models;

namespace Producer.Data {
    public class DbContextClass: DbContext {
        protected readonly IConfiguration Configuration;
        public DbContextClass(IConfiguration configuration) {
            Configuration = configuration;
        }       
        
        protected override void OnConfiguring(DbContextOptionsBuilder options) {
        // connect to mysql with connection string from app settings
        var connectionString = Configuration.GetConnectionString("DefaultConnection");
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));        
        }

        public DbSet < Product > Products {
            get;
            set;
        }
    }
}