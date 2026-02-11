namespace EvaluatorApp.Services;

public static class ThemeService
{
    private const string ThemeKey = "AppTheme";

    /// <summary>
    /// Applies the given theme and saves the preference.
    /// </summary>
    public static void ApplyTheme(bool isDark)
    {
        Application.Current.UserAppTheme = isDark ? AppTheme.Dark : AppTheme.Light;
        Preferences.Set(ThemeKey, isDark ? "Dark" : "Light");
    }

    /// <summary>
    /// Loads the saved theme preference. Defaults to Light if none is saved.
    /// </summary>
    public static void LoadSavedTheme()
    {
        string saved = Preferences.Get(ThemeKey, "Light");
        Application.Current.UserAppTheme = saved == "Dark" ? AppTheme.Dark : AppTheme.Light;
    }

    /// <summary>
    /// Returns true if the current theme is Dark.
    /// </summary>
    public static bool IsDarkMode()
    {
        return Preferences.Get(ThemeKey, "Light") == "Dark";
    }
}
