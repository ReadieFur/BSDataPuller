using IPA;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

namespace DataPuller
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static string Name => "DataPuller";

        [Init]
        public void Init(IPALogger logger)
        {
            Instance = this;
            Logger.Log = logger;
            Logger.Log.Debug("Logger initialized.");
        }

        #region BSIPA Config
        //Uncomment to use BSIPA's config
        /*
        [Init]
        public void InitWithConfig(Config conf)
        {
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Logger.log.Debug("Config loaded");
        }
        */
        #endregion

        [OnStart]
        public void OnApplicationStart()
        {
            Logger.Log.Debug("OnApplicationStart");
            new GameObject("DataPullerController").AddComponent<DataPullerController>();
            new Server().Init();
            new MapEvents().Init();
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Logger.Log.Debug("OnApplicationQuit");
        }
    }
}
