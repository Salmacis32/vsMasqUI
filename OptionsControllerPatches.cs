using HarmonyLib;
using Il2CppVampireSurvivors.Data;
using Il2CppVampireSurvivors.UI;

namespace MasqUI.Patches
{
    /// <summary>
    /// Harmony patches for the GameManager class
    /// </summary>
    [HarmonyPatch(typeof(OptionsController))]
    public static class OptionsControllerPatches
    {
        public static void Deinitialize()
        {
        }

        [HarmonyPatch(nameof(OptionsController.SetPlayerColourIndex))]
        [HarmonyPostfix]
        public static void SetColor(OptionsController __instance, int playerIndex)
        {
            if (!CheckConfigs(__instance, out PlayerOptionsData data)) return;
            GameManagerPatches.SetColorForPlayerIndex(playerIndex, data);
        }

        private static bool CheckConfigs(OptionsController __instance, out PlayerOptionsData data)
        {
            data = __instance._playerOptions.CurrentAdventureSaveData;
            if (data != null)
            {
                return true;
            }
            else if (__instance._playerOptions.MainGameConfig != null)
            {
                data = __instance._playerOptions.MainGameConfig;
                return true;
            }
            return false;
        }

        [HarmonyPatch(nameof(OptionsController.ToggleTintUISelection))]
        [HarmonyPostfix]
        public static void ToggleTint(OptionsController __instance)
        {
            if (!CheckConfigs(__instance, out PlayerOptionsData data)) return;
            GameManagerPatches.LoadPlayerColors(data);
        }
    }
}