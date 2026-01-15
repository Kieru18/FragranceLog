using Core.Entities;

namespace Tests.Common.Builders;
internal sealed class GroupBuilder
{
    private int _id = 1;
    private string _name = "Group";

    public static GroupBuilder Default() => new();

    public GroupBuilder WithId(int id)
    {
        _id = id;
        _name = $"Group {_id}";
        return this;
    }

    public GroupBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public Group Build()
    {
        return new Group
        {
            GroupId = _id,
            Name = _name
        };
    }
}
