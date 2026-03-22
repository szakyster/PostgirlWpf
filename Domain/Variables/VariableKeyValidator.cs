using System.Text.RegularExpressions;

namespace Postgirl.Domain.Variables;

public static class VariableKeyValidator
{
    // Allows: letters, digits, underscore, hyphen, dot. Rejects: spaces, tabs, parentheses, etc.
    private static readonly Regex ValidPattern = new(@"^[\w\-\.]*$", RegexOptions.Compiled);

    public static bool IsValid(string key) => ValidPattern.IsMatch(key);
}
