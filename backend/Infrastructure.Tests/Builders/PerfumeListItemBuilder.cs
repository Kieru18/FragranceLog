using Core.Entities;

namespace Infrastructure.Tests.Builders;

internal sealed class PerfumeListItemBuilder
{
    private int _id = 1;
    private PerfumeList? _list;
    private Perfume? _perfume;
    private DateTime? _creationDate;

    public static PerfumeListItemBuilder Default() => new();

    public PerfumeListItemBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public PerfumeListItemBuilder For(PerfumeList list, Perfume perfume)
    {
        _list = list;
        _perfume = perfume;
        return this;
    }

    public PerfumeListItemBuilder WithCreationDate(DateTime date)
    {
        _creationDate = date;
        return this;
    }

    public PerfumeListItem Build()
    {
        if (_list == null)
            throw new InvalidOperationException("PerfumeListItem requires PerfumeList");

        if (_perfume == null)
            throw new InvalidOperationException("PerfumeListItem requires Perfume");

        var item = new PerfumeListItem
        {
            PerfumeListItemId = _id,
            PerfumeList = _list,
            PerfumeListId = _list.PerfumeListId,
            Perfume = _perfume,
            PerfumeId = _perfume.PerfumeId,
            CreationDate = _creationDate ?? DateTime.UtcNow
        };

        return item;
    }
}
