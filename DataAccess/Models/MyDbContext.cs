using System;
using System.Collections.Generic;
using ForQab.Models;
using Microsoft.EntityFrameworkCore;

namespace ForQab.DataAccess.Models;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Commission> Commissions { get; set; }

    public virtual DbSet<DimRepresentative> DimRepresentatives { get; set; }

    public virtual DbSet<District> Districts { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<ExamBuilding> ExamBuildings { get; set; }

    public virtual DbSet<ExamType> ExamTypes { get; set; }

    public virtual DbSet<ExamRoom> ExamRooms { get; set; }

    public virtual DbSet<ExamCommission> ExamCommissions { get; set; }

    public virtual DbSet<ExamMonitor> ExamMonitors { get; set; }

    public virtual DbSet<MonitorsProfession> MonitorsProfessions { get; set; }

    public virtual DbSet<ExamRepresentative> ExamRepresentatives { get; set; }

    public virtual DbSet<ExamDegree> ExamDegrees { get; set; }

    public virtual DbSet<ExamSubject> ExamSubjects { get; set; }

    public virtual DbSet<ExpertsProfession> ExpertsProfessions { get; set; }

    public virtual DbSet<ExamExpertSubProfession> ExamExpertSubProfessions { get; set; }

    public virtual DbSet<ExamSubCommission> ExamSubCommissions { get; set; }

    public virtual DbSet<Expert> Experts { get; set; }

    public virtual DbSet<Degree> Degrees { get; set; }

    public virtual DbSet<ExpertLog> ExpertLogs { get; set; }

    public virtual DbSet<Gender> Genders { get; set; }

    public virtual DbSet<Monitor> Monitors { get; set; }

    public virtual DbSet<MonitorLog> MonitorLogs { get; set; }

    public virtual DbSet<Profession> Professions { get; set; }

    public virtual DbSet<Region> Regions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<SubCommission> SubCommissions { get; set; }

    public virtual DbSet<SubProfession> SubProfessions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WorkerType> WorkerTypes { get; set; }

    public virtual DbSet<NaturaType> NaturaTypes { get; set; }

    public virtual DbSet<FinancialRate> FinancialRates { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });
        modelBuilder.Entity<ExamCommission>()
        .ToTable("Exam_Commissions"); // Tablo adını doğru belirtin

        modelBuilder.Entity<ExamCommission>()
            .HasKey(ec => new { ec.ExamId, ec.CommissionId }); // Composite key tanımlayın

        modelBuilder.Entity<ExpertsProfession>()
            .HasKey(ec => new { ec.ExpertId, ec.SubProfessionId });


        modelBuilder.Entity<ExamCommission>()
            .HasOne(ec => ec.Exam)
            .WithMany(e => e.ExamCommissions)
            .HasForeignKey(ec => ec.ExamId);

        modelBuilder.Entity<ExamCommission>()
            .HasOne(ec => ec.Commission)
            .WithMany(c => c.ExamCommissions)
            .HasForeignKey(ec => ec.CommissionId);

        modelBuilder.Entity<ExamDegree>()
        .ToTable("Exam_Degrees");

        modelBuilder.Entity<ExamDegree>()
            .HasKey(ec => new { ec.ExamId, ec.DegreeId });

        modelBuilder.Entity<ExamDegree>()
            .HasOne(ec => ec.Exams)
            .WithMany(e => e.ExamDegrees)
            .HasForeignKey(ec => ec.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ExamDegree>()
            .HasOne(ec => ec.Degrees)
            .WithMany(c => c.ExamDegrees)
            .HasForeignKey(ec => ec.DegreeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ExamSubject>()
            .HasKey(ec => new { ec.ExamId, ec.SubjectId });

        modelBuilder.Entity<ExamSubject>()
            .HasOne(ec => ec.Exams)
            .WithMany(e => e.ExamSubjects)
            .HasForeignKey(ec => ec.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ExamSubject>()
            .HasOne(ec => ec.Subjects)
            .WithMany(c => c.ExamSubjects)
            .HasForeignKey(ec => ec.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasOne(d => d.Section).WithMany(p => p.Subjects).HasConstraintName("FK_subject_sections");
        });

        modelBuilder.Entity<ExamExpertSubProfession>()
        .ToTable("Exam_Expert_SubProfessions");

        modelBuilder.Entity<ExamExpertSubProfession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ExamId, e.ExpertId })
                  .IsUnique();
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        }); 
        modelBuilder.Entity<MonitorsProfession>()
            .HasKey(ec => new { ec.MonitorId, ec.SubProfessionId });

        modelBuilder.Entity<ExamExpertSubProfession>()
            .HasOne(ec => ec.Exam)
            .WithMany(e => e.ExamExpertSubProfessions)
            .HasForeignKey(ec => ec.ExamId);

        modelBuilder.Entity<ExamExpertSubProfession>()
            .HasOne(ec => ec.SubProfession)
            .WithMany(c => c.ExamExpertSubProfessions)
            .HasForeignKey(ec => ec.SubProfessionId);

        modelBuilder.Entity<ExamExpertSubProfession>()
            .HasOne(ec => ec.Expert)
            .WithMany(c => c.ExamExpertSubProfessions)
            .HasForeignKey(ec => ec.ExpertId);

        modelBuilder.Entity<ExamExpertSubProfession>()
            .HasOne(ec => ec.Federation)
            .WithMany(c => c.ExamExpertSubProfessions)
            .HasForeignKey(ec => ec.FederationId);

        modelBuilder.Entity<ExamExpertSubProfession>()
            .HasOne(ec => ec.ExamRoom)
            .WithMany(c => c.ExamExpertSubProfessions)
            .HasForeignKey(ec => ec.RoomId);


        modelBuilder.Entity<Commission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__commissi__3213E83F3B4ABE27");

            entity.HasOne(d => d.Section).WithMany(p => p.Commissions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__commissio__secti__5441852A");
        });

        modelBuilder.Entity<DimRepresentative>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__dim_repr__3213E83F4D403BCE");
        });
        modelBuilder.Entity<ExamRepresentative>()
            .HasKey(ec => new { ec.ExamId, ec.RepresentativeId });

        modelBuilder.Entity<District>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__district__3213E83F8DDCBF8C");

            entity.HasOne(d => d.Region).WithMany(p => p.Districts).HasConstraintName("FK__districts__regio__0E6E26BF");
        });
        modelBuilder.Entity<Contract>()
        .HasOne(c => c.Expert)
        .WithMany(e => e.Contracts)
        .HasForeignKey(c => c.ExpertId)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Contract>()
            .HasOne(c => c.Monitor)
            .WithMany(m => m.Contracts)
            .HasForeignKey(c => c.MonitorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__exams__3213E83F47E1B267");

            entity.HasOne(d => d.ExamBuilding).WithMany(p => p.Exams)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__exams__exam_buld__59C55456");

            entity.HasOne(d => d.ExamType).WithMany(p => p.Exams)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__exams__types");

            entity.HasOne(d => d.District).WithMany(p => p.Exams)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Section).WithMany(p => p.Exams)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__exams__section_i__56E8E7AB");

            entity.HasMany(d => d.Experts).WithMany(p => p.Exams)
                .UsingEntity<Dictionary<string, object>>(
                    "ExamExpert",
                    r => r.HasOne<Expert>().WithMany()
                        .HasForeignKey("ExpertId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Exam_Experts_2"),
                    l => l.HasOne<Exam>().WithMany()
                        .HasForeignKey("ExamId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Exam_Experts"),
                    j =>
                    {
                        j.HasKey("ExamId", "ExpertId");
                        j.ToTable("Exam_Experts");
                    });

            entity.HasMany(d => d.Monitors).WithMany(p => p.Exams)
                .UsingEntity<Dictionary<string, object>>(
                    "ExamMonitor",
                    r => r.HasOne<Monitor>().WithMany()
                        .HasForeignKey("MonitorId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Exam_Monitors_2"),
                    l => l.HasOne<Exam>().WithMany()
                        .HasForeignKey("ExamId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Exam_Monitors"),
                    j =>
                    {
                        j.HasKey("ExamId", "MonitorId");
                        j.ToTable("Exam_Monitors");
                    });

            modelBuilder.Entity<ExamMonitor>()
            .HasKey(ec => new { ec.ExamId, ec.MonitorId });

            entity.HasMany(d => d.Representatives).WithMany(p => p.Exams)
                .UsingEntity<Dictionary<string, object>>(
                    "ExamRepresentative",
                    r => r.HasOne<DimRepresentative>().WithMany()
                        .HasForeignKey("RepresentativeId")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    l => l.HasOne<Exam>().WithMany()
                        .HasForeignKey("ExamId")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey("ExamId", "RepresentativeId");
                        j.ToTable("Exam_Representatives");
                    });


            modelBuilder.Entity<ExamMonitor>()
                .HasOne(ec => ec.ExamRooms)
                .WithMany(c => c.ExamMonitors)
                .HasForeignKey(ec => ec.RoomId);
        });

        modelBuilder.Entity<ExamBuilding>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__exam_bui__3213E83FF7187075");

            entity.HasOne(d => d.Section).WithMany(p => p.ExamBuildings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__exam_buil__secti__5165187F");
        });

        modelBuilder.Entity<ExamRoom>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(d => d.Section).WithMany(p => p.ExamRooms)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Expert>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__experts__3213E83FCCBE8A03");

            entity.Property(e => e.AssignmentCount).HasDefaultValue(0);

            entity.HasOne(d => d.DistrictNavigation).WithMany(p => p.Experts).HasConstraintName("FK__districts_experts");

            entity.HasOne(d => d.FederationNavigation).WithMany(p => p.Experts).HasConstraintName("FK__expert_federation");

            entity.HasOne(d => d.GenderNavigation).WithMany(p => p.Experts).HasConstraintName("FK__monitor_experts");

            entity.HasOne(d => d.Section).WithMany(p => p.Experts).HasConstraintName("FK_Experts_Sections");



        });
        modelBuilder.Entity<ExpertsProfession>()
                .HasKey(ep => new { ep.ExpertId, ep.SubProfessionId });

        modelBuilder.Entity<ExpertsProfession>()
            .HasOne(ep => ep.Expert)
            .WithMany(e => e.ExpertsProfessions)
            .HasForeignKey(ep => ep.ExpertId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ExpertsProfession>()
            .HasOne(ep => ep.SubProfession)
            .WithMany(sp => sp.ExpertsProfessions)
            .HasForeignKey(ep => ep.SubProfessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ExpertLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_expert_log_id");

            entity.HasOne(d => d.Expert).WithMany(p => p.ExpertLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_expert_logs_1");
        });

        modelBuilder.Entity<Gender>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__genders__3213E83FC52E1E40");
        });

        modelBuilder.Entity<Monitor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__supervis__3213E83FAB6F6A67");

            entity.HasOne(d => d.DistrictNavigation).WithMany(p => p.Monitors).HasConstraintName("FK__monitor_district");

            entity.HasOne(d => d.GenderNavigation).WithMany(p => p.Monitors).HasConstraintName("FK__monitor_genders");

            entity.HasOne(d => d.RoleNavigation).WithMany(p => p.Monitors).HasConstraintName("FK__monitor_role");

            entity.HasOne(d => d.WorkerTypeNavigation).WithMany(p => p.Monitors).HasConstraintName("FK__monitor_workertype");

            entity.HasOne(d => d.NaturaTypeNavigation).WithMany(p => p.Monitors).HasConstraintName("FK__monitor_naturatype");

            entity.HasOne(d => d.ExamBuilding).WithMany(p => p.Monitors).HasConstraintName("FK__monitor_building");

            entity.HasOne(d => d.Section).WithMany(p => p.Monitors).HasConstraintName("FK__superviso__secti__60A75C0F");

        });

        modelBuilder.Entity<MonitorLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__supervis__3213E83F0BEE1DF8");

            entity.HasOne(d => d.Supervisor).WithMany(p => p.MonitorLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__superviso__super__6B24EA82");
        });

        modelBuilder.Entity<Profession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__professi__3213E83FCA289D09");

            entity.HasOne(d => d.Section).WithMany(p => p.Professions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Professions_Sections");
        });

        modelBuilder.Entity<Region>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__regions__3213E83F8E8C9763");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__roles__3213E83F4B5228FB");
        });

        modelBuilder.Entity<WorkerType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__workertypes__3213E83F4B5228FB");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__sections__3213E83F295BA8C8");
        });

        modelBuilder.Entity<SubCommission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__sub_comm__3213E83FFB765CF8");

            entity.HasOne(d => d.Commission).WithMany(p => p.SubCommissions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__sub_commi__commi__5812160E");

            entity.HasOne(d => d.Section).WithMany(p => p.SubCommissions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__sub_commi__secti__571DF1D5");
        });

        modelBuilder.Entity<SubProfession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__sub_prof__3213E83F14C802DD");

            entity.HasOne(d => d.Profession).WithMany(p => p.SubProfessions).HasConstraintName("FK_Professions_Sub_Professions");

            entity.HasOne(d => d.Section).WithMany(p => p.SubProfessions).HasConstraintName("FK_Sub_Professions_Sections");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3213E83FD3D6EAF2");

            entity.HasOne(d => d.Section).WithMany(p => p.Users).HasConstraintName("FK_Users_Sections");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}