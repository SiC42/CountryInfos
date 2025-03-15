namespace Sic.CountryInfos.Tests;

public class LanguageInfoTest
{
    [Fact]
    public void Value_OfLanguageIso2AndIso3Code_AreIdentical()
    {
        Assert.Multiple(() =>
        {
            foreach (var languageInfo in LanguageInfo.All)
            {
                Assert.Equal((int)languageInfo.LanguageIso2Code, (int)languageInfo.LanguageIso3Code);
            }
        });
    }

    [Fact]
    public Task Test()
    {
        return Verify(LanguageInfo.All);
    }
}