#if UNITY_EDITOR
namespace ShadowGroveGames.SimpleHttpAndRestServer.Editor.Setup
{
    internal static class Asset
    {
        internal const string KEY = "SimpleHTTPandRESTServer";
        internal const string NAME = "Simple HTTP and REST Server";
        internal const string LOGO = "simple-http-and-rest-server-banner";
        internal const string REVIEW_URL = "https://assetstore.unity.com/packages/tools/utilities/simple-http-and-rest-server-244127?utm_source=editor#reviews";
        internal const string README_GUID = "cfbc7d0f39f158046a72ed9c5acff696";

        internal readonly static string[] DONT_SHOW_IF_ASSABMLY_LOADED = new string[]
        {
            "org.Shadow-Grove.LoginWithDiscord.Editor",
            "org.Shadow-Grove.CompleteToolboxForDiscord.Editor",
        };

        // Review
        internal const int REVIEW_MIN_OPENINGS = 2;
        internal const int REVIEW_MIN_DAYS = 10;

        // Editor Prefs
        internal const string EDITOR_PREFS_KEY_GETTING_STARTED = KEY + "-GettingStarted";
        internal const string EDITOR_PREFS_KEY_REVIEW_DISABLE_REMINDER = KEY + "-ReviewReminder";
        internal const string EDITOR_PREFS_KEY_REVIEW_EDITOR_OPEN_COUNT = KEY + "-ReviewEditorOpenCount";
        internal const string EDITOR_PREFS_KEY_REVIEW_INIT_DATE = KEY + "-ReviewInitDate";
    }
}
#endif