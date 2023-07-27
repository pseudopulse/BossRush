using System;
using RoR2.UI;

namespace BossRush.Gamemode {
    public class BossRushRun : Run {
        public override bool spawnWithPod => false;
        public override bool canFamilyEventTrigger => false;
        private float stopwatch = 0f;
        public WaveManager waveManager;
        public float bonusAmbientLevel;
        public override void OverrideRuleChoices(RuleChoiceMask mustInclude, RuleChoiceMask mustExclude, ulong runSeed)
        {
            base.OverrideRuleChoices(mustInclude, mustExclude, runSeed);

            if (true) {
                for (int i = 0; i < ArtifactCatalog.artifactCount; i++) {
                    ArtifactDef def = ArtifactCatalog.GetArtifactDef((ArtifactIndex)i);
                    RuleDef rd = RuleCatalog.FindRuleDef("Artifacts." + def.cachedName);
                    ForceChoice(mustInclude, mustExclude, rd.FindChoice("Off"));
                }
            }
        }

        public override void Start()
        {
            #pragma warning disable
            startingScenes = new SceneDef[] { Assets.SceneDef.itmoon };
            startingSceneGroup = null;
            #pragma warning enable
            base.Start();
            SetEventFlag("NoArtifactWorld");

            Stage.onStageStartGlobal += onStageBegin;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            waveManager.Unhook();
            Stage.onStageStartGlobal -= onStageBegin;
            ObjectivePanelController.collectObjectiveSources -= CollectObjectives;
        }

        public void End() {
            Run.instance.BeginGameOver(RoR2Content.GameEndings.PrismaticTrialEnding);
        }

        public void StartWaves() {
            waveManager.run = this;
            waveManager.Initialize();
            ObjectivePanelController.collectObjectiveSources += CollectObjectives;
        }

        public void CollectObjectives(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> objectiveSourcesList) {
            objectiveSourcesList.Add(new ObjectivePanelController.ObjectiveSourceDescriptor() {
                master = master,
                objectiveType = typeof(WaveManager.BossRushObjective),
                source = this
            });
        }

        public void InvokeWave() {
            waveManager.DoNextWave();
        }

        public void onStageBegin(Stage stage) {
            waveManager = new();
            waveManager.FixScene();
            Invoke(nameof(StartWaves), 5f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void AdvanceStage(SceneDef nextScene)
        {
            base.AdvanceStage(nextScene);
        }

        public override void RecalculateDifficultyCoefficentInternal()
        {
            float num = GetRunStopwatch();
            DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(selectedDifficulty);
            float num2 = Mathf.Floor(num * (1f / 60f));
            float num3 = (float)participatingPlayerCount * 0.3f;
            float num4 = 0.7f + num3;
            float num5 = 0.7f + num3;
            float num6 = Mathf.Pow(participatingPlayerCount, 0.2f);
            float num7 = 0.0506f * difficultyDef.scalingValue * num6;
            float num8 = 0.0506f * difficultyDef.scalingValue * num6;
            float num9 = Mathf.Pow(1.15f, stageClearCount);
            compensatedDifficultyCoefficient = (num5 + num8 * num2) * num9;
            difficultyCoefficient = (num4 + num7 * num2) * num9;
            float num10 = (num4 + num7 * (num * (1f / 60f))) * Mathf.Pow(1.15f, stageClearCount);
            ambientLevel = Mathf.Min((num10 - num4) / 0.33f + 1f, ambientLevelCap);
            ambientLevel += bonusAmbientLevel;
            int num11 = ambientLevelFloor;
            ambientLevelFloor = Mathf.FloorToInt(ambientLevel);
            if (num11 != ambientLevelFloor && num11 != 0 && ambientLevelFloor > num11)
            {
                OnAmbientLevelUp();
            }
        }
    }
}