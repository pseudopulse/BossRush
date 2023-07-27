using System;
using BepInEx.Configuration;
using EntityStates.VoidRaidCrab;
using EntityStates.VoidRaidCrab.Weapon;

namespace BossRush.Tweaks {
    public class Grandparent : TweakBase
    {
        public override GameObject Body => Assets.GameObject.GrandParentBody;

        public override void ProcessTweaks(CharacterBody body, SkillLocator locator)
        {
            base.ProcessTweaks(body, locator);
        }

        public override void Initialize(YAUContentPack pack, ConfigFile config, string identifier)
        {
            base.Initialize(pack, config, identifier);

            On.EntityStates.GrandParent.ChannelSun.CreateSun += OverrideSun;
            On.EntityStates.GrandParent.ChannelSunEnd.OnEnter += OnChannelExit;
        }

        public GameObject OverrideSun(On.EntityStates.GrandParent.ChannelSun.orig_CreateSun orig, EntityStates.GrandParent.ChannelSun self, Vector3 sunPos) {
            Transform holder = GameObject.Find("HOLDER: Stage").transform;
            Transform power = holder.Find("PowerCore");
            if (!power) {
                return orig(self, sunPos); // we arent in boss rush
            }
            VoidSunController controller = power.GetComponent<VoidSunController>();

            if (!power.GetComponent<GenericOwnership>()) {
                power.AddComponent<GenericOwnership>();
            }

            if (!controller) {
                controller = power.AddComponent<VoidSunController>();
            }

            controller.owner = self.characterBody;
            return power.gameObject;
        }

        public void OnChannelExit(On.EntityStates.GrandParent.ChannelSunEnd.orig_OnEnter orig, EntityStates.GrandParent.ChannelSunEnd self) {
            Transform holder = GameObject.Find("HOLDER: Stage").transform;
            Transform power = holder.Find("PowerCore");
            if (!power) {
                orig(self); 
                return; // we arent in boss rush
            }
            VoidSunController controller = power.GetComponent<VoidSunController>();

            controller.owner = null;

            orig(self);
        }

        public class VoidSunController : MonoBehaviour {
            public CharacterBody owner;
            public float stopwatch = 0f;
            public float delay = 0.03f;
            private float angleStopwatch;
            public GameObject beamVfxInstance;
            private float y = 0f;
            private float yDir = 20f;

            public void SpawnVFX() {
                beamVfxInstance = Instantiate(Assets.GameObject.VoidRaidCrabSpinBeamVFX);
                beamVfxInstance.transform.SetParent(base.transform);
                beamVfxInstance.transform.localScale += new Vector3(0.3f, 0.3f, 0.3f);
            }

            public void FixedUpdate() {
                stopwatch += Time.fixedDeltaTime;
                angleStopwatch += 90f * Time.fixedDeltaTime;
                if (angleStopwatch >= 360f) {
                    angleStopwatch = 0f;
                }

                if (owner && !owner.healthComponent.alive) {
                    owner = null;   
                }

                if (!owner) {
                    if (beamVfxInstance) {
                        VfxKillBehavior.KillVfxObject(beamVfxInstance);
                        beamVfxInstance = null;
                    }
                    return;
                }


                if (owner && !beamVfxInstance) {
                    SpawnVFX();
                }

                Vector3 forward = (owner.master.aiComponents[0].currentEnemy.gameObject.transform.position - base.transform.position).normalized;
                /*y += yDir * Time.fixedDeltaTime;
                if (y >= 360f) {
                    yDir = -20f;
                }
                else if (y <= 180f) {
                    yDir = 20f;
                }*/

                y = Mathf.Lerp(y, forward.y, 1.5f * Time.fixedDeltaTime);

                Vector3 rot = Quaternion.AngleAxis(angleStopwatch, base.transform.up) * forward;
                // rot = Quaternion.AngleAxis(y, base.transform.right) * rot;
                Vector3 aim = new(rot.x, y, rot.z);

                beamVfxInstance.transform.forward = aim;
                beamVfxInstance.transform.position = base.transform.position;

                if (stopwatch >= delay) {
                    stopwatch = 0f;
                    BulletAttack bulletAttack = new BulletAttack();
                    bulletAttack.origin = base.transform.position;
                    bulletAttack.aimVector = aim;
                    bulletAttack.minSpread = 0f;
                    bulletAttack.maxSpread = 0f;
                    bulletAttack.maxDistance = 400f;
                    bulletAttack.stopperMask = 0;
                    bulletAttack.radius = 4f;
                    bulletAttack.smartCollision = false;
                    bulletAttack.queryTriggerInteraction = QueryTriggerInteraction.Ignore;
                    bulletAttack.procCoefficient = 1f;
                    bulletAttack.procChainMask = default(ProcChainMask);
                    bulletAttack.owner = owner.gameObject;
                    bulletAttack.weapon = base.gameObject;
                    bulletAttack.damage = 6f * owner.damage * 0.03f;
                    bulletAttack.damageColorIndex = DamageColorIndex.Default;
                    bulletAttack.damageType = DamageType.Generic;
                    bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
                    bulletAttack.force = 0f;
                    bulletAttack.hitEffectPrefab = SpinBeamAttack.beamImpactEffectPrefab;
                    // bulletAttack.tracerEffectPrefab = Assets.GameObject.TracerBarrage;
                    bulletAttack.isCrit = false;
                    bulletAttack.HitEffectNormal = false;
                    
                    if (NetworkServer.active) {
                        bulletAttack.Fire();
                    }
                }
            }
        }
    }
}