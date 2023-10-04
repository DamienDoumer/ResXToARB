

using System.Resources.NetStandard;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Text;

var resxPath = args[0];
var arbFlePath = args[1];
const string NumericPlaceHolder = "placeholder";

if (!File.Exists(resxPath))
    throw new Exception("Resx File does not exist!!!");

using var fileWriter = new StreamWriter(arbFlePath);
using var resxReader = new ResXResourceReader(resxPath);

foreach (DictionaryEntry resource in resxReader)
{
    if (resource.Value == null)
        continue;

    Debug.WriteLine($"## Writing resource: {resource}");
    var placeholders = FindAndReplacePlaceHolders(out string processedString,
        resource.Value.ToString()!, NumericPlaceHolder);
    string arbResourceDef = $"""
        "{resource.Key}": "{processedString}",
        """ + "\n";
    if (placeholders.Any())
    {
        StringBuilder placeholderBuilder = new StringBuilder();
        foreach (var placeholder in placeholders)
        {
            placeholderBuilder.Append($"\n\"{placeholder}\": {{}},\n");
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

    }
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

        result.Add(placeholder);
        return "{"+match.Value+"}";
    });

    processedString = processed;
    return result;
}