using System.Runtime.InteropServices;
using System;
using BossRush.Gamemode;
using UnityEngine.UI;
using RoR2.UI;
using RoR2.Networking;

#pragma warning disable // the obsolete errors on pickupindex are annoying

namespace BossRush {
    public class WaveManager {
        public List<Wave> waves;
        public int waveIndex = 0;
        public Wave currentWave;
        public PhaseCounter mithrixPhaseCounter;
        public BossRushRun run;
        public static GameObject StackingHealthBarPrefab;
        private bool isInWaveTransition = false;
        public bool isPerfectedWave;
        public bool isVoidtouchedWave;
        public bool wasVoidtouchedWave;
        public bool wasPerfectWave;
        public static WaveManager instance;
        public bool canInteractWithShrines = true;

        public WaveManager() {
            instance = this;
        }

        public void Initialize() {
            SetupWaves();

            mithrixPhaseCounter = Run.instance.gameObject.AddComponent<PhaseCounter>();
            mithrixPhaseCounter.phase = 1;

            On.RoR2.CharacterMaster.OnBodyDeath += OnDeath;
            On.RoR2.MusicController.PickCurrentTrack += PlayWaveTrack;

            if (!StackingHealthBarPrefab) {
                InitHealthBarPrefab();
            }

            isInWaveTransition = true;

            foreach (PlayerCharacterMasterController pcmc in PlayerCharacterMasterController._instancesReadOnly) {
                if (pcmc.master && pcmc.master.GetBody()) {
                    pcmc.master.GetBody().SetBuffCount(GameMode.BossRushSpeed.buffIndex, 10);
                }
            }
        }

        public void PlayWaveTrack(On.RoR2.MusicController.orig_PickCurrentTrack orig, MusicController self, ref MusicTrackDef def) {
            if (currentWave != null) {
                def = currentWave.Music;
            }
        }

        public void Unhook() {
            On.RoR2.CharacterMaster.OnBodyDeath -= OnDeath;
            On.RoR2.MusicController.PickCurrentTrack -= PlayWaveTrack;
        }

        public void FixScene() {
            Transform holder = GameObject.Find("HOLDER: Stage").transform;
            Transform arena = holder.Find("ArenaWalls");
            MeshCollider ceil = arena.Find("Ceiling").GetComponent<MeshCollider>();
            MeshCollider walls = arena.Find("CylinderWall").GetComponent<MeshCollider>();
            Transform power = holder.Find("PowerCore");
            ceil.enabled = true;
            walls.enabled = true;
            Transform powerRing = power.Find("ElementalRingVoidBlackHole");
            powerRing.localScale = new(5, 5, 5);
            power.transform.position = new(0, 140, 0);
            foreach (Transform child in power.transform) {
                child.gameObject.layer = LayerIndex.world.intVal;
            }

            // pillars
            Quaternion quat = Quaternion.Euler(270, 0, 0);
            GameObject.Instantiate(Assets.GameObject.MoonPillarHuge, new(100, 1, -1.3f), quat);
            GameObject.Instantiate(Assets.GameObject.MoonPillarHuge, new(-100, 1, -1.3f), quat);
            GameObject.Instantiate(Assets.GameObject.MoonPillarHuge, new(-1.3f, 1, 100), quat);
            GameObject.Instantiate(Assets.GameObject.MoonPillarHuge, new(-1.3f, 1, -100), quat);

            GameObject powerClone = GameObject.Instantiate(power.gameObject, new(100, 70, -1.3f), Quaternion.identity);
            powerClone.transform.localScale = new(0.2f, 0.2f, 0.2f);
            Transform powerRing2 = powerClone.transform.Find("ElementalRingVoidBlackHole");
            Transform powerRing3 = powerRing2.Find("Runes");
            Transform powerRing4 = powerRing2.Find("AreaIndicator");
            powerRing3.gameObject.SetActive(false);
            powerRing4.gameObject.SetActive(false);
            GameObject.Instantiate(powerClone, new(-100, 70, -1.3f), Quaternion.identity);
            GameObject.Instantiate(powerClone, new(-1.3f, 70, 100), Quaternion.identity);
            GameObject.Instantiate(powerClone, new(-1.3f, 70, -100), Quaternion.identity);
            
            if (NetworkServer.active) {
                GameObject slab1 = GameObject.Instantiate(GameMode.ProgressionSlab, new(68, 3, -71), Quaternion.Euler(270, 0, 0));
                GameObject slab2 = GameObject.Instantiate(GameMode.ProgressionSlab, new(-68, 3, 71), Quaternion.Euler(270, 180, 0));
                GameObject shrine1 = GameObject.Instantiate(GameMode.VoidtouchWave, new(-71, 3, -68), Quaternion.Euler(0, 0, 0));
                GameObject shrine2 = GameObject.Instantiate(GameMode.PerfectedWave, new(71, 3, 68), Quaternion.Euler(0, 180, 0));
                NetworkServer.Spawn(slab1);
                NetworkServer.Spawn(slab2);
                NetworkServer.Spawn(shrine1);
                NetworkServer.Spawn(shrine2);
            }
        }

        public static void InitHealthBarPrefab() {
            GameObject HUD = Assets.GameObject.HUDSimple;
            GameObject HealthBar = HUD.transform.Find("MainContainer").Find("MainUIArea").Find("SpringCanvas").Find("TopCenterCluster").Find("BossHealthBarRoot").Find("Container").gameObject;
            StackingHealthBarPrefab = RuntimePrefabManager.CreatePrefab(HealthBar, "StackableBossBar");

            GameObject container = StackingHealthBarPrefab.transform.Find("BossHealthBarContainer").Find("BackgroundPanel").gameObject;
            Image fill = container.transform.Find("FillPanel").GetComponent<Image>();
            Image delayFill = container.transform.Find("DelayFillPanel").GetComponent<Image>();
            HGTextMeshProUGUI hp = container.transform.Find("HealthText").GetComponent<HGTextMeshProUGUI>();
            GameObject root = StackingHealthBarPrefab;
            HGTextMeshProUGUI name = root.transform.Find("BossNameLabel").GetComponent<HGTextMeshProUGUI>();
            name.text = "this should not be here";
            name.RemoveComponent<LanguageTextMeshController>();
            HGTextMeshProUGUI sub = root.transform.Find("BossSubtitleLabel").GetComponent<HGTextMeshProUGUI>();
            sub.text = "this also shouldnt be here";

            StackableHealthBar bar = StackingHealthBarPrefab.AddComponent<StackableHealthBar>();
            bar.FillRect = fill;
            bar.HP = hp;
            bar.Name = name;
            bar.Subtitle = sub;
            bar.DelayFillRect = delayFill;
        }

        public class BossRushObjective : ObjectivePanelController.ObjectiveTracker {
            public WaveManager wave = null;
            public override string GenerateString()
            {
                if (wave == null) {
                    wave = (sourceDescriptor.source as BossRushRun).waveManager;
                }
                return $"Complete the <style=cIsDamage>Boss Rush</style> ({wave.waveIndex}/{wave.waves.Count()})";
            }

            public override bool IsDirty()
            {
                return true;
            }
        }

        public void OnDeath(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, CharacterMaster self, CharacterBody body) {
            orig(self, body);
            int count = currentWave.WaveSpawns.Count;
            int activeCount = GameObject.FindObjectsOfType<WaveMarker>().Where(x => x.body.healthComponent.alive).Count();

            if (isInWaveTransition) {
                return;
            }

            if (activeCount <= 0) {  
                foreach (PlayerCharacterMasterController pcmc in PlayerCharacterMasterController._instancesReadOnly) {
                    if (pcmc.master && pcmc.master.GetBody()) {
                        pcmc.master.GetBody().SetBuffCount(GameMode.BossRushSpeed.buffIndex, 10);
                    }
                }

                canInteractWithShrines = true;           
                if (waveIndex >= waves.Count()) {
                    mithrixPhaseCounter.phase++;
                    if (mithrixPhaseCounter.phase == 2) {
                        mithrixPhaseCounter.phase++;
                    }

                    if (mithrixPhaseCounter.phase > 3 && NetworkServer.active) {
                        run.Invoke(nameof(BossRushRun.End), 3f);
                        return;
                    }

                    foreach (WaveSpawn spawn in currentWave.WaveSpawns) {
                        if (NetworkServer.active) {
                            spawn.DoSpawn();
                        }
                    }

                    foreach (PlayerCharacterMasterController pcmc in PlayerCharacterMasterController._instancesReadOnly) {
                        if (pcmc.master && pcmc.master.GetBody()) {
                            pcmc.master.GetBody().SetBuffCount(GameMode.BossRushSpeed.buffIndex, currentWave.BuffStacks);
                        }
                    }
                }
                else {
                    foreach (PurchaseInteraction interaction in GameObject.FindObjectsOfType<PurchaseInteraction>()) {
                        interaction.available = true;
                    }
                    wasPerfectWave = isPerfectedWave;
                    wasVoidtouchedWave = isVoidtouchedWave;
                    isPerfectedWave = false;
                    isVoidtouchedWave = false;
        
                    if (isInWaveTransition) {
                        return;
                    }
                    currentWave.OnWaveCompleted();
                    isInWaveTransition = true;
                }
            }
        }

        public void SetupWaves() {
            waves = new();

            // wave 1 - solo titan
            waves.Add(
                new Wave(
                    new List<WaveSpawn>() {
                        new(Assets.GameObject.TitanMaster, 2f, 1.2f, false),
                    },
                    3,
                    2,
                    0,
                    0,
                    5,
                    4,
                    Assets.MusicTrackDef.muSong05
                )
            );

            // wave 2 - vagrant and queen
            waves.Add(
                new Wave(
                    new List<WaveSpawn>() {
                        new(Assets.GameObject.BeetleQueenMaster, 1f, 1f, false),
                        new(Assets.GameObject.VagrantMaster, 1f, 1f, true),
                    },
                    2,
                    1,
                    0,
                    0,
                    3,
                    4,
                    Assets.MusicTrackDef.muSong16
                )
            );

            // wave 3 - solo dunestrider
            waves.Add(
                new Wave(
                    new List<WaveSpawn>() {
                        new(Assets.GameObject.ClayBossMaster, 2f, 2f, false),
                    },
                    3,
                    2,
                    1,
                    0,
                    3,
                    2,
                    Assets.MusicTrackDef.muBossfightDLC112
                )
            );

            if (BossRush.IsRARPresent) {
                waves.Add(new Wave(
                    new List<WaveSpawn>() {
                        new(Assets.GameObject.MegaConstructMaster, 1f, 2f, true),
                        new(Assets.GameObject.MajorConstructMaster, 1f, 2f, false)
                    },
                    3,
                    2,
                    0,
                    0,
                    3,
                    2,
                    Assets.MusicTrackDef.muBossfightDLC110
                ));
            }

            // wave 4 - grove and imp
            waves.Add(
                new Wave(
                    new List<WaveSpawn>() {
                        new(Assets.GameObject.GravekeeperMaster, 1.5f, 1.4f, false),
                        new(Assets.GameObject.ImpBossMaster, 1.5f, 1.4f, false),
                    },
                    2,
                    1,
                    0,
                    0,
                    3,
                    3,
                    Assets.MusicTrackDef.muBossfightDLC110
                )
            );

            if (BossRush.IsDireseekerPresent) {
                waves.Add(
                    new Wave(
                        new List<WaveSpawn>() {
                            new(MasterCatalog.FindMasterPrefab("DireseekerBossMaster"), 2f, 2f, false),
                        },
                        3,
                        2,
                        1,
                        0,
                        3,
                        2,
                        Assets.MusicTrackDef.muBossfightDLC112
                    )
                );
            }

            // wave 5 - solo grandparent
            waves.Add(
                new Wave(
                    new List<WaveSpawn>() {
                        new(Assets.GameObject.GrandparentMaster, 3f, 3f, false) {
                            noRandomPos = true
                        },
                    },
                    3,
                    2,
                    1,
                    0,
                    2,
                    5,
                    Assets.MusicTrackDef.muSong22
                )
            );

            // wave 6 - worms
            waves.Add(
                new Wave(
                    new List<WaveSpawn>() {
                        new(Assets.GameObject.MagmaWormMaster, 1f, 3f, false),
                        new(Assets.GameObject.ElectricWormMaster, 1f, 1f, false),
                    },
                    2,
                    2,
                    0,
                    1,
                    2,
                    3,
                    Assets.MusicTrackDef.muSong23
                )
            );

            // wave 7 - solus trio
            waves.Add(
                new Wave(
                    new List<WaveSpawn>() {
                        new(Assets.GameObject.RoboBallBossMaster, 0.6f, 2f, true),
                        new(Assets.GameObject.SuperRoboBallBossMaster, 1.5f, 5f, true),
                        new(Assets.GameObject.RoboBallBossMaster, 0.6f, 2f, true),
                    },
                    2,
                    1,
                    0,
                    1,
                    3,
                    3,
                    Assets.MusicTrackDef.muSong05
                )
            );

            if (BossRush.IsForgorPresent) {
                waves.Add(
                    new Wave(
                        new List<WaveSpawn>() {
                            new(MasterCatalog.FindMasterPrefab("BrassMonolithMonsterMaster"), 3f, 3f, false) {
                                noRandomPos = true
                            },
                        },
                        3,
                        2,
                        0,
                        0,
                        3,
                        2,
                        Assets.MusicTrackDef.muSong22
                    )
                );
            }

            // wave 8 - aurelionite
            waves.Add(
                new Wave(
                    new List<WaveSpawn>() {
                        new(Assets.GameObject.TitanGoldMaster, 5f, 5f, false),
                    },
                    3,
                    2,
                    1,
                    0,
                    2,
                    3,
                    Assets.MusicTrackDef.muBossfightDLC110
                )
            );

            if (BossRush.IsGotcePresent) {
                waves.Add(
                    new Wave(
                        new List<WaveSpawn>() {
                            new(MasterCatalog.FindMasterPrefab("Crowdfunder WoolieMaster"), 8f, 10f, false) {},
                        },
                        3,
                        2,
                        1,
                        0,
                        3,
                        2,
                        Assets.MusicTrackDef.muBossfightDLC112
                    )
                );
            }

            // wave 9 - mithrix
            waves.Add(
                new Wave(
                    new List<WaveSpawn>() {
                        new(Assets.GameObject.BrotherMaster, 1f, 5f, false),
                    },
                    3,
                    2,
                    1,
                    0,
                    2,
                    4,
                    Assets.MusicTrackDef.muSong25
                )
            );
        }
        public void DoNextWave() {
            isInWaveTransition = false;
            currentWave = waves[waveIndex];
            canInteractWithShrines = false;
            foreach (WaveSpawn spawn in currentWave.WaveSpawns) {
                if (NetworkServer.active) {
                    spawn.DoSpawn();
                }
            }

            foreach (PlayerCharacterMasterController pcmc in PlayerCharacterMasterController._instancesReadOnly) {
                if (pcmc.master && pcmc.master.GetBody()) {
                    pcmc.master.GetBody().SetBuffCount(GameMode.BossRushSpeed.buffIndex, currentWave.BuffStacks);
                }
            }

            waveIndex++;

            GenericPickupController[] pickups = GameObject.FindObjectsOfType<GenericPickupController>().Where(x => EquipmentCatalog.GetEquipmentDef(x.pickupIndex.equipmentIndex)).ToArray();
            for (int i = 0; i < pickups.Length; i++) {
                EffectManager.SpawnEffect(Assets.GameObject.ExplosionLunarSun, new EffectData {
                    scale = 2f,
                    origin = pickups[i].transform.position
                }, false);
                GameObject.Destroy(pickups[i].gameObject);
            }

            if (currentWave.Music == Assets.MusicTrackDef.muSong25) {
                AkSoundEngine.SetState("bossPhase", "phase1");
            }
        }
    }

    public class WaveSpawn {
        public SpawnCard spawnCard;
        public GameObject MasterPrefab;
        public float Scale = 1f;
        public float Health = 1f;
        public Wave wave;
        public string customName = null;
        public bool noRandomPos = false;

        public WaveSpawn(GameObject master, float scale, float hp, bool air) {
            MasterPrefab = master;
            Scale = scale;
            Health = hp;

            spawnCard = ScriptableObject.CreateInstance<SpawnCard>();
            spawnCard.directorCreditCost = 0;
            spawnCard.prefab = MasterPrefab;
            spawnCard.nodeGraphType = air ? RoR2.Navigation.MapNodeGroup.GraphType.Air : RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            spawnCard.hullSize = HullClassification.BeetleQueen;
            spawnCard.sendOverNetwork = true;
        }

        public void DoSpawn() {
            if (!NetworkServer.active) {
                return;
            }
            DirectorPlacementRule rule = new();
            rule.maxDistance = 10;
            rule.minDistance = 0;
            rule.placementMode = DirectorPlacementRule.PlacementMode.NearestNode;
            rule.position = new Vector3(-1.6f, 5.2f, 0.3f) + (Random.onUnitSphere * 20f);

            if (noRandomPos) {
                rule.position = new Vector3(-1.6f, 5.2f, 0.3f);
            }

            DirectorSpawnRequest req = new(spawnCard, rule, Run.instance.runRNG);
            req.ignoreTeamMemberLimit = true;
            req.teamIndexOverride = TeamIndex.Monster;
            req.onSpawnedServer = (res) => {        
                if (res.spawnedInstance) {
                    CharacterMaster master = res.spawnedInstance.GetComponent<CharacterMaster>();
                    master.SpawnBodyHere();

                    Health *= Run.instance.livingPlayerCount;
                    if (Run.instance.livingPlayerCount > 1) {
                        Health += Run.instance.livingPlayerCount * 0.5f;
                    }
                    int itemCount = Mathf.RoundToInt((Health - 1f) * 10);
                    master.inventory.GiveItem(RoR2Content.Items.BoostHp, itemCount);
                    master.inventory.GiveItem(RoR2Content.Items.UseAmbientLevel);

                    CharacterBody body = master.GetBody();
                    // body.gameObject.transform.localScale *= Scale;

                    if (body.baseMoveSpeed == 9f) {
                        body.baseMoveSpeed = 12f; // hacky fix for dunestriders paralyzing with r2api, also makes them chase better
                    }

                    if (WaveManager.instance.isPerfectedWave) {
                        master.inventory.GiveItem(RoR2Content.Items.BoostHp, 5);
                        master.inventory.SetEquipmentIndex(RoR2Content.Equipment.AffixLunar.equipmentIndex);
                    }
                    else if (WaveManager.instance.isVoidtouchedWave) {
                        master.inventory.GiveItem(RoR2Content.Items.BoostHp, 5);
                        master.inventory.SetEquipmentIndex(DLC1Content.Equipment.EliteVoidEquipment.equipmentIndex);
                    }

                    NetworkWriter writer = NetworkingHelper.CreateMessage(
                        BossRush.SpawnMarkerMessage,
                        x => {
                            x.Write(body.gameObject);
                            x.Write(Scale);
                        }
                    );

                    NetworkingHelper.ServerSendToAll(writer);

                    /*GameObject light = new("Light");
                    light.transform.position = new Vector3(-1.6f, 20f, 0.3f);
                    Light l = light.AddComponent<Light>();
                    l.color = Color.white;
                    l.intensity = 20;
                    l.range = 50;
                    l.type = LightType.Point;
                    light.transform.SetParent(body.transform);*/
                }
            };

            // Debug.Log(DirectorCore.instance == null ? "null" : "not null");

            DirectorCore.instance.TrySpawnObject(req);
        }
        
        [NetworkMessageHandler(client = true, msgType = BossRush.SpawnMarkerMessage)]
        public static void HandleWaveMarker(NetworkMessage netmsg) {
            Debug.Log("handling netmessage for wave marker");
            if (Run.instance && Run.instance is BossRushRun) {
                GameObject obj = netmsg.reader.ReadGameObject();

                if (obj) {
                    WaveMarker marker = obj.AddComponent<WaveMarker>();
                    marker.scale = netmsg.reader.ReadSingle();
                }
            }
            else {
                Debug.Log("not in a boss rush run");
            }
        }
    }

    public class Wave {
        public List<WaveSpawn> WaveSpawns;
        public int GreenRewards;
        public int BossRewards;
        public int WhiteRewards;
        public int RedRewards;
        public int LevelRewards;
        public int BuffStacks;
        public MusicTrackDef Music;

        public Wave(List<WaveSpawn> spawns, int whites, int greens, int reds, int yellows, int levels, int buffStacks, MusicTrackDef music) {
            WaveSpawns = spawns;
            foreach (WaveSpawn spawn in spawns) {
                spawn.wave = this;
            }

            WhiteRewards = whites;
            GreenRewards = greens;
            RedRewards = reds;
            BossRewards = yellows;
            LevelRewards = levels;
            BuffStacks = buffStacks;
            Music = music;
        }

        public void OnWaveCompleted() {
            List<PlayerCharacterMasterController> pcmcs = PlayerCharacterMasterController._instancesReadOnly.ToList();

            TeamManager.instance.SetTeamLevel(TeamIndex.Player, TeamManager.instance.GetTeamLevel(TeamIndex.Player) + (uint)LevelRewards);
            WaveManager.instance.run.bonusAmbientLevel += (5 * (DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty).scalingValue / 2f));
            
            if (!NetworkServer.active) {
                return;
            }

            foreach (PlayerCharacterMasterController pcmc in PlayerCharacterMasterController._instancesReadOnly) {
                if (pcmc.master.IsDeadAndOutOfLivesServer() && pcmc.isConnected) {
                    pcmc.master.Respawn(pcmc.master.deathFootPosition, Quaternion.identity);
                }
            }

            bool isLunar = WaveManager.instance.wasPerfectWave;
            bool isVoidtouch = WaveManager.instance.wasVoidtouchedWave;

            foreach (PlayerCharacterMasterController pcmc in pcmcs) {
                CharacterMaster master = pcmc.master;

                if (isLunar) {
                    SpawnPickups(master, Run.instance.availableLunarItemDropList, WhiteRewards, ItemTier.Lunar, false);
                }
                else if (isVoidtouch) {
                    SpawnPickups(master, Run.instance.availableVoidTier1DropList, WhiteRewards, ItemTier.VoidTier1, false);
                }
                else {
                    SpawnPickups(master, Run.instance.availableTier1DropList, WhiteRewards, ItemTier.Tier1);
                }

                if (isVoidtouch) {
                    SpawnPickups(master, Run.instance.availableVoidTier2DropList, GreenRewards, ItemTier.VoidTier2, false);
                }
                else {
                    SpawnPickups(master, Run.instance.availableTier2DropList, GreenRewards, ItemTier.Tier2);
                }

                SpawnPickups(master, Run.instance.availableTier3DropList, RedRewards, ItemTier.Tier3, false);
                SpawnPickups(master, Run.instance.availableBossDropList, BossRewards, ItemTier.Boss, false);
                SpawnRandomEquip(master, isLunar ? Run.instance.availableLunarEquipmentDropList : Run.instance.availableEquipmentDropList);
            }

            void SpawnRandomEquip(CharacterMaster master, List<PickupIndex> pickups) {
                if (!master.GetBody()) {
                    return;
                }

                PickupDropletController.CreatePickupDroplet(pickups.GetRandom(Run.instance.runRNG), master.GetBody().corePosition, Vector3.up * 40);
            }


            void SpawnPickups(CharacterMaster master, List<PickupIndex> pickups, int amount, ItemTier tier, bool sort = true) {
                if (!master.GetBody()) {
                    return;
                }
                float angle = 360f / amount;
                Vector3 vector = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
                Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
                for (int i = 0; i < amount; i++) {
                    SpawnPotential(master.GetBody().transform.position, vector, tier, pickups, sort);
                    vector = quaternion * vector;
                }
            }

            void SpawnPotential(Vector3 pos, Vector3 vel, ItemTier tier, List<PickupIndex> drops, bool sort) {
                GenericPickupController.CreatePickupInfo info = new();
                info.position = pos;
                info.prefabOverride = Assets.GameObject.OptionPickup;
                info.rotation = Quaternion.identity;
                info.pickupIndex = PickupCatalog.FindPickupIndex(tier);

                info.pickerOptions = new PickupPickerController.Option[3];
                
                info.pickerOptions[0] = new() {
                    pickupIndex = GetDropFrom(drops, sort ? ItemTag.Damage : ItemTag.Any, new()),
                    available = true
                };

                info.pickerOptions[1] = new() {
                    pickupIndex = GetDropFrom(drops, sort ? ItemTag.Utility : ItemTag.Any, new() { info.pickerOptions[0].pickupIndex }),
                    available = true
                };

                info.pickerOptions[2] = new() {
                    pickupIndex = GetDropFrom(drops, sort ? ItemTag.Healing : ItemTag.Any, new() { info.pickerOptions[0].pickupIndex, info.pickerOptions[1].pickupIndex }),
                    available = true
                };

                PickupDropletController.CreatePickupDroplet(info, pos, vel);
            }

            PickupIndex GetDropFrom(List<PickupIndex> drops, ItemTag reqtag, List<PickupIndex> exclude) {
                return drops.Where(x => (ItemCatalog.GetItemDef(x.itemIndex).ContainsTag(reqtag) || reqtag == ItemTag.Any)
                    && ItemCatalog.GetItemDef(x.itemIndex).DoesNotContainTag(ItemTag.OnKillEffect)
                    && ItemCatalog.GetItemDef(x.itemIndex).DoesNotContainTag(ItemTag.InteractableRelated)
                    && ItemCatalog.GetItemDef(x.itemIndex).DoesNotContainTag(ItemTag.HoldoutZoneRelated)
                    && ItemCatalog.GetItemDef(x.itemIndex).DoesNotContainTag(ItemTag.Scrap)
                    && ItemCatalog.GetItemDef(x.itemIndex).DoesNotContainTag(ItemTag.PriorityScrap)
                    && !exclude.Contains(x)
                ).GetRandom(Run.instance.runRNG);
            }
        }
    }

    public class WaveMarker : MonoBehaviour {
        public Wave wave;
        public float scale;
        public CharacterBody body;

        public void Start() {
            body = GetComponent<CharacterBody>();
            Transform container = GameObject.Find("HUDSimple(Clone)").transform.Find("MainContainer").Find("MainUIArea").Find("SpringCanvas").Find("TopCenterCluster").Find("BossHealthBarRoot");
            GameObject barObj = GameObject.Instantiate(WaveManager.StackingHealthBarPrefab, container);
            StackableHealthBar bar = barObj.GetComponent<StackableHealthBar>();
            bar.target = body;

            HandleScale();
        }

        public void HandleScale() {
            body.modelLocator.modelTransform.localScale *= scale;

            /*KinematicCharacterController.KinematicCharacterMotor motor = body.GetComponent<KinematicCharacterController.KinematicCharacterMotor>();
            CapsuleCollider col = body.GetComponent<CapsuleCollider>();

            if (motor) {
                motor.CapsuleHeight *= scale;
                motor.CapsuleRadius *= scale;
                motor.CapsuleYOffset *= scale;
            }*/
        }
    }
}