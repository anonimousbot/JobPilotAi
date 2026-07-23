using System.Text;
using Microsoft.EntityFrameworkCore;
using JobPilotAi_Backend.Modules.Admin;
using JobPilotAi_Backend.Modules.Analyses;
using JobPilotAi_Backend.Modules.Billing;
using JobPilotAi_Backend.Modules.CoverLetters;
using JobPilotAi_Backend.Modules.Identity;
using JobPilotAi_Backend.Modules.JobMatches;
using JobPilotAi_Backend.Modules.Profiles;
using JobPilotAi_Backend.Modules.Resumes;
using JobPilotAi_Backend.Modules.Subscriptions;

namespace JobPilotAi_Backend.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Profile> Profiles => Set<Profile>();

    public DbSet<Resume> Resumes => Set<Resume>();

    public DbSet<ResumeAnalysis> ResumeAnalyses => Set<ResumeAnalysis>();

    public DbSet<CoverLetter> CoverLetters => Set<CoverLetter>();

    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    public DbSet<UsageRecord> UsageRecords => Set<UsageRecord>();

    public DbSet<PaymentEvent> PaymentEvents => Set<PaymentEvent>();

    public DbSet<AdminLog> AdminLogs => Set<AdminLog>();

    public DbSet<SavedJobMatch> SavedJobMatches => Set<SavedJobMatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("applyai");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.HasIndex(token => token.TokenHash).IsUnique();
            builder.HasIndex(token => token.UserId);
            builder.Property(token => token.TokenHash).HasMaxLength(128).IsRequired();
        });

        modelBuilder.Entity<Profile>(builder =>
        {
            builder.HasIndex(profile => profile.UserId).IsUnique();
            builder.Property(profile => profile.FirstName).HasMaxLength(100);
            builder.Property(profile => profile.LastName).HasMaxLength(100);
            builder.Property(profile => profile.PhoneNumber).HasMaxLength(40);
            builder.Property(profile => profile.Location).HasMaxLength(160);
            builder.Property(profile => profile.LinkedInUrl).HasMaxLength(500);
            builder.Property(profile => profile.PortfolioUrl).HasMaxLength(500);
            builder.Property(profile => profile.CareerGoal).HasMaxLength(80);
            builder.Property(profile => profile.TargetRole).HasMaxLength(160);
            builder.Property(profile => profile.CareerLevel).HasMaxLength(80);
        });

        modelBuilder.Entity<Resume>(builder =>
        {
            builder.HasIndex(resume => resume.UserId);
            builder.HasIndex(resume => new { resume.UserId, resume.ContentHash });
            builder.Property(resume => resume.FileName).HasMaxLength(260).IsRequired();
            builder.Property(resume => resume.FileType).HasMaxLength(16).IsRequired();
            builder.Property(resume => resume.StoragePath).HasMaxLength(1024).IsRequired();
            builder.Property(resume => resume.ContentHash).HasMaxLength(64).IsRequired();
            builder.Property(resume => resume.SkillsJson).IsRequired();
            builder.Property(resume => resume.ExperienceJson).IsRequired();
            builder.Property(resume => resume.EducationJson).IsRequired();
        });

        modelBuilder.Entity<ResumeAnalysis>(builder =>
        {
            builder.HasIndex(analysis => analysis.UserId);
            builder.HasIndex(analysis => analysis.ResumeId);
            builder.Property(analysis => analysis.WeaknessesJson).IsRequired();
            builder.Property(analysis => analysis.RecommendationsJson).IsRequired();
            builder.Property(analysis => analysis.MissingKeywordsJson).IsRequired();
            builder.Property(analysis => analysis.StrengthsJson).IsRequired();
        });

        modelBuilder.Entity<CoverLetter>(builder =>
        {
            builder.HasIndex(letter => letter.UserId);
            builder.HasIndex(letter => letter.ResumeId);
            builder.Property(letter => letter.JobTitle).HasMaxLength(160).IsRequired();
            builder.Property(letter => letter.CompanyName).HasMaxLength(160).IsRequired();
            builder.Property(letter => letter.Tone).HasMaxLength(40).IsRequired();
            builder.Property(letter => letter.JobDescription).IsRequired();
            builder.Property(letter => letter.OriginalContent).IsRequired();
            builder.Property(letter => letter.EditedContent).IsRequired();
        });

        modelBuilder.Entity<Subscription>(builder =>
        {
            builder.HasIndex(subscription => subscription.UserId);
            builder.Property(subscription => subscription.PlanType).HasConversion<string>().HasMaxLength(32).IsRequired();
            builder.Property(subscription => subscription.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            builder.Property(subscription => subscription.BillingCycle).HasMaxLength(32).IsRequired();
        });

        modelBuilder.Entity<UsageRecord>(builder =>
        {
            builder.HasIndex(usage => new { usage.UserId, usage.ActionType, usage.PeriodStart, usage.PeriodEnd });
            builder.Property(usage => usage.ActionType).HasConversion<string>().HasMaxLength(64).IsRequired();
            builder.Property(usage => usage.EstimatedCost).HasPrecision(12, 6);
        });

        modelBuilder.Entity<PaymentEvent>(builder =>
        {
            builder.HasIndex(payment => payment.SubscriptionId);
            builder.Property(payment => payment.Amount).HasPrecision(12, 2);
            builder.Property(payment => payment.Currency).HasMaxLength(3).IsRequired();
            builder.Property(payment => payment.Provider).HasMaxLength(80).IsRequired();
            builder.Property(payment => payment.ProviderReference).HasMaxLength(160).IsRequired();
            builder.Property(payment => payment.Status).HasMaxLength(80).IsRequired();
        });

        modelBuilder.Entity<AdminLog>(builder =>
        {
            builder.HasIndex(log => log.AdminId);
            builder.Property(log => log.Action).HasMaxLength(160).IsRequired();
            builder.Property(log => log.TargetEntity).HasMaxLength(160).IsRequired();
            builder.Property(log => log.MetadataJson).IsRequired();
        });

        modelBuilder.Entity<SavedJobMatch>(builder =>
        {
            builder.HasIndex(match => match.UserId);
            builder.HasIndex(match => new { match.UserId, match.JobMatchId }).IsUnique();
            builder.Property(match => match.JobMatchId).HasMaxLength(80).IsRequired();
            builder.Property(match => match.JobTitle).HasMaxLength(180).IsRequired();
            builder.Property(match => match.CompanyName).HasMaxLength(180).IsRequired();
            builder.Property(match => match.Location).HasMaxLength(180).IsRequired();
            builder.Property(match => match.TagsJson).IsRequired();
        });

        ApplySnakeCaseNames(modelBuilder);
    }

    private static void ApplySnakeCaseNames(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(ToSnakeCase(entity.GetTableName() ?? entity.ClrType.Name));

            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.GetColumnName()));
            }
        }
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var builder = new StringBuilder(value.Length + 8);
        for (var i = 0; i < value.Length; i++)
        {
            var character = value[i];
            if (char.IsUpper(character))
            {
                if (i > 0 && value[i - 1] != '_')
                {
                    builder.Append('_');
                }

                builder.Append(char.ToLowerInvariant(character));
                continue;
            }

            builder.Append(character);
        }

        return builder.ToString();
    }
}
