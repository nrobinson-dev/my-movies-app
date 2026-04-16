namespace MyMoviesApp.Application.Common.Services;

public interface ITitleFormattingService
{
    /// <summary>
    /// Normalizes a title for database storage by moving leading articles to the end.
    /// Example: "The Dark Knight" → "Dark Knight, The"
    /// </summary>
    string NormalizeForStorage(string title);

    /// <summary>
    /// Formats a title for display by moving trailing articles back to the beginning.
    /// Example: "Dark Knight, The" → "The Dark Knight"
    /// </summary>
    string FormatForDisplay(string title);
}

public class TitleFormattingService : ITitleFormattingService
{
    private static readonly string[] Articles = { "the", "a", "an" };

    public string NormalizeForStorage(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return title;

        var trimmedTitle = title.Trim();
        var lowerTitle = trimmedTitle.ToLowerInvariant();

        foreach (var article in Articles)
        {
            if (lowerTitle.StartsWith(article + " "))
            {
                var articleLength = article.Length;
                var rest = trimmedTitle.Substring(articleLength).Trim();
                var capitalizedArticle = char.ToUpper(article[0]) + article.Substring(1);
                return $"{rest}, {capitalizedArticle}";
            }
        }

        return trimmedTitle;
    }

    public string FormatForDisplay(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return title;

        var trimmedTitle = title.Trim();

        foreach (var article in Articles)
        {
            var suffix = $", {article}";
            if (trimmedTitle.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                var rest = trimmedTitle.Substring(0, trimmedTitle.Length - suffix.Length);
                var capitalizedArticle = char.ToUpper(article[0]) + article.Substring(1);
                return $"{capitalizedArticle} {rest}";
            }
        }

        return trimmedTitle;
    }
}