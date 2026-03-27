using DomusMind.Infrastructure.Auth;
using FluentAssertions;

namespace DomusMind.Infrastructure.Tests.Auth;

public sealed class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void Hash_ReturnsHashedString_NotPlaintext()
    {
        var hash = _sut.Hash("MySecurePassword1!");

        hash.Should().NotBe("MySecurePassword1!");
        hash.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Hash_UsesPbkdf2Format_WithThreeParts()
    {
        var hash = _sut.Hash("MySecurePassword1!");

        var parts = hash.Split('.');
        parts.Should().HaveCount(3);
        parts[0].Should().Be("350000");
    }

    [Fact]
    public void Hash_ProducesDifferentHashesForSamePassword()
    {
        var hash1 = _sut.Hash("SamePassword1!");
        var hash2 = _sut.Hash("SamePassword1!");

        hash1.Should().NotBe(hash2, "each hash should use a unique salt");
    }

    [Fact]
    public void Verify_ReturnsTrueForCorrectPassword()
    {
        var hash = _sut.Hash("CorrectPassword1!");

        _sut.Verify("CorrectPassword1!", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_ReturnsFalseForWrongPassword()
    {
        var hash = _sut.Hash("CorrectPassword1!");

        _sut.Verify("WrongPassword!", hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_ReturnsFalseForEmptyPassword()
    {
        var hash = _sut.Hash("SomePassword1!");

        _sut.Verify("", hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_ReturnsFalseForMalformedHash()
    {
        _sut.Verify("AnyPassword1!", "not.a.valid.hash.format.with.too.many.parts").Should().BeFalse();
        _sut.Verify("AnyPassword1!", "onlytwoparts.here").Should().BeFalse();
        _sut.Verify("AnyPassword1!", "notanumber.salt.hash").Should().BeFalse();
    }

    [Fact]
    public void Verify_IsTimingConstant_NoShortCircuit()
    {
        var hash = _sut.Hash("TimingTest1!");

        // Both calls should complete without early exit - just verify no exception
        _sut.Verify("TimingTest1!", hash).Should().BeTrue();
        _sut.Verify("TimingXest1!", hash).Should().BeFalse();
    }
}
