using Core.Entities;

namespace Tests.Common.Builders;

internal sealed class UserBuilder
{
    private static int _seq = 1;

    private int _id = _seq;
    private string? _username;
    private string? _email;
    private string _password = "hashed-password";

    public static UserBuilder Default()
        => new();

    public static IEnumerable<User> Many(int count)
    {
        for (var i = 0; i < count; i++)
            yield return Default().Build();
    }

    public UserBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public UserBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public User Build()
    {
        var n = _seq++;

        return new User
        {
            UserId = _id,
            Username = _username ?? $"user{n}",
            Email = _email ?? $"user{n}@test.local",
            Password = _password
        };
    }
}
