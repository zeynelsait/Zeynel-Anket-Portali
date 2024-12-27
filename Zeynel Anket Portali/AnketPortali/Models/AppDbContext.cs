using AnketPortali.Models;
using AspNetCoreHero.ToastNotification.Notyf.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NETCore.Encrypt.Extensions;

namespace AnketPortali.Models
{
    public class AppDbContext : IdentityDbContext <AppUser,AppRole,string>
    {
        private readonly IConfiguration _config;
        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration config) : base(options)
        {
            _config = config;
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Survey> Surveys { get; set; }
        public DbSet<SurveyQuestion> SurveyQuestions { get; set; }
        public DbSet<SurveyOption> SurveyOptions { get; set; }
        public DbSet<SurveyResponse> SurveyResponses { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // MySQL için string kolonlarını düzenleme
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(256);
                entity.Property(e => e.UserName).HasMaxLength(256);
                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
                entity.Property(e => e.Email).HasMaxLength(256);
                entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            });

            modelBuilder.Entity<AppRole>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(256);
                entity.Property(e => e.Name).HasMaxLength(256);
                entity.Property(e => e.NormalizedName).HasMaxLength(256);
            });

            // Mevcut admin rol ve kullanıcı seed verileri
            var adminRole = Guid.NewGuid().ToString();
            var adminUser = Guid.NewGuid().ToString();

            modelBuilder.Entity<AppRole>().HasData(
                new AppRole
                {
                    Id = adminRole,
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                });

            modelBuilder.Entity<AppRole>().HasData(
                new AppRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "User",
                    NormalizedName = "USER"
                });

            modelBuilder.Entity<AppUser>().HasData(
                new AppUser
                {
                    Id = adminUser,
                    UserName = "admin",
                    FirstName="admin",
                    LastName="admin",
                    NormalizedUserName = "ADMIN",
                    Email = "zeyneladmin@admin.com",
                    NormalizedEmail = "ZEYNELADMIN@ADMIN.COM",
                    EmailConfirmed = true,
                    PasswordHash = new PasswordHasher<AppUser>().HashPassword(null, "Admin..")
                });

            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    UserId = adminUser,
                    RoleId = adminRole
                });
            modelBuilder.Entity<SurveyResponse>()
           .HasOne(r => r.Question)
           .WithMany()
           .HasForeignKey(r => r.QuestionId)
           .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<SurveyResponse>()
               .HasOne(r => r.SelectedOption)
               .WithMany()
               .HasForeignKey(r => r.SelectedOptionId)
               .OnDelete(DeleteBehavior.SetNull);

                    modelBuilder.Entity<SurveyOption>()
             .HasOne(o => o.Question)
             .WithMany(q => q.Options)
             .HasForeignKey(o => o.QuestionId)
             .OnDelete(DeleteBehavior.Cascade); // Alternatif: DeleteBehavior.Cascade
            modelBuilder.Entity<SurveyResponse>()
        .HasOne(r => r.SelectedOption)
        .WithMany()
        .HasForeignKey(r => r.SelectedOptionId)
        .OnDelete(DeleteBehavior.Restrict); // Alternatif: DeleteBehavior.Cascade

            modelBuilder.Entity<SurveyResponse>()
        .HasOne(r => r.Question)
        .WithMany()
        .HasForeignKey(r => r.QuestionId)
        .OnDelete(DeleteBehavior.Restrict); // ON DELETE SET NULL

            modelBuilder.Entity<SurveyResponse>()
                .HasOne(r => r.Survey)
                .WithMany()
                .HasForeignKey(r => r.SurveyId)
                .OnDelete(DeleteBehavior.Restrict); // ON DELETE SET NULL



            base.OnModelCreating(modelBuilder);


        }
    }
}
