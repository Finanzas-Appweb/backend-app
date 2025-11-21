using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Urbania360.Domain.Entities;
using Urbania360.Domain.Enums;
using BCrypt.Net;

namespace Urbania360.Infrastructure.Data;

public static class AppDbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UrbaniaDbContext>();

        // Asegurar que la base de datos existe
        await context.Database.EnsureCreatedAsync();

        // Solo hacer seed si no hay datos
        if (await context.Users.AnyAsync())
        {
            return; // Ya tiene datos
        }

        // Crear usuario Admin
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            FirstName = "Administrador",
            LastName = "Sistema",
            Dni = "12345678",
            FullName = "Administrador Sistema",
            Email = "admin@urbania360.com",
            Phone = "+51999999999",
            Role = Role.Admin,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };

        await context.Users.AddAsync(adminUser);

        // Crear usuario Agent demo
        var agentUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "agente",
            FirstName = "Carlos",
            LastName = "Agente",
            Dni = "87654321",
            FullName = "Carlos Agente",
            Email = "agente@urbania360.com",
            Phone = "+51988888888",
            Role = Role.Agent,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };

        await context.Users.AddAsync(agentUser);
        await context.SaveChangesAsync();

        // Crear preferencias de usuarios
        var preferences = new[]
        {
            new UserPreference
            {
                UserId = adminUser.Id,
                DefaultCurrency = Currency.PEN,
                DefaultRateType = RateType.TEA
            },
            new UserPreference
            {
                UserId = agentUser.Id,
                DefaultCurrency = Currency.PEN,
                DefaultRateType = RateType.TEA
            }
        };

        await context.UserPreferences.AddRangeAsync(preferences);

        // Crear bancos
        var banks = new[]
        {
            new Bank { Name = "BCP", Description = "Banco de Crédito del Perú", AnnualRateTea = 0.0850m, EffectiveFrom = DateTime.UtcNow },
            new Bank { Name = "Interbank", Description = "Banco Internacional del Perú", AnnualRateTea = 0.0890m, EffectiveFrom = DateTime.UtcNow },
            new Bank { Name = "Scotiabank", Description = "Scotiabank Perú", AnnualRateTea = 0.0920m, EffectiveFrom = DateTime.UtcNow }
        };

        await context.Banks.AddRangeAsync(banks);
        await context.SaveChangesAsync();

        // Crear propiedades demo
        var properties = new[]
        {
            new Property
            {
                Id = Guid.NewGuid(),
                Code = "P0001",
                Title = "Casa moderna en San Borja",
                Address = "Av. San Luis 1234",
                District = "San Borja",
                Province = "Lima",
                Type = PropertyType.Casa,
                AreaM2 = 180.50m,
                Price = 350000.00m,
                Currency = Currency.USD,
                CreatedByUserId = adminUser.Id,
                CreatedAtUtc = DateTime.UtcNow
            },
            new Property
            {
                Id = Guid.NewGuid(),
                Code = "P0002",
                Title = "Departamento en Miraflores",
                Address = "Av. Larco 567",
                District = "Miraflores",
                Province = "Lima",
                Type = PropertyType.Departamento,
                AreaM2 = 95.00m,
                Price = 280000.00m,
                Currency = Currency.USD,
                CreatedByUserId = adminUser.Id,
                CreatedAtUtc = DateTime.UtcNow
            },
            new Property
            {
                Id = Guid.NewGuid(),
                Code = "P0003",
                Title = "Oficina en San Isidro",
                Address = "Av. República de Panamá 890",
                District = "San Isidro",
                Province = "Lima",
                Type = PropertyType.Oficina,
                AreaM2 = 120.00m,
                Price = 450000.00m,
                Currency = Currency.USD,
                CreatedByUserId = adminUser.Id,
                CreatedAtUtc = DateTime.UtcNow
            },
            new Property
            {
                Id = Guid.NewGuid(),
                Code = "P0004",
                Title = "Terreno en Pachacamac",
                Address = "Km 25 Panamericana Sur",
                District = "Pachacamac",
                Province = "Lima",
                Type = PropertyType.Terreno,
                AreaM2 = 500.00m,
                Price = 180000.00m,
                Currency = Currency.USD,
                CreatedByUserId = adminUser.Id,
                CreatedAtUtc = DateTime.UtcNow
            }
        };

        await context.Properties.AddRangeAsync(properties);
        await context.SaveChangesAsync();

        // Crear consultas de propiedades (para estadísticas)
        var propertyConsults = new[]
        {
            new PropertyConsult { PropertyId = properties[0].Id, UserId = adminUser.Id, CreatedAtUtc = DateTime.UtcNow.AddDays(-5) },
            new PropertyConsult { PropertyId = properties[0].Id, CreatedAtUtc = DateTime.UtcNow.AddDays(-3) },
            new PropertyConsult { PropertyId = properties[1].Id, UserId = adminUser.Id, CreatedAtUtc = DateTime.UtcNow.AddDays(-2) },
            new PropertyConsult { PropertyId = properties[1].Id, CreatedAtUtc = DateTime.UtcNow.AddDays(-1) },
            new PropertyConsult { PropertyId = properties[2].Id, CreatedAtUtc = DateTime.UtcNow }
        };

        await context.PropertyConsults.AddRangeAsync(propertyConsults);

        // Crear clientes demo
        var clients = new[]
        {
            new Client
            {
                Id = Guid.NewGuid(),
                FirstName = "Juan Carlos",
                LastName = "Pérez García",
                Email = "juan.perez@email.com",
                Phone = "+51987654321",
                AnnualIncome = 60000.00m,
                CreatedByUserId = adminUser.Id,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-10)
            },
            new Client
            {
                Id = Guid.NewGuid(),
                FirstName = "María Elena",
                LastName = "Rodríguez Vásquez",
                Email = "maria.rodriguez@email.com",
                Phone = "+51976543210",
                AnnualIncome = 45000.00m,
                CreatedByUserId = adminUser.Id,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-7)
            },
            new Client
            {
                Id = Guid.NewGuid(),
                FirstName = "Carlos Alberto",
                LastName = "Mendoza Silva",
                Email = "carlos.mendoza@email.com",
                Phone = "+51965432109",
                AnnualIncome = 80000.00m,
                CreatedByUserId = adminUser.Id,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-3)
            }
        };

        await context.Clients.AddRangeAsync(clients);
        await context.SaveChangesAsync();

        // Crear activity logs de muestra
        var activityLogs = new[]
        {
            new ActivityLog
            {
                UserId = adminUser.Id,
                Action = "Create",
                Entity = "Client",
                EntityId = clients[0].Id.ToString(),
                CreatedAtUtc = DateTime.UtcNow.AddDays(-10)
            },
            new ActivityLog
            {
                UserId = adminUser.Id,
                Action = "Create",
                Entity = "Property",
                EntityId = properties[0].Id.ToString(),
                CreatedAtUtc = DateTime.UtcNow.AddDays(-5)
            }
        };

        await context.ActivityLogs.AddRangeAsync(activityLogs);

        await context.SaveChangesAsync();
    }
}