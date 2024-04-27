using ShadowGroveGames.SimpleHttpAndRestServer.Scripts;
using UnityEditor;

namespace ShadowGroveGames.SimpleHttpAndRestServer.Editor
{
    [CustomEditor(typeof(SimpleProgrammaticEventServerScript))]
    public class SimpleProgrammaticEventServerScriptDrawer : SimpleEventServerScriptDrawerEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawProgrammaticEventNote();
        }
        protected void DrawProgrammaticEventNote()
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Events", _labelWordWrapStyle);
            EditorGUILayout.LabelField("Register your functions programatically as events on the OnReceiveRequestEvent event!", _smallLabelWordWrapStyle);
        }
    }
}
