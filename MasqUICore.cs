using HarmonyLib;
using Il2CppDoozy.Engine.Extensions;
using Il2CppVampireSurvivors;
using Il2CppVampireSurvivors.Data;
using Il2CppVampireSurvivors.Framework;
using Il2CppVampireSurvivors.UI;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(MasqUI.MasqUICore), "MasqUI", "0.0.2", "Mercy", null)]
[assembly: MelonGame("poncle", "Vampire Survivors")]

namespace MasqUI
{
    public class MasqUICore : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
        }

        public override void OnDeinitializeMelon()
        {
            GameManagerPatches.Deinitialize();
        }

    }

    [HarmonyPatch(typeof(ButtonNavigator))]
    public static class ButtonNavigatorPatches
    {
        private static ColorBlock colors;
        private static Action<RectTransform> UIButtonSelected = (rTrans) =>
        {
            if (rTrans.gameObject != null && GM.Core?.PlayerOptions?.MainGameConfig != null)
            {
                GameObject go = rTrans.gameObject;
                if (go.active && go.TryGetComponent(out LevelUpItemUI ui) && go.TryGetComponent(out Button button))
                {
                    var index = (GM.Core.InteractingPlayer != null) ? GM.Core.InteractingPlayer._PlayerIndex : 0;
                    Color playerCol = GameManagerPatches.PlayerColors[index];
                    colors = button.colors; colors.selectedColor = playerCol;
                    button.colors = colors;
                }
            }
        };

        public static void Deinitialize()
        {
        }

        [HarmonyPatch(nameof(ButtonNavigator.Start))]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        public static void ButtonNavStart(ButtonNavigator __instance)
        {
            if (__instance.SelectionType != SelectableUI.SelectableType.BUTTON) return;
            SelectableUI.add_UIButtonSelected(UIButtonSelected);
        }
    }

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
        [HarmonyWrapSafe]
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
        [HarmonyWrapSafe]
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