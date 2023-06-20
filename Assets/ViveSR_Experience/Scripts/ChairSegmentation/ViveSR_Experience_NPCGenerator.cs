using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_NPCGenerator : MonoBehaviour
    {    
        bool isGeneratingNPC;
        [SerializeField] int NumOfNPC;
        List<ViveSR_Experience_NPCAnimationRef> NPCRefs = new List<ViveSR_Experience_NPCAnimationRef>();
        [SerializeField] GameObject NPCPrefab;

        ViveSR_Experience_Chair PlayersChair = null;
        List<ViveSR_Experience_Chair> Chairs = new List<ViveSR_Experience_Chair>();

        Vector3 Spawn_Pos, Spawn_Fwd, Portal_Pos;

        [SerializeField] GameObject PortalPrefeb;
        [SerializeField] ViveSR_PortalMgr portalMgr;
        [SerializeField] GameObject Floor;

        private static void TransitionMatCB(Material mat)
        {
            mat.shader = Shader.Find("ViveSR/Standard, AlphaTest, Stencil");
            mat.renderQueue = 2001;
        }
        private void Update()
        {
            //Update NPC
            if (NPCRefs.Count != 0 && !isGeneratingNPC)
            {
                //if player walks close to a chair, the npc yields to player.
                YieldToPlayer();

                //standing npc attempts to find a chair
                foreach (ViveSR_Experience_NPCAnimationRef NPCRef in NPCRefs)
                {
                    if (NPCRef.OccupyingChair == null) AssignClosestChair(NPCRef);
                }
            }
        }

        public void Play(List<ViveSR_Experience_Chair> Chairs = null)
        {
            isGeneratingNPC = false;

            Spawn_Fwd = ViveSR_Experience.instance.AttachPoint.transform.forward;
            Spawn_Pos = ViveSR_Experience.instance.AttachPoint.transform.position + Spawn_Fwd * 8;
            Spawn_Fwd = new Vector3(Spawn_Fwd.x, 0, Spawn_Fwd.z);
            Spawn_Pos = new Vector3(Spawn_Pos.x, 0, Spawn_Pos.z);

            Portal_Pos = Spawn_Pos - Spawn_Fwd * 2;

            ClearScene();

            if (Chairs != null) this.Chairs = Chairs;
            NumOfNPC = Chairs.Count;

            StartCoroutine(GenerateNPCs());
        }

        public void ClearScene()
        {
            Floor.SetActive(false);
            isGeneratingNPC = false;
            portalMgr.ClearAllPortals();

            foreach (ViveSR_Experience_NPCAnimationRef NPCRef in NPCRefs) Destroy(NPCRef.gameObject);
            foreach (ViveSR_Experience_Chair chair in Chairs) chair.SetOccupier(false);
            NPCRefs.Clear();
        }

        IEnumerator GenerateNPCs()
        {
            portalMgr.ClearAllPortals();
            isGeneratingNPC = true;
            Floor.SetActive(true);

            ViveSR_Portal portal = Instantiate(PortalPrefeb.GetComponent<ViveSR_Portal>());

            portal.TransitionMaterialUpdateCB = TransitionMatCB;

            portal.transform.position = Portal_Pos;
            portal.transform.forward = -Spawn_Fwd;

            portalMgr.AddPortal(portal.gameObject);
            portalMgr.UpdateViewerWorld();

            ViveSR_Experience_PortalAnimation pa = portal.GetComponent<ViveSR_Experience_PortalAnimation>();
            pa.PortalLogo.gameObject.layer = LayerMask.NameToLayer("VirtualWorldLayer");

            while (NPCRefs.Count < NumOfNPC && isGeneratingNPC)
            {
                ViveSR_Experience_NPCAnimationRef NPCRef = Instantiate(NPCPrefab).GetComponent<ViveSR_Experience_NPCAnimationRef>();
                NPCRefs.Add(NPCRef);

                NPCRef.transform.position = Spawn_Pos;
                NPCRef.transform.forward = -Spawn_Fwd;

                NPCRef.NPCAnimController.Walk(Portal_Pos, () =>
                {
                    AssignClosestChair(NPCRef);
                });

                //next fairy
                yield return new WaitForSeconds(2f);
            }

            if(pa != null) pa.SetPortalScale(false);

            isGeneratingNPC = false;
        }

        void AssignClosestChair(ViveSR_Experience_NPCAnimationRef NPCRef, ViveSR_Experience_Chair OldChair = null)
        {
            if (NPCRef.NPCAnimController.isActing) return;

            float minDist = 999;
            ViveSR_Experience_Chair targetChair = null;
            for (int i = 0; i < Chairs.Count; i++)
            {
                if (Chairs[i].isOccupied) continue;

                float distToNpc = Vector3.Distance(NPCRef.transform.position, Chairs[i].transform.position);

                if (distToNpc < minDist)
                {
                    minDist = distToNpc;
                    targetChair = Chairs[i];
                }
            }

            if(targetChair != null)
            {
                targetChair.SetOccupier(true, NPCRef);
                NPCRef.NPCAnimController.StartAnimationSequence_ChairFound(targetChair);
            }
            else
            {
                if(OldChair != null)
                    NPCRef.NPCAnimController.StartAnimationSequence_ChairNotFound(OldChair);
            }
        }

        void YieldToPlayer()
        {
            //Player walks away from a chair;
            if (PlayersChair != null && Vector3.Distance(ViveSR_Experience.instance.PlayerHeadCollision.transform.position, PlayersChair.transform.position) > 1)
            {
                PlayersChair.SetOccupier(false);
                PlayersChair = null;
            }

            //player find a closet chair within a distance
            float minDist = 999;
            ViveSR_Experience_Chair targetChair = null;
            for (int i = 0; i < Chairs.Count; i++)
            {
                Vector3 chairPos = Chairs[i].transform.position;
                Vector3 playerHeadPos = ViveSR_Experience.instance.PlayerHeadCollision.transform.position;

                float dist = Vector3.Distance(playerHeadPos, new Vector3(chairPos.x, playerHeadPos.y, chairPos.z));
                if (dist < minDist) minDist = dist;
                else continue; // this chair isn't the closest
                if (minDist >= 1f) continue; // the closest chair is too far
                if (PlayersChair == Chairs[i]) continue; // player is already using the closet chair.
                targetChair = Chairs[i];
            }

            if (targetChair == null) return;

            //NPC on the chair yields to the player
            if (targetChair.OccupyingNPC != null)
            {
                if (targetChair.OccupyingNPC.NPCAnimController.isActing) return;
                if (PlayersChair != null) PlayersChair.SetOccupier(false);

                ViveSR_Experience_NPCAnimationRef NPCToYield = targetChair.OccupyingNPC;
                PlayersChair = targetChair;
                PlayersChair.SetOccupier(true);
                NPCToYield.NPCAnimController.Stand(() => AssignClosestChair(NPCToYield, targetChair));
            }
            else
            {
                if (PlayersChair != null) PlayersChair.SetOccupier(false);
                PlayersChair = targetChair;
                PlayersChair.SetOccupier(true);
            }
        }
    }
}