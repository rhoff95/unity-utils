using Plugins.unity_utils.Scripts.Lighting;
using UnityEditor;
using UnityEngine;

namespace Plugins.unity_utils.Editor
{
    [CustomEditor(typeof(ShadowCaster2DTileMap))]
    public class ShadowCasterGeneratorEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            ShadowCaster2DTileMap generator = (ShadowCaster2DTileMap)target;
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();


            if (GUILayout.Button("Generate"))
            {

                generator.Generate();

            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Destroy All Children"))
            {

                generator.DestroyAllChildren();

            }
        }

    }
}
