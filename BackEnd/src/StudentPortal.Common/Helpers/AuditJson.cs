using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace StudentPortal.Common.Helpers;

public static class AuditJson
{
    /// <summary>
    /// System.Text.Json escapes Vietnamese text into \u1EC5 by default, which makes the
    /// stored JSON unreadable in SSMS. This encoder keeps accented characters as they are.
    /// </summary>
    public static readonly JsonSerializerOptions Options = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };
}
