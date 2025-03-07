using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Sic.CountryInfos.SourceGenerator.Generation;

public class CountryInfoGenerator
{
    private readonly string _namespaceName;

    public CountryInfoGenerator(string namespaceName)
    {
        _namespaceName = namespaceName;
    }

    public string GetCountryStatic(IDictionary<RegionInfo, List<CultureInfo>> regionWithCultures)
    {
        return $@"
using System.Collections.Generic;
using System.Collections.Immutable;
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
    /// The supported locales of the country.
    /// </summary>
    public IReadOnlyList<LocaleCode> SupportedLocales {{ get; }}

    internal CountryInfo(
        string name, 
        {Constants.CountryIso2CodeName} twoLetterIsoCode, 
        {Constants.CountryIso3CodeName} threeLetterIsoCode,
        {Constants.CountryEnumName} country,
        ICollection<LocaleCode> supportedLocales)
    {{
        EnglishName = name;
        TwoLetterIsoCode = twoLetterIsoCode;
        ThreeLetterIsoCode = threeLetterIsoCode;
        Country = country;
        SupportedLocales = supportedLocales.ToList().AsReadOnly();
    }}
    
    /// <summary>
    /// Contains all countries of this package.
    /// </summary>
    public static readonly IReadOnlyList<CountryInfo> All;
    
    private static readonly IReadOnlyDictionary<int, CountryInfo> _byLcid = All
        .SelectMany(c => c.SupportedLocales.Select(l => (LocaleCode: l, Country: c)))
        .ToImmutableDictionary(t => (int)t.LocaleCode, t => t.Country);
    
    private static readonly IReadOnlyDictionary<int, CountryInfo> _byGeoId = All.ToImmutableDictionary(c => (int)c.Country);
    
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
        All = new List<CountryInfo> {GetCountries(regionWithCultures)}
    }}
}}";
    }

    private static string GetCountries(IDictionary<RegionInfo,List<CultureInfo>> regionWithCultures)
    {
        var sb = new StringBuilder();
        sb.AppendLine("\n\t\t{");
        foreach (var kvp in regionWithCultures)
        {
            sb.Append("\t\t\t");
            sb.AppendLine(CreateInstance(kvp.Key, kvp.Value));
        }
        sb.AppendLine("\t\t};");

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