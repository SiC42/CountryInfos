using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Sic.CountryInfos.SourceGenerator.Generation;

public class LocaleCodeEnumBuilder
{
    private readonly StringBuilder _enumBuilder;
    private readonly SortedList<string, CultureInfo> _cultures;

    public LocaleCodeEnumBuilder(string namespaceName)
    {
        _cultures = new();
        _enumBuilder = new StringBuilder();
        _enumBuilder.AppendLine($@"namespace {namespaceName}.Enum;
/// <summary>
/// Represents the locale code.
/// </summary>
public enum LocaleCode
{{");
    }

    public void AddLocaleCode(CultureInfo culture)
    {
        _cultures.Add(culture.Name, culture);
    }


    public string Build()
    {
        foreach (var culture in _cultures.Values)
        {
            _enumBuilder.AppendLine($@"    /// <summary>
    /// {culture.EnglishName} - {culture.Name}
    /// </summary>
    {culture.Name.Replace("-", "_")} = {culture.LCID},");
        }

        _enumBuilder.Append("}");
        return _enumBuilder.ToString();
    }
}