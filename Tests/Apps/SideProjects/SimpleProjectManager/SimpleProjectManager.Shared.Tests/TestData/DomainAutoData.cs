using AutoFixture;
using AutoFixture.Xunit2;
using FluentValidation;
using Tauron.Operations;

namespace SimpleProjectManager.Shared.Tests.TestData;

[AttributeUsage(AttributeTargets.Method)]
public sealed class DomainAutoData : AutoDataAttribute
{
    public DomainAutoData()
        : base(FixtureFactory)
    {
        
    }

    private static IFixture FixtureFactory()
    {
        var fix = new Fixture();
        fix.Customize<SimpleResult>(comp => comp.FromFactory<string>(SimpleResult.Failure));
        fix.Customize<IValidator<string>>(comp => comp.FromFactory(() => new InlineValidator<string>
                                                                         {
                                                                             v => v.RuleFor(s => s).NotEmpty()
                                                                         }));
        return fix;
    }
}