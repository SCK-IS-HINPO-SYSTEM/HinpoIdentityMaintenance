using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HinpoIdentityMaintenance.Data {
    public class ApplicationDbContext : IdentityDbContext {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) {
        }
        public DbSet<ApplicationUser> ApplicationUser { get; set; } //<-- 本当は必要ないが、別アプリケーションであるHinpoMenuが立ち上がっていないと起動時にエラーになる。

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
        }
    }
    public class ApplicationUser : IdentityUser {
#pragma warning disable CS8618
        public string FirstName { get; set; }
        public string LastName { get; set; }
#pragma warning restore CS8618
    }
}