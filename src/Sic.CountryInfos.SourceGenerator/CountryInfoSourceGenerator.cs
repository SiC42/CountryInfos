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