// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Testing;

namespace EFCore5Net5.Migrations
{
    [DbContext(typeof(EFModel1))]
    partial class EFModel1ModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("dbo")
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("Testing.Entity1", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityColumn();

                    b.Property<long>("EntityImplementationId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("EntityImplementationId")
                        .IsUnique();

                    b.ToTable("Entity1");
                });

            modelBuilder.Entity("Testing.EntityAbstract", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityColumn();

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("EntityAbstract");

                    b.HasDiscriminator<string>("Discriminator").HasValue("EntityAbstract");
                });

            modelBuilder.Entity("Testing.EntityRelated", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityColumn();

                    b.Property<long>("EntityAbstractId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("EntityAbstractId");

                    b.ToTable("EntityRelated");
                });

            modelBuilder.Entity("Testing.EntityImplementation", b =>
                {
                    b.HasBaseType("Testing.EntityAbstract");

                    b.Property<string>("Test")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.HasDiscriminator().HasValue("EntityImplementation");
                });

            modelBuilder.Entity("Testing.Entity1", b =>
                {
                    b.HasOne("Testing.EntityImplementation", "EntityImplementation")
                        .WithOne("Entity1")
                        .HasForeignKey("Testing.Entity1", "EntityImplementationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EntityImplementation");
                });

            modelBuilder.Entity("Testing.EntityRelated", b =>
                {
                    b.HasOne("Testing.EntityAbstract", "EntityAbstract")
                        .WithMany("EntityRelated")
                        .HasForeignKey("EntityAbstractId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EntityAbstract");
                });

            modelBuilder.Entity("Testing.EntityAbstract", b =>
                {
                    b.Navigation("EntityRelated");
                });

            modelBuilder.Entity("Testing.EntityImplementation", b =>
                {
                    b.Navigation("Entity1");
                });
#pragma warning restore 612, 618
        }
    }
}
