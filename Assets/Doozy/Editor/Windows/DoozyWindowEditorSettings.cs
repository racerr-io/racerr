// Copyright (c) 2015 - 2019 Doozy Entertainment / Marlink Trading SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using Doozy.Editor.Settings;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Doozy.Editor.Windows
{
    public partial class DoozyWindow
    {
        #region Instance

        private static DoozyWindow s_instance;

        public static DoozyWindow Instance
        {
            get
            {
                if (s_instance != null) return s_instance;
                s_instance = GetWindow<DoozyWindow>();
                return s_instance;
            }
        }

        /*
       * An alternative way to get Window, because
       * GetWindow<DoozyWindow>() forces window to be active and present
       */
        private static DoozyWindow Window
        {
            get
            {
                DoozyWindow[] windows = Resources.FindObjectsOfTypeAll<DoozyWindow>();
                return windows.Length > 0 ? windows[0] : null;
            }
        }

        #endregion

        #region Toolbar and UIView

        private AnimBool m_toolbarAnimBool;

        private AnimBool ToolbarAnimBool
        {
            get
            {
                if (m_toolbarAnimBool != null) return m_toolbarAnimBool;
                m_toolbarAnimBool = GetAnimBool(DoozyWindowSettings.Instance.EditorPrefsKeyWindowToolbarState);
                m_toolbarAnimBool.speed = DoozyWindowSettings.Instance.ToolbarAnimationSpeed;
                return m_toolbarAnimBool;
            }
        }

        private float ToolbarWidth { get { return DoozyWindowSettings.Instance.ToolbarCollapsedWidth + (DoozyWindowSettings.Instance.ToolbarExpandedWidth - DoozyWindowSettings.Instance.ToolbarCollapsedWidth) * ToolbarAnimBool.faded; } }
        private Rect ToolbarRect { get { return new Rect(0, 0, ToolbarWidth, position.height); } }
        private Rect ToolbarShadowRect { get { return new Rect(ToolbarWidth, ToolbarRect.y, DoozyWindowSettings.Instance.ToolbarShadowWidth, position.height); } }
        private float ViewWidth { get { return position.width - ToolbarWidth; } }
        private Rect ViewRect { get { return new Rect(ToolbarWidth, 0, ViewWidth, position.height); } }

        #endregion
    }
}