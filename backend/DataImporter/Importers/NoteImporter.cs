using System;
using AngleSharp;
using AngleSharp.Dom;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using Core.Entities;
using DataImporter.Model;
//using Infrastructure.Data;
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
        
        //Console.WriteLine($"Imported: {result.NoteTypes.Count} note types, {result.Notes.Count} notes");
    }
    
    private IElement FindGridForGroup(IElement groupSection)
    {
        var currentNode = groupSection;
        for (int i = 0; i < 10; i++)
        {
            currentNode = currentNode.NextElementSibling;
            if (currentNode == null)
                break;
                
            var grid = currentNode.QuerySelector("div[class*='grid'][class*='grid-cols-']");
            if (grid != null)
                return grid;
                
            if (currentNode.ClassList.Contains("grid") && 
                currentNode.ClassList.Any(c => c.StartsWith("grid-cols-")))
            {
                return currentNode;
            }
        }
        
        var parent = groupSection.ParentElement;
        if (parent != null)
        {
            var grid = parent.QuerySelector("div[class*='grid'][class*='grid-cols-']");
            if (grid != null)
                return grid;
        }
        
        return null;
    }

    private List<NoteData> ExtractNotesFromGrid(IElement grid, string groupName)
    {
        var notes = new List<NoteData>();
        
        var noteLinks = grid.QuerySelectorAll("a[href*='/notes/']");
        
        foreach (var link in noteLinks)
        {
            var nameElement = link.QuerySelector("p.text-sm.font-medium");
            if (nameElement == null)
            {
                nameElement = link.QuerySelector("p.mt-3");
                if (nameElement == null)
                    continue;
            }
            
            var noteName = nameElement.TextContent.Trim();
            if (!string.IsNullOrEmpty(noteName))
            {
                notes.Add(new NoteData 
                { 
                    Name = noteName, 
                    NoteTypeName = groupName 
                });
            }
        }
        
        return notes;
    }
    
    private async Task<NoteImportResult> ParseNotesHtml(string html)
    {
        var result = new NoteImportResult();
    
        var config = Configuration.Default;
        var context = BrowsingContext.New(config);
    
        var document = await context.OpenAsync(req => req.Content(html));

        var groupSections = document.QuerySelectorAll("div[id^='groupnotes_group_']");
    
        foreach (var groupSection in groupSections)
        {
            var groupNameElement = groupSection.QuerySelector("h2");
            if (groupNameElement == null)
                continue;
            
            var groupName = groupNameElement.TextContent.Trim();
            if (string.IsNullOrEmpty(groupName))
                continue;
        
            //result.NoteTypes.Add(new NoteType { Name = groupName });
        
            var grid = FindGridForGroup(groupSection);
            if (grid != null)
            {
                var notes = ExtractNotesFromGrid(grid, groupName);
                result.Notes.AddRange(notes);
            }
        }

        return result;
    }
    
    
    
    private async Task SaveToDatabase(NoteImportResult result)
    {
        //await using var context = new FragranceLogContext();
        
        //foreach (var noteType in result.NoteTypes)
        //{
        //    if (!context.NoteTypes.Any(nt => nt.Name == noteType.Name))
        //    {
        //        context.NoteTypes.Add(noteType);
        //    }
        //}
        
        //// await context.SaveChangesAsync();
        
        //foreach (var note in result.Notes)
        //{
        //    var noteTypeId = context.NoteTypes
        //        .Where(nt => nt.Name == note.NoteTypeName)
        //        .Select(nt => nt.NoteTypeId)
        //        .FirstOrDefault();
                
        //    if (!context.Notes.Any(n => n.Name == note.Name && n.NoteTypeId == noteTypeId))
        //    {
        //        context.Notes.Add(new Note 
        //        { 
        //            Name = note.Name, 
        //            NoteTypeId = noteTypeId 
        //        });
        //    }
        //}
        // await context.SaveChangesAsync();
    }
}