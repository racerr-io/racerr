// Copyright (c) 2015 - 2019 Doozy Entertainment / Marlink Trading SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using Doozy.Engine.Extensions;
using Doozy.Editor.Nody.NodeGUI;
using Doozy.Engine.UI.Nodes;
using UnityEngine;

namespace Doozy.Editor.UI.Nodes
{
    // ReSharper disable once UnusedMember.Global
    [CustomNodeGUI(typeof(PortalNode))]
    public class PortalNodeGUI : BaseNodeGUI
    {
        private static GUIStyle s_iconStyle;
        private static GUIStyle IconStyle { get { return s_iconStyle ?? (s_iconStyle = Styles.GetStyle(Styles.StyleName.NodeIconPortalNode)); } }
        protected override GUIStyle GetIconStyle() { return IconStyle; }
     
        private PortalNode TargetNode { get { return (PortalNode) Node; } }
        
        protected override void OnNodeGUI()
        {
            DrawNodeBody();
            DrawSocketsList(Node.OutputSockets);
            DrawActionDescription();
        }

        private readonly GUIStyle m_icon = Styles.GetStyle(Styles.StyleName.IconGameEventListener);
        private string m_title;
        private string m_description;
        
        private void DrawActionDescription()
        {
            DynamicHeight += DGUI.Properties.Space(4);
            float x = DrawRect.x + 16;
            float lineHeight = DGUI.Properties.SingleLineHeight;
            float iconLineHeight = lineHeight * 2;
            float iconSize = iconLineHeight * 0.6f;
            var iconRect = new Rect(x, DynamicHeight + (iconLineHeight - iconSize) / 2, iconSize, iconSize);
            float textX = iconRect.xMax + DGUI.Properties.Space(4);
            float textWidth = DrawRect.width - iconSize - DGUI.Properties.Space(4) - 32;
            var titleRect = new Rect(textX, DynamicHeight, textWidth, lineHeight);
            DynamicHeight += titleRect.height;
            var descriptionRect = new Rect(textX, DynamicHeight, textWidth, lineHeight);
            DynamicHeight += descriptionRect.height;
            DynamicHeight += DGUI.Properties.Space(4);

            if (ZoomedBeyondSocketDrawThreshold) return;

            m_title = UILabels.ListeningForGameEvent;
            m_description = TargetNode.GameEventToListenFor;
            
            Color iconAndTextColor = (DGUI.Utility.IsProSkin ? Color.white.Darker() : Color.black.Lighter()).WithAlpha(0.6f);
            DGUI.Icon.Draw(iconRect, m_icon, iconAndTextColor);
            GUI.Label(titleRect, m_title, DGUI.Colors.ColorTextOfGUIStyle(DGUI.Label.Style(Doozy.Editor.Size.S, TextAlign.Left), iconAndTextColor));
            GUI.Label(descriptionRect, m_description, DGUI.Colors.ColorTextOfGUIStyle(DGUI.Label.Style(Doozy.Editor.Size.M, TextAlign.Left), iconAndTextColor));
        }
    }

}