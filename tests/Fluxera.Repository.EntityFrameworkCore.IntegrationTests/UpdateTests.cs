namespace Fluxera.Repository.EntityFrameworkCore.IntegrationTests
{
	using System;
	using System.Threading.Tasks;
	using Fluxera.Repository.UnitTests.Core;
	using Fluxera.Repository.UnitTests.Core.CompanyAggregate;
	using Fluxera.Repository.UnitTests.Core.PersonAggregate;
	using Microsoft.Extensions.DependencyInjection;
	using NUnit.Framework;

	[TestFixture(true)]
	[TestFixture(false)]
	public class UpdateTests : UpdateTestBase
	{
		/// <inheritdoc />
		public UpdateTests(bool isUnitOfWorkEnabled)
			: base(isUnitOfWorkEnabled)
		{
		}

		/// <inheritdoc />
		protected override void AddRepositoryUnderTest(
			IRepositoryBuilder repositoryBuilder,
			string repositoryName,
			Action<IRepositoryOptionsBuilder> configureOptions)
		{
			repositoryBuilder.Services.AddDbContext<RepositoryDbContext>();

			repositoryBuilder.AddEntityFrameworkRepository<RepositoryContext>(repositoryName, configureOptions.Invoke);
		}

		[Test]
		public async Task ShouldAddNewPerson()
		{
			#region Create new Person

			Person personFromUi = new Person();
			personFromUi.Name = Guid.NewGuid()
				.ToString();
			personFromUi.Address.City = Guid.NewGuid()
				.ToString();
			personFromUi.Address.PostCode = Guid.NewGuid()
				.ToString();
			personFromUi.Address.Street = Guid.NewGuid()
				.ToString();
			personFromUi.Address.Number = Guid.NewGuid()
				.ToString();
			personFromUi.Age = 99;

			await this.PersonRepository.AddAsync(personFromUi);
			await this.UnitOfWork.SaveChangesAsync();
			Assert.NotNull(personFromUi.ID);

			#endregion

			#region Confirm it exists

			bool exists = await this.PersonRepository.ExistsAsync(personFromUi.ID);
			Assert.IsTrue(exists);

			var newPerson = await this.PersonRepository.GetAsync(personFromUi.ID);
			Assert.AreEqual(99, newPerson.Age);

			#endregion
		}

		[Test]
		// This test fails
		public async Task ShouldAddNewPersonAndUpdate_ExistingCode_UpdatedByUI()
		{
			#region Prepare test person

			#region Create new Person

			Person originalDatabasePerson = new Person();
			originalDatabasePerson.Name = Guid.NewGuid()
				.ToString();
			originalDatabasePerson.Address.City = Guid.NewGuid()
				.ToString();
			originalDatabasePerson.Address.PostCode = Guid.NewGuid()
				.ToString();
			originalDatabasePerson.Address.Street = Guid.NewGuid()
				.ToString();
			originalDatabasePerson.Address.Number = Guid.NewGuid()
				.ToString();
			originalDatabasePerson.Age = 99;
			await this.PersonRepository.AddAsync(originalDatabasePerson);
			await this.UnitOfWork.SaveChangesAsync();
			Assert.NotNull(originalDatabasePerson.ID);

			#endregion

			#region Confirm it exists

			bool exists = await this.PersonRepository.ExistsAsync(originalDatabasePerson.ID);
			Assert.IsTrue(exists);

			#endregion

			#endregion

			// At this point, the person exists... lets assume the following actions:
			//  User gets the person => /api/persons/get/:id
			// User then updates the person and sends a POST to /api/person/update
			Person updatedPersonFromUi_POST_endpoint = new Person
			{
				ID = originalDatabasePerson.ID,
				Name = originalDatabasePerson.Name,

				// We update the person to age 50
				Age = 50,
				Address = originalDatabasePerson.Address
			};

			var id = originalDatabasePerson.ID;

			Assert.AreEqual(id, originalDatabasePerson.ID);
			Assert.AreEqual(99, originalDatabasePerson.Age);
			await this.PersonRepository.UpdateAsync(updatedPersonFromUi_POST_endpoint);
			await this.UnitOfWork.SaveChangesAsync();
			var confirmPersonWasUpdated = await this.PersonRepository.GetAsync(originalDatabasePerson.ID);
			Assert.AreEqual(50, confirmPersonWasUpdated.Age);
			Assert.AreEqual(id, confirmPersonWasUpdated.ID);
		}

		[Test]
		// This passes
		public async Task ShouldAddNewPersonAndUpdate_RefactoredCode_UpdatedByUI()
		{
			#region Prepare test person

			#region Create new Person

			Person originalDatabasePerson = new Person();
			originalDatabasePerson.Name = Guid.NewGuid()
				.ToString();
			originalDatabasePerson.Address.City = Guid.NewGuid()
				.ToString();
			originalDatabasePerson.Address.PostCode = Guid.NewGuid()
				.ToString();
			originalDatabasePerson.Address.Street = Guid.NewGuid()
				.ToString();
			originalDatabasePerson.Address.Number = Guid.NewGuid()
				.ToString();
			originalDatabasePerson.Age = 99;
			await this.PersonRepository.AddAsync(originalDatabasePerson);
			await this.UnitOfWork.SaveChangesAsync();
			Assert.NotNull(originalDatabasePerson.ID);

			#endregion

			#region Confirm the person exists

			bool exists = await this.PersonRepository.ExistsAsync(originalDatabasePerson.ID);
			Assert.IsTrue(exists);

			#endregion

			#endregion

			// At this point, the person exists... lets assume the following actions:
			//  User gets the person => /api/persons/get/:id
			// User then updates the person and sends a POST to /api/person/update

			#region Simulate (sorta) the person getting updated from the UI

			Guid executeRefactoredCode = Guid.Parse("bd91d6d1-9bcc-4eb2-a67b-dc34ab5e5174");
			Person updatedPersonFromUi_POST_endpoint = new Person
			{
				ID = originalDatabasePerson.ID,

				// This will execute the refactored code
				Name = executeRefactoredCode.ToString(),

				// We update the person to age 50
				Age = 50,
				Address = originalDatabasePerson.Address
			};

			var id = originalDatabasePerson.ID;

			Assert.AreEqual(id, originalDatabasePerson.ID);
			Assert.AreEqual(99, originalDatabasePerson.Age);
			await this.PersonRepository.UpdateAsync(updatedPersonFromUi_POST_endpoint);
			await this.UnitOfWork.SaveChangesAsync();
			var confirmPersonWasUpdated = await this.PersonRepository.GetAsync(originalDatabasePerson.ID);
			Assert.AreEqual(50, confirmPersonWasUpdated.Age);
			Assert.AreEqual(id, confirmPersonWasUpdated.ID);

			#endregion
		}

		[Test]
		// This passes
		public async Task ShouldAddNewPersonAndUpdate_RefactoredCode_UpdatedByService()
		{
			#region Prepare test person

			#region Create new Person

			Person originalDatabasePerson = new Person();
			originalDatabasePerson.Name = Guid.NewGuid()
				.ToString();
			originalDatabasePerson.Address.City = Guid.NewGuid()
				.ToString();
			originalDatabasePerson.Address.PostCode = Guid.NewGuid()
				.ToString();
			originalDatabasePerson.Address.Street = Guid.NewGuid()
				.ToString();
			originalDatabasePerson.Address.Number = Guid.NewGuid()
				.ToString();
			originalDatabasePerson.Age = 99;
			await this.PersonRepository.AddAsync(originalDatabasePerson);
			await this.UnitOfWork.SaveChangesAsync();
			Assert.NotNull(originalDatabasePerson.ID);

			#endregion

			#region Confirm the person exists

			bool exists = await this.PersonRepository.ExistsAsync(originalDatabasePerson.ID);
			Assert.IsTrue(exists);

			#endregion

			#endregion

			// At this point, the person exists... lets assume the following actions:
			// Some random event happens elsewhere in the code, now we need to
			// change the age of of the person 

			#region Updating the person from the database tracked context.

			Guid executeRefactoredCode = Guid.Parse("bd91d6d1-9bcc-4eb2-a67b-dc34ab5e5174");
			var personDbo = await this.PersonRepository.GetAsync(originalDatabasePerson.ID);
			var id = personDbo.ID;

			Assert.AreEqual(id, personDbo.ID);
			Assert.AreEqual(99, personDbo.Age);

			// Arrange
			personDbo.Age = 50;

			// This executes the refactored code
			personDbo.Name = executeRefactoredCode.ToString();

			await this.PersonRepository.UpdateAsync(personDbo);
			await this.UnitOfWork.SaveChangesAsync();
			var confirmPersonWasUpdated = await this.PersonRepository.GetAsync(originalDatabasePerson.ID);
			Assert.AreEqual(50, confirmPersonWasUpdated.Age);
			Assert.AreEqual(id, confirmPersonWasUpdated.ID);

			#endregion
		}
		
		[Test]
		// This passes
		public async Task ShouldAddNewPersonAndUpdate_ExistingCode_UpdatedByService()
		{
			#region Prepare test person

			#region Create new Person

			Person originalDatabasePerson = new Person();
			originalDatabasePerson.Name = Guid.NewGuid()
				.ToString();
			originalDatabasePerson.Address.City = Guid.NewGuid()
				.ToString();
			originalDatabasePerson.Address.PostCode = Guid.NewGuid()
				.ToString();
			originalDatabasePerson.Address.Street = Guid.NewGuid()
				.ToString();
			originalDatabasePerson.Address.Number = Guid.NewGuid()
				.ToString();
			originalDatabasePerson.Age = 99;
			await this.PersonRepository.AddAsync(originalDatabasePerson);
			await this.UnitOfWork.SaveChangesAsync();
			Assert.NotNull(originalDatabasePerson.ID);

			#endregion

			#region Confirm the person exists

			bool exists = await this.PersonRepository.ExistsAsync(originalDatabasePerson.ID);
			Assert.IsTrue(exists);

			#endregion

			#endregion

			// At this point, the person exists... lets assume the following actions:
			// Some random event happens elsewhere in the code, now we need to
			// change the age of of the person 

			#region Updating the person from the database tracked context.


			var personDbo = await this.PersonRepository.GetAsync(originalDatabasePerson.ID);
			var id = personDbo.ID;

			Assert.AreEqual(id, personDbo.ID);
			Assert.AreEqual(99, personDbo.Age);

			// Arrange
			personDbo.Age = 50;

			await this.PersonRepository.UpdateAsync(personDbo);
			await this.UnitOfWork.SaveChangesAsync();
			var confirmPersonWasUpdated = await this.PersonRepository.GetAsync(originalDatabasePerson.ID);
			Assert.AreEqual(50, confirmPersonWasUpdated.Age);
			Assert.AreEqual(id, confirmPersonWasUpdated.ID);

			#endregion
		}
	}
}
