// Copyright (c) 2015 - 2019 Doozy Entertainment / Marlink Trading SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using Doozy.Editor.Internal;
using Doozy.Editor.Nody.Editors;
using Doozy.Engine.Extensions;
using Doozy.Engine.UI.Nodes;
using Doozy.Engine.Utils;
using UnityEditor;
using UnityEngine;

namespace Doozy.Editor.UI.Nodes
{
    [CustomEditor(typeof(PortalNode))]
    public class PortalNodeEditor : BaseNodeEditor
    {
        private PortalNode TargetNode { get { return (PortalNode) target; } }

        private InfoMessage m_infoMessageUnnamedNodeName,
                            m_infoMessageDuplicateNodeName,
                            m_infoMessageNotListeningForAnyGameEvent;

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateNodeName(TargetNode.GameEventToListenFor);
            m_infoMessageUnnamedNodeName = new InfoMessage(InfoMessage.MessageType.Error, UILabels.UnnamedNodeTitle, UILabels.UnnamedNodeMessage);
            m_infoMessageDuplicateNodeName = new InfoMessage(InfoMessage.MessageType.Error, UILabels.DuplicateNodeTitle, UILabels.DuplicateNodeMessage);
            m_infoMessageNotListeningForAnyGameEvent = new InfoMessage(InfoMessage.MessageType.Error, UILabels.NotListeningForAnyGameEventTitle, UILabels.NotListeningForAnyGameEventMessage);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            DrawHeader(Styles.GetStyle(Styles.StyleName.ComponentHeaderPortalNode), MenuUtils.PortalNode_Manual, MenuUtils.PortalNode_YouTube);
            DrawDebugMode(true);
            GUILayout.Space(DGUI.Properties.Space(2));
            DrawNodeName(false);
            m_infoMessageUnnamedNodeName.Draw(TargetNode.ErrorNodeNameIsEmpty, InspectorWidth);
            m_infoMessageDuplicateNodeName.Draw(TargetNode.ErrorDuplicateNameFoundInGraph, InspectorWidth);
            GUILayout.Space(DGUI.Properties.Space(8));
            DrawOutputSockets(BaseNode);
            GUILayout.Space(DGUI.Properties.Space(16));
            DrawGameEvent(GetProperty(PropertyName.m_gameEvent), Styles.GetStyle(Styles.StyleName.IconGameEvent), UILabels.ListeningForGameEvent);
            GUILayout.Space(DGUI.Properties.Space(2));
            serializedObject.ApplyModifiedProperties();
            if (NodeUpdated) UpdateNodeName(TargetNode.GameEventToListenFor + " " + UILabels.Portal);
            SendGraphEventNodeUpdated();
        }

        private void DrawGameEvent(SerializedProperty property, GUIStyle iconStyle, string label)
        {
            bool hasErrors = TargetNode.ErrorNotListeningForAnyGameEvent;
            ColorName colorName = !hasErrors ? DGUI.Colors.ActionColorName : ColorName.Red;
            GUILayout.BeginVertical();
            {
                GUILayout.Space(DGUI.Properties.Space(2));
                float alpha = GUI.color.a;
                GUI.color = GUI.color.WithAlpha(DGUI.Properties.TextIconAlphaValue(hasErrors));
                DrawSmallTitle(iconStyle, label, colorName);
                GUI.color = GUI.color.WithAlpha(alpha);

                GUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();
                    DGUI.Property.Draw(property, "", DGUI.Colors.ActionColorName, hasErrors);
                    if (EditorGUI.EndChangeCheck()) NodeUpdated = true;
                    GUILayout.Space(DGUI.Properties.Space());
                    if (DGUI.Button.Dynamic.DrawIconButton(Styles.GetStyle(Styles.StyleName.IconFaPaste),
                                                           UILabels.Paste,
                                                           Size.S, TextAlign.Left,
                                                           colorName, colorName,
                                                           DGUI.Properties.SingleLineHeight + DGUI.Properties.Space(2), false))
                        property.stringValue = EditorGUIUtility.systemCopyBuffer;
                    GUILayout.Space(DGUI.Properties.Space());
                    bool enabledState = GUI.enabled;
                    GUI.enabled = !hasErrors;
                    if (DGUI.Button.Dynamic.DrawIconButton(Styles.GetStyle(Styles.StyleName.IconFaCopy),
                                                           UILabels.Copy,
                                                           Size.S, TextAlign.Left,
                                                           colorName, colorName,
                                                           DGUI.Properties.SingleLineHeight + DGUI.Properties.Space(2), false))
                    {
                        EditorGUIUtility.systemCopyBuffer = property.stringValue;
                        Debug.Log(UILabels.GameEvent + " '" + property.stringValue + "' " + UILabels.HasBeenAddedToClipboard);
                    }

                    GUI.enabled = enabledState;
                }
                GUILayout.EndHorizontal();

                m_infoMessageNotListeningForAnyGameEvent.Draw(TargetNode.ErrorNotListeningForAnyGameEvent, InspectorWidth);
            }
            GUILayout.EndVertical();
        }
    }
}