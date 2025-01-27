﻿namespace Fluxera.Repository.UnitTests
{
	using System;
	using System.Collections.Generic;
	using Fluxera.Entity;
	using Fluxera.Extensions.Validation;
	using Fluxera.Repository.Validation;
	using Microsoft.Extensions.DependencyInjection;

	public class TesValidationStrategyFactory : IValidationStrategyFactory
	{
		private readonly IServiceProvider serviceProvider;

		public TesValidationStrategyFactory(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		/// <inheritdoc />
		public IValidationStrategy<TAggregateRoot, TKey> CreateStrategy<TAggregateRoot, TKey>()
			where TAggregateRoot : AggregateRoot<TAggregateRoot, TKey>
			where TKey : IComparable<TKey>, IEquatable<TKey>
		{
			return new TestValidationStrategy<TAggregateRoot, TKey>(this.GetValidators());
		}

		private IReadOnlyCollection<IValidator> GetValidators()
		{
			IList<IValidator> validators = new List<IValidator>();

			IEnumerable<IValidatorFactory> validatorFactories = this.serviceProvider.GetServices<IValidatorFactory>();
			foreach(IValidatorFactory validatorFactory in validatorFactories)
			{
				IValidator validator = validatorFactory.CreateValidator();
				validators.Add(validator);
			}

			return validators.AsReadOnly();
		}
	}
}
