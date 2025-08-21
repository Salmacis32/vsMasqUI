using MasqUI.Patches;
using MelonLoader;

[assembly: MelonInfo(typeof(MasqUI.MasqUICore), "MasqUI", "1.0.0", "Mercy", null)]
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
            OptionsControllerPatches.Deinitialize();
            ButtonNavigatorPatches.Deinitialize();
        }
    }
}