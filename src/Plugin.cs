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
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Logger { get; private set; }
        internal Server.Server webSocketServer;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal static readonly HarmonyLib.Harmony harmony = new($"com.readiefur.{PLUGIN_NAME}");

        [Init]
        public void Init(IPALogger logger, Zenjector zenjector)
        {
            Instance = this;
            Logger = logger;
            Logger.Debug("Logger initialized.");

            zenjector.Install<AppInstallers>(Location.App);
            zenjector.Install<PlayerInstallers>(Location.Player);
            zenjector.Expose<ScoreUIController>($"{PLUGIN_NAME}_{nameof(ScoreUIController)}");

            Logger.Debug("Apply Harmony patches");
            try { harmony.PatchAll(Assembly.GetExecutingAssembly()); }
            catch (Exception ex) { Logger.Debug(ex); }

            webSocketServer = new();
        }

        [OnStart]
        public void OnApplicationStart() => Logger.Debug("OnApplicationStart");

        [OnExit]
        public void OnApplicationQuit()
        {
            webSocketServer?.Dispose();

            Logger.Debug("Remove Harmony patches");
            try { harmony.UnpatchSelf(); }
            catch (Exception ex) { Logger.Debug(ex); }

            Logger.Debug("OnApplicationQuit");
        }
    }
}
