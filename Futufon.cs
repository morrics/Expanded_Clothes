using HutongGames.PlayMaker;
using UnityEngine;

namespace Expanded_Clothes
{
    internal class Futufon : MonoBehaviour
    {
        private GameObject DoorPivot;
        private GameObject DoorMesh;
        private Animation dooranim;
        private bool dooropened;

        public Camera mainCam;
        public FsmBool guiuse;
        private FsmString interact;
        private bool rayuse;

        private void Awake()
        {
            mainCam = FsmVariables.GlobalVariables.FindFsmGameObject("POV").Value.GetComponent<Camera>();
            guiuse = FsmVariables.GlobalVariables.FindFsmBool("GUIuse");
            interact = FsmVariables.GlobalVariables.FindFsmString("GUIinteraction");
        }

        public void Start()
        {
            GameObject Locker = GameObject.Find("JOBS/FACTORY/OpeningTimes/LOD1/Lockerroom/Locker");
            Locker.transform.SetParent(null);
            Locker.name = "ExpandedClothes_FACTORY";

            Locker.GetComponent<BoxCollider>().size = new Vector3(0.2f, 0.8f, 1.899867f);
            DoorPivot = Locker.transform.GetChild(0).gameObject;
            DoorMesh = DoorPivot.transform.GetChild(0).gameObject;
            DoorMesh.AddComponent<MeshCollider>();
            DoorMesh.GetComponent<MeshCollider>().sharedMesh = DoorMesh.GetComponent<MeshFilter>().mesh;
            dooranim = DoorPivot.GetComponent<Animation>();

            GameObject factory = GameObject.Instantiate(Expanded_Clothes.Shelf);
            Transform fclothes = factory.transform.GetChild(1);

            GameObject fvisualJacket = fclothes.Find("jacket")?.gameObject;
            GameObject fvisualCoverall = fclothes.Find("coverall")?.gameObject;
            fvisualJacket.transform.SetParent(Locker.transform);
            fvisualCoverall.transform.SetParent(Locker.transform);
            GameObject.Destroy(factory);

            fvisualJacket.transform.localPosition = new Vector3(0f, 0.1f, 0.7f);
            fvisualJacket.transform.localEulerAngles = new Vector3(0f, 0f, 90.00002f); ;
            fvisualJacket.transform.localScale = new Vector3(100f, 60f, 100f);

            fvisualCoverall.transform.localPosition = new Vector3(0f, 0.3f, 0.7f);
            fvisualCoverall.transform.localEulerAngles = new Vector3(0f, 0f, 90.00002f);
            fvisualCoverall.transform.localScale = new Vector3(100f, 60f, 100f);
            API.Register("Factory", Locker, fvisualJacket, fvisualCoverall);
            // factory end
        }
        private void Update()
        {
            rayuse = false;
            Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Physics.Raycast(ray, out RaycastHit hit, 1f))
            {
                GameObject hitObj = hit.collider.gameObject;

             if (hitObj == DoorMesh)
                {
                    rayuse = true;
                    guiuse.Value = true;

                    if (Input.GetMouseButtonDown(0))
                    {
                        dooropened = !dooropened;

                        if (dooropened)
                        {
                            dooranim.Play("locker_open");
                            MasterAudio.PlaySound3DAndForget("CarFoley", DoorMesh.transform, false, 1f, null, 0f, "car_old_trunk_open");
                        }
                        else
                        {
                            dooranim.Play("locker_close");
                            MasterAudio.PlaySound3DAndForget("CarFoley", DoorMesh.transform, false, 1f, null, 0f, "car_old_trunk_close");
                        }
                    }
                }
            }
        }
    }
}
