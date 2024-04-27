#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ShadowGroveGames.SimpleHttpAndRestServer.Editor.Setup
{
    internal static class EditorHelper
    {
        internal static Hashtable s_TextGUIContents = new Hashtable();

        internal static void DrawLinkButton(string url, string title, float windowWidth, bool fullSize = false)
        {
            if (GUILayout.Button(title, GUILayout.Width(windowWidth * (fullSize ? 0.979f : 0.485f))))
                Application.OpenURL(url);
        }

        internal static void DrawFileButton(string guid, string title, float windowWidth, bool fullSize = false)
        {
            string readmePdfPath = GetFilePathByFileGuid(guid);
            if (!string.IsNullOrEmpty(readmePdfPath))
            {
                if (GUILayout.Button(title, GUILayout.Width(windowWidth * (fullSize ? 0.97f : 0.485f))))
                    Application.OpenURL(readmePdfPath);
            }
        }

        internal static string GetFilePathByFileGuid(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return null;

            string relativePath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(relativePath))
                return null;

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
            if (!File.Exists(filePath))
                return null;

            return filePath;
        }

        internal static Texture2D MakeBackgroundTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;

            Texture2D backgroundTexture = new Texture2D(width, height);
            backgroundTexture.SetPixels(pixels);
            backgroundTexture.Apply();
            return backgroundTexture;
        }


        // [0] original name, [1] localized name, [2] localized tooltip
        internal static string[] GetNameAndTooltipString(string nameAndTooltip)
        {
            string[] retval = new string[3];

            string[] s1 = nameAndTooltip.Split('|');

            switch (s1.Length)
            {
                case 0:
                    retval[0] = "";
                    retval[1] = "";
                    break;
                case 1:
                    retval[0] = s1[0].Trim();
                    retval[1] = retval[0];
                    break;
                case 2:
                    retval[0] = s1[0].Trim();
                    retval[1] = retval[0];
                    retval[2] = s1[1].Trim();
                    break;
                default:
                    Debug.LogError("Error in Tooltips: Too many strings in line beginning with '" + s1[0] + "'");
                    break;
            }
            return retval;
        }

        internal static GUIContent TextContent(string textAndTooltip)
        {
            if (textAndTooltip == null)
                textAndTooltip = "";

            string key = textAndTooltip;

            GUIContent gc = (GUIContent)s_TextGUIContents[key];
            if (gc != null)
                return gc;

            string[] strings = GetNameAndTooltipString(textAndTooltip);
            gc = new GUIContent(strings[1]);

            if (strings[2] != null)
                gc.tooltip = strings[2];

            s_TextGUIContents[key] = gc;

            return gc;
        }

        internal static bool CheckAssembliesIsLoaded(string[] assembliesList)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                    continue;

                string assemblyName = assembly.GetName().Name;

                if (assembliesList.Contains(assemblyName))
                    return false;
            }

            return true;
        }
    }
}
#endif