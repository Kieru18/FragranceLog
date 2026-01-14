using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PerfumeRecognition.Services;

public sealed class LabHistogramColorDescriptor : IColorDescriptorExtractor
{
    private readonly int _binsL;
    private readonly int _binsA;
    private readonly int _binsB;
    private readonly int _sampleSize;

    public LabHistogramColorDescriptor(
        int binsL = 8,
        int binsA = 8,
        int binsB = 8,
        int sampleSize = 224)
    {
        _binsL = binsL;
        _binsA = binsA;
        _binsB = binsB;
        _sampleSize = sampleSize;
        Dimension = _binsL * _binsA * _binsB;
    }

    public int Dimension { get; }

    public float[] Extract(string imagePath)
    {
        using var img = Image.Load<Rgba32>(imagePath);

        if (img.Width != _sampleSize || img.Height != _sampleSize)
            img.Mutate(x => x.Resize(_sampleSize, _sampleSize));

        var hist = new float[Dimension];

        int idx(int l, int a, int b) => (l * _binsA * _binsB) + (a * _binsB) + b;

        long count = 0;

        for (int y = 0; y < img.Height; y++)
        {
            for (int x = 0; x < img.Width; x++)
            {
                var p = img[x, y];
                if (p.A <= 10)
                    continue;

                var (L, A, B) = RgbToLab(p.R, p.G, p.B);

                int lBin = ClampToBin(L / 100.0, _binsL);
                int aBin = ClampToBin((A + 128.0) / 255.0, _binsA);
                int bBin = ClampToBin((B + 128.0) / 255.0, _binsB);

                hist[idx(lBin, aBin, bBin)] += 1f;
                count++;
            }
        }

        if (count == 0)
            return hist;

        for (int i = 0; i < hist.Length; i++)
            hist[i] /= count;

        L2NormalizeInPlace(hist);
        return hist;
    }

    private static int ClampToBin(double v01, int bins)
    {
        if (v01 <= 0) return 0;
        if (v01 >= 1) return bins - 1;
        return (int)(v01 * bins);
    }

    private static void L2NormalizeInPlace(float[] v)
    {
        double s = 0;
        for (int i = 0; i < v.Length; i++)
            s += v[i] * v[i];

        var norm = Math.Sqrt(s);
        if (norm <= 0)
            return;

        var inv = 1.0 / norm;
        for (int i = 0; i < v.Length; i++)
            v[i] = (float)(v[i] * inv);
    }

    private static (double L, double A, double B) RgbToLab(byte r8, byte g8, byte b8)
    {
        double r = r8 / 255.0;
        double g = g8 / 255.0;
        double b = b8 / 255.0;

        r = r <= 0.04045 ? (r / 12.92) : Math.Pow((r + 0.055) / 1.055, 2.4);
        g = g <= 0.04045 ? (g / 12.92) : Math.Pow((g + 0.055) / 1.055, 2.4);
        b = b <= 0.04045 ? (b / 12.92) : Math.Pow((b + 0.055) / 1.055, 2.4);

        double X = r * 0.4124564 + g * 0.3575761 + b * 0.1804375;
        double Y = r * 0.2126729 + g * 0.7151522 + b * 0.0721750;
        double Z = r * 0.0193339 + g * 0.1191920 + b * 0.9503041;

        double Xn = 0.95047;
        double Yn = 1.00000;
        double Zn = 1.08883;

        double fx = Fxyz(X / Xn);
        double fy = Fxyz(Y / Yn);
        double fz = Fxyz(Z / Zn);

        double L = 116.0 * fy - 16.0;
        double A = 500.0 * (fx - fy);
        double B = 200.0 * (fy - fz);

        return (L, A, B);
    }

    private static double Fxyz(double t)
    {
        const double delta = 6.0 / 29.0;
        if (t > delta * delta * delta)
            return Math.Pow(t, 1.0 / 3.0);
        return (t / (3.0 * delta * delta)) + (4.0 / 29.0);
    }
}
