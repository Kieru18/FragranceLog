using Core.Entities;

namespace Infrastructure.Tests.Builders;

internal sealed class PerfumeListBuilder
{
    private int _id = 1;
    private User? _user;
    private string _name = "Test List";
    private bool _isSystem;
    private DateTime? _creationDate;

    public static PerfumeListBuilder Default() => new();

    public PerfumeListBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public PerfumeListBuilder ForUser(User user)
    {
        _user = user;
        return this;
    }

    public PerfumeListBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public PerfumeListBuilder AsSystem()
    {
        _isSystem = true;
        return this;
    }

    public PerfumeListBuilder WithCreationDate(DateTime date)
    {
        _creationDate = date;
        return this;
    }

    public PerfumeList Build()
    {
        if (_user == null)
            throw new InvalidOperationException("PerfumeList requires User");

        var list = new PerfumeList
        {
            PerfumeListId = _id,
            User = _user,
            UserId = _user.UserId,
            Name = _name,
            IsSystem = _isSystem,
            CreationDate = _creationDate ?? DateTime.UtcNow
        };

        return list;
    }
}
