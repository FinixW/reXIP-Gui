using System.Text.RegularExpressions;

namespace ReXipGui;

/// <summary>
/// Extracts the in-archive path from one line of `reXIP list` output.
/// NOTE: this currently assumes each line contains one path such as System\shop\xxx.csv.
/// If the real output format differs, this is the only file that needs to change.
/// </summary>
internal static class EntryParser
{
    private static readonly Regex WithDir =
        new(@"[^\s""<>|*?]+(?:\\|/)[^\s""<>|*?]*\.\w{1,8}", RegexOptions.Compiled);

    private static readonly Regex AnyFile =
        new(@"[A-Za-z_][^\s""<>|*?]*\.[A-Za-z]\w{0,7}", RegexOptions.Compiled);

    public static string? TryGetPath(string line)
    {
        var m = WithDir.Match(line);
        if (!m.Success) m = AnyFile.Match(line);
        return m.Success ? m.Value : null;
    }
}
