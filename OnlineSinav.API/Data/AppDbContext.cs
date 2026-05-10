using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineSinav.API.Models;

namespace OnlineSinav.API.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamQuestion> ExamQuestions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Cascade delete çakışmasını önlemek için kural ekliyoruz
            // (Bir sınav silindiğinde, sonuçların silinmesi kurallarını düzenliyoruz)
            builder.Entity<ExamResult>()
                .HasOne(er => er.Exam)
                .WithMany()
                .HasForeignKey(er => er.ExamId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}