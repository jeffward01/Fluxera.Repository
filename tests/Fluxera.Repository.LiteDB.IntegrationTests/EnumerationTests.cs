﻿namespace Fluxera.Repository.LiteDB.IntegrationTests
{
	using System;
	using Fluxera.Repository.UnitTests.Core;
	using Fluxera.Repository.UnitTests.Core.CompanyAggregate;
	using Fluxera.Repository.UnitTests.Core.PersonAggregate;
	using NUnit.Framework;

	[TestFixture]
	public class EnumerationTests : EnumerationTestsBase
	{
		/// <inheritdoc />
		protected override void AddRepositoryUnderTest(IRepositoryBuilder repositoryBuilder,
			string repositoryName, Action<IRepositoryOptionsBuilder> configureOptions)
		{
			repositoryBuilder.AddLiteRepository(repositoryName, options =>
				{
					options.AddSetting("Lite.Database", "test.db");

					configureOptions.Invoke(options);
				},
				mapper =>
				{
					mapper.Entity<Person>()
						.Id(x => x.ID);

					mapper.Entity<Company>()
						.Id(x => x.ID);
				});
		}
	}
}