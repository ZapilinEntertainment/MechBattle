using UnityEngine;
using System.Collections.Generic;
using VContainer;

namespace ZE.MechBattle.Views
{
    public class VirtualRenderer
    {
        private readonly MeshesDepot _meshesDepot;
        private readonly MaterialsDepot _materialsDepot;
        private readonly Dictionary<DrawInstanceGroupKey, GroupDrawer> _drawers = new();
        private readonly List<Matrix4x4> _matrixArgumentsList = new();
        private const int BATCH_LIMIT = 1023;

        private class GroupDrawer
        {
            public bool IsEmpty => Matrices.Count == 0;
            public int ElementsCount => Matrices.Count;

            public readonly List<Matrix4x4> Matrices = new();
        }

        [Inject]
        public VirtualRenderer(MeshesDepot meshesDepot, MaterialsDepot materialsDepot)
        {
            _meshesDepot = meshesDepot;
            _materialsDepot = materialsDepot;
        }

        public void RenderInstances(VirtualRenderData[] dataArray)
        {
            if (dataArray == null || dataArray.Length == 0) return;

            // 1. PREPARE DATA
            for (int i = 0; i < dataArray.Length; i++)
            {
                var data = dataArray[i];
                var key = data.GetDrawGroupKey();
                Matrix4x4 matrix = Matrix4x4.TRS(data.WorldPosition, data.Rotation, data.Scale);

                var groupDrawer = GetOrCreateGroupDrawer(key);
                groupDrawer.Matrices.Add(matrix);
            }


            // 2.DRAW
            foreach (var drawerGroupKvp in _drawers)
            {
                var drawer = drawerGroupKvp.Value;
                if (drawer.IsEmpty)
                    continue;

                var key = drawerGroupKvp.Key;
                if (!_meshesDepot.TryGetMesh(key.MeshId, out var mesh) || !_materialsDepot.TryGetMaterialById(key.MaterialId, out var material))
                {
#if UNITY_EDITOR
                    UnityEngine.Debug.LogError("invalid mesh or material id");
#endif
                    continue;
                }

                DrawMatrices(mesh, material, drawer.Matrices);
            }
        }

        public void DrawMatrices(Mesh mesh, Material material, List<Matrix4x4> matrices) 
        {
            var readIndex = 0;
            var length = matrices.Count;

            if (length < BATCH_LIMIT)
            {
                // all group can be drawn in one batch
                Graphics.DrawMeshInstanced(mesh, 0, material, matrices);
            }
            else
            {
                do
                {
                    var handlingElementsCount = length - readIndex;
                    if (handlingElementsCount > BATCH_LIMIT)
                        handlingElementsCount = BATCH_LIMIT;

                    for (var i = readIndex; i < readIndex + handlingElementsCount; i++)
                    {
                        _matrixArgumentsList.Add(matrices[i]);
                    }

                    readIndex += handlingElementsCount;

                    Graphics.DrawMeshInstanced(mesh, 0, material, _matrixArgumentsList);
                    _matrixArgumentsList.Clear();
                }
                while (readIndex < length);
            }
        }

        private GroupDrawer GetOrCreateGroupDrawer(DrawInstanceGroupKey key)
        {
            if (!_drawers.TryGetValue(key, out var drawer))
            {
                drawer = new();
                _drawers.Add(key, drawer);
            }
            return drawer;
        }
    }
}
