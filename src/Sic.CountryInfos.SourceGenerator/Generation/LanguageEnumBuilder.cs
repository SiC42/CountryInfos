using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Sic.CountryInfos.SourceGenerator.Generation;

internal class LanguageEnumBuilder
{
    private readonly Func<CultureInfo, string> _getLanguage;
    private readonly Func<(CultureInfo, int), int> _getId;
    private readonly StringBuilder _enumBuilder;
    private readonly SortedList<string, CultureInfo> _cultures;

    public LanguageEnumBuilder(
        string documentation,
        string namespaceName,
        string fileName,
        Func<CultureInfo, string> getLanguage,
        Func<(CultureInfo Culture,int Index),int> getId)
    {
        _getLanguage = getLanguage;
        _getId = getId;
        _cultures = new();
        _enumBuilder = new StringBuilder();
        _enumBuilder.AppendLine($@"namespace {namespaceName}.Enums;
/// <summary>
/// {documentation}
/// </summary>
public enum {fileName}
{{");
    }

    public void AddLocaleCode(CultureInfo culture)
    {
        _cultures.Add(culture.Name, culture);
    }


    public string Build()
    {
        var languages = _cultures.Values
            .Select(c => (EnumName: _getLanguage(c), Culture: c))
            // as cultures can have the same language name, we need do Distinct by
            .GroupBy(t => t.EnumName)
            .Select(g => g.First())
            .Select((t, i) => (t.EnumName, t.Culture, Id: _getId((t.Culture, i))));


        foreach (var (enumName, culture, id) in languages)
        {
            _enumBuilder.AppendLine($@"    /// <summary>
    /// {culture.EnglishName.Replace("&", "&amp;")} - {culture.Name}
    /// </summary>
    {enumName} = {id},");
        }

        _enumBuilder.Append("}");
        return _enumBuilder.ToString();
    }
}