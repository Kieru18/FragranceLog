using System.Text;
using DataImporter.Models;

namespace DataImporter.Services;

public class ReportWriter
{
    public async Task WriteReportAsync(
        string path,
        List<MatchDecision> matches,
        List<string> missingImages)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head><meta charset=\"utf-8\" />");
        sb.AppendLine("<title>Perfume Image Import Report</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: system-ui, sans-serif; background:#111; color:#eee; padding:20px; }");
        sb.AppendLine("h1,h2 { color:#f5e6c0; }");
        sb.AppendLine("table { border-collapse:collapse; width:100%; margin-bottom:20px; }");
        sb.AppendLine("th,td { border:1px solid #333; padding:4px 8px; font-size:12px; }");
        sb.AppendLine("th { background:#222; }");
        sb.AppendLine(".exact { color:#8fd98f; }");
        sb.AppendLine(".fuzzy { color:#ffd27f; }");
        sb.AppendLine(".none { color:#ff8f8f; }");
        sb.AppendLine("</style></head><body>");

        sb.AppendLine("<h1>Perfume Image Import Report</h1>");

        var exact = matches.Count(m => m.Kind == MatchKind.Exact);
        var fuzzy = matches.Count(m => m.Kind == MatchKind.Fuzzy);
        var none = matches.Count(m => m.Kind == MatchKind.None);

        sb.AppendLine("<p>");
        sb.AppendLine($"Total records: {matches.Count}<br/>");
        sb.AppendLine($"Exact matches: <span class=\"exact\">{exact}</span><br/>");
        sb.AppendLine($"Fuzzy matches: <span class=\"fuzzy\">{fuzzy}</span><br/>");
        sb.AppendLine($"No match: <span class=\"none\">{none}</span><br/>");
        sb.AppendLine($"Missing image files: {missingImages.Count}<br/>");
        sb.AppendLine("</p>");

        sb.AppendLine("<h2>Exact matches</h2>");
        sb.AppendLine("<table><tr><th>Brand (dataset)</th><th>Name (dataset)</th><th>Brand (DB)</th><th>Name (DB)</th><th>PerfumeId</th></tr>");
        foreach (var m in matches.Where(m => m.Kind == MatchKind.Exact && m.DbPerfume != null).Take(300))
        {
            sb.AppendLine("<tr class=\"exact\">");
            sb.Append("<td>").Append(Escape(m.Dataset.Brand)).AppendLine("</td>");
            sb.Append("<td>").Append(Escape(m.Dataset.NamePerfume)).AppendLine("</td>");
            sb.Append("<td>").Append(Escape(m.DbPerfume!.BrandName)).AppendLine("</td>");
            sb.Append("<td>").Append(Escape(m.DbPerfume!.PerfumeName)).AppendLine("</td>");
            sb.Append("<td>").Append(m.DbPerfume!.PerfumeId).AppendLine("</td>");
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</table>");

        sb.AppendLine("<h2>Fuzzy matches</h2>");
        sb.AppendLine("<table><tr><th>Brand (dataset)</th><th>Name (dataset)</th><th>Brand (DB)</th><th>Name (DB)</th><th>PerfumeId</th><th>Similarity</th></tr>");
        foreach (var m in matches.Where(m => m.Kind == MatchKind.Fuzzy && m.DbPerfume != null).Take(300))
        {
            sb.AppendLine("<tr class=\"fuzzy\">");
            sb.Append("<td>").Append(Escape(m.Dataset.Brand)).AppendLine("</td>");
            sb.Append("<td>").Append(Escape(m.Dataset.NamePerfume)).AppendLine("</td>");
            sb.Append("<td>").Append(Escape(m.DbPerfume!.BrandName)).AppendLine("</td>");
            sb.Append("<td>").Append(Escape(m.DbPerfume!.PerfumeName)).AppendLine("</td>");
            sb.Append("<td>").Append(m.DbPerfume!.PerfumeId).AppendLine("</td>");
            sb.Append("<td>").Append(m.Similarity?.ToString("F3") ?? "").AppendLine("</td>");
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</table>");

        sb.AppendLine("<h2>No match</h2>");
        sb.AppendLine("<table><tr><th>Brand</th><th>Name</th><th>Reason</th></tr>");
        foreach (var m in matches.Where(m => m.Kind == MatchKind.None).Take(300))
        {
            sb.AppendLine("<tr class=\"none\">");
            sb.Append("<td>").Append(Escape(m.Dataset.Brand)).AppendLine("</td>");
            sb.Append("<td>").Append(Escape(m.Dataset.NamePerfume)).AppendLine("</td>");
            sb.Append("<td>").Append(Escape(m.Reason)).AppendLine("</td>");
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</table>");

        if (missingImages.Count > 0)
        {
            sb.AppendLine("<h2>Missing image files</h2>");
            sb.AppendLine("<ul>");
            foreach (var name in missingImages.Take(300))
            {
                sb.Append("<li>").Append(Escape(name)).AppendLine("</li>");
            }
            sb.AppendLine("</ul>");
        }

        sb.AppendLine("</body></html>");

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, sb.ToString(), Encoding.UTF8);
    }

    private static string Escape(string? s)
    {
        return string.IsNullOrEmpty(s) ? "" : System.Net.WebUtility.HtmlEncode(s);
    }
}
