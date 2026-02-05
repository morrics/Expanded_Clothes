using HowMuchIsLeft;
using MSCLoader;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UniversalShoppingSystem;

namespace Expanded_Clothes
{
    public class Expanded_Clothes : Mod
    {
        public override Game SupportedGames => Game.MyWinterCar;
        public override string ID => "Expanded_Clothes";
        public override string Name => "Expanded Clothes";
        public override string Author => "Morri";
        public override string Version => "0.3.2"; //public 0.3.1
        public override string Description => "Small tweaks for improved logic clothes (Beta)";

        SettingsText text;
        SettingsButton button;

        public SettingsCheckBox Shelfs;
        public SettingsCheckBox Racks;
        public SettingsCheckBox ClothesHands;
        public SettingsCheckBox DirtyСlothes;

        

        public static SettingsCheckBox Logs;
        public static Expanded_Clothes Instance;

        public SettingsCheckBox sHouse;
        public SettingsCheckBox sAparment;
        public SettingsCheckBox sCottage;

        public SettingsCheckBox rPSK;
        public SettingsCheckBox rPUB;
        public SettingsCheckBox rINSP;
        public SettingsCheckBox rWFAC;
        public SettingsCheckBox rKUN;
        public SettingsCheckBox rFLET;
        public SettingsCheckBox rCABIN;

        public SettingsCheckBox wAPART;
        public SettingsCheckBox wHOUSE;

        public SettingsHeader Locations;
        public SettingsHeader Locations2;
        public SettingsHeader wash;
        public SettingsHeader uss_install;

        private Texture2D jttexture;
        private Texture2D cvtexture;

        private GameObject psell;

        public static GameObject WasherPrefab;
        public static GameObject Shelf;
        public static GameObject CoatRack;

        private bool NewGame;
        private bool USS;

        private bool W1;
        private bool W2;

        public override void ModSetup()
        {
            SetupFunction(Setup.OnMenuLoad, Mod_OnMenu);
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
            SetupFunction(Setup.PreLoad, Mod_PreLoad);
            SetupFunction(Setup.PostLoad, Mod_PostLoad);
            SetupFunction(Setup.OnSave, Mod_OnSave);
            SetupFunction(Setup.OnNewGame, Mod_OnNewGame);
            SetupFunction(Setup.ModSettings, Mod_Settings);
        }
        private void Mod_Settings()
        {
            Settings.AddHeader("Features");
            Shelfs = Settings.AddCheckBox("Shelfs", "Shelves for coat", true, s_locs_vis);
            Racks = Settings.AddCheckBox("Racks", "Racks for coat", true, r_locs_vis);
            DirtyСlothes = Settings.AddCheckBox("DirtyСlothes", "Clothes can get dirty, washing system (returns dirtiness)", true, wash_visible, false);
            ClothesHands = Settings.AddCheckBox("ClothesHands", "Clothes on hands", true);
            text = Settings.AddText("This button remove save, and reset clothes position", false);
            button = Settings.AddButton("Reset clothes", ActivateItems, false);

            Locations = Settings.AddHeader("Shelfs", true, false);
            Settings.AddText("When disabling locations, ensure that they do not contain any clothing, otherwise you will have to reset it");
            sHouse = Settings.AddCheckBox("sHouse", "Shelf in parrent house", true);
            sAparment = Settings.AddCheckBox("sAparment", "Shelf in apartment", true);
            sCottage = Settings.AddCheckBox("sCottage", "Shelf in cottage", true);

            Locations2 = Settings.AddHeader("Racks", true, false);
            Settings.AddText("When disabling locations, ensure that they do not contain any clothing, otherwise you will have to reset it");
            rPUB = Settings.AddCheckBox("rPUB", "Clothes rack in pub", true);
            rPSK = Settings.AddCheckBox("rPSK", "Clothes rack in PSK", true);
            rINSP = Settings.AddCheckBox("rINSP", "Clothes rack in Inspection center", true);
            rWFAC = Settings.AddCheckBox("rWFAC", "Clothes rack in waterfacility", true);
            rKUN = Settings.AddCheckBox("rKUN", "Clothes rack in nursing home (Kunnalliskoti)", true);
            rFLET = Settings.AddCheckBox("rFLET", "Clothes rack in repair shop (fleetari)", true);
            rCABIN = Settings.AddCheckBox("rCABIN", "Clothes rack in cabin (ventti)", true);

            wash = Settings.AddHeader("Washers", true, false);
            text = Settings.AddText("When disabling locations, ensure that they do not contain any clothing, otherwise you will have to reset it");
            wAPART = Settings.AddCheckBox("wAPART", "Washer in apartment", true);
            wHOUSE = Settings.AddCheckBox("wHOUSE", "Washer in parrent house", true);

            Settings.AddHeader("Misc", true);
            Logs = Settings.AddCheckBox("Logs", "Logging mod actions during loading", false);

            uss_install = Settings.AddHeader("Mods needed for systems", true);
            Settings.AddText("<b>Washing system</b>", TextAlignment.Center);
            Settings.AddText("Download and put it in your MWC Mod folder under <b><color=yellow>References</color></b>");
            Settings.AddButton("Universal Shopping System", USS_NEXUS);
            Settings.AddText("");
            Settings.AddText("<b>Mod supports (not required for installation)</b>", TextAlignment.Center);
            Settings.AddText("A mod that displays the contents of some items when you look at them");
            Settings.AddButton("How Much Is Left", HMIL_NEXUS);

            Settings.AddHeader("GitHub source (Wiki)", true);
            Settings.AddButton("Expanded Clothes repo", GITHUB_REPO);

            Settings.AddHeader("Credits");
            Settings.AddText("<b>Huge thanks</b>", TextAlignment.Center);
            Settings.AddText("<b><color=green>honeycomb936</color></b>: idea with mod, help with code");
            Settings.AddText("<b><color=teal>DUBOVYK</color></b>: high-quality hand textures");
            Settings.AddText("<b><color=brown>traxr</color></b>: high-quality clothes");
            Settings.AddText("<b><color=orange>cinnerax</color></b>: high-quality 3d-models");
            Settings.AddText("");
            Settings.AddText("<b>Resources used</b>", TextAlignment.Center);
            Settings.AddText("<b><color=yellow>Universal Shopping System</color></b> by <b><color=teal>honeycomb936</color></b>");
            Settings.AddText("<b><color=yellow>HowMuchIsLeft</color></b> by <b><color=teal>casper-3</color></b>");
            Settings.AddText("<b>Script from GitHub, from <color=yellow>HowMuchIsLeft</color></b>: for cloth status <i>(ItemContentDescription.cs)</i>");
        }
        private void wash_visible() => wash.SetVisibility(DirtyСlothes.GetValue());
        private void s_locs_vis() => Locations.SetVisibility(Shelfs.GetValue());
        private void r_locs_vis() => Locations2.SetVisibility(Racks.GetValue());
        private void USS_NEXUS() => Process.Start("https://www.nexusmods.com/mywintercar/mods/796");
        private void HMIL_NEXUS() => Process.Start("https://www.nexusmods.com/mywintercar/mods/724");
        private void GITHUB_REPO() => Process.Start("https://github.com/morrics/Expanded_Clothes");

        private void Mod_OnMenu()
        {
            s_locs_vis();
            r_locs_vis();
            if (ModLoader.IsReferencePresent("UniversalShoppingSystem"))
            {
                USS = true;
                DirtyСlothes.SetVisibility(true);
                wash_visible();
            }
        }
        private void Mod_PreLoad()
        {
            Instance = this;
            AssetBundle ab = LoadAssets.LoadBundle("Expanded_Clothes.Assets.exclothes.unity3d");

            if (Shelfs.GetValue())
                Shelf = ab.LoadAsset<GameObject>("ExpandedClothes.prefab");

            if (Racks.GetValue())
                CoatRack = ab.LoadAsset<GameObject>("coat_rack.prefab");

            if (DirtyСlothes.GetValue() && USS)
                WasherPrefab = ab.LoadAsset<GameObject>("WASHMACHINE.prefab");

            if (File.Exists(ModLoader.GetModAssetsFolder(this) + "/clothes/jacket.png"))
            {
                jttexture = LoadAssets.LoadTexture(this, "clothes/jacket.png");
                Shelf.transform.Find("CLOTHES").transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", jttexture);
                CoatRack.transform.Find("CLOTHES").transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", jttexture);
            }
            if (File.Exists(ModLoader.GetModAssetsFolder(this) + "/clothes/coverall.png"))
            {
                cvtexture = LoadAssets.LoadTexture(this, "clothes/coverall.png");
                Shelf.transform.Find("CLOTHES").transform.GetChild(1).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", cvtexture);
                CoatRack.transform.Find("CLOTHES").transform.GetChild(1).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", cvtexture);
            }

            ab.Unload(false);
            text.SetVisibility(true);
            button.SetVisibility(true);
        }

        public void Load_Location()
        {
            if (Shelfs.GetValue())
            {
                if (sHouse.GetValue())
                {
                    API.Shelf("House", new Vector3(-10.94f, 1.359f, 10.666f), new Vector3(270f, 180f, 0f));
                    GameObject.Find("YARD/Building/LIVINGROOM/hat_shelf").SetActive(false);
                }

                if (sAparment.GetValue())
                {
                    API.Shelf("HOMENEW", new Vector3(-1285.335f, 2.068f, 1080.581f), new Vector3(270f, 121.7128f, 0f));
                    GameObject.Find("HOMENEW").transform.GetChild(3).GetChild(8).gameObject.SetActive(false);
                }

                if (sCottage.GetValue())
                {
                    API.Shelf("Cottage", new Vector3(-848.7f, -0.8f, 506.9f), new Vector3(270f, 73f, 0f));
                }
            }

            if (Racks.GetValue())
            {
                if (rPUB.GetValue())
                    API.CoatRack("PUB", new Vector3(-1543.4f, 4.23f, 1185.4f), new Vector3(0f, 285f, 0f), true);

                if (rPSK.GetValue())
                    API.CoatRack("PSK", new Vector3(-1735.249f, 3.75f, 934.5574f), new Vector3(0f, 255f, 0f), true);

                if (rINSP.GetValue())
                    API.CoatRack("INSPECTION", new Vector3(-1528.015f, 3.55f, 1257.432f), new Vector3(0f, 150f, 0f), false);

                if (rWFAC.GetValue())
                    API.CoatRack("WFAC", new Vector3(1517.547f, 5.75f, 1342.587f), new Vector3(0f, 75f, 0f), false);

                if (rKUN.GetValue())
                    API.CoatRack("Kunnalliskoti", new Vector3(-1479.352f, 3.95f, 1237.906f), new Vector3(0f, 150f, 0f), true);

                if (rFLET.GetValue())
                    API.CoatRack("Fleetari", new Vector3(1557.859f, 5.2f, 737.1321f), new Vector3(0f, 60f, 0f), true);

                if (rCABIN.GetValue())
                    API.CoatRack("CABIN", new Vector3(-163.3342f, -3.5f, 1019.265f), new Vector3(0f, 120f, 0f), false);
            }

            if (DirtyСlothes.GetValue())
            {
                if (wHOUSE.GetValue() && USS)
                {
                    GameObject.Find("YARD/Building/MIDDLEROOM/washingmachine").SetActive(false);
                    GameObject.Find("YARD/Building/MIDDLEROOM/LOD_middleroom/homo_004").SetActive(false);
                    API.Washer("House", new Vector3(-13.4f, 0f, 3.85f), new Vector3(0f, 0f, 0f));
                    W1 = true;
                }
                if (wAPART.GetValue() && USS)
                {
                    API.Washer("HOMENEW", new Vector3(-1287.4f, 0.7f, 1081.6f), new Vector3(0f, 302f, 0f));
                    W2 = true;
                }
            }
        }

        private void Mod_OnLoad()
        {
            new ClothesManager(this);


            if (jttexture != null)
            {
                ClothesManager.Instance.jacketItem.transform.Find("mesh").GetComponent<MeshRenderer>().material.SetTexture("_MainTex", jttexture);
            }
            if (cvtexture != null)
            {
                ClothesManager.Instance.coverallItem.transform.Find("mesh").GetComponent<MeshRenderer>().material.SetTexture("_MainTex", cvtexture);
            }

            if (ModLoader.IsReferencePresent("UniversalShoppingSystem") && DirtyСlothes.GetValue())
            {
                AssetBundle ab = LoadAssets.LoadBundle("Expanded_Clothes.Assets.exclothes.unity3d");
                psell = GameObject.Instantiate(ab.LoadAsset<GameObject>("powdersell.prefab"));
                ab.Unload(false);
            }

            GameObject.Find("JOBS").AddComponent<Futufon>();
            if (ClothesHands.GetValue() != false)
                GameObject.Find("PLAYER").AddComponent<NewHands>();
            if (DirtyСlothes.GetValue() != false)
                GameObject.Find("PLAYER").AddComponent<Dirty>();
            Load_Location();
            
            if (W1 || W2)
                GameObject.Find("YARD").AddComponent<HouseElec>();
        }
        
        private void Mod_OnNewGame()
        {
            NewGame = true;
        }

        public void Save()
        {
            var shop = psell.GetComponent<ItemShop>();
            if (shop == null || shop.BoughtItems == null) return;

            var grams = new List<int>(shop.BoughtItems.Count);

            for (int i = 0; i < shop.BoughtItems.Count; i++)
            {
                var go = shop.BoughtItems[i];
                if (go == null)
                {
                    grams.Add(0);
                    continue;
                }

                var p = go.GetComponent<Powder>();
                grams.Add(p != null ? p.gram : 0);
            }
            if (USS && DirtyСlothes.GetValue())
                SaveLoad.WriteValue(Instance, "pow_grams", grams);
        }


        public void Load()
        {
            var shop = psell.GetComponent<ItemShop>();
            if (shop == null || shop.BoughtItems == null) return;

            var grams = SaveLoad.ReadValueAsList<int>(Instance, "pow_grams");
            if (grams == null || grams.Count == 0) return;

            int count = Mathf.Min(shop.BoughtItems.Count, grams.Count);

            for (int i = 0; i < count; i++)
            {
                var go = shop.BoughtItems[i];
                if (go == null) continue;

                int g = grams[i];

                var p = go.GetComponent<Powder>();
                if (p != null) p.gram = g;

                go.SetActive(g > 0);
            }
        }

        private void Mod_PostLoad()
        {
            if (ModLoader.IsReferencePresent("UniversalShoppingSystem") && DirtyСlothes.GetValue())
            {
                USSAPI();
            }
            
            var USS_NEED = SaveLoad.ValueExists(Instance, "USS_NEED") && SaveLoad.ReadValue<bool>(Instance, "USS_NEED");

            if (USS_NEED == false && !USS)
            {
                string format;
                string title;
                title = "EXPANDED CLOTHES: DIRTINESS SYSTEM";
                format = "With the recent update, a new system has been added to the game\n\nFor it to work, you need <b><color=yellow>Universal Shopping System</color></b>\nIf you do not need this system, you can ignore the notification.\nit will appear only once";
                ModUI.ShowCustomMessage(format, title, new MsgBoxBtn[2]
                {
                ModUI.CreateMessageBoxBtn("OK"),
                ModUI.CreateMessageBoxBtn("GO TO NEXUSMODS", USS_NEXUS)
                }, new MsgBoxBtn[0]);
                SaveLoad.WriteValue(Instance, "USS_NEED", true);
            }

            if (ModLoader.IsModPresent("HowMuchIsLeft") && DirtyСlothes.GetValue())
            {
                HMILAPI();
            }    
        }
        private void USSAPI()
        {
            new StateDirty();
            psell.GetComponent<ItemShop>().LoadShop(Instance);
            if (NewGame != true)
                Load();
        }
        private void HMILAPI()
        {
            HowMuchIsLeftAPI.RegisterItem("washing powder(Clone)", (item) =>
            {
                int gram = item.GetComponent<Powder>().gram;
                int max = 400;
                HowMuchIsLeftAPI.GenerateText(gram, max, "gram", false);
            });
        }

        public void SaveAllWashers()
        {
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            for (int i = 0; i < allObjects.Length; i++)
            {
                var go = allObjects[i];
                if (!go.name.StartsWith("ExpandedClothes_Washer_")) continue;
                var washer = go.GetComponent<WashingMachine>();

                try
                {
                    washer.Save();
                }
                catch (System.Exception)
                {
                    return;
                }
            }
        }

        public void Mod_OnSave()
        {
            ClothesManager.Instance.Save();
            text.SetVisibility(false);
            button.SetVisibility(false);

            if (ModLoader.IsReferencePresent("UniversalShoppingSystem") && DirtyСlothes.GetValue())
            {
                USS_SAVE();
            }
        }

        private void USS_SAVE()
        {
            psell.GetComponent<ItemShop>().SaveShop(Instance);
            Save();
            SaveAllWashers();
        }

        private void ActivateItems()
        {
            ClothesManager.Instance.SetItemLocation("None", ClothesKind.Jacket);
            ClothesManager.Instance.SetItemLocation("None", ClothesKind.Coverall);

            ClothesManager.Instance.FindItems();

            GameObject jacket = ClothesManager.Instance.jacketItem;
            GameObject coverall = ClothesManager.Instance.coverallItem;

            Vector3 spawnPos = new Vector3(-1284.823f, 0.3f, 1080.405f);

            if (jacket != null)
            {
                jacket.transform.position = spawnPos + Vector3.right;
                jacket.SetActive(true);
                ClothesManager.Instance.EnablePhysicalItem(jacket);
            }

            if (coverall != null)
            {
                coverall.transform.position = spawnPos + Vector3.left;
                coverall.SetActive(true);
                ClothesManager.Instance.EnablePhysicalItem(coverall);
            }

            ModConsole.Print("<b><color=teal>[Expanded Clothes]</color></b>: Save cleared and items reset");
        }
    }

    /// <summary>
    /// Expanded Clothes API
    /// </summary>
    public static class API
    {
        public static void Register(string shelfID, GameObject rootObj, GameObject visualJacket, GameObject visualCoverall)
        {
            if (Expanded_Clothes.Instance.Shelfs.GetValue())
            {
                var shelf = rootObj.AddComponent<ClothesShelf>();
                shelf.Init(shelfID, visualJacket, visualCoverall);

                if (Expanded_Clothes.Logs.GetValue())
                    ModConsole.Print($"<b><color=teal>[Expanded Clothes API]</color></b>: {shelfID} registered");
            }
            else
            {
                if (Expanded_Clothes.Logs.GetValue())
                    ModConsole.Print($"<b><color=teal>[Expanded Clothes API]</color></b>: {shelfID} not registred, shelves disabled");
            }
        }
        /// <summary> 
        /// Create default shelf || ModID, pos(Vector3), ros(Vector3)
        /// </summary>
        public static void Shelf(string shelfID, Vector3 position, Vector3 rotation)
        {
            if (Expanded_Clothes.Instance.Shelfs.GetValue())
            {
                GameObject shelf = GameObject.Instantiate(Expanded_Clothes.Shelf);

                shelf.name = $"ExpandedClothes_{shelfID}";
                shelf.transform.position = position;
                shelf.transform.eulerAngles = rotation;

                Transform clothes = shelf.transform.GetChild(1);
                GameObject visualJacket = clothes.Find("jacket")?.gameObject;
                GameObject visualCoverall = clothes.Find("coverall")?.gameObject;

                var script = shelf.AddComponent<ClothesShelf>();
                script.Init(shelfID, visualJacket, visualCoverall);

                if (Expanded_Clothes.Logs.GetValue())
                    ModConsole.Print($"<b><color=teal>[Expanded Clothes API]</color></b>: {shelfID} created");
            }
            else
            {
                if (Expanded_Clothes.Logs.GetValue())
                    ModConsole.Print($"<b><color=teal>[Expanded Clothes API]</color></b>: {shelfID} not registred, shelves disabled");
            }

        }
        /// <summary> 
        /// Create coat rack || ModID, pos(Vector3), ros(Vector3), enable random hats
        /// </summary>
        public static void CoatRack(string shelfID, Vector3 position, Vector3 rotation, bool hats = false)
        {
            if (Expanded_Clothes.Instance.Racks.GetValue())
            {
                GameObject shelf = GameObject.Instantiate(Expanded_Clothes.CoatRack);

                shelfID = "rack_" + shelfID;

                shelf.name = $"ExpandedClothes_{shelfID}";
                shelf.transform.position = position;
                shelf.transform.eulerAngles = rotation;

                Transform clothes = shelf.transform.GetChild(1);
                GameObject visualJacket = clothes.Find("jacket")?.gameObject;
                GameObject visualCoverall = clothes.Find("coverall")?.gameObject;

                if (hats)
                {
                    Transform vhats = shelf.transform.GetChild(0);

                    for (int i = 0; i < 5; i++)
                        vhats.GetChild(i).gameObject.SetActive(false);

                    for (int i = 0; i < 3; i++)
                        vhats.GetChild(i).gameObject.SetActive(Random.value < 0.5f);

                    if (Random.value < 0.5f)
                        vhats.GetChild(3).gameObject.SetActive(true);
                    else
                        vhats.GetChild(4).gameObject.SetActive(true);
                }
                else
                {
                    shelf.transform.GetChild(0).gameObject.SetActive(false);
                }

                var script = shelf.AddComponent<ClothesShelf>();
                script.Init(shelfID, visualJacket, visualCoverall);

                var script2 = shelf.AddComponent<HatRack>();
                script2.Init(shelf);

                if (Expanded_Clothes.Logs.GetValue())
                    ModConsole.Print($"<b><color=teal>[Expanded Clothes API]</color></b>: {shelfID} created");
            }
            else
            {
                if (Expanded_Clothes.Logs.GetValue())
                    ModConsole.Print($"<b><color=teal>[Expanded Clothes API]</color></b>: {shelfID} not registred, racks disabled");
            }

        }
        /// <summary> 
        /// Create washer || ModID, pos(Vector3), ros(Vector3)
        /// </summary>
        public static void Washer(string washerID, Vector3 position, Vector3 rotation)
        {
            GameObject washer = GameObject.Instantiate(Expanded_Clothes.WasherPrefab);

            if (ModLoader.IsReferencePresent("UniversalShoppingSystem"))
            {
                if (Expanded_Clothes.Instance.DirtyСlothes.GetValue())
                {
                    washer.name = $"ExpandedClothes_Washer_{washerID}";
                    washer.transform.position = position;
                    washer.transform.eulerAngles = rotation;

                    if (Expanded_Clothes.Logs != null && Expanded_Clothes.Logs.GetValue())
                        ModConsole.Print($"<b><color=teal>[Expanded Clothes API]</color></b>: washer_{washerID} created");
                }
                else
                {
                    if (Expanded_Clothes.Logs.GetValue())
                        ModConsole.Print($"<b><color=teal>[Expanded Clothes API]</color></b>: {washerID} not registred, dirtness disabled");
                }

            }
            else
            {
                if (Expanded_Clothes.Logs.GetValue())
                    ModConsole.Print($"<b><color=teal>[Expanded Clothes API]</color></b>: ({washerID}) Washer not working, USS not installed - check settings");
            }
        }
    }
}