using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Sic.CountryInfos.SourceGenerator.Generation;

namespace Sic.CountryInfos.SourceGenerator;

/// <summary>
/// A sample source generator that creates C# classes based on the text file (in this case, Domain Driven Design ubiquitous language registry).
/// When using a simple text file as a baseline, we can create a non-incremental source generator.
/// </summary>
[Generator]
public class SampleSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization required for this generator.
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // If you would like to put some data to non-compilable file (e.g. a .txt file), mark it as an Additional File.


        var regionInfos = CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Where(x => x.IsNeutralCulture is false)
            .Where(x => x.Equals(CultureInfo.InvariantCulture) is false)
            .Select(c => (CultureInfo: c, RegionInfo: CreateRegionInfo(c)))
            .Where(t => t.RegionInfo is not null)
            .Where(t => t.RegionInfo!.IsLetters())
            .GroupBy(t => t.RegionInfo!)
            .ToDictionary(g => g.Key,
                g => g.Select(t => t.CultureInfo).ToList());

        var countryIso2Builder = new CountryEnumBuilder(
            "Sic.CountryInfos",
            Constants.CountryIso2CodeName,
            r => r.TwoLetterISORegionName);
        var countryIso3Builder = new CountryEnumBuilder(
            "Sic.CountryInfos",
            Constants.CountryIso3CodeName,
            r => r.ThreeLetterISORegionName);
        var countryIsoBuilder = new CountryEnumBuilder(
            "Sic.CountryInfos",
            Constants.CountryEnumName,
            Constants.GetCountryName);
        var localeCodeEnumBuilder = new LocaleCodeEnumBuilder("Sic.CountryInfos");

        foreach (var kvp in regionInfos)
        {
            var (region, locales) = (kvp.Key, kvp.Value);
            countryIso2Builder.AddCountry(region);
            countryIso3Builder.AddCountry(region);
            countryIsoBuilder.AddCountry(region);
            foreach (var locale in locales)
            {
                localeCodeEnumBuilder.AddLocaleCode(locale);
            }
        }

        context.AddSource($"{Constants.CountryIso2CodeName}.g.cs", countryIso2Builder.Build());
        context.AddSource($"{Constants.CountryIso3CodeName}.g.cs", countryIso3Builder.Build());
        context.AddSource($"{Constants.CountryEnumName}.g.cs", countryIsoBuilder.Build());
        context.AddSource("LocaleCode.g.cs", localeCodeEnumBuilder.Build());

        var countryInfoGenerator = new CountryInfoGenerator("Sic.CountryInfos");
        var countryInfoSource = countryInfoGenerator.GetCountryStatic(regionInfos);
        context.AddSource("CountryInfoStatic.g.cs", countryInfoSource);
    }

    private static RegionInfo? CreateRegionInfo(CultureInfo x)
    {
        try
        {
            return new RegionInfo(x.LCID);
        }
        catch
        {
            return null;
        }
    }
}