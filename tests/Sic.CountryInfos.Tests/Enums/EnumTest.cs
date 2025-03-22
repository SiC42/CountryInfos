
using Sic.CountryInfos.Enums;

namespace Sic.CountryInfos.Tests.Enums;

public class EnumTest
{
    [Fact]
    public Task VerifyCountry()
    {
        return VerifyEnum<Country>();
    }

    [Fact]
    public Task VerifyCountryIso2Code()
    {
        return VerifyEnum<CountryIso2Code>();
    }

    [Fact]
    public Task VerifyCountryIso3Code()
    {
        return VerifyEnum<CountryIso3Code>();
    }

    [Fact]
    public Task VerifyLanguageIso2Code()
    {
        return VerifyEnum<LanguageIso2Code>();
    }


    [Fact]
    public Task VerifyLanguageIso3Code()
    {
        return VerifyEnum<LanguageIso3Code>();
    }

    [Fact]
    public Task VerifyLocaleCode()
    {
        return VerifyEnum<LocaleCode>();
    }

    private async Task VerifyEnum<T>() where T : struct, Enum
    {
        Dictionary<string, int> enumValues = Enum.GetValues<T>()
            .ToDictionary(v => Enum.GetName(v)!, v => Convert.ToInt32(v));
        await Verify(enumValues);
    }


}