using Core.Entities;

namespace Tests.Common.Builders;

internal sealed class PerfumePhotoBuilder
{
    private int _perfumeId = 1;
    private string _path = "/img.jpg";

    public PerfumePhotoBuilder WithPerfumeId(int perfumeId)
    {
        _perfumeId = perfumeId;
        return this;
    }

    public PerfumePhotoBuilder WithPath(string path)
    {
        _path = path;
        return this;
    }

    public PerfumePhoto Build()
    {
        return new PerfumePhoto
        {
            PerfumeId = _perfumeId,
            Path = _path
        };
    }
}
