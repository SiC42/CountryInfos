using Sic.CountryInfos.Enums;

namespace Sic.CountryInfos.Tests;

public class LanguageInfoTest
{
    [Fact]
    public void LocaleCode_AllValuesCorrectlyInCountryInfo()
    {
        Assert.Equal(Enum.GetValues<LocaleCode>().Length, LanguageInfo.All.SelectMany(l => l.LocaleCodes).Count());
        Assert.Multiple(() =>
        {
            foreach (var language in Enum.GetValues<LocaleCode>())
            {
                Assert.Contains(language, LanguageInfo.Get(language).LocaleCodes);
            }
        });
    }

    [Fact]
    public void LanguageIso2Code_AllValuesCorrectlyInCountryInfo()
    {
        Assert.Equal(Enum.GetValues<LanguageIso2Code>().Length, LanguageInfo.All.Count);
        Assert.Multiple(() =>
        {
            foreach (var language in Enum.GetValues<LanguageIso2Code>())
            {
                Assert.Equal(language, LanguageInfo.Get(language).LanguageIso2Code);
            }
        });
    }

    [Fact]
    public void LanguageIs3Code_AllValuesCorrectlyInCountryInfo()
    {
        Assert.Equal(Enum.GetValues<LanguageIso3Code>().Length, LanguageInfo.All.Count);
        Assert.Multiple(() =>
        {
            foreach (var language in Enum.GetValues<LanguageIso3Code>())
            {
                Assert.Equal(language, LanguageInfo.Get(language).LanguageIso3Code);
            }
        });
    }
}