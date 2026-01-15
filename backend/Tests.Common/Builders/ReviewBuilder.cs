using Core.Entities;

namespace Tests.Common.Builders;

internal sealed class ReviewBuilder
{
    private int? _id;
    private int _rating = 5;
    private string? _comment = "Nice";
    private DateTime _date = DateTime.UtcNow;
    private Perfume? _perfume;
    private User? _user;

    public static ReviewBuilder Default() => new();

    public ReviewBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public ReviewBuilder WithRating(int rating)
    {
        _rating = rating;
        return this;
    }

    public ReviewBuilder WithComment(string? comment)
    {
        _comment = comment;
        return this;
    }

    public ReviewBuilder WithDate(DateTime date)
    {
        _date = date;
        return this;
    }

    public ReviewBuilder For(Perfume perfume, User user)
    {
        _perfume = perfume;
        _user = user;
        return this;
    }

    public Review Build()
    {
        if (_perfume == null || _user == null)
            throw new InvalidOperationException("Review requires Perfume and User");

        var review = new Review
        {
            Perfume = _perfume,
            PerfumeId = _perfume.PerfumeId,
            User = _user,
            UserId = _user.UserId,
            Rating = _rating,
            Comment = _comment,
            ReviewDate = _date
        };

        if (_id.HasValue)
            review.ReviewId = _id.Value;

        return review;
    }
}
