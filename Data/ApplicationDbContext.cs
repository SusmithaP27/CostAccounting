using CostAccounting.Models;
using Microsoft.EntityFrameworkCore;
namespace CostAccounting.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {

        }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        public DbSet<User> Users { get; set; }

        public DbSet<Road> Roads { get; set; }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<Material> Materials { get; set; }

        public DbSet<MaterialRate> MaterialsRate { get; set; }

        public DbSet<Equipment> Equipments { get; set; }

        public DbSet<EquipmentRate> EquipmentsRate { get; set; }

        public DbSet<Operation> Operations { get; set; }

        public DbSet<Allowance> Allowances { get; set; }

        public DbSet<Lookup> Lookups { get; set; }

        public DbSet<User> Login_Users { get; set; }
        public DbSet<Employee> Employees { get; set; }
public DbSet<EmployeeRate> EmployeeRates { get; set; }

        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Road>().ToTable("Road", "dbo");
            //modelBuilder.Entity<Material>(entity =>
            //{
            //    entity.HasNoKey();              // ✅ Required for views
            //    entity.ToView("vw_MaterialDisplay", "dbo");   // ✅ Map to SQL view
            //});
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Login_Users");
                entity.HasKey(u => u.ObjectID);
                entity.HasIndex(u => u.Username).IsUnique();
            });
            modelBuilder.Entity<Material>().ToTable("Material", "dbo");
            modelBuilder.Entity<MaterialRate>().ToTable("MaterialRate", "dbo");
            modelBuilder.Entity<Equipment>().ToTable("Equipment", "dbo");
            modelBuilder.Entity<EquipmentRate>().ToTable("EquipmentRate", "dbo");
            modelBuilder.Entity<Operation>().ToTable("Operation", "dbo");
            modelBuilder.Entity<Allowance>().ToTable("Allowance", "dbo");
            modelBuilder.Entity<Lookup>().ToTable("Lookup", "dbo");
            modelBuilder.Entity<Employee>().ToTable("Employee", "dbo");
            // modelBuilder.Entity<EmployeeVM>(eb =>
            // {
            //     eb.HasNoKey();      // keyless entity — required for stored-procedure results
            //     eb.ToView(null);     // not mapped to any table/view, only used via FromSqlRaw
            // });
            // --- OnModelCreating additions ---
// "ObjectID" doesn't match EF Core's key-detection convention (Id / <ClassName>Id),
// so it has to be configured explicitly — same as your Material entity.
modelBuilder.Entity<Employee>().HasKey(e => e.ObjectID);
modelBuilder.Entity<EmployeeRate>().HasKey(er => er.ObjectID);

// Mirrors the explicit FK config used for MaterialRate -> Material,
// which was needed to stop EF Core from misresolving the relationship.
modelBuilder.Entity<EmployeeRate>()
    .HasOne(er => er.Employee)
    .WithMany(e => e.EmployeeRates)
    .HasForeignKey(er => er.EmployeeObjectId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<Employee>().ToTable("Employee");
modelBuilder.Entity<EmployeeRate>().ToTable("EmployeeRate");
        }
    }
}
