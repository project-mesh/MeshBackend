﻿// <auto-generated />
using System;
using MeshBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MeshBackend.Migrations
{
    [DbContext(typeof(MeshContext))]
    partial class MeshContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("MeshBackend.Models.Admin", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<string>("Nickname")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<string>("PasswordDigest")
                        .HasColumnType("varchar(70)")
                        .HasMaxLength(70);

                    b.Property<string>("RememberDigest")
                        .HasColumnType("varchar(70)")
                        .HasMaxLength(70);

                    b.Property<DateTime>("UpdatedTime")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime");

                    b.HasKey("Id");

                    b.ToTable("Admins");
                });

            modelBuilder.Entity("MeshBackend.Models.Assign", b =>
                {
                    b.Property<int>("TaskId")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int?>("AdminId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime");

                    b.Property<DateTime>("UpdatedTime")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime");

                    b.HasKey("TaskId", "Title", "UserId");

                    b.HasIndex("AdminId");

                    b.HasIndex("UserId");

                    b.ToTable("Assigns");
                });

            modelBuilder.Entity("MeshBackend.Models.Bulletin", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("BoardId")
                        .HasColumnType("int");

                    b.Property<string>("Content")
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100);

                    b.Property<DateTime>("CreatedTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<DateTime>("UpdatedTime")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime");

                    b.HasKey("Id");

                    b.HasIndex("BoardId");

                    b.ToTable("Bulletins");
                });

            modelBuilder.Entity("MeshBackend.Models.BulletinBoard", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime");

                    b.Property<int>("ProjectId")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedTime")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime");

                    b.Property<string>("description")
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100);

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.ToTable("BulletinBoards");
                });

            modelBuilder.Entity("MeshBackend.Models.BulletinFeed", b =>
                {
                    b.Property<int>("BulletinId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime");

                    b.Property<DateTime>("UpdatedTime")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("BulletinId");

                    b.HasIndex("UserId");

                    b.ToTable("BulletinFeeds");
                });

            modelBuilder.Entity("MeshBackend.Models.Cooperation", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int>("TeamId")
                        .HasColumnType("int");

                    b.Property<int?>("AdminId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime");

                    b.Property<DateTime>("UpdatedTime")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime");

                    b.HasKey("UserId", "TeamId");

                    b.HasIndex("AdminId");

                    b.HasIndex("TeamId");

                    b.ToTable("Cooperations");
                });

            modelBuilder.Entity("MeshBackend.Models.Project", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("AdminId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<int>("TeamId")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedTime")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime");

                    b.HasKey("Id");

                    b.HasIndex("TeamId");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("MeshBackend.Models.ProjectMemo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("CollectionId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime");

                    b.Property<string>("Text")
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100);

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<DateTime>("UpdatedTime")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CollectionId");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("ProjectMemos");
                });

            modelBuilder.Entity("MeshBackend.Models.ProjectMemoCollection", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime");

                    b.Property<string>("Description")
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100);

                    b.Property<int>("ProjectId")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedTime")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId")
                        .IsUnique();

                    b.ToTable("ProjectMemoCollections");
                });

            modelBuilder.Entity("MeshBackend.Models.Subtask", b =>
                {
                    b.Property<int>("TaskId")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<DateTime>("CreatedTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime");

                    b.Property<string>("Description")
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100);

                    b.Property<bool>("Finished")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<DateTime>("UpdatedTime")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime");

                    b.HasKey("TaskId", "Title");

                    b.ToTable("Subtasks");
                });

            modelBuilder.Entity("MeshBackend.Models.Task", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int?>("AdminId")
                        .HasColumnType("int");

                    b.Property<int>("BoardId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime");

                    b.Property<string>("Description")
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100);

                    b.Property<DateTime>("EndTime")
                        .HasColumnType("datetime");

                    b.Property<bool>("Finished")
                        .HasColumnType("bit");

                    b.Property<int>("LeaderId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<int>("Priority")
                        .HasColumnType("int");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("datetime");

                    b.Property<DateTime>("UpdatedTime")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime");

                    b.HasKey("Id");

                    b.HasIndex("AdminId");

                    b.HasIndex("BoardId");

                    b.HasIndex("LeaderId");

                    b.ToTable("Tasks");
                });

            modelBuilder.Entity("MeshBackend.Models.TaskBoard", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime");

                    b.Property<string>("Description")
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100);

                    b.Property<int>("ProjectId")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedTime")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.ToTable("TaskBoards");
                });

            modelBuilder.Entity("MeshBackend.Models.TaskFeed", b =>
                {
                    b.Property<int>("TaskId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime");

                    b.Property<DateTime>("UpdatedTime")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("TaskId");

                    b.HasIndex("UserId");

                    b.ToTable("TaskFeeds");
                });

            modelBuilder.Entity("MeshBackend.Models.TaskTag", b =>
                {
                    b.Property<int>("TaskId")
                        .HasColumnType("int");

                    b.Property<string>("tag")
                        .HasColumnType("varchar(20)")
                        .HasMaxLength(20);

                    b.Property<DateTime>("CreatedTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime");

                    b.Property<DateTime>("UpdatedTime")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime");

                    b.HasKey("TaskId", "tag");

                    b.ToTable("TaskTags");
                });

            modelBuilder.Entity("MeshBackend.Models.Team", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("AdminId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<DateTime>("UpdatedTime")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime");

                    b.HasKey("Id");

                    b.HasIndex("AdminId");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("MeshBackend.Models.TeamMemo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("CollectionId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime");

                    b.Property<int?>("TeamMemoCollectionId")
                        .HasColumnType("int");

                    b.Property<string>("Text")
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100);

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<DateTime>("UpdatedTime")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TeamMemoCollectionId");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("TeamMemos");
                });

            modelBuilder.Entity("MeshBackend.Models.TeamMemoCollection", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime");

                    b.Property<string>("Description")
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100);

                    b.Property<int>("TeamId")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedTime")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime");

                    b.HasKey("Id");

                    b.HasIndex("TeamId");

                    b.ToTable("TeamMemoCollections");
                });

            modelBuilder.Entity("MeshBackend.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<string>("Nickname")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<string>("PasswordDigest")
                        .HasColumnType("varchar(70)")
                        .HasMaxLength(70);

                    b.Property<string>("RememberDigest")
                        .HasColumnType("varchar(70)")
                        .HasMaxLength(70);

                    b.Property<DateTime>("UpdatedTime")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("MeshBackend.Models.Assign", b =>
                {
                    b.HasOne("MeshBackend.Models.Admin", null)
                        .WithMany("Assigns")
                        .HasForeignKey("AdminId");

                    b.HasOne("MeshBackend.Models.Task", "Task")
                        .WithMany("Assigns")
                        .HasForeignKey("TaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MeshBackend.Models.User", "User")
                        .WithMany("Assigns")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MeshBackend.Models.Bulletin", b =>
                {
                    b.HasOne("MeshBackend.Models.BulletinBoard", "BulletinBoard")
                        .WithMany()
                        .HasForeignKey("BoardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MeshBackend.Models.BulletinBoard", b =>
                {
                    b.HasOne("MeshBackend.Models.Project", "Project")
                        .WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MeshBackend.Models.BulletinFeed", b =>
                {
                    b.HasOne("MeshBackend.Models.Bulletin", "Bulletin")
                        .WithMany()
                        .HasForeignKey("BulletinId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MeshBackend.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MeshBackend.Models.Cooperation", b =>
                {
                    b.HasOne("MeshBackend.Models.Admin", null)
                        .WithMany("Cooperations")
                        .HasForeignKey("AdminId");

                    b.HasOne("MeshBackend.Models.Team", "Team")
                        .WithMany("Cooperations")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MeshBackend.Models.User", "User")
                        .WithMany("Cooperations")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MeshBackend.Models.Project", b =>
                {
                    b.HasOne("MeshBackend.Models.Team", null)
                        .WithMany("Projects")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MeshBackend.Models.ProjectMemo", b =>
                {
                    b.HasOne("MeshBackend.Models.ProjectMemoCollection", "ProjectMemoCollection")
                        .WithMany("ProjectMemos")
                        .HasForeignKey("CollectionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MeshBackend.Models.User", "User")
                        .WithOne("ProjectMemo")
                        .HasForeignKey("MeshBackend.Models.ProjectMemo", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MeshBackend.Models.ProjectMemoCollection", b =>
                {
                    b.HasOne("MeshBackend.Models.Project", "Project")
                        .WithOne("ProjectMemoCollection")
                        .HasForeignKey("MeshBackend.Models.ProjectMemoCollection", "ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MeshBackend.Models.Subtask", b =>
                {
                    b.HasOne("MeshBackend.Models.Task", "Task")
                        .WithMany("Subtasks")
                        .HasForeignKey("TaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MeshBackend.Models.Task", b =>
                {
                    b.HasOne("MeshBackend.Models.Admin", null)
                        .WithMany("Tasks")
                        .HasForeignKey("AdminId");

                    b.HasOne("MeshBackend.Models.TaskBoard", "TaskBoard")
                        .WithMany()
                        .HasForeignKey("BoardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MeshBackend.Models.User", "User")
                        .WithMany("Tasks")
                        .HasForeignKey("LeaderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MeshBackend.Models.TaskBoard", b =>
                {
                    b.HasOne("MeshBackend.Models.Project", "Project")
                        .WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MeshBackend.Models.TaskFeed", b =>
                {
                    b.HasOne("MeshBackend.Models.Task", "Task")
                        .WithMany()
                        .HasForeignKey("TaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MeshBackend.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MeshBackend.Models.TaskTag", b =>
                {
                    b.HasOne("MeshBackend.Models.Task", "Task")
                        .WithMany("TaskTags")
                        .HasForeignKey("TaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MeshBackend.Models.Team", b =>
                {
                    b.HasOne("MeshBackend.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("AdminId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MeshBackend.Models.TeamMemo", b =>
                {
                    b.HasOne("MeshBackend.Models.TeamMemoCollection", "TeamMemoCollection")
                        .WithMany("TeamMemos")
                        .HasForeignKey("TeamMemoCollectionId");

                    b.HasOne("MeshBackend.Models.User", "User")
                        .WithOne("TeamMemo")
                        .HasForeignKey("MeshBackend.Models.TeamMemo", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MeshBackend.Models.TeamMemoCollection", b =>
                {
                    b.HasOne("MeshBackend.Models.Team", "Team")
                        .WithMany()
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
