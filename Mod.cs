using System.Reflection;
using Colossal.Logging;
using CS2ModsTesting.Systems;
using Game;
using Game.Modding;

namespace CS2ModsTesting
{
    public sealed class Mod : IMod
    {

        public static Mod Instance { get; private set; }

        internal ILog Log { get; private set; }

        public void OnLoad()
        {
            Instance = this;

            Log = LogManager.GetLogger("CS2ModsTesting");

#if DEBUG
            Log.effectivenessLevel = Level.Debug;
#endif
            Log.Info($"loading CS2ModsTesting version {Assembly.GetExecutingAssembly().GetName().Version}");
        }

        public void OnCreateWorld(UpdateSystem updateSystem)
        {
            Log.Info("Starting On Create World");

            updateSystem.UpdateAt<VehicleSnowMeltSystem>(SystemUpdatePhase.ModificationEnd);

        }

        public void OnDispose()
        {
            Log.Info("Disposing");
            Instance = null;
        }
    }
}

