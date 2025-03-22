using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Sic.CountryInfos.SourceGenerator.Generation;

namespace Sic.CountryInfos.SourceGenerator;

[Generator]
public class CountryInfoSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get the configuration options
        var configOptions = context.AnalyzerConfigOptionsProvider
            .Select((options, _) => GetNamespace(options.GlobalOptions));

        // Get the additional file if it exists
        var additionalFile = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith("Countries.txt"))
            .Select((file, _) => (file.Path, file.GetText()?.ToString() ?? string.Empty))
            .Select(static (file, _) => file);

        // Combine the inputs and generate the source
        var combined = context.CompilationProvider.Combine(configOptions).Select((t, i) => t.Right).Combine(additionalFile.Collect());
        //var combined = additionalFile.Combine(configOptions);

        context.RegisterSourceOutput(combined, (spc, source) =>
        {
            var (namespaceName, additionalFileInfo ) = source;
            var filter = DetermineFilter(additionalFileInfo);
            var regionInfos = GetRegionInfos(filter);

            GenerateOutput(spc, namespaceName, regionInfos);
        });
    }

    private static string GetNamespace(
        AnalyzerConfigOptions options)
    {
        options.TryGetValue("build_property.rootnamespace", out var rootNamespace);
        options.TryGetValue("build_property.siccountryinfoscustomnamespace", out var customNamespace);

        return customNamespace ?? rootNamespace ?? string.Empty;
    }

    private static Dictionary<RegionInfo, List<CultureInfo>> GetRegionInfos(Func<RegionInfo, bool> filter)
    {
        return CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Where(x => !x.IsNeutralCulture && !x.Equals(CultureInfo.InvariantCulture))
            .Select(c => (CultureInfo: c, RegionInfo: CreateRegionInfo(c)))
            .Where(t => t.RegionInfo is not null)
            .Where(t => t.RegionInfo!.IsLetters())
            .Where(c => filter(c.RegionInfo!))
            .GroupBy(t => t.RegionInfo!)
            .ToDictionary(g => g.Key,
                g => g.Select(t => t.CultureInfo).ToList());
    }

    private static void GenerateOutput(SourceProductionContext context, string namespaceName,
        Dictionary<RegionInfo, List<CultureInfo>> regionInfos)
    {
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
        var localeCodeEnumBuilder = new LanguageEnumBuilder("""
                                                            Represents the locale code
                                                            /// <seealso href='https://en.wikipedia.org/wiki/Locale_(computer_software)'/>
                                                            """,
            namespaceName,
            Constants.LocaleCodeEnumName,
            c => c.Name.Replace("-", "_"),
            t => t.Culture.LCID,
            c => $"{c.EnglishName.Replace("&", "&amp;")} - {c.Name}"
            );

        var alpha2CodeBuilder = new LanguageEnumBuilder(
            "Represents the ISO 3166 ALPHA-2 code of a language.",
            namespaceName,
            Constants.LanguageIso2CodeEnumName,
            c => c.TwoLetterISOLanguageName.ToUpper(),
            t => t.Index, // unfortunately, we can't use the LCID as the ID, as there are multiple cultures with the same LCID
            c => c.EnglishName.Split('(')[0].Trim()
            );

        var alpha3CodeBuilder = new LanguageEnumBuilder(
            "Represents the ISO 3166 ALPHA-3 code of a language.",
            namespaceName,
            Constants.LanguageIso3CodeEnumName,
            c => c.ThreeLetterISOLanguageName.ToUpper(),
            t => t.Index, // unfortunately, we can't use the LCID as the ID, as there are multiple cultures with the same LCID
            c => c.EnglishName.Split('(')[0].Trim()
        );

        foreach (var kvp in regionInfos)
        {
            var (region, locales) = (kvp.Key, kvp.Value);
            countryIso2Builder.AddCountry(region);
            countryIso3Builder.AddCountry(region);
            countryIsoBuilder.AddCountry(region);
            foreach (var locale in locales)
            {
                localeCodeEnumBuilder.AddLocaleCode(locale);
                alpha2CodeBuilder.AddLocaleCode(locale);
                alpha3CodeBuilder.AddLocaleCode(locale);
            }

        }

        context.AddSource($"{Constants.CountryIso2CodeName}.g.cs", countryIso2Builder.Build());
        context.AddSource($"{Constants.CountryIso3CodeName}.g.cs", countryIso3Builder.Build());
        context.AddSource($"{Constants.CountryEnumName}.g.cs", countryIsoBuilder.Build());
        context.AddSource($"{Constants.LocaleCodeEnumName}.g.cs", localeCodeEnumBuilder.Build());
        context.AddSource($"{Constants.LanguageIso2CodeEnumName}.g.cs", alpha2CodeBuilder.Build());
        context.AddSource($"{Constants.LanguageIso3CodeEnumName}.g.cs", alpha3CodeBuilder.Build());

        var countryInfoGenerator = new CountryInfoGenerator(namespaceName);
        var countryInfoSource = countryInfoGenerator.GetCountryClass(regionInfos);
        context.AddSource("CountryInfo.g.cs", countryInfoSource);
        context.AddSource("CountryInfoEnumExtensions.g.cs", ExtensionBuilder.Create(namespaceName));

        var languageInfoGenerator = new LanguageInfoGenerator(namespaceName);
        var cultureInfos = regionInfos.Values
            .SelectMany(c => c)
            //.Distinct()
            .ToList();
        var languageInfoSource = languageInfoGenerator.GetLanguageClass(cultureInfos);
        context.AddSource("LanguageInfo.g.cs", languageInfoSource);
    }

    private static Func<RegionInfo, bool> DetermineFilter(ImmutableArray<(string Path, string Content)> additionalFiles)
    {
        var additionalFile = additionalFiles.FirstOrDefault();
        if (string.IsNullOrEmpty(additionalFile.Content))
        {
            return _ => true;
        }

        var fileName = Path.GetFileName(additionalFile.Path);
        var allowedCountries = additionalFile.Content
            .Split('\n')
            .Select(l => l.Trim())
            .Where(l => string.IsNullOrWhiteSpace(l) is false)
            .ToImmutableHashSet();

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