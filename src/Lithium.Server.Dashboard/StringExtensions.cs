using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace Lithium.Server.Dashboard;

public static partial class StringExtensions
{
    public static MarkupString ToFormattedMarkup(this string str)
    {
        var result = MyRegex2().Replace(str, "<b>$1</b>");

        // 2. url -> <a href="url">url</a>
        result = MyRegex1().Replace(result,
            m => $"<a href=\"{m.Value}\" target=\"_blank\" rel=\"noopener noreferrer\">{m.Value}</a>"
        );

        return new MarkupString(result);
    }

    [GeneratedRegex(@"https?:\/\/[^\s""<>]+")]
    private static partial Regex MyRegex1();

    [GeneratedRegex("\"([^\"]*)\"")]
    private static partial Regex MyRegex2();
}