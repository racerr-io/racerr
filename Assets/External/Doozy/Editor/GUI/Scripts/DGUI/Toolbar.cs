// Copyright (c) 2015 - 2019 Doozy Entertainment / Marlink Trading SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using Doozy.Engine.Extensions;
using UnityEngine;

namespace Doozy.Editor
{
    public static partial class DGUI
    {
        public static class Toolbar
        {
            public static int Draw(int index, List<ToolbarButton> buttons)
            {
                int result = index;
                GUILayout.BeginHorizontal();
                {
                    for (int i = 0; i < buttons.Count; i++)
                    {
                        ToolbarButton button = buttons[i];
                        bool isSelected = index == i;
                        if (i == 0)
                        {
                            if (DrawButton(button, TabPosition.Left, isSelected)) result = i;
                        }
                        else if (i == buttons.Count - 1)
                        {
                            if (DrawButton(button, TabPosition.Right, isSelected)) result = i;
                        }
                        else
                        {
                            if (DrawButton(button, TabPosition.Middle, isSelected)) result = i;
                        }
                    }

                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                return result;
            }

            private static bool DrawButton(ToolbarButton button, TabPosition tabPosition, bool isSelected)
            {
                Color initialColor = GUI.color;
                GUIStyle buttonStyle = Button.ButtonStyle(tabPosition, isSelected);

                if (button.IconStyle != null)
                {
                    GUILayout.BeginVertical();
                    buttonStyle.alignment = TextAnchor.MiddleLeft;
                    buttonStyle.padding.left = (int) (buttonStyle.fixedHeight + Properties.Space(2));
                }

                buttonStyle.padding.right = (int) Properties.Space(3);

                GUI.color = Colors.GetDColor(isSelected ? button.SelectedColorNameOld : button.NormalColorNameOld).Normal.WithAlpha(GUI.color.a * 0.8f);
                bool result = GUILayout.Button(new GUIContent(button.Name), buttonStyle);
                GUI.color = initialColor;

                if (button.IconStyle != null)
                {
                    float iconSize = buttonStyle.fixedHeight - Properties.Space(3);
                    GUILayout.Space(-buttonStyle.fixedHeight);
                    GUILayout.BeginHorizontal(GUILayout.Width(iconSize));
                    {
                        GUILayout.Space(Properties.Space(3));
                        GUI.color = Colors.GetDColor(isSelected ? button.SelectedColorNameOld : button.NormalColorNameOld).Normal.WithAlpha(GUI.color.a * 0.8f);
                        Icon.Draw(button.IconStyle, iconSize, buttonStyle.fixedHeight);
                        GUI.color = initialColor;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                }


                // ReSharper disable once InvertIf
                if (result)
                {
                    Properties.ResetKeyboardFocus();
                    if (button.Callback != null) button.Callback.Invoke();
                }

                return result;
            }

            [Serializable]
            public class ToolbarButton
            {
                public string Name;
                public GUIStyle IconStyle;
                public ColorName NormalColorNameOld;
                public ColorName SelectedColorNameOld;
                public Action Callback;

                public ToolbarButton(string name, GUIStyle iconStyle, ColorName normalColorNameOld, ColorName selectedColorNameOld, Action callback = null)
                {
                    Name = name;
                    IconStyle = iconStyle;
                    NormalColorNameOld = normalColorNameOld;
                    SelectedColorNameOld = selectedColorNameOld;
                    Callback = callback;
                }

                public ToolbarButton(string name, GUIStyle iconStyle, ColorName normalColorNameOld, Action callback = null)
                {
                    Name = name;
                    IconStyle = iconStyle;
                    NormalColorNameOld = normalColorNameOld;
                    SelectedColorNameOld = normalColorNameOld;
                    Callback = callback;
                }

                public ToolbarButton(string name, ColorName normalColorNameOld, ColorName selectedColorNameOld, Action callback = null)
                {
                    Name = name;
                    IconStyle = null;
                    NormalColorNameOld = normalColorNameOld;
                    SelectedColorNameOld = selectedColorNameOld;
                    Callback = callback;
                }

                public ToolbarButton(string name, ColorName normalColorNameOld, Action callback = null)
                {
                    Name = name;
                    IconStyle = null;
                    NormalColorNameOld = normalColorNameOld;
                    SelectedColorNameOld = normalColorNameOld;
                    Callback = callback;
                }
            }
        }
    }
}