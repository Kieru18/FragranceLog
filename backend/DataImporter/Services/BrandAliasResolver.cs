using System;
using System.Collections.Generic;

namespace DataImporter.Services;

public sealed class BrandAliasResolver
{
    private static readonly Dictionary<string, string> Aliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "Banderas", "Antonio Banderas" },
            { "Zoologist", "Zoologist Perfumes" },
            { "Rabanne", "Paco Rabanne" },
            { "Nishane Istanbul", "Nishane" },
            { "Shay & Blue", "Shay Blue London" },
            { "Tfk", "The Fragrance Kitchen" },
            { "Oscar", "Oscar De La Renta" },
            { "Roja", "Roja Dove" },
            { "Dunhill London", "Alfred Dunhill" },
            { "Demeter Fragrance Library", "Demeter Fragrance" },
            { "Lattafa", "Lattafa Perfumes" },
            { "Acqua Dell'Elba", "Acqua Dell Elba" },
            { "Au Pays De La Fleur D’Oranger", "Au Pays De La Fleur D Oranger" },
            { "Etat Libre D'Orange", "Etat Libre D Orange" },
            { "Initio Parfums Privé", "Initio Parfums Prives" },
            { "Jardins D'Écrivains", "Jardins D Ecrivains" },
            { "L'Artisan Parfumeur", "L Artisan Parfumeur" },
            { "Perfumer'S Workshop", "Perfumer S Workshop" },
            { "Tesori D’Oriente", "Tesori D Oriente" },
            { "Victoria'S Secret", "Victoria S Secret" },
            { "Bulgari", "Bvlgari" },
            { "D.S. & Durga", "Ds Durga" },
            { "D'Orsay", "D Orsay" },
            { "Eau D'Italie", "Eau D Italie" },
            { "L'Erbolario", "L Erbolario" },
            { "Liquides Imaginaires", "Les Liquides Imaginaires" },
            { "Penhaligon'S", "Penhaligon S" },
            { "Stéphane Humbert Lucas", "Stephane Humbert Lucas 777" },
            { "Women'Secret", "Women Secret" },
            { "Adopt", "Adopt Parfums" },
            { "Donna Karan Dkny", "Donna Karan" },
            { "Bond No. 9 I Love Ny", "Bond No 9" },
            //{ "L'Occitane", "L Occitane Au Bresil" },
            //{ "L'Occitane", "L Occitane En Provence" },
            { "Nikos Sculpture", "Nikos" },
            { "Rosendo Mateu", "Rosendo Mateu Olfactive Expressions" },
            { "Berdoues", "Parfums Berdoues" },
            { "Memo", "Memo Paris" },
            { "Ungaro", "Emanuel Ungaro" },
            { "Nicolaï", "Nicolai Parfumeur Createur" },
            { "Korloff", "Korloff Paris" },
            { "Michalsky", "Michael Michalsky" },
            { "Laurent Mazzone", "Laurent Mazzone Parfums" },
            { "Bugatti", "Bugatti Fashion" },
            { "Comporta", "Comporta Perfumes" },
            { "Kilian", "By Kilian" },
            { "Al Haramain", "Al Haramain Perfumes" },
            { "Armani Privé", "Giorgio Armani" },
            { "Dana Buchman", "Dana" },
            { "Le Couvent", "Le Couvent Maison De Parfum" },
            { "Tiffany & Co.", "Tiffany" },
            { "Monotheme", "Monotheme Venezia" },
        };

    private readonly NameNormalizer _normalizer;

    public BrandAliasResolver(NameNormalizer normalizer)
    {
        _normalizer = normalizer;
    }


    public string Resolve(string? rawBrand)
    {
        var norm = _normalizer.NormalizeWithDiacritics(rawBrand);

        if (string.IsNullOrWhiteSpace(norm))
            return "";

        foreach (var kv in Aliases)
        {
            var aliasNorm = _normalizer.NormalizeWithDiacritics(kv.Key);
            if (aliasNorm.Equals(norm, StringComparison.OrdinalIgnoreCase))
            {
                return _normalizer.NormalizeWithDiacritics(kv.Value);
            }
        }

        return norm;
    }
}
