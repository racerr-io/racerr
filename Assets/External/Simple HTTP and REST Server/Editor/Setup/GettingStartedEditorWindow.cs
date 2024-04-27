#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ShadowGroveGames.SimpleHttpAndRestServer.Editor.Setup
{
    public class GettingStartedEditorWindow : EditorWindow
    {
        private Texture2D _mainLogo, _reviewButtonBg, _reviewButtonBgHover;
        private GUIStyle _labelStyle, _reviewButtonStyle;
        private static bool _subscribed;

        [MenuItem("Window/Shadow Grove Games/" + Asset.NAME + "/Getting Started")]
        public static void ShowGettingStartedWindow()
        {
            GettingStartedEditorWindow window = (GettingStartedEditorWindow)EditorWindow.GetWindow(typeof(GettingStartedEditorWindow));
            window.position = new Rect(0, 0, 355, 260);
            window.titleContent = EditorHelper.TextContent("Getting Started: " + Asset.NAME);
            Rect mainPos;

#if UNITY_2020_1_OR_NEWER
            mainPos = EditorGUIUtility.GetMainWindowPosition();
#else
            mainPos = new Rect(Vector2.zero, Vector2.zero);
#endif
            var pos = window.position;
            float w = (mainPos.width - pos.width) * 0.5f;
            float h = (mainPos.height - pos.height) * 0.5f;
            pos.x = mainPos.x + w;
            pos.y = mainPos.y + h;
            window.position = pos;
        }

        void OnGUI()
        {
            _mainLogo = (Texture2D)Resources.Load(Asset.LOGO, typeof(Texture2D));
            _labelStyle = new GUIStyle("label");
            _labelStyle.fontSize = 13;
            _labelStyle.wordWrap = true;

            _reviewButtonBg = EditorHelper.MakeBackgroundTexture(1, 1, new Color32(61, 119, 194, 255));
            _reviewButtonBgHover = EditorHelper.MakeBackgroundTexture(1, 1, new Color32(106, 196, 252, 255));
            _reviewButtonStyle = new GUIStyle("button");
            _reviewButtonStyle.fontSize = 18;
            _reviewButtonStyle.fontStyle = FontStyle.Bold;
            _reviewButtonStyle.normal.background = _reviewButtonBg;
            _reviewButtonStyle.active.background = _reviewButtonBgHover;
            _reviewButtonStyle.focused.background = _reviewButtonBgHover;
            _reviewButtonStyle.onFocused.background = _reviewButtonBgHover;
            _reviewButtonStyle.hover.background = _reviewButtonBgHover;
            _reviewButtonStyle.onHover.background = _reviewButtonBgHover;
            _reviewButtonStyle.alignment = TextAnchor.MiddleCenter;
            _reviewButtonStyle.normal.textColor = new Color(1, 1, 1, 1);

            GUILayout.Box(_mainLogo, GUILayout.Width(this.position.width), GUILayout.Height(_mainLogo.height));
            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();

            EditorHelper.DrawFileButton(Asset.README_GUID, "Readme", this.position.width);
            EditorHelper.DrawLinkButton("https://discord.gg/hrTXpR3zaA", "Discord", this.position.width);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            EditorHelper.DrawLinkButton("https://shadow-grove.org/", "Website", this.position.width);
            EditorHelper.DrawLinkButton("https://github.com/ShadowGroveGames", "Github", this.position.width);

            EditorGUILayout.EndHorizontal();

            EditorHelper.DrawLinkButton("https://discord.gg/hrTXpR3zaA", "Contact us", this.position.width, true);

            GUILayout.Space(15);

            GUILayout.Label("Please rate the asset " + Asset.NAME + ". Your rating helps me a lot as a developer in the asset store and motivates me to make more assets for you!", _labelStyle);

            GUILayout.Space(10);

            if (GUILayout.Button("Leave us a review!", _reviewButtonStyle))
                Application.OpenURL(Asset.REVIEW_URL);
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            if (!EditorHelper.CheckAssembliesIsLoaded(Asset.DONT_SHOW_IF_ASSABMLY_LOADED))
                return;

            SubscribeToUpdate();
        }

        private static void SubscribeToUpdate()
        {
            if (Application.isBatchMode)
                return;

            if (!_subscribed && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                _subscribed = true;
                EditorApplication.update += ShowGettingStarted;
            }
        }

        private static void ShowGettingStarted()
        {
            EditorApplication.update -= ShowGettingStarted;

            bool shown = EditorPrefs.GetBool(Asset.EDITOR_PREFS_KEY_GETTING_STARTED, false);
            if (!shown)
            {
                EditorPrefs.SetBool(Asset.EDITOR_PREFS_KEY_GETTING_STARTED, true);
                ShowGettingStartedWindow();
            }
        }
    }
}
#endif