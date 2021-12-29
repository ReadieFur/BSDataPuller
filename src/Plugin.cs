using IPA;
using IPALogger = IPA.Logging.Logger;
using SiraUtil.Zenject;
using DataPuller.Installers;

namespace DataPuller
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Logger { get; private set; }

        internal Server.Server webSocketServer;

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
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            webSocketServer?.Dispose();
            Logger.Debug("OnApplicationQuit");
        }
    }
}
