using UnityEngine;

namespace Expanded_Clothes
{
    internal class HouseElec : MonoBehaviour
    {
        private PlayMakerFSM HOUSE1;
        private PlayMakerFSM HOUSE2;

        private GameObject WASH1;
        private GameObject WASH2;

        private bool wash1;
        private bool wash2;

        private bool ELEC_HOUSE1;
        private bool ELEC_HOUSE2;

        private void Start()
        {
            HOUSE1 = GameObject.Find("HOMENEW/Functions/ElectricThings/HouseElectricity").transform.GetChild(0).GetComponent<PlayMakerFSM>();
            HOUSE2 = GameObject.Find("YARD/Building/Dynamics/HouseElectricity").transform.GetChild(2).GetComponent<PlayMakerFSM>();

            if (Expanded_Clothes.Instance.wAPART.GetValue())
            {
                WASH1 = GameObject.Find("ExpandedClothes_Washer_HOMENEW");
                wash1 = true;
            }
            if (Expanded_Clothes.Instance.wHOUSE.GetValue())
            {
                WASH2 = GameObject.Find("ExpandedClothes_Washer_House");
                wash2 = true;
            }
        }

        private void Update()
        {
            if (HOUSE1.gameObject.activeSelf && HOUSE1.FsmVariables.GetFsmBool("Bedroom1").Value)
                ELEC_HOUSE1 = true;
            else ELEC_HOUSE1 = false;
            if (HOUSE2.gameObject.activeSelf && HOUSE2.FsmVariables.GetFsmBool("Bathroom+Entry").Value)
                ELEC_HOUSE2 = true;
            else ELEC_HOUSE2 = false;

            if (wash1)
                WASH1.GetComponent<WashingMachine>().HasElectricity = ELEC_HOUSE1;
            if (wash2)
                WASH2.GetComponent<WashingMachine>().HasElectricity = ELEC_HOUSE2;
        }
    }
}
