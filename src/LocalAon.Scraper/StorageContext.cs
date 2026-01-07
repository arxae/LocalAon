using LocalAon.Models;
using LocalAon.Models.Products;
using Microsoft.EntityFrameworkCore;

namespace LocalAon.Scraper;

public class StorageContext : DbContext
{
    // Product
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductItem> ProductItems { get; set; }

    // Content
    public DbSet<BloodlineDisplayItem> Bloodlines { get; set; }
    public DbSet<Curse> Curses { get; set; }
    public DbSet<Disease> Diseases { get; set; }
    public DbSet<DruidCompanion> DruidCompanions { get; set; }
    public DbSet<PoisonDisplay> Poisons { get; set; }
    public DbSet<SpellDisplayItem> Spells { get; set; }
    public DbSet<TrapItem> Traps { get; set; }

    public string DbPath => Path.Combine(
        AppContext.BaseDirectory,
        "local_archive_of_nethys.db");

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite($"Data Source={DbPath}");
}
