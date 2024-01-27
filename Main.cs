using HarmonyLib;
using Kitchen;
using KitchenAchievementTracker.Utils;
using KitchenMods;
using PreferenceSystem;
using System.Reflection;
using UnityEngine;

// Namespace should have "Kitchen" in the beginning
namespace KitchenAchievementTracker
{
    public class Main : IModInitializer
    {
        public const string MOD_GUID = $"IcedMilo.PlateUp.{MOD_NAME}";
        public const string MOD_NAME = "Achievement Tracker";
        public const string MOD_VERSION = "0.1.1";

        internal static readonly ViewType AchievementsTrackerViewType = (ViewType)HashUtils.GetID($"{MOD_GUID}:AchievementsTrackerView");

        internal static PreferenceSystemManager PrefManager;

        public Main()
        {
            Harmony harmony = new Harmony(MOD_GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void PostActivate(KitchenMods.Mod mod)
        {
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");

            PrefManager = new PreferenceSystemManager(MOD_GUID, MOD_NAME);
            PrefManager.
                AddButtonWithConfirm(
                    "Clear All Achievements",
                    "Are you sure you want to clear ALL achievements?",
                    delegate (GenericChoiceDecision decision)
                    {
                        switch (decision)
                        {
                            case GenericChoiceDecision.Accept:
                                SteamworksUtils.ClearAchievements();
                                break;
                            default:
                                break;
                        }
                    })
                .AddSpacer()
                .AddSpacer();

            PrefManager.RegisterMenu(PreferenceSystemManager.MenuType.PauseMenu);
        }

        public void PreInject()
        {
        }

        public void PostInject()
        {
        }

        #region Logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion
    }
}
