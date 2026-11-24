using Microsoft.EntityFrameworkCore;
using RestFulWebService.Models;

namespace RestFulWebService.Data;

public class WeatherDbContext(DbContextOptions<WeatherDbContext> options) : DbContext(options)
{
    public DbSet<WeatherForecast> Forecasts => Set<WeatherForecast>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WeatherForecast>(entity =>
        {
            entity.HasKey(forecast => forecast.Id);
            entity.Property(forecast => forecast.Summary)
                .IsRequired()
                .HasMaxLength(60);
            entity.HasIndex(forecast => forecast.Date);
        });
    }
}
