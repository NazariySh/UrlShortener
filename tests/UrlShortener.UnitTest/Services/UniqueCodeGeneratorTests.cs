using FluentAssertions;
using UrlShortener.Application.Services;

namespace UrlShortener.UnitTest.Services;

public class UniqueCodeGeneratorTests
{
    private readonly UniqueCodeGenerator _uniqueCodeGenerator;

    public UniqueCodeGeneratorTests()
    {
        _uniqueCodeGenerator = new UniqueCodeGenerator();
    }

    [Fact]
    public void Generate_Should_ReturnCorrectLengthCode()
    {
        var code = _uniqueCodeGenerator.Generate();

        code.Should().HaveLength(UniqueCodeGenerator.UniqueCodeLength);
    }

    [Fact]
    public void Generate_Should_ReturnOnlyValidCharacters()
    {
        var validChars = new HashSet<char>(UniqueCodeGenerator.UniqueCodeCharacters);

        var code = _uniqueCodeGenerator.Generate();

        code.All(c => validChars.Contains(c)).Should().BeTrue();
    }


    [Theory]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
    public void Generate_Should_ReturnUniqueCodes(int count)
    {
        var codes = new HashSet<string>();

        for (var i = 0; i < count; i++)
        {
            var code = _uniqueCodeGenerator.Generate();
            codes.Add(code);
        }

        codes.Should().HaveCount(count);
    }
}