using Akkatecture.Core;
using AutoMapper;
using UnitsNet;

namespace TestApp.Test2;

public sealed record TestData(ToSerialize ToSerialize, Temperature Temperature, TestSingle TestSingle);

public sealed record TestDataConvert
{
    public string ToSerialize { get; init; }

    public double Temperature { get; init; }

    public string TestSingle { get; init; }
}

public sealed class TestSingle : Identity<TestSingle>
{
    public TestSingle(string value) : base(value) { }
}

public static class SerialTest
{
    public static void Run()
    {
        var config = new MapperConfiguration(
            cfg =>
            {
                cfg.CreateMap<Temperature, double>().ConvertUsing(temperature => temperature.As(UnitSystem.SI));
                cfg.CreateMap<double, Temperature>().ConvertUsing(d => new Temperature(d, UnitSystem.SI));
                cfg.CreateMap<TestData, TestDataConvert>()
                   .ReverseMap();
            });

        config.AssertConfigurationIsValid();

        IMapper mapper = config.CreateMapper();


        var testData = new TestData(ToSerialize.Empty, Temperature.FromDegreesCelsius(20), TestSingle.New);

        var dto = mapper.Map<TestDataConvert>(testData);

        var result = mapper.Map<TestData>(dto);
    }
}