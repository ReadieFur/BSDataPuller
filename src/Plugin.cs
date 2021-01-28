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
            zenjector.OnApp<TestInstaller>();
            //zenjector.OnGame<TestInstaller>().Expose<ScoreController>();
#endif
            webSocketServer = new Server.Server();
            zenjector.OnGame<ClientInstaller>(false);
            zenjector.On<GameCoreSceneSetup>().Pseudo((_) => {}).Expose<ScoreUIController>();
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Logger.Debug("OnApplicationStart");
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            webSocketServer?.Dispose(); //Do I need to do this even though the application is closing?
            Logger.Debug("OnApplicationQuit");
        }
    }
}
