using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Reflection;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace BossRush {
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("pseudopulse.YAU")]
    [BepInDependency("com.rob.RegigigasMod", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.rob.Tyranitar", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.TeamMoonstorm.Starstorm2", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.rob.Direseeker", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Viliger.EnemiesReturns", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.funkfrog_sipondo.sharesuite", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.TeamMoonstorm.Starstorm2", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("Wolfo.SimulacrumAdditions", BepInDependency.DependencyFlags.SoftDependency)]
    
    public class BossRush : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "pseudopulse";
        public const string PluginName = "BossRush";
        public const string PluginVersion = "1.3.0";

        public static BepInEx.Logging.ManualLogSource ModLogger;
        public static YAUContentPack contentPack;

        public const short SpawnMarkerMessage = 120;
        public const short InteractShrineMessage = 121;
        public const short EndlessModeMessage = 122;
        public const short InitCustomShrineMessage = 123;

        public static bool IsEndlessModeEnabled;
        public static bool RandomModeEnabled;

        public static bool IsGotcePresent;
        public static bool IsRARPresent;
        public static bool IsForgorPresent;
        public static bool IsDireseekerPresent;
        public static bool IsBasedInstalled;
        public static bool IsSharesuiteInstalled;
        public static bool IsStarstormInstalled;
        public static bool IsRegigigasInstalled;
        public static bool IsTyranitarInstalled;
        public static bool IsEnemiesReturnsInstalled;
        public static bool IsSimulAdditionsInstalled;

        public void Awake() {
            // set logger
            ModLogger = Logger;

            contentPack = ContentPackManager.CreateContentPack(Assembly.GetExecutingAssembly(), "BossRush");

            Gamemode.GameMode.Create();

            ContentScanner.ScanTypes<Tweaks.TweakBase>(Assembly.GetExecutingAssembly(), x => x.Initialize(contentPack, Config, "BossRush"));

            // IsEndlessModeEnabled = Config.Bind<bool>("Customization", "Endless Mode", false, "Waves are endless and scale infinitely.").Value;

            IsGotcePresent = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.TheBestAssociatedLargelyLudicrousSillyheadGroup.GOTCE");
            IsForgorPresent = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("PlasmaCore.ForgottenRelics");
            IsRARPresent = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("prodzpod.RecoveredAndReformed");
            IsDireseekerPresent = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rob.Direseeker");
            IsBasedInstalled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("BALLS.WellRoundedBalance");
            IsSharesuiteInstalled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.funkfrog_sipondo.sharesuite");
            IsStarstormInstalled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.TeamMoonstorm.Starstorm2");
            IsRegigigasInstalled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rob.RegigigasMod");
            IsTyranitarInstalled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rob.Tyranitar");
            IsEnemiesReturnsInstalled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Viliger.EnemiesReturns");
            IsSimulAdditionsInstalled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("Wolfo.SimulacrumAdditions");
        }
    }
}