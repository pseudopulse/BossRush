using System;
using RoR2.UI;
using UnityEngine.UI;

namespace BossRush.Gamemode {
    public class StackableHealthBar : MonoBehaviour {
        public Image FillRect;
        public Image DelayFillRect;
        public HUD HUD;
        public WaveSpawn waveSpawn;
        public HGTextMeshProUGUI HP;
        public HGTextMeshProUGUI Name;
        public HGTextMeshProUGUI Subtitle;
        public CharacterBody target;
        private Run.TimeStamp nextAllowedSourceUpdateTime = Run.TimeStamp.negativeInfinity;
        private float delayedTotalHealthFraction;
	    private float healthFractionVelocity;

        public void LateUpdate() {
            if (!target) {
                GameObject.Destroy(base.gameObject);
            }

            float totalObservedHealth = target.healthComponent.combinedHealth;
			float totalMaxObservedMaxHealth = target.healthComponent.fullCombinedHealth;
			float num = ((totalMaxObservedMaxHealth == 0f) ? 0f : Mathf.Clamp01(totalObservedHealth / totalMaxObservedMaxHealth));
			delayedTotalHealthFraction = Mathf.Clamp(Mathf.SmoothDamp(delayedTotalHealthFraction, num, ref healthFractionVelocity, 0.1f, float.PositiveInfinity, Time.deltaTime), num, 1f);
			FillRect.fillAmount = num;
			DelayFillRect.fillAmount = delayedTotalHealthFraction;
			HP.SetText($"{(int)totalObservedHealth} / {(int)totalMaxObservedMaxHealth}");
			Name.SetText(Util.GetBestBodyName(target.gameObject));
            // Debug.Log("target name: " + target.GetDisplayName());
            Subtitle.SetText("<sprite name=\"CloudLeft\" tint=1> " + target.GetSubtitle() + " <sprite name=\"CloudRight\" tint=1>");
            // Debug.Log("target subtitle: " + target.GetSubtitle());
        }
    }
}