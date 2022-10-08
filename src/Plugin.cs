using System;
using System.Reflection;
using IPA;
using IPALogger = IPA.Logging.Logger;
using SiraUtil.Zenject;
using DataPuller.Installers;

#nullable enable
namespace DataPuller
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public const string PLUGIN_NAME = "datapuller";

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal static Plugin instance { get; private set; }
        internal static IPALogger logger { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal Server.Server webSocketServer = new();
        internal static readonly HarmonyLib.Harmony harmony = new($"com.readiefur.{PLUGIN_NAME}");

        [Init]
        public void Init(IPALogger logger, Zenjector zenjector)
        {
            instance = this;
            Plugin.logger = logger;
            Plugin.logger.Debug("Logger initialized.");

#if DEBUG
            zenjector.Install<Testing.TestInstaller>(Location.App);
#endif
            zenjector.Install<ClientInstaller>(Location.Player);
            zenjector.Expose<ScoreUIController>($"{PLUGIN_NAME}_{nameof(ScoreUIController)}");
        }

        [OnStart]
        public void OnApplicationStart() => logger.Debug("OnApplicationStart");

        [OnExit]
        public void OnApplicationQuit()
        {
            webSocketServer?.Dispose();
            RemoveHarmonyPatches();

            logger.Debug("OnApplicationQuit");
        }

        internal static void ApplyHarmonyPatches()
        {
            try { harmony.PatchAll(Assembly.GetExecutingAssembly()); }
            catch (Exception ex) { logger.Debug(ex); }
        }

        internal static void RemoveHarmonyPatches()
        {
            try { harmony.UnpatchSelf(); }
            catch (Exception ex) { logger.Debug(ex); }
        }
    }
}
