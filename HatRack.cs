
using UnityEngine;

namespace Expanded_Clothes
{
    internal class HatRack : MonoBehaviour
    {
        private GameObject visualJacket;
        private GameObject visualCoverall;
        private GameObject Rack;
        public void Init(GameObject rack)
        {
            Rack = rack;
            Transform clothes = Rack.transform.GetChild(1);
            visualJacket = clothes.Find("jacket")?.gameObject;
            visualCoverall = clothes.Find("coverall")?.gameObject;
        }
        public void Update()
        {
            if (visualJacket.activeSelf || visualCoverall.activeSelf)
                Rack.GetComponent<ClothesShelf>().canass = false;
            else
                Rack.GetComponent<ClothesShelf>().canass = true;
        }
    }
}
