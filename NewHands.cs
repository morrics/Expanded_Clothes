using HutongGames.PlayMaker;
using MSCLoader;
using System.IO;
using UnityEngine;

namespace Expanded_Clothes
{
    public class NewHands : MonoBehaviour
    {
        private Texture2D handNormal, handNormalTattoo, Jacket, Coverall, JacketTattoo, CoverallTattoo;
        private GameObject handplayer;
        private SkinnedMeshRenderer handRenderer;
        private FsmBool hasTattoo;
        private FsmInt Clothing;

        public void Start()
        {
            var Instance = Expanded_Clothes.Instance;

            var clothingFSM = GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera/FPSCamera/Clothing")?.GetComponent<PlayMakerFSM>();
            Clothing = clothingFSM.FsmVariables.GetFsmInt("Type");
            
            var db = GameObject.Find("PlayerDatabase");
            hasTattoo = PlayMakerExtensions.GetPlayMaker(db, "Simulation").FsmVariables.GetFsmBool("Tattoo");
            
            handplayer = GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera/FPSCamera/MiddleFinger/Pivot/hand/hand_rigged");
            handRenderer = handplayer.GetComponent<SkinnedMeshRenderer>();

            string assetsPath = ModLoader.GetModAssetsFolder(Instance);

            AssetBundle ab = LoadAssets.LoadBundle("Expanded_Clothes.Assets.exclothes.unity3d");

            if (File.Exists(assetsPath + "/hands/handNormal.png"))
            {
                handNormal = LoadAssets.LoadTexture(Instance, "hands/handNormal.png");
            }
            else
            {
                handNormal = ab.LoadAsset<Texture2D>("handNormal.png");
            }

            // handTattoo
            if (File.Exists(assetsPath + "/hands/handTattoo.png"))
            {
                handNormalTattoo = LoadAssets.LoadTexture(Instance, "hands/handTattoo.png");
            }
            else
            {
                handNormalTattoo = ab.LoadAsset<Texture2D>("handTattoo.png");
            }

            // Jacket
            if (File.Exists(assetsPath + "/hands/Jacket.png"))
            {
                Jacket = LoadAssets.LoadTexture(Instance, "hands/Jacket.png");
            }
            else
            {
                Jacket = ab.LoadAsset<Texture2D>("Jacket.png");
            }

            // Coverall
            if (File.Exists(assetsPath + "/hands/Coverall.png"))
            {
                Coverall = LoadAssets.LoadTexture(Instance, "hands/Coverall.png");
            }
            else
            {
                Coverall = ab.LoadAsset<Texture2D>("Coverall.png");
            }

            // JacketTattoo
            if (File.Exists(assetsPath + "/hands/JacketTattoo.png"))
            {
                JacketTattoo = LoadAssets.LoadTexture(Instance, "hands/JacketTattoo.png");
            }
            else
            {
                JacketTattoo = ab.LoadAsset<Texture2D>("JacketTattoo.png");
            }

            // CoverallTattoo
            if (File.Exists(assetsPath + "/hands/CoverallTattoo.png"))
            {
                CoverallTattoo = LoadAssets.LoadTexture(Instance, "hands/CoverallTattoo.png");
            }
            else
            {
                CoverallTattoo = ab.LoadAsset<Texture2D>("CoverallTattoo.png");
            }

            ab.Unload(false);
        }

        private void Update()
        {
            Texture2D targetTex = null;
            int type = Clothing.Value;
            bool tattoo = hasTattoo.Value;

            if (type == 1) targetTex = tattoo ? JacketTattoo : Jacket;
            else if (type == 2) targetTex = tattoo ? CoverallTattoo : Coverall;
            else targetTex = tattoo ? handNormalTattoo : handNormal;

            if (handRenderer.sharedMaterial.GetTexture("_MainTex") != targetTex)
                handRenderer.sharedMaterial.SetTexture("_MainTex", targetTex);
        }
    }
}