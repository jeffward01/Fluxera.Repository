namespace Fluxera.Repository.EntityFrameworkCore
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Reflection;
	using System.Threading;
	using System.Threading.Tasks;
	using Fluxera.Entity;
	using Fluxera.Guards;
	using Fluxera.Repository.Options;
	using Fluxera.Repository.Specifications;
	using Microsoft.EntityFrameworkCore;
	using Microsoft.EntityFrameworkCore.ChangeTracking;
	using Microsoft.EntityFrameworkCore.Metadata;

	internal sealed class EntityFrameworkCoreRepository<TAggregateRoot, TKey> : LinqRepositoryBase<TAggregateRoot, TKey>
		where TAggregateRoot : AggregateRoot<TAggregateRoot, TKey>
		where TKey : IComparable<TKey>, IEquatable<TKey>
	{
		private readonly EntityFrameworkCoreContext context;
		private readonly DbSet<TAggregateRoot> dbSet;
		private readonly RepositoryOptions options;

		public EntityFrameworkCoreRepository(
			EntityFrameworkCoreContextProvider contextProvider,
			IRepositoryRegistry repositoryRegistry)
			: base(repositoryRegistry)
		{
			Guard.Against.Null(contextProvider, nameof(contextProvider));
			Guard.Against.Null(repositoryRegistry, nameof(repositoryRegistry));

			RepositoryName repositoryName = repositoryRegistry.GetRepositoryNameFor<TAggregateRoot>();
			this.options = repositoryRegistry.GetRepositoryOptionsFor(repositoryName);

			this.context = contextProvider.GetContextFor(repositoryName);
			this.dbSet = this.context.Set<TAggregateRoot>();
		}

		private static string Name => "Fluxera.Repository.EntityFrameworkCoreRepository";

		/// <inheritdoc />
		protected override IQueryable<TAggregateRoot> Queryable => this.dbSet;

		/// <inheritdoc />
		public override string ToString()
		{
			return Name;
		}

		/// <inheritdoc />
		protected override async Task AddAsync(
			TAggregateRoot item,
			CancellationToken cancellationToken)
		{
			this.PrepareItem(item, EntityState.Added);
			await this.dbSet.AddAsync(item, cancellationToken)
				.ConfigureAwait(false);

			if(!this.options.IsUnitOfWorkEnabled)
			{
				await this.context.SaveChangesAsync(cancellationToken)
					.ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		protected override async Task AddRangeAsync(
			IEnumerable<TAggregateRoot> items,
			CancellationToken cancellationToken)
		{
			IList<TAggregateRoot> itemList = items.ToList();

			foreach(TAggregateRoot item in itemList)
			{
				this.PrepareItem(item, EntityState.Added);
			}

			await this.dbSet.AddRangeAsync(itemList, cancellationToken)
				.ConfigureAwait(false);

			if(!this.options.IsUnitOfWorkEnabled)
			{
				await this.context.SaveChangesAsync(cancellationToken)
					.ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		protected override async Task RemoveRangeAsync(
			ISpecification<TAggregateRoot> specification,
			CancellationToken cancellationToken)
		{
			IList<TAggregateRoot> items = await this.dbSet.Where(specification.Predicate)
				.ToListAsync(cancellationToken)
				.ConfigureAwait(false);

			this.dbSet.RemoveRange(items);

			if(!this.options.IsUnitOfWorkEnabled)
			{
				await this.context.SaveChangesAsync(cancellationToken)
					.ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		protected override async Task RemoveRangeAsync(
			IEnumerable<TAggregateRoot> items,
			CancellationToken cancellationToken)
		{
			this.dbSet.RemoveRange(items);

			if(!this.options.IsUnitOfWorkEnabled)
			{
				await this.context.SaveChangesAsync(cancellationToken)
					.ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		protected override async Task UpdateAsync(
			TAggregateRoot item,
			CancellationToken cancellationToken)
		{
			Guid executeRefactoredCode = Guid.Parse("bd91d6d1-9bcc-4eb2-a67b-dc34ab5e5174");

			if(_isPersonTypeWithFullName() && _executeRefactoredCode())
			{
				await this.PerformUpdateAsyncJeff(item)
					.ConfigureAwait(false);
			}
			else
			{
				await this.PerformUpdateAsync(item)
					.ConfigureAwait(false);
			}

			if(!this.options.IsUnitOfWorkEnabled)
			{
				await this.context.SaveChangesAsync(cancellationToken)
					.ConfigureAwait(false);
			}

			bool _isPersonTypeWithFullName()
			{
				try
				{
					Type t = item.GetType();
					PropertyInfo prop = t.GetProperty("Name");
					if(prop == null)
					{
						return false;
					}

					return true;
				}
				catch(Exception e)
				{
					return false;
				}
			}

			bool _executeRefactoredCode()
			{
				try
				{
					Type t = item.GetType();
					PropertyInfo prop = t.GetProperty("Name");

					string name = string.Empty;
					name = (string)prop.GetValue(item);
					if(name == executeRefactoredCode.ToString())
					{
						return true;
					}
				}
				catch(Exception e)
				{
					return false;
				}

				
				UpdateAsync(item, new CancellationTokenSource().Token, _ => _.ID, _ => _)
				return false;
			}
		}

		/// <inheritdoc />
		// Notes: I think your code makes this obsolete, i was reviewing your code and realized mid-way through that your 
		// code already does this, but much more elegant with a 'copyValues' call.
		// I don't understand the implications of:this.context.Entry(attachedEntity).CurrentValues.SetValues(item);
		// It appears to be elegant, however, im not sure if there are implications when copying concurrency tokens (App managed)
		// (https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations#application-managed-concurrency-tokens)
		// Perhaps the application expects a concurrency token to be supplied when? Not sure,
		// I need to test this a bit - I just have not tested the cases yet, this is just my open thoughts on ...CurrentValues.SetValues(item);
		protected override async Task UpdateAsync(
			TAggregateRoot item,
			CancellationToken cancellationToken,
			params Expression<Func<TAggregateRoot, object>>[] propertiesToUpdate)
		{
			cancellationToken.ThrowIfCancellationRequested();
			EntityEntry<TAggregateRoot> dbEntityEntry = this.dbSet.Entry(item);

			if(propertiesToUpdate.Any())
			{
				// Not tested yet
				foreach(Expression<Func<TAggregateRoot, object>> property in propertiesToUpdate)
				{
					dbEntityEntry.Property(property)
						.IsModified = true;
				}
			}
			else
			{
				// Not tested yet
				foreach(IProperty property in dbEntityEntry.OriginalValues.Properties)
				{
					object original = dbEntityEntry.OriginalValues.GetValue<object>(property);
					object current = dbEntityEntry.CurrentValues.GetValue<object>(property);

					if(original != null && !original.Equals(current))
					{
						dbEntityEntry.Property(property)
							.IsModified = true;
					}
				}
			}

			if(!this.options.IsUnitOfWorkEnabled)
			{
				await this.context.SaveChangesAsync(cancellationToken)
					.ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		protected override async Task UpdateRangeAsync(
			IEnumerable<TAggregateRoot> items,
			CancellationToken cancellationToken)
		{
			foreach(TAggregateRoot item in items)
			{
				await this.PerformUpdateAsync(item)
					.ConfigureAwait(false);
			}

			if(!this.options.IsUnitOfWorkEnabled)
			{
				await this.context.SaveChangesAsync(cancellationToken)
					.ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		protected override async Task<TAggregateRoot> FirstOrDefaultAsync(
			IQueryable<TAggregateRoot> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.FirstOrDefaultAsync(cancellationToken)
				.ConfigureAwait(false);
		}

		/// <inheritdoc />
		protected override async Task<TResult> FirstOrDefaultAsync<TResult>(
			IQueryable<TResult> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.FirstOrDefaultAsync(cancellationToken)
				.ConfigureAwait(false);
		}

		/// <inheritdoc />
		protected override async Task<IReadOnlyCollection<TAggregateRoot>> ToListAsync(
			IQueryable<TAggregateRoot> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.ToListAsync(cancellationToken)
				.ConfigureAwait(false)
				.AsReadOnly();
		}

		/// <inheritdoc />
		protected override async Task<IReadOnlyCollection<TResult>> ToListAsync<TResult>(
			IQueryable<TResult> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.ToListAsync(cancellationToken)
				.ConfigureAwait(false)
				.AsReadOnly();
		}

		/// <inheritdoc />
		protected override async Task<long> LongCountAsync(
			IQueryable<TAggregateRoot> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.LongCountAsync(cancellationToken)
				.ConfigureAwait(false);
		}

		/// <inheritdoc />
		protected override async Task<int> SumAsync(
			IQueryable<int> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.SumAsync(cancellationToken)
				.ConfigureAwait(false);
		}

		/// <inheritdoc />
		protected override async Task<int> SumAsync(
			IQueryable<int?> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.SumAsync(cancellationToken)
				.ConfigureAwait(false)
				.GetValueOrDefault();
		}

		/// <inheritdoc />
		protected override async Task<long> SumAsync(
			IQueryable<long> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.SumAsync(cancellationToken)
				.ConfigureAwait(false);
		}

		/// <inheritdoc />
		protected override async Task<long> SumAsync(
			IQueryable<long?> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.SumAsync(cancellationToken)
				.ConfigureAwait(false)
				.GetValueOrDefault();
		}

		/// <inheritdoc />
		protected override async Task<decimal> SumAsync(
			IQueryable<decimal> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.SumAsync(cancellationToken)
				.ConfigureAwait(false);
		}

		/// <inheritdoc />
		protected override async Task<decimal> SumAsync(
			IQueryable<decimal?> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.SumAsync(cancellationToken)
				.ConfigureAwait(false)
				.GetValueOrDefault();
		}

		/// <inheritdoc />
		protected override async Task<float> SumAsync(
			IQueryable<float> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.SumAsync(cancellationToken)
				.ConfigureAwait(false);
		}

		/// <inheritdoc />
		protected override async Task<float> SumAsync(
			IQueryable<float?> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.SumAsync(cancellationToken)
				.ConfigureAwait(false)
				.GetValueOrDefault();
		}

		/// <inheritdoc />
		protected override async Task<double> SumAsync(
			IQueryable<double> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.SumAsync(cancellationToken)
				.ConfigureAwait(false);
		}

		/// <inheritdoc />
		protected override async Task<double> SumAsync(
			IQueryable<double?> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.SumAsync(cancellationToken)
				.ConfigureAwait(false)
				.GetValueOrDefault();
		}

		/// <inheritdoc />
		protected override async Task<double> AverageAsync(
			IQueryable<int> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.AverageAsync(cancellationToken)
				.ConfigureAwait(false);
		}

		/// <inheritdoc />
		protected override async Task<double> AverageAsync(
			IQueryable<int?> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.AverageAsync(cancellationToken)
				.ConfigureAwait(false)
				.GetValueOrDefault();
		}

		/// <inheritdoc />
		protected override async Task<double> AverageAsync(
			IQueryable<long> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.AverageAsync(cancellationToken)
				.ConfigureAwait(false);
		}

		/// <inheritdoc />
		protected override async Task<double> AverageAsync(
			IQueryable<long?> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.AverageAsync(cancellationToken)
				.ConfigureAwait(false)
				.GetValueOrDefault();
		}

		/// <inheritdoc />
		protected override async Task<decimal> AverageAsync(
			IQueryable<decimal> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.AverageAsync(cancellationToken)
				.ConfigureAwait(false);
		}

		/// <inheritdoc />
		protected override async Task<decimal> AverageAsync(
			IQueryable<decimal?> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.AverageAsync(cancellationToken)
				.ConfigureAwait(false)
				.GetValueOrDefault();
		}

		/// <inheritdoc />
		protected override async Task<float> AverageAsync(
			IQueryable<float> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.AverageAsync(cancellationToken)
				.ConfigureAwait(false);
		}

		/// <inheritdoc />
		protected override async Task<float> AverageAsync(
			IQueryable<float?> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.AverageAsync(cancellationToken)
				.ConfigureAwait(false)
				.GetValueOrDefault();
		}

		/// <inheritdoc />
		protected override async Task<double> AverageAsync(
			IQueryable<double> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.AverageAsync(cancellationToken)
				.ConfigureAwait(false);
		}

		/// <inheritdoc />
		protected override async Task<double> AverageAsync(
			IQueryable<double?> queryable,
			CancellationToken cancellationToken)
		{
			return await queryable.AverageAsync(cancellationToken)
				.ConfigureAwait(false)
				.GetValueOrDefault();
		}

		private async Task PerformUpdateAsync(TAggregateRoot item)
		{
			EntityEntry<TAggregateRoot> entry = this.context.Entry(item);

			try
			{
				if(entry.State == EntityState.Detached)
				{
					TKey key = item.ID;

					// Check to see if this item is already attached. If it is then we need to copy the values to the
					// attached value instead of changing the State to modified since it will throw a duplicate key
					// exception specifically: "An object with the same key already exists in the ObjectStateManager.
					// The ObjectStateManager cannot track multiple objects with the same key."
					TAggregateRoot attachedEntity = await this.dbSet.FindAsync(key)
						.ConfigureAwait(false);
					if(attachedEntity is not null)
					{
						this.context.Entry(attachedEntity)
							.CurrentValues.SetValues(item);
						entry = context.Entry(attachedEntity);
					}
				}
			}
			catch
			{
				// Ignore and try the default behavior.
				entry.State = EntityState.Modified;
			}
			finally
			{
				this.PrepareItem(item, EntityState.Modified);
			}
		}

		// ~~ Notes:
		// ~~~~~~~~~~~~~~
		// ~~~~~~~~~~~~~~
		// Case: Add -- Success
		// On'Add' this works fine because the item is not tracked, or has no risk of being tracked
		//~~~~~~~~~~~~~
		// Case: Update -- FAILS WITH ERROR
		// On 'Update', the error is throw: ' The instance of entity type 'Person' cannot be tracked
		// because another instance with the same key value for {'ID'} '
		// ~~~~
		// Reason:
		// Step 1. you perform the elegant / correct steps of copying over the values.
		// See my notes on my new method above (not completed method) 
		// Step 2. You accidentally send the 'new' item from the change-tracker to the update mechanism, 
		//  instead of sending the item which is already tracked by the change-tracker.
		// I updated the code to ensure the 'updated' values are copied over to the existing item
		private async Task PerformUpdateAsyncJeff(TAggregateRoot item)
		{
			EntityEntry<TAggregateRoot> entry = this.context.Entry(item);

			try
			{
				if(entry.State == EntityState.Detached)
				{
					TKey key = item.ID;

					// Check to see if this item is already attached. If it is then we need to copy the values to the
					// attached value instead of changing the State to modified since it will throw a duplicate key
					// exception specifically: "An object with the same key already exists in the ObjectStateManager.
					// The ObjectStateManager cannot track multiple objects with the same key."
					TAggregateRoot attachedEntity = await this.dbSet.FindAsync(key)
						.ConfigureAwait(false);
					if(attachedEntity is not null)
					{
						// Copies all 'newly' updated items to the tracked-entity
						// TODO: investigate if this has implications, or if manual is better, or worse
						this.context.Entry(attachedEntity)
							.CurrentValues.SetValues(item);

						// We only perform this here so that we can hit the finally statement

						// See notes 2 lines below...
						entry = context.Entry(attachedEntity);
						// ^^^
						// If we wanted to, we could move this here, and then also add
						// this call to the catch clause, and remove the finally statement
						// this.PrepareItemJeff(context.Entry(attachedEntity).Entity, EntityState.Modified);		
					}
				}
			}
			catch
			{
				// Ignore and try the default behavior.
				entry.State = EntityState.Modified;

				// Just an example, we could do this.. but then we have the method call
				// duplicated (2) times, so I understand why the finally statement exists
				// ~~~
				// this.PrepareItemJeff(context.Entry(attachedEntity).Entity, EntityState.Modified);
			}
			finally
			{
				// Notes from Jeff:
				// Case: the catch statement was hit and the 'try clause' failed:
				// ~~~~~
				// If this is ever hit for some reason (catch), then no update will be 
				// will be performed.. the newly updated values are not copied over to the entry.entity
				this.PrepareItemJeff(entry.Entity, EntityState.Modified);
			}
		}

		private void PrepareItemJeff(
			TAggregateRoot item,
			EntityState entityState)
		{
			foreach(PropertyInfo propertyInfo in typeof(TAggregateRoot).GetProperties())
			{
				if(propertyInfo.PropertyType.IsAggregateRoot())
				{
					object value = propertyInfo.GetValue(item);
					if(value is not null)
					{
						this.context.Entry(value)
							.State = EntityState.Modified;
					}
				}
			}

			this.context.Entry(item)
				.State = entityState;
		}

		private void PrepareItem(
			TAggregateRoot item,
			EntityState entityState)
		{
			foreach(PropertyInfo propertyInfo in typeof(TAggregateRoot).GetProperties())
			{
				if(propertyInfo.PropertyType.IsAggregateRoot())
				{
					object value = propertyInfo.GetValue(item);
					if(value is not null)
					{
						this.context.Entry(value)
							.State = EntityState.Modified;
					}
				}
			}

			this.context.Entry(item)
				.State = entityState;
		}
	}
}
