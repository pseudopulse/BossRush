using System;
using BepInEx.Configuration;

namespace BossRush.Tweaks {
    public abstract class TweakBase : GenericBase<TweakBase> {
        public abstract GameObject Body { get; }

        public override void Initialize(YAUContentPack pack, ConfigFile config, string identifier)
        {
            base.Initialize(pack, config, identifier);
            On.RoR2.CharacterBody.Start += OnBodyStart;
        }

        private void OnBodyStart(On.RoR2.CharacterBody.orig_Start orig, CharacterBody body) {
            orig(body);
            if (body.bodyIndex == BodyCatalog.FindBodyIndex(Body)) {
                ProcessTweaks(body, body.skillLocator);
            }
        }

        public virtual void ProcessTweaks(CharacterBody body, SkillLocator locator) {

        }
    }
}