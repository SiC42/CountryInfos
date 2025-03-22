using Sic.CountryInfos.Enums;

namespace Sic.CountryInfos.Tests;

public class CountryInfoTest
{
    [Fact]
    public void Country_AllValuesCorrectlyInCountryInfo()
    {
       Assert.Equal(Enum.GetValues<Country>().Length, CountryInfo.All.Count);
       Assert.Multiple(() =>
       {
           foreach (var country in Enum.GetValues<Country>())
           {
               Assert.Equal(country, CountryInfo.Get(country).Country);
           }
       });
    }

    [Fact]
    public void CountryIso2Code_AllValuesCorrectlyInCountryInfo()
    {
       Assert.Equal(Enum.GetValues<CountryIso2Code>().Length, CountryInfo.All.Count);
       Assert.Multiple(() =>
       {
           foreach (var country in Enum.GetValues<CountryIso2Code>())
           {
               Assert.Equal(country, CountryInfo.Get(country).TwoLetterIsoCode);
           }
       });
    }

    [Fact]
    public void CountryIso3Code_AllValuesCorrectlyInCountryInfo()
    {
       Assert.Equal(Enum.GetValues<CountryIso3Code>().Length, CountryInfo.All.Count);
       Assert.Multiple(() =>
       {
           foreach (var country in Enum.GetValues<CountryIso3Code>())
           {
               Assert.Equal(country, CountryInfo.Get(country).ThreeLetterIsoCode);
           }
       });
    }
}