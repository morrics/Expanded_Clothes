using HutongGames.PlayMaker;
using MSCLoader;
using System.Collections;
using UnityEngine;

namespace Expanded_Clothes
{
    internal class Dirty : MonoBehaviour
    {
        private const float MaxDirtness = 100f;
        private const float SecondsToDirtyFromClothes = 30f * 60f;
        private const float SecondsToDirtyClothesFromPlayer = 30f * 60f;

        public float JacketDirty;
        public float CoverallDirty;

        private FsmFloat playerDirtness;
        private FsmInt clothingType;

        private FsmFloat playerSweat;

        private PlayMakerFSM bodyTempFsm;
        private FsmFloat bodyheatAdd;

        private GameObject PLAYER;

        public string infoJacket;
        public string infoCoverall;

        private void Start()
        {
            var hud = GameObject.Find("GUI/HUD").transform.GetChild(8);
            hud.gameObject.SetActive(true);
            hud.localPosition = new Vector3(-11.5f, 6f, 0f);

            var jail = GameObject.Find("GUI/HUD").transform.GetChild(12);
            jail.localPosition = new Vector3(-11.5f, 5.6f, 0f);

            PLAYER = GameObject.Find("PLAYER");

            playerDirtness = FsmVariables.GlobalVariables.GetFsmFloat("PlayerDirtiness");
            playerSweat = FsmVariables.GlobalVariables.GetFsmFloat("PlayerSweat");

            var clothingFsm = GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera/FPSCamera/Clothing").GetComponent<PlayMakerFSM>();
            clothingType = clothingFsm != null ? clothingFsm.FsmVariables.GetFsmInt("Type") : null;

            bodyTempFsm = GameObject.Find("PLAYER/BodyTemp").GetComponent<PlayMakerFSM>();
            bodyheatAdd = bodyTempFsm.FsmVariables.GetFsmFloat("BodyheatAdd");

            if (SaveLoad.ValueExists(Expanded_Clothes.Instance, "EC_jacket_dirty"))
            {
                JacketDirty = SaveLoad.ReadValue<float>(Expanded_Clothes.Instance, $"EC_jacket_dirty");
                CoverallDirty = SaveLoad.ReadValue<float>(Expanded_Clothes.Instance, $"EC_coverall_dirty");
            }    

            StartCoroutine(DirtyPlayerTick());
        }

        private void Update()
        {
            if (JacketDirty <= 50)
                infoJacket = "clean";
            if (JacketDirty >= 50 && JacketDirty <= 70)
                infoJacket = "slightly stained";
            if (JacketDirty >= 70)
                infoJacket = "extremely dirty";

            if (CoverallDirty <= 50)
                infoCoverall = "clean";
            if (CoverallDirty >= 50 && CoverallDirty <= 70)
                infoCoverall = "slightly stained";
            if (CoverallDirty >= 70)
                infoCoverall = "extremely dirty";

            int type = clothingType.Value;

            if (type == 1)
                ApplyDirtyExchange(ref JacketDirty);
            else if (type == 2)
                ApplyDirtyExchange(ref CoverallDirty);

            ApplySweatToPlayerDirtiness();
            ApplyHeat(type);
        }

        private void ApplyDirtyExchange(ref float clothesDirty)
        {
            float delta = Time.deltaTime;

            float p = Mathf.Clamp(playerDirtness.Value, 0f, MaxDirtness);
            float c = Mathf.Clamp(clothesDirty, 0f, MaxDirtness);

            float sweat = playerSweat != null ? playerSweat.Value : 0f;
            float clothesFromPlayerMult = (sweat > 20f) ? 10f : 1f;

            if (p > c)
            {
                float rate = (MaxDirtness / SecondsToDirtyClothesFromPlayer) * clothesFromPlayerMult;
                c = Mathf.MoveTowards(c, p, rate * delta);
            }

            if (c > p)
            {
                float rate = MaxDirtness / SecondsToDirtyFromClothes;
                p = Mathf.MoveTowards(p, c, rate * delta);
                playerDirtness.Value = p;
            }

            clothesDirty = c;
        }
        private void ApplyHeat(int type)
        {
            float baseHeat = type == 1 ? 0.5f : type == 2 ? 1.8f : 0.025f;
            float dirt = type == 1 ? JacketDirty : type == 2 ? CoverallDirty : 0f;

            dirt = Mathf.Clamp(dirt, 0f, 100f);

            float heatMult = Mathf.Lerp(1f, 0.4f, dirt / 100f);
            bodyheatAdd.Value = baseHeat * heatMult;

            /*if (type == 1 || type == 2)
            {
                if (dirt >= 70f)
                {
                    float t = Mathf.InverseLerp(70f, 100f, dirt);
                    float sweat = Mathf.Lerp(0f, 30f, t);

                    playerSweat.Value = sweat; 
                }
            }*/ //TODO: reworking sweat system
        }

        private void ApplySweatToPlayerDirtiness()
        {
            float sweat = playerSweat.Value;
            if (sweat > 20f)
            {
                float addPerSec = Mathf.Lerp(0.02f, 0.08f, Mathf.InverseLerp(20f, 100f, sweat));
                playerDirtness.Value = Mathf.Clamp(playerDirtness.Value + addPerSec * Time.deltaTime, 0f, MaxDirtness);
            }
        }

        private IEnumerator DirtyPlayerTick()
        {
            var wait = new WaitForSeconds(180f);

            while (true)
            {
                yield return wait;
                if (playerDirtness.Value >= 70f)
                {
                    string variant = (Random.value < 0.5f) ? "shit02" : "shit03";
                    MasterAudio.PlaySound3DAndForget("Shit", PLAYER.transform, false, 1f, null, 0f, variant);
                }
            }
        }
    }
}
