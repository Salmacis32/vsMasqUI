using MelonLoader;

[assembly: MelonInfo(typeof(MasqUI.Core), "MasqUI", "1.0.0", "Mercy", null)]
[assembly: MelonGame("poncle", "Vampire Survivors")]

namespace MasqUI
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
        }
    }
}