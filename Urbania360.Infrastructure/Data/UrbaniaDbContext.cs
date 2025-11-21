using Microsoft.EntityFrameworkCore;
using Urbania360.Domain.Entities;
using Urbania360.Domain.Enums;

namespace Urbania360.Infrastructure.Data;

public class UrbaniaDbContext : DbContext
{
    public UrbaniaDbContext(DbContextOptions<UrbaniaDbContext> options) : base(options)
    {
    }

    // DbSets de todas las entidades
    public DbSet<User> Users { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<UserPreference> UserPreferences { get; set; }
    public DbSet<Bank> Banks { get; set; }
    public DbSet<Property> Properties { get; set; }
    public DbSet<PropertyImage> PropertyImages { get; set; }
    public DbSet<PropertyConsult> PropertyConsults { get; set; }
    public DbSet<LoanSimulation> LoanSimulations { get; set; }
    public DbSet<AmortizationItem> AmortizationItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar collation para SQL Server
        modelBuilder.UseCollation("Modern_Spanish_100_CI_AI_SC");

        // Configuraciones específicas por entidad
        ConfigureUser(modelBuilder);
        ConfigureClient(modelBuilder);
        ConfigureActivityLog(modelBuilder);
        ConfigureUserPreference(modelBuilder);
        ConfigureBank(modelBuilder);
        ConfigureProperty(modelBuilder);
        ConfigurePropertyImage(modelBuilder);
        ConfigurePropertyConsult(modelBuilder);
        ConfigureLoanSimulation(modelBuilder);
        ConfigureAmortizationItem(modelBuilder);
    }

    private void ConfigureUser(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<User>();

        // PK con NEWSEQUENTIALID()
        builder.Property(x => x.Id)
               .HasColumnType("uniqueidentifier")
               .HasDefaultValueSql("NEWSEQUENTIALID()");

        // Índice único en Username
        builder.HasIndex(x => x.Username)
               .IsUnique()
               .HasDatabaseName("IX_Users_Username");

        // Índice único en Email
        builder.HasIndex(x => x.Email)
               .IsUnique()
               .HasDatabaseName("IX_Users_Email");

        // Configurar enums
        builder.Property(x => x.Role)
               .HasConversion<int>();

        builder.Property(x => x.CreatedAtUtc)
               .HasColumnType("datetime2");
    }

    private void ConfigureClient(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Client>();

        // PK con NEWSEQUENTIALID()
        builder.Property(x => x.Id)
               .HasColumnType("uniqueidentifier")
               .HasDefaultValueSql("NEWSEQUENTIALID()");

        // Precision para montos
        builder.Property(x => x.AnnualIncome)
               .HasPrecision(18, 2);

        builder.Property(x => x.CreatedAtUtc)
               .HasColumnType("datetime2");

        // Relación con User
        builder.HasOne(x => x.CreatedByUser)
               .WithMany(x => x.CreatedClients)
               .HasForeignKey(x => x.CreatedByUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }

    private void ConfigureActivityLog(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<ActivityLog>();

        // BIGINT IDENTITY
        builder.Property(x => x.Id)
               .UseIdentityColumn();

        builder.Property(x => x.CreatedAtUtc)
               .HasColumnType("datetime2");

        // Relación con User
        builder.HasOne(x => x.User)
               .WithMany(x => x.ActivityLogs)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }

    private void ConfigureUserPreference(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<UserPreference>();

        // INT IDENTITY
        builder.Property(x => x.Id)
               .UseIdentityColumn();

        // Índice único en UserId
        builder.HasIndex(x => x.UserId)
               .IsUnique()
               .HasDatabaseName("IX_UserPreferences_UserId");

        // Configurar enums
        builder.Property(x => x.DefaultCurrency)
               .HasConversion<int>();

        builder.Property(x => x.DefaultRateType)
               .HasConversion<int>();

        // Relación con User (one-to-one)
        builder.HasOne(x => x.User)
               .WithOne(x => x.UserPreference)
               .HasForeignKey<UserPreference>(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureBank(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Bank>();

        // INT IDENTITY
        builder.Property(x => x.Id)
               .ValueGeneratedOnAdd();

        // Precision para tasas
        builder.Property(x => x.AnnualRateTea)
               .HasPrecision(6, 4);

        // Índice único en Name
        builder.HasIndex(x => x.Name)
               .IsUnique()
               .HasDatabaseName("IX_Banks_Name");

        builder.Property(x => x.EffectiveFrom)
               .HasColumnType("datetime2");
    }

    private void ConfigureProperty(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Property>();

        // PK con NEWSEQUENTIALID()
        builder.Property(x => x.Id)
               .HasColumnType("uniqueidentifier")
               .HasDefaultValueSql("NEWSEQUENTIALID()");

        // Índice único en Code
        builder.HasIndex(x => x.Code)
               .IsUnique()
               .HasDatabaseName("IX_Properties_Code");

        // Precision para montos
        builder.Property(x => x.AreaM2)
               .HasPrecision(18, 2);

        builder.Property(x => x.Price)
               .HasPrecision(18, 2);

        // Configurar enums
        builder.Property(x => x.Type)
               .HasConversion<int>();

        builder.Property(x => x.Currency)
               .HasConversion<int>();

        builder.Property(x => x.CreatedAtUtc)
               .HasColumnType("datetime2");

        // Relación con User
        builder.HasOne(x => x.CreatedByUser)
               .WithMany(x => x.CreatedProperties)
               .HasForeignKey(x => x.CreatedByUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }

    private void ConfigurePropertyImage(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<PropertyImage>();

        // BIGINT IDENTITY
        builder.Property(x => x.Id)
               .UseIdentityColumn();

        // Relación con Property
        builder.HasOne(x => x.Property)
               .WithMany(x => x.PropertyImages)
               .HasForeignKey(x => x.PropertyId)
               .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigurePropertyConsult(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<PropertyConsult>();

        // BIGINT IDENTITY
        builder.Property(x => x.Id)
               .UseIdentityColumn();

        // Índice para reportes por fecha
        builder.HasIndex(x => x.CreatedAtUtc)
               .HasDatabaseName("IX_PropertyConsults_CreatedAtUtc");

        builder.Property(x => x.CreatedAtUtc)
               .HasColumnType("datetime2");

        // Relación con Property
        builder.HasOne(x => x.Property)
               .WithMany(x => x.PropertyConsults)
               .HasForeignKey(x => x.PropertyId)
               .OnDelete(DeleteBehavior.Restrict);

        // Relación con User (opcional)
        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.SetNull);
    }

    private void ConfigureLoanSimulation(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<LoanSimulation>();

        // PK con NEWSEQUENTIALID()
        builder.Property(x => x.Id)
               .HasColumnType("uniqueidentifier")
               .HasDefaultValueSql("NEWSEQUENTIALID()");

        // Precision para montos
        builder.Property(x => x.Principal)
               .HasPrecision(18, 2);

        builder.Property(x => x.BonusAmount)
               .HasPrecision(18, 2);

        builder.Property(x => x.MonthlyPayment)
               .HasPrecision(18, 2);

        builder.Property(x => x.VAN)
               .HasPrecision(18, 2);

        builder.Property(x => x.TotalInterest)
               .HasPrecision(18, 2);

        builder.Property(x => x.TotalCost)
               .HasPrecision(18, 2);

        // Precision para tasas
        builder.Property(x => x.TEA)
               .HasPrecision(6, 4);

        builder.Property(x => x.TNA)
               .HasPrecision(6, 4);

        builder.Property(x => x.TEM)
               .HasPrecision(6, 4);

        builder.Property(x => x.TCEA)
               .HasPrecision(6, 4);

        builder.Property(x => x.TIR)
               .HasPrecision(6, 4);

        // Configurar enums
        builder.Property(x => x.Currency)
               .HasConversion<int>();

        builder.Property(x => x.RateType)
               .HasConversion<int>();

        builder.Property(x => x.GraceType)
               .HasConversion<int>();

        // Índice para reportes por mes
        builder.HasIndex(x => x.CreatedAtUtc)
               .HasDatabaseName("IX_LoanSimulations_CreatedAtUtc");

        builder.Property(x => x.StartDate)
               .HasColumnType("date");

        builder.Property(x => x.CreatedAtUtc)
               .HasColumnType("datetime2");

        // Relaciones
        builder.HasOne(x => x.Client)
               .WithMany(x => x.LoanSimulations)
               .HasForeignKey(x => x.ClientId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Property)
               .WithMany(x => x.LoanSimulations)
               .HasForeignKey(x => x.PropertyId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Bank)
               .WithMany(x => x.LoanSimulations)
               .HasForeignKey(x => x.BankId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
               .WithMany(x => x.CreatedSimulations)
               .HasForeignKey(x => x.CreatedByUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }

    private void ConfigureAmortizationItem(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<AmortizationItem>();

        // BIGINT IDENTITY
        builder.Property(x => x.Id)
               .UseIdentityColumn();

        // Índice único compuesto
        builder.HasIndex(x => new { x.SimulationId, x.Period })
               .IsUnique()
               .HasDatabaseName("IX_AmortizationItems_SimulationId_Period");

        // Precision para todos los montos
        builder.Property(x => x.OpeningBalance)
               .HasPrecision(18, 2);

        builder.Property(x => x.Interest)
               .HasPrecision(18, 2);

        builder.Property(x => x.Principal)
               .HasPrecision(18, 2);

        builder.Property(x => x.Installment)
               .HasPrecision(18, 2);

        builder.Property(x => x.LifeInsurance)
               .HasPrecision(18, 2);

        builder.Property(x => x.RiskInsurance)
               .HasPrecision(18, 2);

        builder.Property(x => x.Fees)
               .HasPrecision(18, 2);

        builder.Property(x => x.ClosingBalance)
               .HasPrecision(18, 2);

        builder.Property(x => x.DueDate)
               .HasColumnType("date");

        // Relación con LoanSimulation
        builder.HasOne(x => x.Simulation)
               .WithMany(x => x.AmortizationItems)
               .HasForeignKey(x => x.SimulationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}