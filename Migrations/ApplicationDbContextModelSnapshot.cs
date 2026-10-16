using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "10.0.0")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("RaceDay.Models.User", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
                b.Property<string>("Email").IsRequired().HasMaxLength(256).HasColumnType("nvarchar(256)");
                b.Property<string>("PasswordHash").IsRequired().HasColumnType("nvarchar(max)");
                b.Property<string>("FirstName").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
                b.Property<string>("LastName").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
                b.Property<string>("PhoneNumber").HasMaxLength(30).HasColumnType("nvarchar(30)");
                b.Property<string>("Role").IsRequired().HasMaxLength(50).HasColumnType("nvarchar(50)");
                b.Property<DateTime?>("DateOfBirth").HasColumnType("datetime2");
                b.Property<string>("Gender").HasMaxLength(50).HasColumnType("nvarchar(50)");
                b.Property<string>("IdNumber").HasMaxLength(50).HasColumnType("nvarchar(50)");
                b.Property<string>("EmergencyContactName").HasMaxLength(200).HasColumnType("nvarchar(200)");
                b.Property<string>("EmergencyContactNumber").HasMaxLength(30).HasColumnType("nvarchar(30)");
                b.Property<string>("MedicalConditions").HasColumnType("nvarchar(max)");
                b.Property<string>("ClubAffiliation").HasMaxLength(200).HasColumnType("nvarchar(200)");
                b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
                b.HasKey("Id");
                b.HasIndex("Email").IsUnique();
                b.ToTable("Users");
            });

            modelBuilder.Entity("RaceDay.Models.Organiser", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
                b.Property<int>("UserId").HasColumnType("int");
                b.Property<string>("OrganisationName").HasMaxLength(200).HasColumnType("nvarchar(200)");
                b.HasKey("Id");
                b.HasIndex("UserId").IsUnique();
                b.ToTable("Organisers");
            });

            modelBuilder.Entity("RaceDay.Models.Participant", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
                b.Property<int>("UserId").HasColumnType("int");
                b.HasKey("Id");
                b.HasIndex("UserId").IsUnique();
                b.ToTable("Participants");
            });

            modelBuilder.Entity("RaceDay.Models.Event", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
                b.Property<string>("Title").IsRequired().HasMaxLength(200).HasColumnType("nvarchar(200)");
                b.Property<string>("Description").HasColumnType("nvarchar(max)");
                b.Property<DateTime>("EventDate").HasColumnType("datetime2");
                b.Property<TimeSpan?>("EventTime").HasColumnType("time");
                b.Property<string>("Venue").HasMaxLength(200).HasColumnType("nvarchar(200)");
                b.Property<string>("City").HasMaxLength(100).HasColumnType("nvarchar(100)");
                b.Property<string>("Province").HasMaxLength(100).HasColumnType("nvarchar(100)");
                b.Property<string>("EventType").HasMaxLength(100).HasColumnType("nvarchar(100)");
                b.Property<decimal>("Distance").HasColumnType("decimal(8,2)");
                b.Property<string>("DistanceUnit").IsRequired().HasMaxLength(10).HasColumnType("nvarchar(10)");
                b.Property<int?>("MaxParticipants").HasColumnType("int");
                b.Property<DateTime?>("RegistrationOpenDate").HasColumnType("datetime2");
                b.Property<DateTime?>("RegistrationCloseDate").HasColumnType("datetime2");
                b.Property<string>("Metadata").HasColumnType("nvarchar(max)");
                b.Property<string>("Status").HasMaxLength(50).HasColumnType("nvarchar(50)");
                b.Property<int>("OrganiserId").HasColumnType("int");
                b.HasKey("Id");
                b.HasIndex("OrganiserId");
                b.ToTable("Events");
            });

            modelBuilder.Entity("RaceDay.Models.Category", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
                b.Property<int>("EventId").HasColumnType("int");
                b.Property<string>("CategoryName").IsRequired().HasMaxLength(200).HasColumnType("nvarchar(200)");
                b.Property<string>("Description").HasColumnType("nvarchar(max)");
                b.Property<decimal>("Distance").HasColumnType("decimal(8,2)");
                b.Property<string>("DistanceUnit").HasMaxLength(10).HasColumnType("nvarchar(10)");
                b.Property<int?>("AgeGroupMin").HasColumnType("int");
                b.Property<int?>("AgeGroupMax").HasColumnType("int");
                b.Property<string>("GenderRestriction").HasMaxLength(20).HasColumnType("nvarchar(20)");
                b.Property<decimal>("EntryFee").HasColumnType("decimal(10,2)");
                b.Property<decimal?>("LateEntryFee").HasColumnType("decimal(10,2)");
                b.Property<int?>("MaxParticipants").HasColumnType("int");
                b.Property<TimeSpan?>("StartTime").HasColumnType("time");
                b.Property<TimeSpan?>("CutOffTime").HasColumnType("time");
                b.HasKey("Id");
                b.HasIndex("EventId");
                b.ToTable("Categories");
            });

            modelBuilder.Entity("RaceDay.Models.Enrolment", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
                b.Property<int>("ParticipantId").HasColumnType("int");
                b.Property<int>("EventId").HasColumnType("int");
                b.Property<int>("CategoryId").HasColumnType("int");
                b.Property<string>("Status").HasMaxLength(50).HasColumnType("nvarchar(50)");
                b.Property<DateTime>("EnrolmentDate").HasColumnType("datetime2");
                b.HasKey("Id");
                b.HasIndex("CategoryId");
                b.HasIndex("EventId");
                b.HasIndex("ParticipantId");
                b.ToTable("Enrolments");
            });

            modelBuilder.Entity("RaceDay.Models.Result", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
                b.Property<int>("EnrolmentId").HasColumnType("int");
                b.Property<TimeSpan?>("FinishTime").HasColumnType("time");
                b.Property<TimeSpan?>("GunTime").HasColumnType("time");
                b.Property<TimeSpan?>("ChipTime").HasColumnType("time");
                b.Property<int?>("OverallPosition").HasColumnType("int");
                b.Property<int?>("GenderPosition").HasColumnType("int");
                b.Property<int?>("CategoryPosition").HasColumnType("int");
                b.Property<string>("Pace").HasMaxLength(50).HasColumnType("nvarchar(50)");
                b.Property<bool>("IsDisqualified").HasColumnType("bit");
                b.Property<string>("DisqualificationReason").HasColumnType("nvarchar(max)");
                b.Property<string>("ResultNotes").HasColumnType("nvarchar(max)");
                b.Property<DateTime>("RecordedAt").HasColumnType("datetime2");
                b.HasKey("Id");
                b.HasIndex("EnrolmentId").IsUnique();
                b.ToTable("Results");
            });

            modelBuilder.Entity("RaceDay.Models.Organiser", b =>
            {
                b.HasOne("RaceDay.Models.User", null)
                    .WithOne("Organiser")
                    .HasForeignKey("RaceDay.Models.Organiser", "UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("RaceDay.Models.Participant", b =>
            {
                b.HasOne("RaceDay.Models.User", null)
                    .WithOne("Participant")
                    .HasForeignKey("RaceDay.Models.Participant", "UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("RaceDay.Models.Event", b =>
            {
                b.HasOne("RaceDay.Models.Organiser", null)
                    .WithMany("Events")
                    .HasForeignKey("OrganiserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("RaceDay.Models.Category", b =>
            {
                b.HasOne("RaceDay.Models.Event", null)
                    .WithMany("Categories")
                    .HasForeignKey("EventId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("RaceDay.Models.Enrolment", b =>
            {
                b.HasOne("RaceDay.Models.Category", null)
                    .WithMany("Enrolments")
                    .HasForeignKey("CategoryId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("RaceDay.Models.Event", null)
                    .WithMany("Enrolments")
                    .HasForeignKey("EventId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("RaceDay.Models.Participant", null)
                    .WithMany("Enrolments")
                    .HasForeignKey("ParticipantId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("RaceDay.Models.Result", b =>
            {
                b.HasOne("RaceDay.Models.Enrolment", null)
                    .WithOne("Result")
                    .HasForeignKey("RaceDay.Models.Result", "EnrolmentId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });
        }
    }
}
