using HarmonyLib;
using UnityEngine;
using static TownOfHost.Translator;

namespace TownOfHost
{
    //From The Other Roles source
    //https://github.com/Eisbison/TheOtherRoles/blob/main/TheOtherRoles/Patches/CredentialsPatch.cs
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    class PingTrackerPatch
    {
        private static GameObject modStamp;
        static void Prefix(PingTracker __instance)
        {
            if (modStamp == null)
            {
                modStamp = new GameObject("ModStamp");
                var rend = modStamp.AddComponent<SpriteRenderer>();
                rend.color = new Color(1, 1, 1, 0.5f);
                modStamp.transform.parent = __instance.transform.parent;
                modStamp.transform.localScale *= 0.6f;
            }
            float offset = (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started) ? 0.75f : 0f;
            modStamp.transform.position = HudManager.Instance.MapButton.transform.position + Vector3.down * offset;
        }

        static void Postfix(PingTracker __instance)
        {
            __instance.text.alignment = TMPro.TextAlignmentOptions.TopRight;
            __instance.text.text += Main.credentialsText;
            if (Options.NoGameEnd.GetBool()) __instance.text.text += $"\r\n" + Helpers.ColorString(Color.red, GetString("NoGameEnd"));
            if (Options.IsStandardHAS) __instance.text.text += $"\r\n" + Helpers.ColorString(Color.yellow, GetString("StandardHAS"));
            if (Options.CurrentGameMode() == CustomGameMode.HideAndSeek) __instance.text.text += $"\r\n" + Helpers.ColorString(Color.red, GetString("HideAndSeek"));
            if (Options.EnableGM.GetBool()) __instance.text.text += $"\r\n" + Helpers.ColorString(Color.red, $"{GetString("GM")} Is On");
            if (Main.AmDebugger.Value) __instance.text.text += "\r\n" + Helpers.ColorString(Color.green, "debug ();");
            if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
                __instance.gameObject.GetComponent<AspectPosition>().DistanceFromEdge = PlayerControl.LocalPlayer.Data.IsDead ? new Vector3(2.0f, 0.0f, 0f) : new Vector3(1.2f, 0.0f, 0f);
            else
            {
                __instance.gameObject.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(2.7f, 0.0f, 0f);
                if (Options.IsStandardHAS && !CustomRoles.Sheriff.IsEnable() && !CustomRoles.SerialKiller.IsEnable() && CustomRoles.Egoist.IsEnable()) __instance.text.text += $"\r\n" + Helpers.ColorString(Color.red, GetString("Warning.EgoistCannotWin"));
            }
        }
        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        class VersionShowerPatch
        {
            private static TMPro.TextMeshPro ErrorText;
            static void Postfix(VersionShower __instance)
            {
                Main.credentialsText = $"\r\n<color={Main.modColor}>Town Of Host: The Other Roles</color>\r\n<color={Main.modColor}>v{Main.PluginVersion}b" + (Main.AmDebugger.Value ? Main.FullDevVersion : "") + $"</color>\r\nMod by: Discussions\r\nHelp From: Detective, 단풍잎";
                Main.versionText = $"\r\nTown Of Host: The Other Roles v{Main.PluginVersion}b" + (Main.AmDebugger.Value ? Main.FullDevVersion : "") + $"\r\nMod by: Discussions\r\nHelp From: Detective, 단풍잎";
                if (Main.NewYears)
                {
                    Main.credentialsText += "\r\nHAPPY NEW YEAR!";
                    Main.versionText += "\r\nHAPPY NEW YEAR!";
                }
                if (ThisAssembly.Git.Branch != "main")
                {
                    Main.credentialsText += $"\r\n<color={Main.modColor}>{ThisAssembly.Git.Branch}({ThisAssembly.Git.Commit})</color>";
                    Main.versionText += $"\r\n{ThisAssembly.Git.Branch}({ThisAssembly.Git.Commit})";
                }
                var credentials = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(__instance.text);
                credentials.text = Main.credentialsText;
                credentials.alignment = TMPro.TextAlignmentOptions.TopRight;
                credentials.transform.position = new Vector3(4.3f, __instance.transform.localPosition.y + 0.3f, 0);

                if (Main.hasArgumentException && !Main.ExceptionMessageIsShown)
                {
                    Main.ExceptionMessageIsShown = true;
                    ErrorText = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(__instance.text);
                    ErrorText.transform.position = new Vector3(0, 0.20f, 0);
                    ErrorText.alignment = TMPro.TextAlignmentOptions.Center;
                    ErrorText.text = $"エラー:Lang系DictionaryにKeyの重複が発生しています!\r\n{Main.ExceptionMessage}";
                    ErrorText.color = Color.red;
                }
            }
        }
        [HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
        class AwakePatch
        {
            public static void Prefix(ModManager __instance)
            {
                __instance.ShowModStamp();
                LateTask.Update(Time.deltaTime);
                CheckMurderPatch.Update();
            }
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        class LogoPatch
        {
            static void Postfix(PingTracker __instance)
            {
                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo != null)
                {
                    amongUsLogo.transform.localScale *= 0.4f;
                    amongUsLogo.transform.position += Vector3.up * 0.25f;
                }

                var tohLogo = new GameObject("tohtor-logo");
                tohLogo.transform.position = Vector3.up;
                tohLogo.transform.localScale *= 1.2f;
                var renderer = tohLogo.AddComponent<SpriteRenderer>();
                renderer.sprite = Helpers.LoadSpriteFromResources("TownOfHost.Resources.tohtor-logo.png", 300f);
            }
        }
    }
}