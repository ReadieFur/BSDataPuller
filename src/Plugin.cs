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

        [Init]
        public void Init(IPALogger _logger, Zenjector zenjector)
        {
            Instance = this;
            Logger = _logger;
            Logger.Debug("Logger initialized.");

            #if DEBUG
            //zenjector.OnGame<TestInstaller>().Expose<ScoreController>();
            #endif
            zenjector.OnApp<ServerInstaller>();
            zenjector.OnGame<ClientInstaller>(false);
            zenjector.On<GameCoreSceneSetup>().Pseudo((_) => {}).Expose<ScoreUIController>();
        }

        [OnStart]
        public void OnApplicationStart()
        { Logger.Debug("OnApplicationStart"); }

        [OnExit]
        public void OnApplicationQuit()
        { Logger.Debug("OnApplicationQuit"); }
    }
}
