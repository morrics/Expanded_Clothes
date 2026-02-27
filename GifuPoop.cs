// by Talia Kuznetsova

using HutongGames.PlayMaker;
using UnityEngine;

namespace Expanded_Clothes 
{
    internal class GifuPoop : MonoBehaviour
    {
        private FsmBool Spilling;
        private Dirty Dirtyss;

        private GameObject ShitTank;
        private GameObject Player;
        void Start()
        {
            Player = GameObject.Find("PLAYER");
            Dirtyss = Player.GetComponent<Dirty>();
            ShitTank = GameObject.Find("GIFU(750/450psi)/ShitTank");

            var SpillFSM = PlayMakerFSM.FindFsmOnGameObject(ShitTank, "SpillPump");
            Spilling = SpillFSM.FsmVariables.GetFsmBool("SpillPump");
        }

        public void OnTriggerStay(Collider PoorBastard)
        {
            if (Spilling.Value)
            {
                if (PoorBastard.gameObject.name == "PLAYER")
                {
                    MasterAudio.PlaySound3DAndForget("Shit", Player.transform, false, 1f, null, 0f, "shit01");

                    if (Dirtyss.clothingType.Value == 1)
                    {
                        Dirtyss.JacketDirty = 100f;
                        Dirtyss.playerDirtness.Value = 100f;
                    }

                    else if (Dirtyss.clothingType.Value == 2)
                    {
                        Dirtyss.CoverallDirty = 100f;
                        Dirtyss.playerDirtness.Value = 100f;
                    }

                    else
                        Dirtyss.playerDirtness.Value = 100f;
                }
            }
        }
    }
}
