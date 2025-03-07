using System;
using System.Globalization;

namespace Sic.CountryInfos.SourceGenerator;

internal class Constants
{
    public const string CountryEnumName = "Country";
    public const string CountryIso2CodeName = "CountryIso2Code";
    public const string CountryIso3CodeName = "CountryIso3Code";


    public static readonly Func<RegionInfo, string> GetCountryName = region => region.EnglishName
        .Replace(" ", string.Empty)
        .Replace("&", "And")
        .Replace("’", string.Empty)
        .Replace("(", string.Empty)
        .Replace(")", string.Empty);
}