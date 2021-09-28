using UnityEngine;
using UnityEditor;

public class GUISkinViewer : EditorWindow
{
    private Vector2 scrollPosition = Vector2.zero;
    private string search = string.Empty;

    [MenuItem("SNTool/GUISkinViewer")]
    public static void Init()
    {
        GetWindow(typeof(GUISkinViewer));
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal("HelpBox");
        GUILayout.Label("Click To Copy Style Name", "label");
        GUILayout.FlexibleSpace();
        GUILayout.Label("Search : ");
        search = EditorGUILayout.TextField(search);
        GUILayout.EndHorizontal();

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        foreach (GUIStyle style in GUI.skin)
        {
            if (style.name.ToLower().Contains(search.ToLower()))
            {
                GUILayout.BeginHorizontal("PopupCurveSwatchBackground");
                GUILayout.Space(7);
                if (GUILayout.Button(style.name, style))
                {
                    EditorGUIUtility.systemCopyBuffer = "\"" + style.name + "\"";
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.SelectableLabel("\"" + style.name + "\"");
                GUILayout.EndHorizontal();
                GUILayout.Space(11);
            }
        }

        GUILayout.EndScrollView();
    }
}