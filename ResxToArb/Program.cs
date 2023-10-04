using System.Resources.NetStandard;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections;
using System.Text;

var resxPath = args[0];
var arbFlePath = args[1];
const string NumericPlaceHolder = "placeholder";

if (!File.Exists(resxPath))
    throw new Exception("Resx File does not exist!!!");
if (!File.Exists(arbFlePath))
    File.Create(arbFlePath);

using var fileWriter = new StreamWriter(arbFlePath);
using var resxReader = new ResXResourceReader(resxPath);
int resCount = 0;

fileWriter.WriteLine("{");

foreach (DictionaryEntry resource in resxReader)
{
    if (resource.Value == null)
        continue;

    Debug.WriteLine($"## Writing resource: {resource}");
    var placeholders = FindAndReplacePlaceHolders(out string processedString,
        resource.Value.ToString()!, NumericPlaceHolder);
    string arbResourceDef = $"""
        "{CamelCaseString(resource.Key.ToString()!)}": "{processedString}",
        """;

    fileWriter.WriteLine(arbResourceDef);

    if (placeholders.Any())
    {
        StringBuilder placeholderBuilder = new StringBuilder();
        foreach (var placeholder in placeholders)
        {
            placeholderBuilder.Append($"\n\t\t\"{placeholder}\": {{}},\n");
        }

        var arbPlaceholderValue = $"\"@{resource.Key}\": {{\n" +
            """
                "placeholders": {
            """ 
                + 
                placeholderBuilder.ToString()
                +
           """
                }
            }
            """;

        fileWriter.WriteLine(arbPlaceholderValue);
    }

    resCount++;
}

fileWriter.WriteLine("}");
fileWriter.Close();

Console.WriteLine($"{resCount} resources converted to arb. \nPress Any key to exit.");
Console.ReadKey();

static string CamelCaseString(string inputString)
{
    return char.ToLower(inputString[0]) + inputString.Substring(1);
}

static List<string> FindAndReplacePlaceHolders(out string processedString, string resxValue, string numericPlaceHolderValue)
{
    string pattern = @"\{([^{}]+)\}";
    Regex regex = new Regex(pattern);
    List<string> result = new List<string>();

    string processed = regex.Replace(resxValue, match =>
    {
        string placeholder = match.Groups[1].Value;
        if (int.TryParse(placeholder, out int number))
        {
            var newPlaceholder = $"{numericPlaceHolderValue}{number}";
            result.Add(newPlaceholder);
            return "{"+newPlaceholder+"}";
        }

        result.Add(CamelCaseString(placeholder));
        return "{"+CamelCaseString(match.Value)+"}";
    });

    processedString = processed;
    return result;
}