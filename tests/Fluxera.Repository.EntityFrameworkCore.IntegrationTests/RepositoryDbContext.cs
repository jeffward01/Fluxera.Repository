﻿namespace Fluxera.Repository.EntityFrameworkCore.IntegrationTests
{
	using Fluxera.Repository.UnitTests.Core.CompanyAggregate;
	using Fluxera.Repository.UnitTests.Core.EmployeeAggregate;
	using Fluxera.Repository.UnitTests.Core.PersonAggregate;
	using Fluxera.Repository.UnitTests.Core.ReferenceAggregate;
	using JetBrains.Annotations;
	using Microsoft.EntityFrameworkCore;

	[PublicAPI]
	public sealed class RepositoryDbContext : DbContext
	{
		private readonly string databaseName;

		public RepositoryDbContext()
		{
		}

		internal RepositoryDbContext(string databaseName)
		{
			this.databaseName = databaseName;
		}

		public DbSet<Person> People { get; set; }

		public DbSet<Company> Companies { get; set; }

		public DbSet<Employee> Employees { get; set; }

		public DbSet<Reference> References { get; set; }

		/// <inheritdoc />
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if(!optionsBuilder.IsConfigured)
			{
				optionsBuilder.UseSqlServer(this.databaseName is null
					? GlobalFixture.ConnectionString
					: GlobalFixture.ConnectionString.Replace($"Database={GlobalFixture.Database}", $"Database={this.databaseName}"));
			}
		}

		/// <inheritdoc />
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Person>(entity =>
			{
				entity.ToTable("People");
				entity.OwnsOne(x => x.Address);

				entity.UseRepositoryDefaults();
			});

			modelBuilder.Entity<Company>(entity =>
			{
				entity.UseRepositoryDefaults();
			});

			modelBuilder.Entity<Employee>(entity =>
			{
				entity
					.Property(x => x.SalaryDecimal)
					.HasConversion<double>();

				entity
					.Property(x => x.SalaryNullableDecimal)
					.HasConversion<double?>();

				entity.UseRepositoryDefaults();
			});

			modelBuilder.Entity<Reference>(entity =>
			{
				entity.UseRepositoryDefaults();
			});
		}
	}
}
