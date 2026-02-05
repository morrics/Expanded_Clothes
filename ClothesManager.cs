using MSCLoader;
using System.Linq;
using UnityEngine;

namespace Expanded_Clothes
{
    public enum ClothesKind { None, Jacket, Coverall }

    public class ClothesManager
    {
        public static ClothesManager Instance;
        private Mod modInstance;

        public GameObject jacketItem;
        public GameObject coverallItem;

        public string jacketLocation = "None";
        public string coverallLocation = "None";

        public ClothesManager(Mod mod)
        {
            Instance = this;
            modInstance = mod;

            LoadData();
            FindItems();
        }

        public void FindItems()
        {
            jacketItem = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == "winter jacket(itemx)");
            coverallItem = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == "winter coverall(itemx)");
        }

        public bool IsItemOnShelf(string shelfID, ClothesKind kind)
        {
            if (kind == ClothesKind.Jacket) return jacketLocation == shelfID;
            if (kind == ClothesKind.Coverall) return coverallLocation == shelfID;
            return false;
        }

        public void SetItemLocation(string shelfID, ClothesKind kind)
        {
            if (kind == ClothesKind.Jacket) jacketLocation = shelfID;
            if (kind == ClothesKind.Coverall) coverallLocation = shelfID;   
        }

        public void DisablePhysicalItem(GameObject item)
        {
            var mesh = item.GetComponentInChildren<MeshRenderer>();
            if (mesh) mesh.enabled = false;
            var col = item.GetComponent<Collider>();
            if (col) col.enabled = false;
            var rb = item.GetComponent<Rigidbody>();
            if (rb) { rb.isKinematic = true; rb.useGravity = false; }
        }
        
        public void EnablePhysicalItem(GameObject item)
        {
            var mesh = item.GetComponentInChildren<MeshRenderer>();
            if (mesh) mesh.enabled = true;
            var col = item.GetComponent<Collider>();
            if (col) col.enabled = true;
            var rb = item.GetComponent<Rigidbody>();
            if (rb) 
            { 
                rb.isKinematic = false; 
                rb.useGravity = true; 
                rb.WakeUp(); 
            }
        }

        public void Save()
        {
            SaveLoad.WriteValue(modInstance, "EC_JacketLocation", jacketLocation);
            SaveLoad.WriteValue(modInstance, "EC_CoverallLocation", coverallLocation);
        }

        private void LoadData()
        {
            if (SaveLoad.ValueExists(modInstance, "EC_JacketLocation"))
                jacketLocation = SaveLoad.ReadValue<string>(modInstance, "EC_JacketLocation");

            if (SaveLoad.ValueExists(modInstance, "EC_CoverallLocation"))
                coverallLocation = SaveLoad.ReadValue<string>(modInstance, "EC_CoverallLocation");
        }
    }
}