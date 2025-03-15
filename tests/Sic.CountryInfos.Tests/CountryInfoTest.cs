namespace Sic.CountryInfos.Tests;

public class CountryInfoTest
{
    [Fact]
    public Task Test()
    {
        return Verify(LanguageInfo.All);
    }
}