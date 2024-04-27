#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace ShadowGroveGames.SimpleHttpAndRestServer.Editor.Setup
{
    public class ReviewEditorWindow : EditorWindow
    {
        private Texture2D _mainLogo;
        private GUIStyle _labelStyle;
        private static bool _subscribed;

        [MenuItem("Window/Shadow Grove Games/" + Asset.NAME + "/Review")]
        public static void ShowReviewWindow()
        {
            ReviewEditorWindow window = (ReviewEditorWindow)EditorWindow.GetWindow(typeof(ReviewEditorWindow));
            window.position = new Rect(0, 0, 390, 200);
            window.titleContent = EditorHelper.TextContent("Review: " + Asset.NAME);
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

            GUILayout.Box(_mainLogo, GUILayout.Width(this.position.width), GUILayout.Height(_mainLogo.height));
            GUILayout.Space(5);

            GUILayout.Label("Please rate the asset " + Asset.NAME + ". Your rating helps me a lot as a developer in the asset store and motivates me to make more assets for you!", _labelStyle);
            GUILayout.Space(15);

            if (GUILayout.Button("Don't Ask Again", GUILayout.Width(this.position.width)))
            {
                this.Close();
                EditorPrefs.SetBool(Asset.EDITOR_PREFS_KEY_REVIEW_DISABLE_REMINDER, true);
            }

            if (GUILayout.Button("Ask Later", GUILayout.Width(this.position.width)))
            {
                this.Close();
                ResetReviewInitDate();
            }

            if (GUILayout.Button("Leave A Review", GUILayout.Width(this.position.width)))
            {
                this.Close();
                EditorPrefs.SetBool(Asset.EDITOR_PREFS_KEY_REVIEW_DISABLE_REMINDER, true);
                Application.OpenURL(Asset.REVIEW_URL);
            }
        }

        private static void ResetReviewInitDate()
        {
            EditorPrefs.SetString(Asset.EDITOR_PREFS_KEY_REVIEW_INIT_DATE, DateTime.Now.ToBinary().ToString());
        }

        private static void CheckReview()
        {
            if (EditorPrefs.GetBool(Asset.EDITOR_PREFS_KEY_REVIEW_DISABLE_REMINDER, false))
                return;

            int checkRemindCount = (EditorPrefs.GetInt(Asset.EDITOR_PREFS_KEY_REVIEW_EDITOR_OPEN_COUNT, 0) + 1);
            EditorPrefs.SetInt(Asset.EDITOR_PREFS_KEY_REVIEW_EDITOR_OPEN_COUNT, checkRemindCount);

            if (checkRemindCount < Asset.REVIEW_MIN_OPENINGS)
                return;

            string dateTimeString = EditorPrefs.GetString(Asset.EDITOR_PREFS_KEY_REVIEW_INIT_DATE, string.Empty);

            // If empty reset 
            if (string.IsNullOrWhiteSpace(dateTimeString))
            {
                ResetReviewInitDate();
                return;
            }

            long dateTimeBinary;
            // Failed to parse long
            if (!long.TryParse(dateTimeString, out dateTimeBinary))
            {
                ResetReviewInitDate();
                return;
            }

            // Not enough time passed.
            DateTime dateTime = DateTime.FromBinary(dateTimeBinary);
            Debug.Log("Date:" + (DateTime.Now - dateTime).TotalDays);
            if ((DateTime.Now - dateTime).TotalDays < Asset.REVIEW_MIN_DAYS)
                return;

            // Show!
            EditorPrefs.SetInt(Asset.EDITOR_PREFS_KEY_REVIEW_DISABLE_REMINDER, 0);

            ShowReviewWindow();
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
                EditorApplication.update += ShowReview;
            }
        }

        private static void ShowReview()
        {
            EditorApplication.update -= ShowReview;

            bool shown = EditorPrefs.GetBool(Asset.EDITOR_PREFS_KEY_GETTING_STARTED, false);
            if (shown)
                CheckReview();
        }
    }
}
#endif