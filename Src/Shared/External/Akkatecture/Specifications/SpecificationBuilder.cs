using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Tauron.Operations;

namespace Akkatecture.Specifications;

public interface ISpecificationBuilder<in TData>
{
    ISpecification<TData> Build();
}

[PublicAPI]
public static class  SpecificationBuilder
{
    public static SpecificationBuilder<TData, TData, TData> Create<TData>()
        => new MasterSpecificationBuilder<TData>();

    public static SpecificationBuilder<TMaster, TData, TActual> All<TMaster, TData, TActual>(
        this IEnumerable<SpecificationBuilder<TMaster, TData, TActual>> specifications)
    {

    }

    public static SpecificationBuilder<TMaster, TData, TActual> All<TMaster, TData, TActual>(
        this SpecificationBuilder<TMaster, TData, TActual>  specification, params SpecificationBuilder<>)
    {

    }

    public static SpecificationBuilder<TMaster, TData, TActual> AtLeast<TMaster, TData, TActual>(this IEnumerable<SpecificationBuilder<TMaster, TData, TActual>> specifications, int requiredSpecifications)

    public static SpecificationBuilder<TMaster, TData, TActual> And<TMaster, TData, TActual>(this SpecificationBuilder<TMaster, TData, TActual> specification1, SpecificationBuilder<TMaster, TData, TActual> specification2)

    public static SpecificationBuilder<TMaster, TData, TActual> Or<TMaster, TData, TActual>(this SpecificationBuilder<TMaster, TData, TActual> specification1, SpecificationBuilder<TMaster, TData, TActual> specification2)

    public static SpecificationBuilder<TMaster, TData, TActual> Not<TMaster, TData, TActual>(this SpecificationBuilder<TMaster, TData, TActual> specification)
}

public sealed class MasterSpecificationBuilder<TMaster> : SpecificationBuilder<TMaster, TMaster, TMaster>
{
    public MasterSpecificationBuilder()
        :base(d => d, null!)
    {
        
    }

    protected override SpecificationBuilder<TMaster, TMaster, TMaster> MasterBuilder => this;
}

[PublicAPI]
public class SpecificationBuilder<TMaster, TData, TActual> : ISpecificationBuilder<TData>
{
    private readonly List<Func<Func<TActual, IEnumerable<Error>>>> _validators = new();
    private readonly List<ISpecificationBuilder<TActual>> _nestedValidators = new();
    private readonly Func<TData, TActual> _acessor;
    private readonly SpecificationBuilder<TMaster, TMaster, TMaster> _masterBuilder;

    // ReSharper disable once ConvertToAutoProperty
    protected virtual SpecificationBuilder<TMaster, TMaster, TMaster> MasterBuilder => _masterBuilder;

    internal SpecificationBuilder(Func<TData, TActual> acessor, SpecificationBuilder<TMaster, TMaster, TMaster> masterBuilder)
    {
        _acessor = acessor;
        _masterBuilder = masterBuilder;
    }

    public SpecificationBuilder<TMaster, TData, TActual> WithValidator(Func<TActual, Error?> validator)
    {
        _validators.Add(
            () => a =>
            {
                var err = validator(a);
                return err != null ? Enumerable.Repeat(err.Value, 1) : Enumerable.Empty<Error>();
            });

        return this;
    }

    public SpecificationBuilder<TMaster, TData, TActual> WithValidator(Func<TActual, IEnumerable<Error>> validator)
    {
        _validators.Add(() => validator);

        return this;
    }

    public SpecificationBuilder<TMaster, TData, TActual> WithValidator(Func<Func<TActual, IEnumerable<Error>>> validator)
    {
        _validators.Add(validator);

        return this;
    }

    public SpecificationBuilder<TMaster, TActual, TNewActual> CreateNestedSpec<TNewActual>(Func<TActual, TNewActual> convert)
    {
        var builder = new SpecificationBuilder<TMaster, TActual, TNewActual>(convert, MasterBuilder);

        _nestedValidators.Add(builder);

        return builder;
    }

    public SpecificationBuilder<TMaster, TMaster, TMaster> GetMasterBuilder()
        => MasterBuilder;

    public ISpecification<TData> Build()
        => new BuilderSpec(
            _acessor, 
            _validators.Select(c => c())
           .Concat(_nestedValidators.Select(b => new Func<TActual, IEnumerable<Error>>(b.Build().WhyIsNotSatisfiedBy)))
           .ToImmutableList());

    public sealed class BuilderSpec : Specification<TData>
    {
        private readonly Func<TData, TActual> _accessor;
        private readonly ImmutableList<Func<TActual, IEnumerable<Error>>> _validators;

        public BuilderSpec(Func<TData, TActual> accessor, ImmutableList<Func<TActual, IEnumerable<Error>>> validators)
        {
            _accessor = accessor;
            _validators = validators;
        }

        protected override IEnumerable<Error> IsNotSatisfiedBecause(TData aggregate)
            => _validators.SelectMany(f => f(_accessor(aggregate)));
    }
}