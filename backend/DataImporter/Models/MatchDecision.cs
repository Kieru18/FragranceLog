namespace DataImporter.Models;

public sealed record MatchDecision
{
    public MatchKind Kind { get; init; }
    public DatasetPerfumeRecord Dataset { get; init; }
    public DbPerfumeRow? DbPerfume { get; init; }
    public IReadOnlyList<DbPerfumeRow>? DbPerfumes { get; init; }
    public double? Similarity { get; init; }
    public string? Reason { get; init; }

    private MatchDecision(
        MatchKind kind,
        DatasetPerfumeRecord dataset,
        DbPerfumeRow? dbPerfume,
        IReadOnlyList<DbPerfumeRow>? dbPerfumes,
        double? similarity,
        string? reason)
    {
        Kind = kind;
        Dataset = dataset;
        DbPerfume = dbPerfume;
        DbPerfumes = dbPerfumes;
        Similarity = similarity;
        Reason = reason;
    }

    public static MatchDecision Exact(DatasetPerfumeRecord d, DbPerfumeRow db) =>
        new(MatchKind.Exact, d, db, null, 1.0, null);

    public static MatchDecision Fuzzy(DatasetPerfumeRecord d, DbPerfumeRow db, double similarity) =>
        new(MatchKind.Fuzzy, d, db, null, similarity, null);

    public static MatchDecision Canonical(DatasetPerfumeRecord d, IReadOnlyList<DbPerfumeRow> dbs) =>
        new(MatchKind.Canonical, d, null, dbs, null, null);

    public static MatchDecision NoMatch(DatasetPerfumeRecord d, string reason) =>
        new(MatchKind.None, d, null, null, null, reason);
}
