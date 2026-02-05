// THIS CODE BY casper-3 (thurbridi) 
// https://github.com/thurbridi/MSC-HowMuchIsLeft?tab=Unlicense-1-ov-file
// REWORKED FOR MY NEEDS IN MOD

using UnityEngine;

namespace Expanded_Clothes
{
    public class StateDirty
    {
        static GameObject state;
        static TextMesh foregroundText;
        static TextMesh shadowText;


        static StateDirty()
        {
            CreateContentDescription();
        }

        static void CreateContentDescription()
        {
            GameObject partName = GameObject.Find("GUI/Indicators/Partname");

            state = GameObject.Instantiate(partName);

            GameObject.Destroy(state.GetComponent<PlayMakerFSM>());

            state.name = "StateDirty";
            state.transform.parent = partName.transform.parent;
            state.transform.localPosition = new Vector3(0.0f, -0.21f, 0.0f);

            foregroundText = state.GetComponent<TextMesh>();
            shadowText = state.transform.GetChild(0).GetComponent<TextMesh>();

            foregroundText.characterSize = 0.05f;
            shadowText.characterSize = 0.05f;
        }

        public static void SetText(string text)
        {
            foregroundText.text = text;
            shadowText.text = text;
        }

        public static void ClearText()
        {
            foregroundText.text = "";
            shadowText.text = "";
        }
    }
}