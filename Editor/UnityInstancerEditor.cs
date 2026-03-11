using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnityInstancer))]
public class UnityInstancerEditor : Editor
{
    private const string addButtonLabel = "Add Objects";
    private const string clearButtonLabel = "Clear";
    private const string runButtonLabel = "Add Duplicates";
    private const string autodestroyButtonLabel = "Remove Comp from Proyect";
    private const string autodestroyDialogTitle = "Remove Component";
    private const string autodestroyDialogMessage =
        "This will remove UnityInstancer and UnityInstancerEditor scripts from the project. Continue?";
    private const string autodestroyDialogOk = "Remove";
    private const string autodestroyDialogCancel = "Cancel";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var inst = (UnityInstancer)target;

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(addButtonLabel))
        {
            // get current selection transforms
            var sel = Selection.transforms;
            if (sel != null && sel.Length > 0)
            {
                // Record undo for the target object so change can be undone
                Undo.RecordObject(inst, "Add Objects to ObjectsCollection");

                inst.AddObjects(sel);
                EditorUtility.SetDirty(inst);
            }
        }

        if (GUILayout.Button(clearButtonLabel))
        {
            Undo.RecordObject(inst, "Clear ObjectsCollection");
            inst.ClearObjectsCollection();
            EditorUtility.SetDirty(inst);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        if (GUILayout.Button(runButtonLabel))
        {
            Undo.RecordObject(inst, "Instantiate Duplicates");
            var created = inst.InstantiateDuplicates();
            foreach (var go in created)
                Undo.RegisterCreatedObjectUndo(go, "Instantiate Duplicates");
            EditorUtility.SetDirty(inst);
        }

        EditorGUILayout.Space();
        var prevBg = GUI.backgroundColor;
        var prevContent = GUI.contentColor;
        GUI.backgroundColor = Color.red;
        GUI.contentColor = Color.white;
        if (GUILayout.Button(autodestroyButtonLabel))
        {
            var confirmed = EditorUtility.DisplayDialog(
                autodestroyDialogTitle,
                autodestroyDialogMessage,
                autodestroyDialogOk,
                autodestroyDialogCancel);

            if (confirmed)
            {
                // Resolve script asset paths before destroying the component
                var runtimeScript = MonoScript.FromMonoBehaviour(inst);
                var editorScript = MonoScript.FromScriptableObject(this);

                var runtimePath = runtimeScript != null ? AssetDatabase.GetAssetPath(runtimeScript) : null;
                var editorPath = editorScript != null ? AssetDatabase.GetAssetPath(editorScript) : null;

                // Remove the component from the current GameObject
                Undo.DestroyObjectImmediate(inst);

                if (!string.IsNullOrEmpty(runtimePath))
                    AssetDatabase.DeleteAsset(runtimePath);
                if (!string.IsNullOrEmpty(editorPath))
                    AssetDatabase.DeleteAsset(editorPath);

                Debug.Log(
                    "Removed UnityInstancer scripts:\n" +
                    $"Runtime: {runtimePath}\n" +
                    $"Editor: {editorPath}");

                AssetDatabase.Refresh();
            }
        }
        GUI.backgroundColor = prevBg;
        GUI.contentColor = prevContent;
    }
}
