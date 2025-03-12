using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;



namespace ERP.Models;


public class ErpUser : IdentityUser
{
    public int? EmployeeId { get; set; }
    public virtual Employee Employee { get; set; }

    public ErpUser() : base() { }
    public ErpUser(string options) : base(options) { }
}



public partial class ErpContext : IdentityDbContext<ErpUser>
{
    public ErpContext()
    {
    }

    public ErpContext(DbContextOptions<ErpContext> options)
       : base(options)
    {
    }

    public virtual DbSet<Client> Clients { get; set; }
    public virtual DbSet<Contact> Contacts { get; set; }
    public virtual DbSet<DeliveryAddress> DeliveryAddresses { get; set; }
    public virtual DbSet<Employee> Employees { get; set; }
    public virtual DbSet<Project> Projects { get; set; }
    public virtual DbSet<Item> Items { get; set; }
    public virtual DbSet<ProjectFile> ProjectFiles { get; set; }
    public virtual DbSet<Requisite> Requisites { get; set; }
    public virtual DbSet<Website> Websites { get; set; }
    public virtual DbSet<Color> Colors { get; set; }
    public virtual DbSet<ItemsType> ItemTypes { get; set; }
    public virtual DbSet<Material> Materials { get; set; }
    public virtual DbSet<TaskItem> Tasks { get; set; }
    public virtual DbSet<ProjectPayment> ProjectPayments { get; set; }
    public virtual DbSet<SalaryEmployeeMonth> SalaryEmployeeMonths { get; set; }

    public virtual DbSet<YandexAccount> YandexAccounts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=aspnet-ERP-a9c9ae8d-072e-4486-bb33-3ba9ed06aef2;Trusted_Connection=True;MultipleActiveResultSets=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ErpUser>(entity =>
        {
            entity.HasOne(e => e.Employee)
                  .WithMany()
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.ClientId).HasName("PK__Client__E67E1A2458A459C2");

            entity.ToTable("Client");

            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(100);
        });

        modelBuilder.Entity<YandexAccount>(entity =>
        {
            entity.HasKey(a => a.Id);  // Первичный ключ

            entity.Property(a => a.Email)
            .IsRequired()
            .HasMaxLength(255);

            entity.Property(a => a.AccessToken)
            .IsRequired();

            entity.Property(a => a.RefreshToken)
            .IsRequired();

            entity.Property(a => a.ExpiryDate)
            .IsRequired();

            entity.Property(a => a.CreatedAt)
            .HasDefaultValueSql("GETDATE()");

        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.ContactId).HasName("PK__Contact__5C66259B016BAD5B");
            entity.Property(e => e.ContactId).ValueGeneratedOnAdd();
            entity.ToTable("Contact");

            entity.Property(e => e.ContactName).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(30);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);

            entity.HasOne(d => d.Client).WithMany(p => p.Contacts)
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("FK__Contact__ClientI__48CFD27E");
        });

        modelBuilder.Entity<DeliveryAddress>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK__Delivery__091C2AFBEF6FC892");
            entity.Property(e => e.AddressId).ValueGeneratedOnAdd();
            entity.ToTable("DeliveryAddress");


            entity.Property(e => e.DeliveryAddress1).HasColumnName("DeliveryAddress");

            entity.HasOne(d => d.Client).WithMany(p => p.DeliveryAddresses)
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("FK__DeliveryA__Clien__45F365D3");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__7AD04F11CD55FEBC");
            entity.ToTable("Employee");
            entity.Property(e => e.Email).HasMaxLength(30);
            entity.Property(e => e.EmployeeName).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.Position).HasMaxLength(50);
            entity.Property(e => e.Passport).HasMaxLength(150);
            entity.Property(i => i.BossId).HasColumnName("BossId");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__Item__727E838BFA193C84");

            entity.ToTable("Item");

            entity.Property(e => e.ItemType).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(6, 2)");
            entity.Property(i => i.Price)
                    .HasColumnType("decimal(10,2)");

            entity.HasOne(d => d.Project).WithMany(p => p.Items)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Item__ProjectId__5441852A");

            //entity.HasOne(d => d.SketchNavigation).WithMany(p => p.Items)
            //    .HasForeignKey(d => d.Sketch)
            //    .HasConstraintName("FK__Item__Sketch__5535A963");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("PK__Project__761ABEF006CB3F38");

            entity.ToTable("Project");

            entity.Property(e => e.AdvanceRate).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PaymentTotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProjectName).HasMaxLength(100);

            entity.HasOne(d => d.Client).WithMany(p => p.Projects)
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("FK__Project__ClientI__4222D4EF");

            entity.HasOne(d => d.DeliveryToAddressNavigation).WithMany(p => p.Projects)
                .HasForeignKey(d => d.DeliveryToAddress)
                .HasConstraintName("FK__Project__Deliver__4316F928");

            entity.HasOne(d => d.DeliveryToContactNavigation).WithMany(p => p.Projects)
                .HasForeignKey(d => d.DeliveryToContact)
                .HasConstraintName("FK_Project_DeliveryToContact");

            entity.HasOne(d => d.Employee).WithMany(p => p.Projects)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__Project__Employe__412EB0B6");

            entity.Property(p => p.EmployeePayment)
                .HasPrecision(18, 2);
        });

        modelBuilder.Entity<ProjectFile>(entity =>
        {
            entity.HasKey(e => e.FileId).HasName("PK__ProjectF__6F0F98BF593087A7");

            entity.ToTable("ProjectFile");

            entity.Property(e => e.FileTitle).HasMaxLength(255);
            entity.Property(e => e.FileType).HasMaxLength(50);
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectFiles)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK__ProjectFi__Proje__47DBAE45");
        });

        modelBuilder.Entity<Requisite>(entity =>
        {
            entity.HasKey(e => e.RequisiteId).HasName("PK__Requisit__32FEEC2884A854AA");

            entity.ToTable("Requisite");
            entity.Property(e => e.RequisiteId).ValueGeneratedOnAdd();

            //entity.Property(e => e.RequisiteId).ValueGeneratedNever();
            entity.Property(e => e.FileTitle).HasMaxLength(255);
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Client).WithMany(p => p.Requisites)
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("FK__Requisite__Clien__6FE99F9F");
        });

        modelBuilder.Entity<Website>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Website");

            entity.Property(e => e.Website1)
                .HasMaxLength(255)
                .HasColumnName("Website");

            entity.HasOne(d => d.Client).WithMany()
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("FK__Website__ClientI__5DCAEF64");
        });

        modelBuilder.Entity<ItemsType>(entity =>
        {
            entity.HasKey(e => e.ItemTypeId).HasName("PK__ItemType__F51540FBE00BE195");

            entity.ToTable("ItemType");

            entity.Property(e => e.ItemTypeName).HasMaxLength(50);
        });

        modelBuilder.Entity<Material>(entity =>
        {
            entity.HasKey(e => e.MaterialId).HasName("PK__Material__C50610F7B32E7551");

            entity.ToTable("Material");

            entity.Property(e => e.MaterialName).HasMaxLength(50);
        });

        modelBuilder.Entity<Color>(entity =>
        {
            entity.HasKey(e => e.ColorId).HasName("PK__Color__8DA7674D668C3A7C");

            entity.ToTable("Color");

            entity.Property(e => e.ColorName).HasMaxLength(50);
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PK__Task__7C6949B15FF753D3");

            entity.ToTable("Task");

            entity.HasOne(d => d.Employee).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Task__EmployeeId__76969D2E");

            entity.HasOne(d => d.Project).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Task__ProjectId__75A278F5");
        });

        modelBuilder.Entity<ProjectPayment>(entity =>
        {
            entity.HasKey(e => e.ProjectPaymentId).HasName("PK__ProjectP__D131DB604907FF91");

            entity.ToTable("ProjectPayment");

            entity.Property(e => e.Amount).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.Punishment).HasColumnType("decimal(8, 2)");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectPayments)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK__ProjectPa__Proje__7D439ABD");

            entity.HasOne(d => d.Salary).WithMany(p => p.ProjectPayments)
                .HasForeignKey(d => d.SalaryId)
                .HasConstraintName("FK__ProjectPa__Salar__7C4F7684");
        });

        modelBuilder.Entity<SalaryEmployeeMonth>(entity =>
        {
            entity.HasKey(e => e.SalaryId).HasName("PK__SalaryEm__4BE2045780727A81");

            entity.ToTable("SalaryEmployeeMonth");

            entity.Property(e => e.FinallyAmount).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.FixSalary).HasColumnType("decimal(8, 2)");

            entity.HasOne(d => d.Employee).WithMany(p => p.SalaryEmployeeMonths)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__SalaryEmp__Emplo__797309D9");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
