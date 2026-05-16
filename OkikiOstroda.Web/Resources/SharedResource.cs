namespace OkikiOstroda.Web;

public class SharedResource
{
    private static readonly System.Resources.ResourceManager ResourceManager = new(typeof(SharedResource));

    public static string ValidationRequired => GetString(nameof(ValidationRequired));

    public static string ValidationEmail => GetString(nameof(ValidationEmail));

    public static string ValidationPhone => GetString(nameof(ValidationPhone));

    public static string ValidationGuestsRange => GetString(nameof(ValidationGuestsRange));

    public static string ValidationDateRangeRequired => GetString(nameof(ValidationDateRangeRequired));

    private static string GetString(string name) => ResourceManager.GetString(name) ?? name;
}
