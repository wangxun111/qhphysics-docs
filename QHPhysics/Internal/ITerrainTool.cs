using System;
using UnityEngine;

namespace QH.Physics {
    public interface ITerrainTool {
        Boolean IsValid {
            get;
        }
        Single GetHeight(Vector3 pos);
        Vector3 GetNormal(Vector3 pos);

        public void Clear();
    }
}