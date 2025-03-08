# Country Info Generator

The BCL (Base Class Library) only has the `RegionInfo` and `CultureInfo` classes to represent countries and cultures.
Unfortunately, the data representation (using mainly strings) can make handling conversions between different formats error-prone.

This project aims to create a source generator that generates a C# class for each country, with properties for the country's name, ISO code, and other relevant information.
It uses the BCL's `RegionInfo` and `CultureInfo` class to get the data.
Additionally, a second nuget package is present that contains the generated data.


[ToC]

## Installation
You may install the package from NuGet:
```bash
dotnet add package Sic.CountryInfos
```

If you prefer to use the source generator, with which the namespace and the contained data can be configured, see [Source Generator](#source-generator).

## Usage
There are three enums representing the country's data:
- `Country`: Contains the english name of the country (spaces and special characters omitted).
- `CountryIso2Code`: Contains the ISO 3166-1 alpha-2 code of the country.
- `CountryIso3Code`: Contains the ISO 3166-1 alpha-3 code of the country.

All enums use the GeoId (which is the same as the `RegionInfo`'s `GeoId`) as the enum value.
Additionally, there is a `IsSameCountry`-extension method that checks if two (different) enums represent the same country, e.g.:
```csharp
bool isSameCountry = Country.Germany.IsSameCountry(CountryIso2CodeName.DE); // true
```

Additionally, there is a `LocaleCode` enum that contains the locale code of the countries. The values represent the LCID (Locale Identifier) value.
```csharp
    /// <summary>
    /// Afrikaans (South Africa) - af-ZA
    /// </summary>
    af_ZA = 1078,
    /// <summary>
    /// Amharic (Ethiopia) - am-ET
    /// </summary>
    am_ET = 1118
```


All this information is additionally present in the `CountryInfo` class. An instance of this class can be created by using the `CountryInfo.Get`-method:
```csharp
CountryInfo germany = CountryInfo.Get(Country.Germany);
// or
germany = CountryInfo.Get(CountryIso2Code.DE);
// or
germany = CountryInfo.Get(CountryIso3Code.DEU);
// or
germany = CountryInfo.Get(LocaleCode.de_DE);
```
The `CountryInfo` can than be used to get the country's name, ISO codes, and other information.
```csharp
germany.EnglishName; // Germany as string
Country country = germany.Country; // Country.Germany
CountryIso2Code iso2Code = germany.TwoLetterIsoCode; // CountryIso2CodeName.DE
CountryIso3Code iso3Code = germany.ThreeLetterIsoCode; // CountryIso3CodeName.DEU
IReadOnlyList<LocaleCode> supportedLocales = germany.SupportedLocales; // [LocaleCode.de_DE, LocaleCode.dsb_DE, ...]
```

Additionally, the `CountryInfo` class contains a `All`-property that contains a list of all countries.

There are also extensions for the `Country`, `CountryIso2Code`, `CountryIso3Code`, and `LocaleCode` enums that allow getting the `CountryInfo` instance directly:
```csharp
CountryInfo germany = Country.Germany.GetCountryInfo();
```

## Source Generator
You might want to use the source generator, if you would like to
- change the root namespace of the generated classes
- filter the countries (and its locales) that should be generated

Install the generator via
```bash
dotnet add package Sic.CountryInfos.SourceGenerator
```

### Filter Countries
You can filter the countries that should be generated 
by providing a list of either a `Country.Countries.txt`, `CountryIso2Code.Countries.txt`, 
or `CountryIso3Code.Countries.txt` enum names, e.g. for `CountryIso2Code.Countries.txt`:
```
DE
AT
BE
SE
```
The file has to be added as Additional File in the project file:
```xml
<ItemGroup>
    <None Remove="CountryIso2Code.Countries.txt"/>
    <AdditionalFiles Include="CountryIso2Code.Countries.txt"/>
</ItemGroup>
```
The source generator will then generate all enums only for the countries (and its locales) that are listed in the file.

### Change Root Namespace
On default, the generator will generate the classes in the root namespace of your project.
If you want to change the namespace, you can add the `SicCountryInfosCustomNamespace` property to your project file:
```xml
<ItemGroup>
       <CompilerVisibleProperty Include="SicCountryInfosCustomNamespace" />
</ItemGroup>

<PropertyGroup>
   <SicCountryInfosCustomNamespace>My.Custom.Namespace</SicCountryInfosCustomNamespace>
</PropertyGroup>
```
