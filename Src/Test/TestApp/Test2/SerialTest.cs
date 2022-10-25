using Akkatecture.Core;
using SimpleProjectManager.Server.Data;
using SimpleProjectManager.Server.Data.DataConverters;
using UnitsNet;

namespace TestApp.Test2;

public sealed record TestData(ToSerialize ToSerialize, Temperature Temperature, TestSingle TestSingle);

public sealed record TestDataConvert
{
    public string ToSerialize { get; init; }
    
    public UnitNetData Temperature { get; init; }
    
    public string TestSingle { get; init; }
};

public sealed class TestSingle : Identity<TestSingle>
{
    public TestSingle(string value) : base(value) { }
}

public static class SerialTest
{
    public static void Run()
    {
        var converterContainer = new DataConverter();
        
        var toser = Temperature.FromDegreesCelsius(20);

        var conv = converterContainer.Get<Temperature, UnitNetData>();

        var toserstring = conv.ToDto(toser);
        toser = conv.FromDto(toserstring);


        
        
        var toser2 = ToSerialize.From("Hallo");

        var conv2 = converterContainer.Get<ToSerialize, string>();

        var toserstring2 = conv2.ToDto(toser2);
        toser2 = conv2.FromDto(toserstring2);
        
        
        
        
        var toser3 = ToSerialize.Empty;

        var conv3 = converterContainer.Get<ToSerialize, string>();

        var toserstring3 = conv3.ToDto(toser3);
        toser3 = conv2.FromDto(toserstring3);
        
        
        
        
        var toser4 = TestSingle.New;

        var conv4 = converterContainer.Get<TestSingle, string>();

        var toserstring4 = conv4.ToDto(toser4);
        toser4 = conv4.FromDto(toserstring4);
        
        
        
        var toser5 = new TestData(toser3, toser, toser4);

        var conv5 = converterContainer.Get<TestData, TestDataConvert>();

        var toserstring5 = conv5.ToDto(toser5);
        toser5 = conv5.FromDto(toserstring5);
    }
}