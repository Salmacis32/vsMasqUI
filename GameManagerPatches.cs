using HarmonyLib;
using Il2CppDoozy.Engine.Extensions;
using Il2CppVampireSurvivors.Data;
using Il2CppVampireSurvivors.Framework;
using UnityEngine;

namespace MasqUI.Patches
{
    /// <summary>
    /// Harmony patches for the GameManager class
    /// </summary>
    [HarmonyPatch(typeof(GameManager))]
    public static class GameManagerPatches
    {
        public static PlayerOptionsData CurrentGameConfig;
        public static Color[] PlayerColors;

        public static Color32 ConvertToColor32(uint aCol) =>
            new Color32(r: (byte)((aCol >> 16) & 0xFF), g: (byte)((aCol >> 8) & 0xFF), b: (byte)((aCol) & 0xFF), a: (byte)((aCol >> 24) & 0xFF));

        public static void Deinitialize()
        {
            PlayerColors = null;
            CurrentGameConfig = null;
        }

        /// <summary>
        /// Patch to load custom content into a game session
        /// </summary>
        /// <remarks>
        /// Currently I have to use this to properly grab a Projectile prefab, as I still need to figure out how to create one from scratch.
        /// </remarks>
        [HarmonyPatch(nameof(GameManager.InitializeGameSessionPostLoad))]
        [HarmonyPostfix]
        public static void Load(GameManager __instance)
        {
            if (CurrentGameConfig == null) CurrentGameConfig = (__instance.PlayerOptions.CurrentAdventureSaveData != null)
                    ? __instance.PlayerOptions.CurrentAdventureSaveData : __instance.PlayerOptions.MainGameConfig;
            LoadPlayerColors(CurrentGameConfig);
        }

        public static void LoadPlayerColors(PlayerOptionsData playerOptions)
        {
            PlayerColors = new Color[playerOptions.PlayerColours.Length];

            for (var i = 0; i < PlayerColors.Length; i++)
            {
                SetColorForPlayerIndex(i, playerOptions);
            }
        }

        /// <summary>
        /// Cleanup for custom content
        /// </summary>
        /// <remarks>
        /// There might be more places that should be hooked for this, but this seemed safe enough for the time being.
        /// </remarks>
        [HarmonyPatch(nameof(GameManager.ResetGameSession))]
        [HarmonyPrefix]
        public static void ResetGameSession(GameManager __instance)
        {
            ResetCustomContent();
        }

        public static void SetColorForPlayerIndex(int i, PlayerOptionsData data)
        {
            Color col = Color.white;
            if (data.TintUISelection) col = ConvertToColor32(data.PlayerColours[i]);
            PlayerColors[i] = col.WithAlpha(1.0f);
        }

        private static void ResetCustomContent()
        {
            Array.Clear(PlayerColors);
            CurrentGameConfig = null;
        }
    }
}