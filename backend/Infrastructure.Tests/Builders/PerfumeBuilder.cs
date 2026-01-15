using Core.Entities;
using Infrastructure.Tests.Builders;

internal sealed class PerfumeBuilder
{
    private int _id = 1;
    private string _name = "Test Perfume";
    private string _country = "POL";
    private Brand? _brand;
    private bool _withPhoto;

    public static PerfumeBuilder Default() => new();

    public PerfumeBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public PerfumeBuilder WithBrand(Brand brand)
    {
        _brand = brand;
        return this;
    }

    public PerfumeBuilder WithCountry(string code)
    {
        _country = code;
        return this;
    }

    public PerfumeBuilder WithPhoto()
    {
        _withPhoto = true;
        return this;
    }

    public PerfumeBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public Perfume Build()
    {
        if (_brand == null)
            throw new InvalidOperationException("Perfume requires Brand");

        var perfume = new Perfume
        {
            PerfumeId = _id,
            Name = _name,
            Brand = _brand,
            BrandId = _brand.BrandId,
            CountryCode = _country
        };

        if (_withPhoto)
        {
            perfume.PerfumePhoto = new PerfumePhoto
            {
                Path = "photo.jpg"
            };
        }

        return perfume;
    }
}
