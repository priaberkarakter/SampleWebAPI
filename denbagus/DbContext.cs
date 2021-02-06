using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DenBagus
{
    public class DataAPI: DbContext
    {
        private const int MAX_DB_IDENTIFIER_NAME = 64;
        private readonly IConfiguration configuration;

        public DataAPI(DbContextOptions<DataAPI> options, IConfiguration configuration) : base(options)
        {
            this.configuration = configuration;
        }
        
        public DbSet<Models.Stock> Stock { get; set; }
        //public DbSet<Models.Vendor> Vendor { get; set; }

        public DbSet<Models.Customer> Customer { get; set;}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder != null)
            {
                //optionsBuilder.UseLoggerFactory(this.loggerFactory);
                optionsBuilder.EnableSensitiveDataLogging();
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder != null)
            {
                foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
                {
                    // setup foreign key constraint
                    foreach (var key in entityType.GetForeignKeys())
                    {
                        // Set all foreign key constraint to on delete restrict
                        key.DeleteBehavior = DeleteBehavior.Restrict;
                    }
                }
            }
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Models.Stock>()
            .HasOne(x => x.Vendor)
            .WithMany(x => x.Stocks)
            .HasForeignKey(x => x.vendor_code);

        }
    }
}
