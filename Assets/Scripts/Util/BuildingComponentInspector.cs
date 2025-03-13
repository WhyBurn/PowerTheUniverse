using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(BuildingComponent)), CanEditMultipleObjects]
public class BuildingComponentInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Orient On Planet"))
        {
            ((BuildingComponent)target).SetPositionAndUpNormal();
        }
    }
}
#endif