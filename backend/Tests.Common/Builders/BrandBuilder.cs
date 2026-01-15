using Core.Entities;

namespace Tests.Common.Builders;

internal sealed class BrandBuilder
{
    private static int _seq = 1;

    private int _id = _seq;
    private string _name = $"Test Brand {_seq++}";

    public static BrandBuilder Default() => new();

    public BrandBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public BrandBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public Brand Build()
    {
        return new Brand
        {
            BrandId = _id,
            Name = _name,
            CompanyId = 1
        };
    }
}
