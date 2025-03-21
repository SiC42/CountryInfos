using System;
using System.Globalization;
using System.Linq;

namespace Sic.CountryInfos.SourceGenerator;

internal static class Constants
{
    public const string AutoGeneratedComment = """
                                               // <auto-generated>
                                               // This code was generated by Sic.CountryInfos.SourceGenerator.
                                               // Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
                                               // </auto-generated>
                                               """;


    public const string CountryEnumName = "Country";
    public const string CountryIso2CodeName = "CountryIso2Code";
    public const string CountryIso3CodeName = "CountryIso3Code";

    // culture enum names
    public const string LocaleCodeEnumName = "LocaleCode";
    public const string LanguageIso2CodeEnumName = "LanguageIso2Code";
    public const string LanguageIso3CodeEnumName = "LanguageIso3Code";




    public static readonly Func<RegionInfo, string> GetCountryName = region =>
        new string(region.EnglishName
            .Replace("&", "And")
            .Where(c => char.IsLetterOrDigit(c) || c == '_')
            .ToArray());


}