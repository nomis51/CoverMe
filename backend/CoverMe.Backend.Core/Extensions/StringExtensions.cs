namespace CoverMe.Backend.Core.Extensions;

public static class StringExtensions
{
    #region Public methods

    public static string ConvertPathToUnix(this string path)
    {
        return path.Replace('\\', '/');
    }
    
    public static string ConvertPathToWindows(this string path)
    {
        return path.Replace('/', '\\');
    }

    #endregion
}