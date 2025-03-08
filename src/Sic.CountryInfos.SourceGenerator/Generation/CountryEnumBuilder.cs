using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Sic.CountryInfos.SourceGenerator.Generation;

internal class CountryEnumBuilder
{
    private readonly Func<RegionInfo, string> _stringPicker;
    private readonly StringBuilder _enumBuilder;
    private readonly SortedList<string, RegionInfo> _regions;

    public CountryEnumBuilder(
        string documentation,
        string namespaceName,
        string fileName,
        Func<RegionInfo, string> stringPicker)
    {
        _regions = [];
        _stringPicker = stringPicker;
        _enumBuilder = new StringBuilder();
        _enumBuilder.AppendLine($@"namespace {namespaceName}.Enum;
/// <summary>
/// {documentation}
/// </summary>
public enum {fileName}
{{");
    }

    public void AddCountry(RegionInfo region)
    {
        _regions.Add(_stringPicker(region), region);
    }


    public string Build()
    {
        foreach (var region in _regions.Values)
        {
            _enumBuilder.AppendLine($@"    /// <summary>
    /// {region.EnglishName.Replace("&", "&amp;")} - {region.Name}
    /// </summary>
    {_stringPicker(region)} = {region.GeoId},");
        }
        _enumBuilder.Append("}");
        return _enumBuilder.ToString();
    }
}