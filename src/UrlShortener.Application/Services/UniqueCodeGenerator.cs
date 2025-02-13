using UrlShortener.Application.Interfaces;

namespace UrlShortener.Application.Services;

public class UniqueCodeGenerator : IUniqueCodeGenerator
{
    public const int UniqueCodeLength = 7;
    private const string UniqueCodeCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public string Generate()
    {
        var random = Random.Shared;

        var codeChars = new char[UniqueCodeLength];

        for (var i = 0; i < UniqueCodeLength; i++)
        {
            var randomIndex = random.Next(UniqueCodeCharacters.Length);

            codeChars[i] = UniqueCodeCharacters[randomIndex];
        }

        return new string(codeChars);
    }
}