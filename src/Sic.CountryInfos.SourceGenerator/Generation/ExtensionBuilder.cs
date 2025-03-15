namespace Sic.CountryInfos.SourceGenerator.Generation;

internal static class ExtensionBuilder
{

    public static string Create(string namespaceName)
    {
        return $@"using {namespaceName}.Enums;
namespace {namespaceName};
/// <summary>
/// Provides extension methods for the country enums.
/// </summary>
public static class CountryInfoEnumExtensions
{{
    /// <summary>
    /// Determines whether the specified ISO 3166 ALPHA-2 code is equal to the specified ISO 3166 ALPHA-3 code.
    /// </summary>
    public static bool IsSameCountry(this {Constants.CountryIso2CodeName} iso2Code, {Constants.CountryIso3CodeName} iso3Code)
    {{
        return (int)iso2Code == (int)iso3Code;
    }}
    /// <summary>
    /// Determines whether the specified ISO 3166 ALPHA-3 code is equal to the specified ISO 3166 ALPHA-2 code.
    /// </summary>
    public static bool IsSameCountry(this {Constants.CountryIso3CodeName} iso3Code, {Constants.CountryIso2CodeName} iso2Code)
    {{
        return (int)iso3Code == (int)iso2Code;
    }}

    /// <summary>
    /// Determines whether the specified ISO 3166 ALPHA-2 code is equal to the specified country.
    /// </summary>
    public static bool IsSameCountry(this {Constants.CountryIso2CodeName} iso2Code, {Constants.CountryEnumName} country)
    {{
        return (int)iso2Code == (int)country;
    }}

    /// <summary>
    /// Determines whether the specified country is equal to the specified ISO 3166 ALPHA-2 code.
    /// </summary>
    public static bool IsSameCountry(this {Constants.CountryEnumName} country, {Constants.CountryIso2CodeName} iso2Code)
    {{
        return (int)country == (int)iso2Code;
    }}

    /// <summary>
    /// Determines whether the specified ISO 3166 ALPHA-3 code is equal to the specified country.
    /// </summary>
    public static bool IsSameCountry(this {Constants.CountryIso3CodeName} iso3Code, {Constants.CountryEnumName} country)
    {{
        return (int)iso3Code == (int)country;
    }}

    /// <summary>
    /// Determines whether the specified country is equal to the specified ISO 3166 ALPHA-3 code.
    /// </summary>
    public static bool IsSameCountry(this {Constants.CountryEnumName} country, {Constants.CountryIso3CodeName} iso3Code)
    {{
        return (int)country == (int)iso3Code;
    }}

    /// <summary>
    /// Gets the country information for the specified ISO 3166 ALPHA-2 code.
    /// </summary>
    public static CountryInfo GetCountryInfo(this {Constants.CountryIso2CodeName} iso2Code)
    {{
        return CountryInfo.Get(iso2Code);
    }}

    /// <summary>
    /// Gets the country information for the specified ISO 3166 ALPHA-3 code.
    /// </summary>
    public static CountryInfo GetCountryInfo(this {Constants.CountryIso3CodeName} iso3Code)
    {{
        return CountryInfo.Get(iso3Code);
    }}

    /// <summary>
    /// Gets the country information for the specified Country.
    /// </summary>
    public static CountryInfo GetCountryInfo(this {Constants.CountryEnumName} country)
    {{
        return CountryInfo.Get(country);
    }}

    /// <summary>
    /// Gets the country information for the specified locale.
    /// </summary>
    public static CountryInfo GetCountryInfo(this LocaleCode locale)
    {{
        return CountryInfo.Get(locale);
    }}
}}";
    }
}