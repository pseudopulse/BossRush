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
    
    public class BossRush : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "pseudopulse";
        public const string PluginName = "BossRush";
        public const string PluginVersion = "1.0.0";

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

            Debug.Log("Direseeker? : " + IsDireseekerPresent);
            Debug.Log("GOTCE? : " + IsGotcePresent);
            Debug.Log("RAR? : " + IsRARPresent);
            Debug.Log("Forgor? : " + IsForgorPresent);
        }
    }
}