using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Tauron.SourceGenerators.Utils;
using VerifyXunit;
using Xunit;

namespace Tauron.SourceGenerators.Tests.Utils;

[UsesVerify]
public class AttributeHelpersTests
{
    [Fact]
    public Task TestGenerateAttribute()
    {
        const AttributeTargets targets = AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface;
        const string testNamespace = "Tauron.Test";
        const string testClass = "TestClassAttribute";
        var testPropertys = ImmutableDictionary<string, string>.Empty.Add("string", "TestProperty").Add("int", "TestProperty2");

        var result = AttributeHelpers.GenerateAttribute(targets, testNamespace, testClass, testPropertys)();

        return Verifier.Verify(result);
    }
}