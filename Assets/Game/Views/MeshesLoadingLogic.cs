using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace ZE.MechBattle.Views
{
    public static class MeshesLoadingLogic
    {
        public static async Awaitable LoadSessionMeshes(IObjectResolver resolver)
        {
            var meshDepot = resolver.Resolve<MeshesDepot>();
            LoadPrebuiltMesh(meshDepot, ViewConstants.CUBE_MESH_KEY, "Cube.fbx");
            LoadPrebuiltMesh(meshDepot, ViewConstants.SPHERE_MESH_KEY, "Sphere.fbx");
            // replaced by custom cylinder_bullet (z-axis oriented)
            //LoadPrebuiltMesh(meshDepot, ViewConstants.CYLINDER_MESH_KEY, "Cylinder.fbx");

            await AssetsManager.LoadAssetsByLabel<IList<Mesh>>(
               Register,
               AssetConstants.MESH_LABEL, AssetConstants.SESSION_PRELOAD_LABEL);          

            void Register(IList<Mesh> meshes, string primaryKey)
            {
                if (meshes.Count == 1)
                {
                    meshDepot.RegisterElement(primaryKey, meshes[0]);
                }
                else
                {
                    foreach (var mesh in meshes)
                    {
                        meshDepot.RegisterElement(primaryKey + $"[{mesh.name}]", mesh);
                    }
                }
            }
        }

       

        private static void LoadPrebuiltMesh(MeshesDepot meshesDepot, string assetName, string resourceName)
        {
            var mesh = Resources.GetBuiltinResource<Mesh>(resourceName);
            meshesDepot.RegisterElement(assetName, mesh);
        }

    }
}
