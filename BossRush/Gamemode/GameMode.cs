using RoR2.Networking;
using System;
using RoR2.EntityLogic;

namespace BossRush.Gamemode {
    public static class GameMode {
        public static GameObject bossRushPrefab;
        public static BuffDef BossRushSpeed;
        public static GameObject ProgressionSlab;
        public static GameObject VoidtouchWave;
        public static GameObject PerfectedWave;
        public static GameObject HotPooGames;
        public enum ShrineType {
            Waveskip,
            Perfected,
            Voidtouched
        }
        public static void Create() {
            bossRushPrefab = RuntimePrefabManager.CreatePrefab(new("bossRushRun"), "bossRushRun");
            GameObject classic = Assets.GameObject.ClassicRun;

            BossRushRun run = bossRushPrefab.AddComponent<BossRushRun>();
            run.lobbyBackgroundPrefab = classic.GetComponent<Run>().lobbyBackgroundPrefab;
            run.uiPrefab = classic.GetComponent<Run>().uiPrefab;
            run.userPickable = true;
            run.nameToken = "BOSSRUSH_MENU_NAME";
            run.gameOverPrefab = classic.GetComponent<Run>().gameOverPrefab;
            run.startingSceneGroup = classic.GetComponent<Run>().startingSceneGroup;
            

            bossRushPrefab.AddComponent<NetworkIdentity>();
            
            bossRushPrefab.AddComponent<DirectorCore>();

            bossRushPrefab.AddComponent<TeamManager>();
            bossRushPrefab.AddComponent<NetworkRuleBook>();
            bossRushPrefab.AddComponent<RunCameraManager>();

            RuntimePrefabManager.MakeNetworkPrefab(bossRushPrefab);

            BossRushSpeed = ScriptableObject.CreateInstance<BuffDef>();
            BossRushSpeed.canStack = true;
            BossRushSpeed.iconSprite = Assets.BuffDef.bdCloakSpeed.iconSprite;
            BossRushSpeed.name = "Blessing of the Void";
            BossRushSpeed.buffColor = Color.magenta;

            PerfectedWave = RuntimePrefabManager.CreatePrefab(Assets.GameObject.ShrineBoss, "PerfectedWave");
            VoidtouchWave = RuntimePrefabManager.CreatePrefab(Assets.GameObject.ShrineBoss, "VoidtouchWave");
            ProgressionSlab = RuntimePrefabManager.CreatePrefab(Assets.GameObject.LunarRecycler, "SkipWave");

            Debug.Log("PerfectedWave ID pre-reset: " + PerfectedWave.GetComponent<NetworkIdentity>().assetId);

            PerfectedWave.RemoveComponent<ShrineBossBehavior>();
            VoidtouchWave.RemoveComponent<ShrineBossBehavior>();

            PurchaseInteraction voidtouch = VoidtouchWave.GetComponent<PurchaseInteraction>();
            voidtouch.displayNameToken = "BOSSRUSH_VOID_SHRINE";
            voidtouch.costType = CostTypeIndex.None;
            voidtouch.contextToken = "BOSSRUSH_VOID_CONTEXT";

            BossRushShrineController v = VoidtouchWave.AddComponent<BossRushShrineController>();
            v.isVoidtouch = true;

            foreach (MeshRenderer renderer in VoidtouchWave.GetComponentsInChildren<MeshRenderer>()) {
                if (renderer.gameObject.name == "Symbol") {
                    renderer.material = Assets.Material.matDeepVoidPortalCenter;
                }
                else {
                    renderer.material = Assets.Material.matDeepVoidPortalOpaque;
                }
            }

            BossRushShrineController b = PerfectedWave.AddComponent<BossRushShrineController>();
            b.isPerfected = true;

            PurchaseInteraction perfected = PerfectedWave.GetComponent<PurchaseInteraction>();
            perfected.displayNameToken = "BOSSRUSH_PERFECTED_SHRINE";
            perfected.costType = CostTypeIndex.None;
            perfected.contextToken = "BOSSRUSH_PERFECTED_CONTEXT";

            foreach (MeshRenderer renderer in PerfectedWave.GetComponentsInChildren<MeshRenderer>()) {
                if (renderer.gameObject.name == "Symbol") {
                    renderer.material = Assets.Material.matLunarGolemChargeGlow;
                }
                else {
                    renderer.material = Assets.Material.matLunarGolem;
                }
            }

            if (!ProgressionSlab.GetComponent<PurchaseInteraction>()) {
                ProgressionSlab.AddComponent<PurchaseInteraction>(); // gotce removes this component
            }
            ProgressionSlab.GetComponent<PurchaseInteraction>().costType = CostTypeIndex.None;
            ProgressionSlab.GetComponent<PurchaseInteraction>().contextToken = "BOSSRUSH_WAVESKIP_CONTEXT";
            ProgressionSlab.GetComponent<PurchaseInteraction>().displayNameToken = "BOSSRUSH_WAVESKIP_NAME";

            BossRushShrineController d = ProgressionSlab.AddComponent<BossRushShrineController>();
            d.isWaveskip = true;


            "BOSSRUSH_PERFECTED_SHRINE".Add("Shrine of the Moon");
            "BOSSRUSH_PERFECTED_CONTEXT".Add("Take on the challenge of the Moon?");
            "BOSSRUSH_VOID_SHRINE".Add("Shrine of the Void");
            "BOSSRUSH_VOID_CONTEXT".Add("Take on the challenge of the Void?");
            "BOSSRUSH_WAVESKIP_NAME".Add("...");
            "BOSSRUSH_WAVESKIP_CONTEXT".Add("Advance the simulation...?");

            Debug.Log("PerfectedWave ID post-reset: " + PerfectedWave.GetComponent<NetworkIdentity>().assetId);

            On.RoR2.CharacterBody.RecalculateStats += (orig, self) => {
                orig(self);

                if (self.HasBuff(BossRushSpeed)) {
                    self.moveSpeed *= 1 + (0.1f * self.GetBuffCount(BossRushSpeed));
                }
            };

            BossRush.contentPack.RegisterGameObject(bossRushPrefab);
            BossRush.contentPack.RegisterScriptableObject(BossRushSpeed);
            UI.Initalize();
        }

        public static void SetupShrineWaveskip(GameObject ProgressionSlab) {
            if (!ProgressionSlab.GetComponent<PurchaseInteraction>()) {
                ProgressionSlab.AddComponent<PurchaseInteraction>(); // gotce removes this component
            }
            ProgressionSlab.GetComponent<PurchaseInteraction>().costType = CostTypeIndex.None;
            ProgressionSlab.GetComponent<PurchaseInteraction>().contextToken = "BOSSRUSH_WAVESKIP_CONTEXT";
            ProgressionSlab.GetComponent<PurchaseInteraction>().displayNameToken = "BOSSRUSH_WAVESKIP_NAME";

            BossRushShrineController v = ProgressionSlab.AddComponent<BossRushShrineController>();
            v.isWaveskip = true;
        }

        public class BossRushShrineController : MonoBehaviour {
            public bool isVoidtouch = false;
            public bool isPerfected = false;
            public bool isWaveskip = false;
            public PurchaseInteraction interaction;
            public void Start() {
                interaction = GetComponent<PurchaseInteraction>();
                GetComponent<PurchaseInteraction>().onPurchase.AddListener(OnInteract);
            }

            public void FixedUpdate() {
                WaveManager manager = WaveManager.instance;

                if (!manager.canInteractWithShrines) {
                    interaction.available = false;
                    return;
                }

                if (isPerfected && manager.isVoidtouchedWave) {
                    interaction.available = false;
                    return;
                }

                if (isVoidtouch && manager.isPerfectedWave) {
                    interaction.available = false;
                    return;
                }
            }

            public void OnInteract(Interactor interactor) {
                GetComponent<PurchaseInteraction>().Networkavailable = false;

                NetworkWriter writer = NetworkingHelper.CreateMessage(BossRush.InteractShrineMessage,
                    x => {
                        x.Write(isVoidtouch);
                        x.Write(isWaveskip);
                        x.Write(isPerfected);
                    }
                );

                NetworkingHelper.ServerSendToAll(writer);
            }

            [NetworkMessageHandler(client = true, msgType = BossRush.InteractShrineMessage)]
            public static void HandleNet(NetworkMessage netmsg) {
                if (!Run.instance || Run.instance is not BossRushRun) {
                    return;
                }
                bool wasVoid = netmsg.reader.ReadBoolean();
                bool wasSkip = netmsg.reader.ReadBoolean();
                bool wasPerf = netmsg.reader.ReadBoolean();

                HandleInteract(wasVoid, wasPerf, wasSkip);

                Debug.Log("Handling netmessage");
            }

            public static void HandleInteract(bool isVoidtouch, bool isPerfected, bool isWaveskip) {
                if (isVoidtouch) {
                    WaveManager.instance.isVoidtouchedWave = true;
                    Chat.AddMessage("<style=cIsVoid>The Void imbues its power in the wave to come...</style>");
                }

                if (isPerfected) {
                    WaveManager.instance.isPerfectedWave = true;
                    Chat.AddMessage("<style=cIsUtility>The Moon shines its blessing on the wave to come...</style>");
                }

                if (isWaveskip) {
                    WaveManager.instance.DoNextWave();
                    Chat.AddMessage("<style=cIsDamage>Advancing the simulation!</style>");
                }
            }
        }
    }
}