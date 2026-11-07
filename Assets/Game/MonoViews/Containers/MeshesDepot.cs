using System.Collections.Generic;
using UnityEngine;

namespace ZE.MechBattle
{
    public class MeshesDepot
    {
        private readonly Dictionary<int, Mesh> _dict = new(8);
        private int _nextId = 1;

        public int Register(Mesh mesh)
        {
            var id = _nextId++;
            _dict.Add(id, mesh);
            return id;
        }

        public bool TryGetMesh(int id, out Mesh mesh) => _dict.TryGetValue(id, out mesh);
    
    }
}
