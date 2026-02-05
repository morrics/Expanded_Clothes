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

        private bool canAssembleJacket = false;
        private bool canAssembleCoverall = false;
        private Coroutine resetCoroutine;

        private Transform itemPivot;
        private Camera mainCam;

        public bool canass = true;

        public void Init(string shelfID, GameObject vJacket, GameObject vCoverall)
        {
            SetupGUI();
            this.ShelfID = shelfID;
            this.visualJacket = vJacket;
            this.visualCoverall = vCoverall;

            GameObject pivotObj = GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera/1Hand_Assemble/ItemPivot");
            itemPivot = pivotObj.transform;

            this.isInitialized = true;
        }

        private void SetupGUI()
        {
            var globals = PlayMakerGlobals.Instance.Variables;
            interact = globals.GetFsmString("GUIinteraction");
            guiuse = globals.GetFsmBool("GUIuse");
            guiassemble = globals.GetFsmBool("GUIassemble");
            mainCam = FsmVariables.GlobalVariables.FindFsmGameObject("POV").Value.GetComponent<Camera>();
        }

        private void Update()
        {
            if (!isInitialized || ClothesManager.Instance == null) return;

            SyncItem(ClothesKind.Jacket, ref jacketSynced, visualJacket);
            SyncItem(ClothesKind.Coverall, ref coverallSynced, visualCoverall);

            if (Input.GetMouseButtonDown(0))
            {
                if (canAssembleJacket) { PrepareAndAssemble(ClothesKind.Jacket); return; }
                if (canAssembleCoverall) { PrepareAndAssemble(ClothesKind.Coverall); return; }
            }

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

                GameObject shelfModel = transform.gameObject;
                if (hitObj == shelfModel || hitObj.transform.IsChildOf(shelfModel.transform))
                {
                    if (canass)
                        CheckHandAndPrepare();
                    return;
                }

            }

            if ((canAssembleJacket || canAssembleCoverall))
            {
                resetCoroutine = StartCoroutine(ResetFlagsDelayed());
            }
        }

        private void CheckHandAndPrepare()
        {
            GameObject inHand = itemPivot.GetChild(0).gameObject;

            bool isJacket = inHand.name.Contains("winter jacket");
            bool isCoverall = inHand.name.Contains("winter coverall");

            if (!isJacket && !isCoverall) return;

            if (isJacket && !ClothesManager.Instance.IsItemOnShelf(ShelfID, ClothesKind.Jacket))
            {
                canAssembleJacket = true;
                canAssembleCoverall = false;
            }
            else if (isCoverall && !ClothesManager.Instance.IsItemOnShelf(ShelfID, ClothesKind.Coverall))
            {
                canAssembleCoverall = true;
                canAssembleJacket = false;
            }

            if (resetCoroutine != null) 
            { 
                StopCoroutine(resetCoroutine); 
                resetCoroutine = null; 
            }

            guiassemble.Value = true;
            guiuse.Value = false;
            interact.Value = "ASSEMBLE CLOTHES";
        }

        private void PrepareAndAssemble(ClothesKind kind)
        {
            GameObject physItem = (kind == ClothesKind.Jacket)
                ? ClothesManager.Instance.jacketItem
                : ClothesManager.Instance.coverallItem;

            if (itemPivot.childCount > 0)
            {
                GameObject item = itemPivot.GetChild(0).gameObject;
                ClothesManager.Instance.DisablePhysicalItem(item);
            }


            if (physItem != null)
            {
                Assemble(physItem, kind);
            }

            canAssembleJacket = false;
            canAssembleCoverall = false;
        }

        private IEnumerator ResetFlagsDelayed()
        {
            ResetUI();
            yield return new WaitForSeconds(0.1f);
            canAssembleJacket = false;
            canAssembleCoverall = false;
            resetCoroutine = null;
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
            if (shouldBeOnShelf && !syncedFlag)
            {
                if (visualObj && !visualObj.activeSelf) visualObj.SetActive(true);
                GameObject physItem = (kind == ClothesKind.Jacket) ? ClothesManager.Instance.jacketItem : ClothesManager.Instance.coverallItem;
                if (physItem != null)
                {
                    ClothesManager.Instance.DisablePhysicalItem(physItem);
                    syncedFlag = true;
                }
            }
        }

        private void Assemble(GameObject physicalItem, ClothesKind kind)
        {
            ClothesManager.Instance.DisablePhysicalItem(physicalItem);
            ClothesManager.Instance.SetItemLocation(ShelfID, kind);

            if (kind == ClothesKind.Jacket) { jacketSynced = true; if (visualJacket) visualJacket.SetActive(true); }
            if (kind == ClothesKind.Coverall) { coverallSynced = true; if (visualCoverall) visualCoverall.SetActive(true); }

            MasterAudio.PlaySound3DAndForget("PlayerMisc", transform, false, 1f, null, 0f, "clothing" + Random.Range(1, 3));
            ResetUI();
        }

        public void TakeFromShelf(ClothesKind kind)
        {
            ClothesManager.Instance.SetItemLocation("None", kind);

            if (kind == ClothesKind.Jacket) { jacketSynced = false; if (visualJacket) visualJacket.SetActive(false); }
            if (kind == ClothesKind.Coverall) { coverallSynced = false; if (visualCoverall) visualCoverall.SetActive(false); }

            GameObject item = (kind == ClothesKind.Jacket) ? ClothesManager.Instance.jacketItem : ClothesManager.Instance.coverallItem;
            ClothesManager.Instance.EnablePhysicalItem(item);
            item.transform.position = mainCam.transform.position + mainCam.transform.forward * 0.3f;
            ResetUI();
        }

        private void ShowTakeGUI(string text, ClothesKind kind)
        {
            guiuse.Value = true;
            interact.Value = text;
            if (Input.GetMouseButtonDown(0)) TakeFromShelf(kind);
        }

        private void ResetUI()
        {
            guiassemble.Value = false;
            guiuse.Value = false;
            interact.Value = "";
        }
    }
}