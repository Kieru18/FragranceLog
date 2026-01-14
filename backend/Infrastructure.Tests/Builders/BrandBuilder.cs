using Core.Entities;

namespace Infrastructure.Tests.Builders;

internal sealed class BrandBuilder
{
    private int _id = 1;
    private string _name = "Test Brand";
    private Company? _company;

    public static BrandBuilder Default() => new();

    public BrandBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public BrandBuilder WithCompany(Company company)
    {
        _company = company;
        return this;
    }

    public Brand Build()
    {
        _company ??= new Company
        {
            CompanyId = 1,
            Name = "Test Company"
        };

        return new Brand
        {
            BrandId = _id,
            Name = _name,
            Company = _company,
            CompanyId = _company.CompanyId
        };
    }
}
