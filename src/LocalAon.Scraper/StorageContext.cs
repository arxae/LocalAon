using LocalAon.Models;
using LocalAon.Models.Products;
using Microsoft.EntityFrameworkCore;

namespace LocalAon.Scraper;

internal class StorageContext : DbContext
{
    // Product
    internal DbSet<Product> Products { get; set; }
    internal DbSet<ProductItem> ProductedItems { get; set; }

    // Content
    internal DbSet<Curse> Curses { get; set; }
    internal DbSet<Disease> Diseases { get; set; }
    internal DbSet<DruidCompanion> DruidCompanions { get; set; }
    internal DbSet<SpellDisplayItem> Spells { get; set; }
    internal DbSet<TrapItem> Traps { get; set; }

    internal string DbPath { get; }

    internal StorageContext()
    {
        DbPath = "local_archive_of_nethys.db";
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}
