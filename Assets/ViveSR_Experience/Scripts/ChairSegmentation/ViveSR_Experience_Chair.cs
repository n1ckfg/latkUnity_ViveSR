using UnityEngine;
using UnityEngine.AI;

namespace Vive.Plugin.SR.Experience
{                                 
    public class ViveSR_Experience_Chair : MonoBehaviour
    {
        NavMeshObstacle NaveMeshObstacle;
        public bool isOccupied { get; private set; }         
        public ViveSR_Experience_NPCAnimationRef OccupyingNPC { get; private set; }

        public void CreateChair(Vector3 Position, Vector3 Forward)
        {
            transform.position = Position;
            transform.forward = Forward;
            NaveMeshObstacle = gameObject.AddComponent<NavMeshObstacle>();
            NaveMeshObstacle.size = new Vector3(0.7f, 1, 0.05f);
            NaveMeshObstacle.center = new Vector3(0f, 0f, -0.2f);
        }


        public void SetOccupier(bool isOccupied, ViveSR_Experience_NPCAnimationRef OccupyingNPC = null)
        {
            ViveSR_Experience_NPCAnimationRef OldNPC = OccupyingNPC;
            ViveSR_Experience_NPCAnimationRef NewNPC = OccupyingNPC;

            this.isOccupied = isOccupied;
            this.OccupyingNPC = OccupyingNPC;

            if (OldNPC != null) OldNPC.OccupyingChair = null;
            if (NewNPC != null) NewNPC.OccupyingChair = this;
        }
    }
}