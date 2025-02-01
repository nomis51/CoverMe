namespace CoverMe.Backend.Extensions;

public static class StringExtensions
{
    #region Public methods

    public static string Ellipsis(this string text, int maxLength = 30)
    {
        return text.Length <= maxLength ? text : text[..maxLength] + "...";
    }

    #endregion
}