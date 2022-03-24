using IPA;
using IPALogger = IPA.Logging.Logger;
using SiraUtil.Zenject;
using DataPuller.Installers;
using System;
using System.Reflection;

namespace DataPuller
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Logger { get; private set; }

        internal Server.Server webSocketServer;

        public const string HarmonyId = "com.DataPuller";
        internal static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(HarmonyId);

        [Init]
        public void Init(IPALogger _logger, Zenjector zenjector)
        {
            Instance = this;
            Logger = _logger;
            Logger.Debug("Logger initialized.");

#if DEBUG
            zenjector.Install<TestInstaller>(Location.App);
#endif
            webSocketServer = new Server.Server();
            zenjector.Install<ClientInstaller>(Location.Player);
            zenjector.Expose<ScoreUIController>("BSDP_ScoreUIController");
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Logger.Debug("OnApplicationStart");
            //ApplyHarmonyPatches();
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            webSocketServer?.Dispose();
            RemoveHarmonyPatches();

            Logger.Debug("OnApplicationQuit");
        }

        internal static void ApplyHarmonyPatches()
        {
            try
            {
                //Logger.Debug("Applying Harmony patches.");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                //Logger.Error("Error applying Harmony patches: " + ex.Message);
                Logger.Debug(ex);
            }
        }


        internal static void RemoveHarmonyPatches()
        {
            try
            {
                harmony.UnpatchSelf();
            }
            catch (Exception ex)
            {
                //Logger.Error("Error removing Harmony patches: " + ex.Message);
                Logger.Debug(ex);
            }
        }
    }
}
