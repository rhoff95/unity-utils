using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Plugins.unity_utils.Scripts.Lighting
{
    [RequireComponent(typeof(CompositeCollider2D))]
    public class ShadowCaster2DTileMap : MonoBehaviour
    {
        [Space]
        [SerializeField]
        private bool selfShadows = true;

        private CompositeCollider2D _tilemapCollider;

        static readonly FieldInfo meshField = typeof(ShadowCaster2D).GetField("m_Mesh", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo shapePathField = typeof(ShadowCaster2D).GetField("m_ShapePath", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo shapePathHashField = typeof(ShadowCaster2D).GetField("m_ShapePathHash", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo generateShadowMeshMethod = typeof(ShadowCaster2D)
            .Assembly
            .GetType("UnityEngine.Rendering.Universal.ShadowUtility")
            .GetMethod("GenerateShadowMesh", BindingFlags.Public | BindingFlags.Static);
        public void Generate()
        {
            DestroyAllChildren();

            _tilemapCollider = GetComponent<CompositeCollider2D>();

            for (var i = 0; i < _tilemapCollider.pathCount; i++)
            {
                var pathVertices = new Vector2[_tilemapCollider.GetPathPointCount(i)];
                _tilemapCollider.GetPath(i, pathVertices);
                var shadowCaster = new GameObject("shadow_caster_" + i)
                {
                    transform =
                    {
                        parent = gameObject.transform
                    }
                };
                var shadowCasterComponent = shadowCaster.AddComponent<ShadowCaster2D>();
                shadowCasterComponent.selfShadows = this.selfShadows;

                var testPath = new Vector3[pathVertices.Length];
                for (var j = 0; j < pathVertices.Length; j++)
                {
                    testPath[j] = pathVertices[j];
                }

                shapePathField.SetValue(shadowCasterComponent, testPath);
                shapePathHashField.SetValue(shadowCasterComponent, Random.Range(int.MinValue, int.MaxValue));
                meshField.SetValue(shadowCasterComponent, new Mesh());
                generateShadowMeshMethod.Invoke(shadowCasterComponent, new object[] { meshField.GetValue(shadowCasterComponent), shapePathField.GetValue(shadowCasterComponent) });
            }
        }
        public void DestroyAllChildren()
        {

            var tempList = transform.Cast<Transform>().ToList();
            foreach (var child in tempList)
            {
                DestroyImmediate(child.gameObject);
            }

        }

    }
}