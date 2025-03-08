using System;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Sic.CountryInfos.SourceGenerator.Generation;

namespace Sic.CountryInfos.SourceGenerator;

/// <summary>
/// A sample source generator that creates C# classes based on the text file (in this case, Domain Driven Design ubiquitous language registry).
/// When using a simple text file as a baseline, we can create a non-incremental source generator.
/// </summary>
[Generator]
public class CountryInfoSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization required for this generator.
    }

    public void Execute(GeneratorExecutionContext context)
    {
        context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.rootnamespace", out var rootNamespace);

        string namespaceName =
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.siccountryinfoscustomnamespace", out var ns)
                ? ns
                : rootNamespace!;

        var additionalFile = context.AdditionalFiles.FirstOrDefault(f => f.Path.EndsWith("Countries.txt"));
        var filter = DetermineFilter(additionalFile);

        var regionInfos = CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Where(x => x.IsNeutralCulture is false)
            .Where(x => x.Equals(CultureInfo.InvariantCulture) is false)
            .Select(c => (CultureInfo: c, RegionInfo: CreateRegionInfo(c)))
            .Where(t => t.RegionInfo is not null)
            .Where(t => t.RegionInfo!.IsLetters())
            .Where(c => filter(c.RegionInfo!))
            .GroupBy(t => t.RegionInfo!)
            .ToDictionary(g => g.Key,
                g => g.Select(t => t.CultureInfo).ToList());


        var countryIso2Builder = new CountryEnumBuilder(
            "Represents the ISO 3166 ALPHA-2 code of a country.",
            namespaceName,
            Constants.CountryIso2CodeName,
            r => r.TwoLetterISORegionName);
        var countryIso3Builder = new CountryEnumBuilder(
            "Represents the ISO 3166 ALPHA-3 code of a country.",
            namespaceName,
            Constants.CountryIso3CodeName,
            r => r.ThreeLetterISORegionName);
        var countryIsoBuilder = new CountryEnumBuilder(
            "Represents a country.",
            namespaceName,
            Constants.CountryEnumName,
            Constants.GetCountryName);
        var localeCodeEnumBuilder = new LocaleCodeEnumBuilder(namespaceName);

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

        var countryInfoGenerator = new CountryInfoGenerator(namespaceName);
        var countryInfoSource = countryInfoGenerator.GetCountryStatic(regionInfos);
        context.AddSource("CountryInfo.g.cs", countryInfoSource);
        context.AddSource("CountryInfoEnumExtensions.g.cs", ExtensionBuilder.Create(namespaceName));
    }

    private static Func<RegionInfo, bool> DetermineFilter(AdditionalText? additionalFile)
    {
        if (additionalFile is null)
        {
            return _ => true;
        }

        var fileName = Path.GetFileName(additionalFile.Path);
        var allowedCountries = additionalFile
            .GetText()?
            .Lines
            .Select(l => l.ToString().Trim())
            .ToImmutableHashSet();

        if (allowedCountries is null)
        {
            return _ => true;
        }

        if (fileName.StartsWith("IsoCode2"))
        {
            return r => allowedCountries.Contains(r.TwoLetterISORegionName);
        }

        if (fileName.StartsWith("IsoCode3"))
        {
            return r => allowedCountries.Contains(r.ThreeLetterISORegionName);
        }

        return r => allowedCountries.Contains(r.EnglishName);
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