using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using DataImporter.Model;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace DataImporter.Importers;

public class NoteImporter : BaseImporter
{
    public override async Task ImportData(string url)
    {
        Console.WriteLine($"Importing notes from: {url}");
        
        var html = await GetHtmlAsync(url);
        
        var result = ParseNotesHtml(html);
        
        await SaveToDatabase(result);
        
        Console.WriteLine($"Imported: {result.NoteTypes.Count} note types, {result.Notes.Count} notes");
    }
    
    private NoteImportResult ParseNotesHtml(string html)
    {
        var result = new NoteImportResult();
        
        // Your parsing logic here
        // Extract NoteTypes
        // Extract Notes for each NoteType
        
        return result;
    }
    
    private async Task SaveToDatabase(NoteImportResult result)
    {
        using var context = new FragranceLogContext();
        
        // Save NoteTypes first
        foreach (var noteType in result.NoteTypes)
        {
            if (!context.NoteTypes.Any(nt => nt.Name == noteType.Name))
            {
                context.NoteTypes.Add(noteType);
            }
        }
        await context.SaveChangesAsync();
        
        // Then save Notes with proper NoteTypeId
        foreach (var note in result.Notes)
        {
            var noteTypeId = context.NoteTypes
                .Where(nt => nt.Name == note.NoteTypeName)
                .Select(nt => nt.Id)
                .FirstOrDefault();
                
            if (!context.Notes.Any(n => n.Name == note.Name && n.NoteTypeId == noteTypeId))
            {
                context.Notes.Add(new Note 
                { 
                    Name = note.Name, 
                    NoteTypeId = noteTypeId 
                });
            }
        }
        await context.SaveChangesAsync();
    }
}