namespace DataImporter.Models;

public sealed record MatchDecision
{
    public MatchKind Kind { get; init; }
    public DatasetPerfumeRecord Dataset { get; init; }
    public DbPerfumeRow? DbPerfume { get; init; }
    public double? Similarity { get; init; }
    public string? Reason { get; init; }

    private MatchDecision(
        MatchKind kind,
        DatasetPerfumeRecord dataset,
        DbPerfumeRow? dbPerfume,
        double? similarity,
        string? reason)
    {
        Kind = kind;
        Dataset = dataset;
        DbPerfume = dbPerfume;
        Similarity = similarity;
        Reason = reason;
    }

    public static MatchDecision Exact(DatasetPerfumeRecord d, DbPerfumeRow db) =>
        new MatchDecision(MatchKind.Exact, d, db, 1.0, null);

    public static MatchDecision Fuzzy(DatasetPerfumeRecord d, DbPerfumeRow db, double similarity) =>
        new MatchDecision(MatchKind.Fuzzy, d, db, similarity, null);

    public static MatchDecision NoMatch(DatasetPerfumeRecord d, string reason) =>
        new MatchDecision(MatchKind.None, d, null, null, reason);
}
