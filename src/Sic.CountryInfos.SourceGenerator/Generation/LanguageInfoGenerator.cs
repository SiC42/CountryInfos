namespace Sic.CountryInfos.SourceGenerator.Generation;

public class LanguageInfoGenerator
{
    private readonly string _namespaceName;

    public LanguageInfoGenerator(string namespaceName)
    {
        _namespaceName = namespaceName;
    }

    public string GetLanguageClass(List<CultureInfo> cultures)
    {
        return $@"using {_namespaceName}.Enums;
using System.Collections.Generic;
using System.Linq;
namespace {_namespaceName};
public class LanguageInfo
{{
    public {Constants.LanguageIso2CodeEnumName} LanguageIso2Code {{ get; }}
    public {Constants.LanguageIso3CodeEnumName} LanguageIso3Code {{ get; }}
    public IReadOnlyCollection<{Constants.LocaleCodeEnumName}> LocaleCodes {{ get; }}
    
    private LanguageInfo(
        {Constants.LanguageIso2CodeEnumName} languageIso2Code, 
        {Constants.LanguageIso3CodeEnumName} languageIso3Code,
        IEnumerable<{Constants.LocaleCodeEnumName}> localeCodes)
    {{
        LanguageIso2Code = languageIso2Code;
        LanguageIso3Code = languageIso3Code;
        LocaleCodes = localeCodes.ToList().AsReadOnly();
    }}
    
    /// <inheritdoc />
    public bool Equals(LanguageInfo other) => LanguageIso2Code == other.LanguageIso2Code;
    
    /// <summary>
    /// Contains all countries of this package.
    /// </summary>
    public static readonly IReadOnlyList<LanguageInfo> All;
    
    private static readonly IReadOnlyDictionary<int, LanguageInfo> _byId;
    
    /// <summary>
    /// Gets the language by the ISO 639-1 code.
    /// </summary>
    public static LanguageInfo Get({Constants.LanguageIso2CodeEnumName} languageCode) => _byId[(int)languageCode];
    /// <summary>
    /// Gets the language by the ISO 639-2 code.
    /// </summary>
    public static LanguageInfo Get({Constants.LanguageIso3CodeEnumName} languageCode) => _byId[(int)languageCode];

    private static readonly IReadOnlyDictionary<int, LanguageInfo> _byLcid;

    public static LanguageInfo Get({Constants.LocaleCodeEnumName} localeCode) => _byLcid[(int)localeCode];
    
    static LanguageInfo()
    {{
        All = new List<LanguageInfo> {GetLanguages(cultures)}
        _byId = All.ToDictionary(l => (int)l.LanguageIso2Code);
        _byLcid = All
        .SelectMany(l => l.LocaleCodes.Select(lc => (LocaleCode: lc, Language: l)))
        .ToDictionary(t => (int)t.LocaleCode, t => t.Language);
    }}
}}";
    }

    private static string GetLanguages(List<CultureInfo> cultures)
    {
        var sb = new StringBuilder();
        sb.AppendLine("\n\t\t{");
        foreach (var group in cultures.GroupBy(c => c.TwoLetterISOLanguageName))
        {
            var culture = group.First();
            sb.AppendLine($"\t\t\tnew LanguageInfo({Constants.LanguageIso2CodeEnumName}.{culture.TwoLetterISOLanguageName.ToUpper()}, {Constants.LanguageIso3CodeEnumName}.{culture.ThreeLetterISOLanguageName.ToUpper()}, [{GetLocaleCodes(group)}]),");
        }
        sb.AppendLine("\t\t};");

        return sb.ToString();
    }

    private static string GetLocaleCodes(IEnumerable<CultureInfo> cultureInfos)
    {
        return cultureInfos.Select(c => $"{Constants.LocaleCodeEnumName}.{c.Name.Replace("-", "_")}").Aggregate((a, b) => $"{a}, {b}");
    }
}

