using AngleSharp;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace DataImporter.Importers;

public class NoteImporter : BaseImporter
{
    public override async Task ImportData(string url)
    {
        Console.WriteLine($"Importing notes from: {url}");
        
        var html = await GetHtmlAsync(url);
        
        var result = await ParseNotesHtml(html);
        
        await SaveToDatabase(result);
        
        Console.WriteLine($"Imported {result.Count} notes");
    }
    
    private async Task<List<string>> ParseNotesHtml(string html)
    {
        var noteNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        var config = Configuration.Default;
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync(req => req.Content(html));
        
        var noteLinks = document.QuerySelectorAll("a[href*='/notes/']");
        
        Console.WriteLine($"Found {noteLinks.Length} note links");
        
        foreach (var link in noteLinks)
        {
            var nameElement = link.QuerySelector("p.mt-3.text-sm.font-medium");
            if (nameElement == null)
            {
                nameElement = link.QuerySelector("p.mt-3");
            }
            
            if (nameElement != null)
            {
                var noteName = nameElement.TextContent.Trim();
                if (!string.IsNullOrEmpty(noteName))
                {
                    noteNames.Add(noteName);
                }
            }
        }
        
        Console.WriteLine($"Extracted {noteNames.Count} unique note names");
        return noteNames.OrderBy(n => n).ToList();
    }
    
    private async Task SaveToDatabase(List<string> noteNames)
    {
        int addedCount = 0;
        int skippedCount = 0;
        
        await using var context = new FragranceLogContext();
        
        foreach (var noteName in noteNames)
        {
            var exists = await context.Notes
                .AnyAsync(n => n.Name.ToLower() == noteName.ToLower());
                
            if (!exists)
            {
                context.Notes.Add(new Note 
                { 
                    Name = noteName 
                });
                addedCount++;
            }
            else
            {
                skippedCount++;
            }
        }
        
        await context.SaveChangesAsync();
        Console.WriteLine($"Added {addedCount} new notes, skipped {skippedCount} duplicates");
    }
}