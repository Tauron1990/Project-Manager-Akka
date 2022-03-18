using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Tauron.SourceGenerators.Utils;
using VerifyTests;
using VerifyXunit;
using Xunit;

namespace Tauron.SourceGenerators.Tests.Utils;

[UsesVerify]
public class AttributeHelpersTests
{
    [Fact]
    public Task TestGenerateAttribute()
    {
        var targets = AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface;
        var testNamespace = "Tauron.Test";
        var testClass = "TestClassAttribute";
        var testPropertys = ImmutableDictionary<string, string>.Empty.Add("string", "TestProperty").Add("int", "TestProperty2");

        var result = AttributeHelpers.GenerateAttribute(targets, testNamespace, testClass, testPropertys);

        return Verifier.Verify(result);
    }
}