using System.Globalization;
using System.Linq;

namespace Sic.CountryInfos.SourceGenerator;

internal static class RegionInfoExtensions
{
    public static bool IsLetters(this RegionInfo regionInfo)
    {
        return regionInfo.TwoLetterISORegionName.All(char.IsLetter);
    }
}