using HutongGames.PlayMaker;
using MSCLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Expanded_Clothes
{
    public class WashingMachine : MonoBehaviour
    {
        public GameObject washerBody;
        public GameObject doorMesh;
        public Animation doorAnimation;
        public GameObject powderTrayMesh;
        public Animation powderAnimation;
        public GameObject startButton;
        public Animation drumAnimation;
        public GameObject pileClothes;
        public GameObject drumMesh;
        public GameObject powderIndicator;

        public AudioClip DRUM_START;
        public AudioClip DRUM_SLOW_SPEED;
        public AudioClip DRUM_MAIN_SPEED;
        public AudioClip DRUM_FAST_SPEED;
        public AudioClip DRUM_END;
        public AudioClip USE_SOUND;

        public bool HasElectricity = true;

#if !Mini
        private readonly List<ClothesKind> clothesStack = new(2);

        private Camera mainCam;
        private Transform itemPivot;
        private FsmString interact;
        private FsmBool guiuse;
        private PlayMakerFSM hand;

        private bool doorOpen;
        private bool powderOpen;
        private bool isRunning;
        private int powderLoaded;

        private Coroutine addClothesCoroutine;

        private Dirty dirty;

        private float PREP_SECONDS = 20f;
        private float WASH_SECONDS = 600f;
        private float FADE_SECONDS = 4f;

        private float SLOW_PHASE = 120f;
        private float MAIN_PHASE = 420f;
        private float FAST_PHASE = 60f;

        private float CLEAN_TICK = 2f;

        private AudioSource asStart;
        private AudioSource asSlow;
        private AudioSource asMain;
        private AudioSource asFast;
        private AudioSource asEnd;
        private AudioSource use_sound;

        private enum WashState
        {
            Off,
            Prep,
            Slow,
            Main,
            Fast,
            FadeOut
        }

        private WashState state = WashState.Off;

        private float stateStart;
        private float stateEnd;

        private float fadeStart;
        private float fadeEnd;

        private float vSlow0, vMain0, vFast0;
        private float vSlowT, vMainT, vFastT;

        private bool cleaningActive;
        private float cleanStartTime;
        private float cleanEndTime;
        private int cleanStartJ;
        private int cleanStartC;
        private bool cleanHasJacket;
        private bool cleanHasCoverall;
        private float nextCleanTick;

        private bool waitingDrumToStop;
        private string activeDrumClip;
        private Coroutine waitDrumCoroutine;

        public void Save()
        {
            name = gameObject.name;
            SaveLoad.WriteValue(Expanded_Clothes.Instance, $"EC_{name}_loaded", powderLoaded);
        }

        public void Start()
        {
            mainCam = FsmVariables.GlobalVariables.FindFsmGameObject("POV").Value.GetComponent<Camera>();
            guiuse = FsmVariables.GlobalVariables.FindFsmBool("GUIuse");
            interact = FsmVariables.GlobalVariables.FindFsmString("GUIinteraction");

            itemPivot = GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera/1Hand_Assemble/ItemPivot").transform;
            hand = GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera/1Hand_Assemble/Hand").GetComponent<PlayMakerFSM>();

            dirty = GameObject.Find("PLAYER").GetComponent<Dirty>();

            SetupAudio();

            if (ClothesManager.Instance.jacketLocation == gameObject.name)
            {
                clothesStack.Add(ClothesKind.Jacket);
                ClothesManager.Instance.DisablePhysicalItem(ClothesManager.Instance.jacketItem);
            }

            if (ClothesManager.Instance.coverallLocation == gameObject.name)
            {
                clothesStack.Add(ClothesKind.Coverall);
                ClothesManager.Instance.DisablePhysicalItem(ClothesManager.Instance.coverallItem);
            }

            UpdatePileClothes();

            name = gameObject.name;
            if (SaveLoad.ValueExists(Expanded_Clothes.Instance, $"EC_{name}_loaded"))
                powderLoaded = SaveLoad.ReadValue<int>(Expanded_Clothes.Instance, $"EC_{name}_loaded");
        }

        private void SetupAudio()
        {
            Transform snd = transform.Find("SOUND");

            void Configure3D(AudioSource a, bool loop, float vol)
            {
                a.playOnAwake = false;
                a.loop = loop;
                a.spatialBlend = 1f;
                a.volume = vol;
                a.dopplerLevel = 1f;
                a.rolloffMode = AudioRolloffMode.Custom;
                a.spread = 0;
                a.maxDistance = 5f;
            }

            asStart = snd.gameObject.AddComponent<AudioSource>();
            Configure3D(asStart, false, 1f);
            asStart.clip = DRUM_START;

            asEnd = snd.gameObject.AddComponent<AudioSource>();
            Configure3D(asEnd, false, 1f);
            asEnd.clip = DRUM_END;

            asSlow = snd.gameObject.AddComponent<AudioSource>();
            Configure3D(asSlow, true, 0f);
            asSlow.clip = DRUM_SLOW_SPEED;

            asMain = snd.gameObject.AddComponent<AudioSource>();
            Configure3D(asMain, true, 0f);
            asMain.clip = DRUM_MAIN_SPEED;

            asFast = snd.gameObject.AddComponent<AudioSource>();
            Configure3D(asFast, true, 0f);
            asFast.clip = DRUM_FAST_SPEED;

            use_sound = snd.gameObject.AddComponent<AudioSource>();
            Configure3D(use_sound, false, 0f);
            use_sound.clip = USE_SOUND;
        }

        private void Update()
        {
            if (isRunning && !HasElectricity)
                StopWash_NoPower();

            UpdatePileClothes();

            if (isRunning)
            {
                TickWash();
                TickCleaning();
            }

            Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (!Physics.Raycast(ray, out RaycastHit hit, 1f))
                return;

            GameObject hitObj = hit.collider.gameObject;

            if (hitObj == ClothesManager.Instance.jacketItem)
            {
                StateDirty.SetText(dirty.infoJacket);
                return;
            }
            if (hitObj == ClothesManager.Instance.coverallItem)
            {
                StateDirty.SetText(dirty.infoCoverall);
                return;
            }
            StateDirty.ClearText();

            if (hitObj.transform.IsChildOf(doorMesh.transform))
            {
                if (isRunning || waitingDrumToStop) return;

                guiuse.Value = true;
                interact.Value = doorOpen ? "CLOSE DOOR" : "OPEN DOOR";
                if (Input.GetMouseButtonDown(0))
                    ToggleDoor();
                return;
            }

            if (hitObj == powderTrayMesh)
            {
                if (isRunning || waitingDrumToStop) return;
                HandlePowderTrayInteraction();
                return;
            }

            if (hitObj == startButton)
            {
                guiuse.Value = true;

                if (isRunning)
                {
                    interact.Value = "STOP";
                    if (Input.GetMouseButtonDown(0))
                        StopWash();
                    return;
                }

                if (waitingDrumToStop)
                {
                    interact.Value = "STOPPING...";
                    return;
                }

                if (!HasElectricity)
                {
                    interact.Value = "NO POWER";
                    return;
                }

                if (powderOpen)
                {
                    interact.Value = "CLOSE POWDER TRAY";
                    return;
                }

                if (doorOpen)
                {
                    interact.Value = "CLOSE DOOR";
                    return;
                }

                interact.Value = "START WASH";
                if (Input.GetMouseButtonDown(0))
                    StartWash();

                return;
            }

            if (hitObj.transform.IsChildOf(drumMesh.transform))
            {
                if (isRunning || waitingDrumToStop) return;
                HandleClothesInteraction();
                return;
            }
        }

        private void SanitizeTimings()
        {
            if (PREP_SECONDS < 0f) PREP_SECONDS = 0f;
            if (WASH_SECONDS < 60f) WASH_SECONDS = 600f;
            if (FADE_SECONDS < 0.1f) FADE_SECONDS = 4f;

            if (SLOW_PHASE < 1f) SLOW_PHASE = 120f;
            if (MAIN_PHASE < 1f) MAIN_PHASE = 420f;
            if (FAST_PHASE < 1f) FAST_PHASE = 60f;

            if (CLEAN_TICK < 0.1f) CLEAN_TICK = 2f;
        }

        private void StartWash()
        {
            if (!HasElectricity) return;
            if (powderOpen || doorOpen) return;
            if (waitingDrumToStop) return;

            SanitizeTimings();

            isRunning = true;
            state = WashState.Prep;

            float now = Time.realtimeSinceStartup;
            stateStart = now;
            stateEnd = now + PREP_SECONDS;

            StartLoopsIfNeeded();
            SetAllLoopVolumes(0f);

            if (asStart.clip != null) asStart.Play();

            BeginCleaningIfNeeded(now);

            if (use_sound.clip != null)
            {
                use_sound.volume = 1f;
                use_sound.Play();
            }
        }

        private void StopWash_NoPower()
        {
            if (!isRunning) return;

            isRunning = false;
            state = WashState.Off;
            cleaningActive = false;

            StopAllSounds();
            BeginDrumStop();
        }

        private void StopWash()
        {
            if (!isRunning) return;

            isRunning = false;
            state = WashState.Off;
            cleaningActive = false;

            StopAllSounds();

            if (HasElectricity && use_sound.clip != null)
            {
                use_sound.volume = 1f;
                use_sound.Play();
            }

            BeginDrumStop();
        }

        private void BeginDrumStop()
        {
            waitingDrumToStop = true;

            activeDrumClip = GetCurrentDrumClip();

            if (!string.IsNullOrEmpty(activeDrumClip) && drumAnimation.GetClip(activeDrumClip) != null)
                drumAnimation[activeDrumClip].wrapMode = WrapMode.Once;

            if (waitDrumCoroutine != null)
                StopCoroutine(waitDrumCoroutine);

            waitDrumCoroutine = StartCoroutine(WaitDrumToFinish());
        }

        private IEnumerator WaitDrumToFinish()
        {
            if (string.IsNullOrEmpty(activeDrumClip) || drumAnimation.GetClip(activeDrumClip) == null)
            {
                drumAnimation.Stop();
                waitingDrumToStop = false;
                waitDrumCoroutine = null;
                yield break;
            }

            while (drumAnimation.IsPlaying(activeDrumClip))
                yield return null;

            waitingDrumToStop = false;
            waitDrumCoroutine = null;
        }

        private string GetCurrentDrumClip()
        {
            if (drumAnimation.IsPlaying("drum_fast")) return "drum_fast";
            if (drumAnimation.IsPlaying("drumanim")) return "drumanim";
            if (drumAnimation.IsPlaying("drum_slow")) return "drum_slow";
            return "drum_slow";
        }

        private void StopAllSounds()
        {
            asStart.Stop();
            asEnd.Stop();

            asSlow.Stop(); asSlow.volume = 0f;
            asMain.Stop(); asMain.volume = 0f;
            asFast.Stop(); asFast.volume = 0f;

            use_sound.Stop(); use_sound.volume = 0f;
        }

        private void FinishWash()
        {
            isRunning = false;
            state = WashState.Off;

            StopLoopSoundsOnly();

            if (asEnd.clip != null)
                asEnd.Play();

            if (powderLoaded > 0)
            {
                if (clothesStack.Contains(ClothesKind.Jacket)) dirty.JacketDirty = 0;
                if (clothesStack.Contains(ClothesKind.Coverall)) dirty.CoverallDirty = 0;
            }

            powderLoaded = 0;
            cleaningActive = false;

            use_sound.Stop();
            use_sound.volume = 0f;

            BeginDrumStop();
        }

        private void TickWash()
        {
            if (!HasElectricity)
            {
                StopWash_NoPower();
                return;
            }

            float now = Time.realtimeSinceStartup;

            if (state == WashState.Prep)
            {
                if (now >= stateEnd)
                {
                    drumAnimation.Play("drum_slow");
                    drumAnimation["drum_slow"].wrapMode = WrapMode.Loop;

                    float slow = Mathf.Max(1f, SLOW_PHASE);
                    float main = Mathf.Max(1f, MAIN_PHASE);
                    float fast = Mathf.Max(1f, FAST_PHASE);
                    float total = slow + main + fast;
                    float scale = WASH_SECONDS / total;

                    slow *= scale; main *= scale; fast *= scale;

                    state = WashState.Slow;
                    stateStart = now;
                    stateEnd = now + slow;

                    fadeStart = now;
                    fadeEnd = now + FADE_SECONDS;
                    SetFadeTarget(asSlow, 1f, asMain, 0f, asFast, 0f);
                }
                return;
            }

            if (state == WashState.Slow)
            {
                TickFade(now);

                if (now >= stateEnd)
                {
                    drumAnimation.Play("drumanim");
                    drumAnimation["drumanim"].wrapMode = WrapMode.Loop;

                    float slow = Mathf.Max(1f, SLOW_PHASE);
                    float main = Mathf.Max(1f, MAIN_PHASE);
                    float fast = Mathf.Max(1f, FAST_PHASE);
                    float total = slow + main + fast;
                    float scale = WASH_SECONDS / total;

                    main *= scale;

                    state = WashState.Main;
                    stateStart = now;
                    stateEnd = now + main;

                    fadeStart = now;
                    fadeEnd = now + FADE_SECONDS;
                    SetFadeTarget(asSlow, 0f, asMain, 1f, asFast, 0f);
                }
                return;
            }

            if (state == WashState.Main)
            {
                TickFade(now);

                if (now >= stateEnd)
                {
                    drumAnimation.Play("drum_fast");
                    drumAnimation["drum_fast"].wrapMode = WrapMode.Loop;

                    float slow = Mathf.Max(1f, SLOW_PHASE);
                    float main = Mathf.Max(1f, MAIN_PHASE);
                    float fast = Mathf.Max(1f, FAST_PHASE);
                    float total = slow + main + fast;
                    float scale = WASH_SECONDS / total;

                    fast *= scale;

                    state = WashState.Fast;
                    stateStart = now;
                    stateEnd = now + fast;

                    fadeStart = now;
                    fadeEnd = now + FADE_SECONDS;
                    SetFadeTarget(asSlow, 0f, asMain, 0f, asFast, 1f);
                }
                return;
            }

            if (state == WashState.Fast)
            {
                TickFade(now);

                if (now >= stateEnd)
                {
                    state = WashState.FadeOut;
                    fadeStart = now;
                    fadeEnd = now + FADE_SECONDS;

                    SetFadeTarget(asSlow, 0f, asMain, 0f, asFast, 0f);
                }
                return;
            }

            if (state == WashState.FadeOut)
            {
                TickFade(now);

                if (now >= fadeEnd)
                    FinishWash();

                return;
            }
        }

        private void StartLoopsIfNeeded()
        {
            if (asSlow.clip != null && !asSlow.isPlaying) asSlow.Play();
            if (asMain.clip != null && !asMain.isPlaying) asMain.Play();
            if (asFast.clip != null && !asFast.isPlaying) asFast.Play();
        }

        private void SetAllLoopVolumes(float v)
        {
            asSlow.volume = v;
            asMain.volume = v;
            asFast.volume = v;
        }

        private void StopLoopSoundsOnly()
        {
            asSlow.Stop(); asSlow.volume = 0f;
            asMain.Stop(); asMain.volume = 0f;
            asFast.Stop(); asFast.volume = 0f;
        }

        private void SetFadeTarget(AudioSource slow, float slowTarget, AudioSource main, float mainTarget, AudioSource fast, float fastTarget)
        {
            vSlow0 = slow.volume;
            vMain0 = main.volume;
            vFast0 = fast.volume;

            vSlowT = slowTarget;
            vMainT = mainTarget;
            vFastT = fastTarget;
        }

        private void TickFade(float now)
        {
            float dur = Mathf.Max(0.01f, fadeEnd - fadeStart);
            float k = Mathf.Clamp01((now - fadeStart) / dur);

            asSlow.volume = Mathf.Lerp(vSlow0, vSlowT, k);
            asMain.volume = Mathf.Lerp(vMain0, vMainT, k);
            asFast.volume = Mathf.Lerp(vFast0, vFastT, k);
        }

        private void BeginCleaningIfNeeded(float now)
        {
            cleaningActive = false;

            if (powderLoaded <= 0) return;
            if (clothesStack.Count <= 0) return;

            cleanHasJacket = clothesStack.Contains(ClothesKind.Jacket);
            cleanHasCoverall = clothesStack.Contains(ClothesKind.Coverall);

            cleanStartJ = Mathf.RoundToInt(Mathf.Clamp(dirty.JacketDirty, 0f, 100f));
            cleanStartC = Mathf.RoundToInt(Mathf.Clamp(dirty.CoverallDirty, 0f, 100f));

            cleanStartTime = now;
            cleanEndTime = now + (PREP_SECONDS + WASH_SECONDS);
            nextCleanTick = now;

            cleaningActive = true;
        }

        private void TickCleaning()
        {
            if (!cleaningActive) return;

            float now = Time.realtimeSinceStartup;
            if (now < nextCleanTick) return;

            float dur = Mathf.Max(1f, cleanEndTime - cleanStartTime);
            float p = Mathf.Clamp01((now - cleanStartTime) / dur);

            if (cleanHasJacket)
                dirty.JacketDirty = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(cleanStartJ, 0f, p)), 0, 100);

            if (cleanHasCoverall)
                dirty.CoverallDirty = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(cleanStartC, 0f, p)), 0, 100);

            if (p >= 1f)
                cleaningActive = false;

            nextCleanTick = now + CLEAN_TICK;
        }

        private void HandlePowderTrayInteraction()
        {
            guiuse.Value = true;

            if (powderOpen)
            {
                if (TryGetPowderInHand(out Powder powder))
                {
                    if (powderLoaded >= 100)
                    {
                        interact.Value = "TRAY FULL";
                        if (Input.GetMouseButtonDown(0)) TogglePowderTray();
                        return;
                    }

                    if (powder.gram <= 0)
                    {
                        interact.Value = "EMPTY PACKAGE";
                        return;
                    }

                    interact.Value = "POUR WASHING POWDER";
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        int transferAmount = Mathf.Min(100, powder.gram);
                        int canTake = Mathf.Min(100 - powderLoaded, transferAmount);

                        powder.gram = Mathf.Max(0, powder.gram - canTake);
                        powderLoaded = Mathf.Clamp(powderLoaded + canTake, 0, 100);
                    }
                    return;
                }

                interact.Value = (powderLoaded > 0) ? "POWDER LOADED" : "CLOSE POWDER TRAY";

                if (Input.GetMouseButtonDown(0)) TogglePowderTray();
                return;
            }

            interact.Value = "OPEN POWDER TRAY";
            if (Input.GetMouseButtonDown(0)) TogglePowderTray();
        }

        private void HandleClothesInteraction()
        {
            if (!doorOpen)
            {
                interact.Value = "OPEN DOOR";
                return;
            }

            if (TryGetClothesInHand(out ClothesKind kind, out _))
            {
                interact.Value = "PUTTING...";
                if (addClothesCoroutine == null)
                {
                    addClothesCoroutine = StartCoroutine(AddClothesDelayed(kind));
                    hand.Fsm.Event(FsmEvent.FindEvent("PROCEED Drop"));
                }
                return;
            }

            if (clothesStack.Count > 0 && itemPivot.childCount == 0)
            {
                guiuse.Value = true;
                interact.Value = "TAKE CLOTHES";
                if (Input.GetMouseButtonDown(0))
                    TakeLastClothes();
                return;
            }

            guiuse.Value = true;
        }

        private IEnumerator AddClothesDelayed(ClothesKind kind)
        {
            float start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - start < 0.1f) yield return null;

            if (!doorOpen || isRunning)
            {
                addClothesCoroutine = null;
                yield break;
            }

            if (clothesStack.Contains(kind))
            {
                addClothesCoroutine = null;
                yield break;
            }

            GameObject physicalItem = (kind == ClothesKind.Jacket)
                ? ClothesManager.Instance.jacketItem
                : ClothesManager.Instance.coverallItem;

            ClothesManager.Instance.DisablePhysicalItem(physicalItem);
            physicalItem.transform.SetParent(null);

            clothesStack.Add(kind);
            ClothesManager.Instance.SetItemLocation(gameObject.name, kind);

            UpdatePileClothes();
            addClothesCoroutine = null;
        }

        private void TakeLastClothes()
        {
            int lastIndex = clothesStack.Count - 1;
            if (lastIndex < 0) return;

            ClothesKind kind = clothesStack[lastIndex];
            clothesStack.RemoveAt(lastIndex);

            ClothesManager.Instance.SetItemLocation("None", kind);

            GameObject item = (kind == ClothesKind.Jacket)
                ? ClothesManager.Instance.jacketItem
                : ClothesManager.Instance.coverallItem;

            ClothesManager.Instance.EnablePhysicalItem(item);
            item.transform.position = mainCam.transform.position + mainCam.transform.forward * 0.3f;

            UpdatePileClothes();
        }

        private void ToggleDoor()
        {
            if (isRunning || waitingDrumToStop) return;

            doorOpen = !doorOpen;
            doorAnimation.Play(doorOpen ? "wash_open" : "wash_close");
        }

        private void TogglePowderTray()
        {
            if (isRunning || waitingDrumToStop) return;

            powderOpen = !powderOpen;
            powderAnimation.Play(powderOpen ? "powder_open" : "powder_close");
        }

        private void UpdatePileClothes()
        {
            powderIndicator.SetActive(powderLoaded > 0);

            int count = clothesStack.Count;
            if (count <= 0)
            {
                if (pileClothes.activeSelf) pileClothes.SetActive(false);
                return;
            }

            if (!pileClothes.activeSelf) pileClothes.SetActive(true);

            pileClothes.transform.localPosition = (count == 1)
                ? new Vector3(-0.002f, 0f, -0.000183f)
                : new Vector3(-0.001232f, 0f, -0.000183f);
        }

        private bool TryGetClothesInHand(out ClothesKind kind, out GameObject item)
        {
            kind = ClothesKind.None;
            item = null;

            if (itemPivot.childCount == 0)
                return false;

            GameObject handItem = itemPivot.GetChild(0).gameObject;

            if (handItem.name.Contains("winter jacket"))
                kind = ClothesKind.Jacket;
            else if (handItem.name.Contains("winter coverall"))
                kind = ClothesKind.Coverall;
            else
                return false;

            item = handItem;
            return true;
        }

        private bool TryGetPowderInHand(out Powder powder)
        {
            var handItem = itemPivot.GetChild(0).gameObject;
            powder = handItem.GetComponent<Powder>();
            return true;
        }
#endif
    }
}
