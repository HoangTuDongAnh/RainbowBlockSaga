using System.Collections.Generic;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Utils
{
    public static class TransformUtils
    {
        public static void SetParentPosition(this Transform mainTR, Vector3 v)
        {
            var list = new List<Transform>();
            var parent = mainTR.parent;
            for (var i = parent.childCount - 1; i >= 0; --i)
            {
                var child = parent.GetChild(i);
                child.SetParent(parent.parent);
                list.Add(child);
            }

            parent.transform.position = v;

            foreach (var child in list)
            {
                child.SetParent(parent, true);
            }
        }
    }
}