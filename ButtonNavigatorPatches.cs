using HarmonyLib;
using Il2CppVampireSurvivors;
using Il2CppVampireSurvivors.Framework;
using Il2CppVampireSurvivors.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MasqUI.Patches
{
    /// <summary>
    /// Harmony patches for the ButtonNavigator class
    /// </summary>
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
        public static void ButtonNavStart(ButtonNavigator __instance)
        {
            if (__instance.SelectionType != SelectableUI.SelectableType.BUTTON) return;
            SelectableUI.add_UIButtonSelected(UIButtonSelected);
        }
    }
}