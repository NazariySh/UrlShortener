﻿namespace UrlShortener.WebApp.Models;

public class ShortUrlViewModel
{
    public string OriginalUrl { get; set; } = string.Empty;
    public string ShortUrl { get; set; } = string.Empty;
}