using System;
using RoR2.UI;

namespace BossRush.Gamemode {
    public static class UI {
        public static void Initalize() {
            "BOSSRUSH_MENU_NAME".Add("Boss Rush");
            "BOSSRUSH_HOVERDESC".Add("Fight through a gauntlet of powered-up versions of every boss!.");

            SceneManager.activeSceneChanged += OnSceneChanged;
            On.RoR2.ConsoleFunctions.SubmitCmd += (orig, self, str) => {
                if (self.GetComponent<HGButton>() && self.GetComponent<HGButton>().hoverToken == "BOSSRUSH_HOVERDESC") {
                    if (str == "transition_command \"gamemode bossRushRun; host 0;\"") {
                        orig(self, str);
                    }
                }
                else {
                    orig(self, str);
                }
            };
        }

        private static void OnSceneChanged(Scene prev, Scene next) {
            if (next.name == "title") {
                GameObject gameObject = GameObject.Find("MainMenu");
                Transform transform = gameObject.transform.Find("MENU: Extra Game Mode/ExtraGameModeMenu/Main Panel/GenericMenuButtonPanel/JuicePanel/GenericMenuButton (Eclipse)");
                if (transform)
                {
                    GameObject button = UnityEngine.Object.Instantiate(transform.gameObject, transform.parent);
                    button.GetComponent<LanguageTextMeshController>().token = "BOSSRUSH_MENU_NAME";
                    ConsoleFunctions consoleFunctions = button.GetComponent<ConsoleFunctions>();
                    HGButton component = button.GetComponent<HGButton>();
                    component.hoverToken = "BOSSRUSH_HOVERDESC";
                    component.onClick.RemoveAllListeners();
                    component.onClick.AddListener(delegate
                    {
                        consoleFunctions.SubmitCmd("transition_command \"gamemode bossRushRun; host 0;\"");
                    });

                }
                else {
                    Debug.Log("Could not find transform.");
                }
            }
        }
    }
}