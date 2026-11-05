using UnityEngine;
using UnityEngine.UI;

namespace RainbowBlockSaga.Presentation.Scripts.Map.ScrollableMap
{
    // Draw the road behind the actual pins, in the same scrolling coordinate space.
    public sealed class CandyMapRoad : MaskableGraphic
    {
        private Vector2[] centers;
        private static readonly float[] Edges = { -28, -23, -15, 0, 15, 23, 28 };
        private static readonly Color32[] Icing =
        {
            new(187, 108, 169, 255), new(255, 229, 246, 255), new(255, 166, 202, 255),
            new(255, 213, 199, 255), new(255, 191, 208, 255), new(255, 246, 219, 255),
            new(237, 165, 187, 255)
        };

        public void Connect(LevelPin[] pins)
        {
            centers = new Vector2[pins.Length];
            Vector2 min = Vector2.positiveInfinity, max = Vector2.negativeInfinity;
            for (int i = 0; i < pins.Length; i++)
            {
                var node = pins[i].transform;
                Vector3 point = Vector3.zero;
                while (node != rectTransform.parent)
                {
                    point = node.localRotation * Vector3.Scale(node.localScale, point) + node.localPosition;
                    node = node.parent;
                }
                centers[i] = point;
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            Vector2 middle = (min + max) * .5f;
            rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(.5f, .5f);
            rectTransform.sizeDelta = max - min + Vector2.one * 120f;
            rectTransform.localPosition = middle;
            for (int i = 0; i < centers.Length; i++)
                centers[i] -= middle;

            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (centers == null || centers.Length < 2) return;

            const int steps = 8;
            for (int i = 0; i < centers.Length - 1; i++)
            {
                Vector2 a = centers[i], b = centers[i + 1];
                Vector2 tangentA = (b - centers[Mathf.Max(0, i - 1)]) * .5f;
                Vector2 tangentB = (centers[Mathf.Min(i + 2, centers.Length - 1)] - a) * .5f;
                for (int j = i == 0 ? 0 : 1; j <= steps; j++)
                {
                    float t = j / (float)steps, u = 1f - t;
                    Vector2 c = a + tangentA / 3f, d = b - tangentB / 3f;
                    Vector2 point = u*u*u*a + 3*u*u*t*c + 3*u*t*t*d + t*t*t*b;
                    Vector2 direction = (u*u*(c-a) + 2*u*t*(d-c) + t*t*(b-d)).normalized;
                    Vector2 normal = new(-direction.y, direction.x);
                    int row = mesh.currentVertCount;
                    for (int k = 0; k < Edges.Length; k++)
                    {
                        mesh.AddVert(point + normal * Edges[k], Icing[k], Vector2.zero);
                        if (row == 0 || k == 0) continue;
                        int end = row + k, prev = end - Edges.Length;
                        mesh.AddTriangle(prev - 1, prev, end);
                        mesh.AddTriangle(prev - 1, end, end - 1);
                    }
                }
            }
        }
    }
}
