using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public class PlayerSpawnPoint : MonoBehaviour
    {
        public RigidTransform ToRigidTransform() => new(transform.rotation, transform.position);
    }
}
