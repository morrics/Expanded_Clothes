using HutongGames.PlayMaker;
using MSCLoader;
using UnityEngine;
using System.Collections;

namespace Expanded_Clothes
{
    public class ClothesShelf : MonoBehaviour
    {
        public string ShelfID;
        public GameObject visualJacket;
        public GameObject visualCoverall;

        private FsmString interact;
        private FsmBool guiuse;
        private FsmBool guiassemble;

        private bool isInitialized = false;

        private bool jacketSynced = false;
        private bool coverallSynced = false;

        private Transform itemPivot;
        private Camera mainCam;
        private PlayMakerFSM hand;

        public bool canass = true;

        private Coroutine hangCoroutine;

        public void Init(string shelfID, GameObject vJacket, GameObject vCoverall)
        {
            SetupGUI();

            ShelfID = shelfID;
            visualJacket = vJacket;
            visualCoverall = vCoverall;

            var pivotObj = GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera/1Hand_Assemble/ItemPivot");
            itemPivot = pivotObj != null ? pivotObj.transform : null;

            var handObj = GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera/1Hand_Assemble/Hand");
            hand = handObj != null ? handObj.GetComponent<PlayMakerFSM>() : null;

            isInitialized = true;
        }

        private void SetupGUI()
        {
            var globals = PlayMakerGlobals.Instance.Variables;
            interact = globals.GetFsmString("GUIinteraction");
            guiuse = globals.GetFsmBool("GUIuse");
            guiassemble = globals.GetFsmBool("GUIassemble");

            var pov = FsmVariables.GlobalVariables.FindFsmGameObject("POV").Value;
            mainCam = pov != null ? pov.GetComponent<Camera>() : null;
        }

        private void Update()
        {
            if (!isInitialized || ClothesManager.Instance == null || mainCam == null || itemPivot == null) return;

            SyncItem(ClothesKind.Jacket, ref jacketSynced, visualJacket);
            SyncItem(ClothesKind.Coverall, ref coverallSynced, visualCoverall);

            Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Physics.Raycast(ray, out RaycastHit hit, 1.5f))
            {
                GameObject hitObj = hit.collider.gameObject;

                if (visualJacket != null && visualJacket.activeSelf && hitObj == visualJacket)
                {
                    ShowTakeGUI("TAKE JACKET", ClothesKind.Jacket);
                    return;
                }
                if (visualCoverall != null && visualCoverall.activeSelf && hitObj == visualCoverall)
                {
                    ShowTakeGUI("TAKE COVERALL", ClothesKind.Coverall);
                    return;
                }

                GameObject shelfModel = gameObject;
                if (hitObj == shelfModel || hitObj.transform.IsChildOf(shelfModel.transform))
                {
                    if (canass)
                        HandleAutoHang();
                    return;
                }
            }
        }

        private void HandleAutoHang()
        {
            if (!TryGetClothesInHand(out ClothesKind kind, out GameObject inHand))
            {
                ResetUI();
                return;
            }

            hangCoroutine = StartCoroutine(HangDelayed(kind, inHand));

            if (hand != null)
                hand.Fsm.Event(FsmEvent.FindEvent("PROCEED Drop"));
        }

        private IEnumerator HangDelayed(ClothesKind kind, GameObject inHand)
        {
            float start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - start < 0.1f)
                yield return null;

            if (!canass || ClothesManager.Instance == null)
            {
                hangCoroutine = null;
                yield break;
            }

            if (ClothesManager.Instance.IsItemOnShelf(ShelfID, kind))
            {
                hangCoroutine = null;
                yield break;
            }

            if (inHand != null)
                ClothesManager.Instance.DisablePhysicalItem(inHand);

            Assemble(kind);

            hangCoroutine = null;
        }

        private bool TryGetClothesInHand(out ClothesKind kind, out GameObject inHand)
        {
            kind = default;
            inHand = null;

            if (itemPivot == null || itemPivot.childCount == 0)
                return false;

            inHand = itemPivot.GetChild(0).gameObject;
            if (inHand == null) return false;

            string n = inHand.name.ToLowerInvariant();

            if (n.Contains("winter jacket"))
            {
                kind = ClothesKind.Jacket;
                return true;
            }

            if (n.Contains("winter coverall"))
            {
                kind = ClothesKind.Coverall;
                return true;
            }

            return false;
        }

        private void SyncItem(ClothesKind kind, ref bool syncedFlag, GameObject visualObj)
        {
            bool shouldBeOnShelf = ClothesManager.Instance.IsItemOnShelf(ShelfID, kind);

            if (!shouldBeOnShelf)
            {
                if (visualObj && visualObj.activeSelf) visualObj.SetActive(false);
                syncedFlag = false;
                return;
            }

            if (!syncedFlag)
            {
                if (visualObj && !visualObj.activeSelf) visualObj.SetActive(true);

                GameObject physItem = (kind == ClothesKind.Jacket)
                    ? ClothesManager.Instance.jacketItem
                    : ClothesManager.Instance.coverallItem;

                if (physItem != null)
                {
                    ClothesManager.Instance.DisablePhysicalItem(physItem);
                    syncedFlag = true;
                }
            }
        }

        private void Assemble(ClothesKind kind)
        {
            GameObject physicalItem = (kind == ClothesKind.Jacket)
                ? ClothesManager.Instance.jacketItem
                : ClothesManager.Instance.coverallItem;

            if (physicalItem != null)
                ClothesManager.Instance.DisablePhysicalItem(physicalItem);

            ClothesManager.Instance.SetItemLocation(ShelfID, kind);

            if (kind == ClothesKind.Jacket)
            {
                jacketSynced = true;
                if (visualJacket) visualJacket.SetActive(true);
            }
            else
            {
                coverallSynced = true;
                if (visualCoverall) visualCoverall.SetActive(true);
            }

            MasterAudio.PlaySound3DAndForget("PlayerMisc", transform, false, 1f, null, 0f,
                "clothing" + Random.Range(1, 3));

            ResetUI();
        }

        public void TakeFromShelf(ClothesKind kind)
        {
            ClothesManager.Instance.SetItemLocation("None", kind);

            if (kind == ClothesKind.Jacket)
            {
                jacketSynced = false;
                if (visualJacket) visualJacket.SetActive(false);
            }
            else
            {
                coverallSynced = false;
                if (visualCoverall) visualCoverall.SetActive(false);
            }

            GameObject item = (kind == ClothesKind.Jacket)
                ? ClothesManager.Instance.jacketItem
                : ClothesManager.Instance.coverallItem;

            ClothesManager.Instance.EnablePhysicalItem(item);
            item.transform.position = mainCam.transform.position + mainCam.transform.forward * 0.3f;

            ResetUI();
        }

        private void ShowTakeGUI(string text, ClothesKind kind)
        {
            guiuse.Value = true;
            guiassemble.Value = false;
            interact.Value = text;

            // take оставил на лкм как было
            if (Input.GetMouseButtonDown(0))
                TakeFromShelf(kind);
        }

        private void ResetUI()
        {
            guiassemble.Value = false;
            guiuse.Value = false;
            interact.Value = "";
        }
    }
}