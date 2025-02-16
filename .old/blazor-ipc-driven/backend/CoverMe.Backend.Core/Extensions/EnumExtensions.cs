using System.ComponentModel;
using System.Reflection;

namespace CoverMe.Backend.Core.Extensions;

public static class EnumExtensions
{
    #region Public methods

    public static string Description(this Enum value)
    {
        return value.GetType()
            .GetMember(value.ToString())
            .FirstOrDefault()?
            .GetCustomAttribute<DescriptionAttribute>()?
            .Description ?? value.ToString();
    }

    public static T FromDescription<T>(this string description)
        where T : struct

    {
        var enumType = typeof(T);
        if (!enumType.IsEnum) return (T)(object)0;

        var fields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
        foreach (var field in fields)
        {
            if (field.GetCustomAttribute(typeof(DescriptionAttribute)) is not DescriptionAttribute attr) continue;
            if (attr.Description == description) return Enum.Parse<T>(field.Name);
        }

        return (T)(object)0;
    }

    #endregion
}