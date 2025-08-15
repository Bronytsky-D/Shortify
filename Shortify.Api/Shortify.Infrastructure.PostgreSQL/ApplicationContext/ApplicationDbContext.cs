using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shortify.Domain.Models;


namespace Shortify.Infrastructure.PostgreSQL.ApplicationContext
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<string>, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<LinkEntry> LinkEntries { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserToken>(b =>
            {
                b.ToTable("UserTokens");              
                b.HasKey(x => x.Id);

                b.Property(x => x.Token)
                    .IsRequired()
                    .HasMaxLength(512);

                b.Property(x => x.UserId)
                    .IsRequired();

                b.Property(x => x.ExpiresAt)
                    .IsRequired();

                b.Property(x => x.IsRevoked)
                    .HasDefaultValue(false);

                b.HasIndex(x => x.UserId);
                b.HasIndex(x => x.Token).IsUnique();  

                b.HasOne<User>()
                    .WithMany(u => u.UserTokens)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade); 
            });
        }
    }
}
