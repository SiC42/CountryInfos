using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Sic.CountryInfos.SourceGenerator.Generation;

internal class CountryInfoGenerator
{
    private readonly string _namespaceName;

    public CountryInfoGenerator(string namespaceName)
    {
        _namespaceName = namespaceName;
    }

    public string GetCountryClass(IDictionary<RegionInfo, List<CultureInfo>> regionWithCultures)
    {
        return $@"using {_namespaceName}.Enums;
using System.Collections.Generic;
using System.Linq;
namespace {_namespaceName};

/// <summary>
/// Contains information about a country.
/// </summary>
public class CountryInfo
{{
    /// <summary>
    /// The ISO 3166 ALPHA-2 code of the country.
    /// </summary>
    public {Constants.CountryIso2CodeName} TwoLetterIsoCode {{ get; }}

    /// <summary>
    /// The ISO 3166 ALPHA-2 code of the country.
    /// </summary>
    public {Constants.CountryIso3CodeName} ThreeLetterIsoCode {{ get; }}

    /// <summary>
    /// Enum representing the country.
    /// </summary>
    public {Constants.CountryEnumName} Country {{ get; }}

    /// <summary>
    /// The english name of the country.
    /// </summary>
    public string EnglishName {{ get; }}
    
    /// <summary>
    /// The locales of the country.
    /// </summary>
    public IReadOnlyList<{Constants.LocaleCodeEnumName}> Locales {{ get; }}

    /// <summary>
    /// The supported ISO 3166 ALPHA-2 code languages of the country.
    /// </summary>
    public IReadOnlyList<{Constants.LanguageIso2CodeEnumName}> SupportedIso2CodeLanguages {{ get; }}

    /// <summary>
    /// The supported ISO 3166 ALPHA-3 code languages of the country.
    /// </summary>
    public IReadOnlyList<{Constants.LanguageIso3CodeEnumName}> SupportedIso3CodeLanguages {{ get; }}

    private CountryInfo(
        string name, 
        {Constants.CountryIso2CodeName} twoLetterIsoCode, 
        {Constants.CountryIso3CodeName} threeLetterIsoCode,
        {Constants.CountryEnumName} country,
        IEnumerable<{Constants.LocaleCodeEnumName}> supportedLocales)
    {{
        EnglishName = name;
        TwoLetterIsoCode = twoLetterIsoCode;
        ThreeLetterIsoCode = threeLetterIsoCode;
        Country = country;
        Locales = supportedLocales.ToList().AsReadOnly();
        var languages = Locales.Select(LanguageInfo.Get).ToList();
        SupportedIso2CodeLanguages = languages.Select(l => l.LanguageIso2Code).ToList().AsReadOnly();
        SupportedIso3CodeLanguages = languages.Select(l => l.LanguageIso3Code).ToList().AsReadOnly();
    }}

    /// <inheritdoc />
    public bool Equals(CountryInfo other) => TwoLetterIsoCode == other.TwoLetterIsoCode;
    
    /// <summary>
    /// Contains all countries of this package.
    /// </summary>
    public static readonly IReadOnlyList<CountryInfo> All;
    
    private static readonly IReadOnlyDictionary<int, CountryInfo> _byLcid;
    
    private static readonly IReadOnlyDictionary<int, CountryInfo> _byGeoId;
    
    /// <summary>
    /// Gets the country information for the specified locale.
    /// </summary>
    public static CountryInfo Get(LocaleCode localeCode) => _byLcid[(int)localeCode];
    
    /// <summary>
    /// Gets the country information for the specified country.
    /// </summary>
    public static CountryInfo Get({Constants.CountryEnumName} country) => _byGeoId[(int)country];
    
    /// <summary>
    /// Tries to get the country information for the specified LCID locale.
    /// </summary>
    public static bool TryGet(int lcid, out CountryInfo countryInfo) => _byLcid.TryGetValue(lcid, out countryInfo);
    
    /// <summary>
    /// Gets the country information for the specified ISO 3166 ALPHA-2 code.
    /// </summary>
    public static CountryInfo Get({Constants.CountryIso2CodeName} iso2Code) => _byGeoId[(int)iso2Code];
    
    /// <summary>
    /// Gets the country information for the specified ISO 3166 ALPHA-3 code.
    /// </summary>
    public static CountryInfo Get({Constants.CountryIso3CodeName} iso3Code) => _byGeoId[(int)iso3Code];
    
    static CountryInfo()
    {{
        All = new List<CountryInfo> {GetCountries(regionWithCultures)}.AsReadOnly();
        _byLcid = All
        .SelectMany(c => c.Locales.Select(l => (LocaleCode: l, Country: c)))
        .ToDictionary(t => (int)t.LocaleCode, t => t.Country);
        _byGeoId = All.ToDictionary(c => (int)c.Country);
    }}
}}";
    }

    private static string GetCountries(IDictionary<RegionInfo,List<CultureInfo>> regionWithCultures)
    {
        var sb = new StringBuilder();
        sb.AppendLine("\n\t\t{");
        foreach (var kvp in regionWithCultures.OrderBy(kvp => kvp.Key.EnglishName))
        {
            sb.Append("\t\t\t");
            sb.AppendLine(CreateInstance(kvp.Key, kvp.Value));
        }
        sb.AppendLine("\t\t}");

        return sb.ToString();
    }

    private static string CreateInstance(RegionInfo region, List<CultureInfo> cultures)
    {
        return $@"new CountryInfo(""{region.EnglishName}"", {Constants.CountryIso2CodeName}.{region.TwoLetterISORegionName}, {Constants.CountryIso3CodeName}.{region.ThreeLetterISORegionName}, {Constants.CountryEnumName}.{Constants.GetCountryName(region)}, {GetLocales(cultures)}),";
    }

    private static string GetLocales(List<CultureInfo> cultures)
    {
        return "[" + string.Join(",", cultures.Select(c => $"LocaleCode.{c.Name.Replace("-", "_")}")) + "]";
    }
}