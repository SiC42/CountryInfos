
using Sic.CountryInfos.Enums;

namespace Sic.CountryInfos.Tests.Enums;

public class EnumTest
{
    [Fact]
    public Task VerifyCountry()
    {
        return Verify(Enum.GetNames<Country>());
    }

    [Fact]
    public Task VerifyCountryIso2Code()
    {
        return Verify(Enum.GetNames<CountryIso2Code>());
    }

    [Fact]
    public Task VerifyCountryIso3Code()
    {
        return Verify(Enum.GetNames<CountryIso3Code>());
    }

    [Fact]
    public Task VerifyLanguageIso2Code()
    {
        return Verify(Enum.GetNames<LanguageIso2Code>());
    }


    [Fact]
    public Task VerifyLanguageIso3Code()
    {
        return Verify(Enum.GetNames<LanguageIso3Code>());
    }

    [Fact]
    public Task VerifyLocaleCode()
    {
        return Verify(Enum.GetNames<LocaleCode>());
    }


}