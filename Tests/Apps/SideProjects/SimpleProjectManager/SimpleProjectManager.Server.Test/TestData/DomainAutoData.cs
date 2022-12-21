using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Tauron.Operations;

namespace SimpleProjectManager.Shared.Tests.TestData;

[AttributeUsage(AttributeTargets.Method)]
public sealed class DomainAutoData : AutoDataAttribute
{
    public DomainAutoData()
        : base(FixtureFactory)
    {
        
    }

    public static IFixture FixtureFactory()
    {
        var fix = new Fixture();

        fix.Customize(new ImmutableCollectionsCustomization());
        fix.Customize(new AutoNSubstituteCustomization());
        fix.RegisterLoggerFactory(new LoggerFactory());

        fix.Register(() => ProjectFileId.New);
        fix.Register(() => ProjectId.New);
        fix.Register(() => new ProjectDeadline(DateTime.Now + TimeSpan.FromDays(1)));
        fix.Register<string, SimpleResult>(SimpleResult.Failure);
        fix.Register<IValidator<string>>(
            () => new InlineValidator<string>
                  {
                      v => v.RuleFor(s => s).NotEmpty(),
                  });
        
        return fix;
    }
}