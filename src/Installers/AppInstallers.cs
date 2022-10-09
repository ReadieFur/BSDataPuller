using DataPuller.Data;
using Zenject;

namespace DataPuller.Installers
{
    internal class AppInstallers : MonoInstaller
    {
        public override void InstallBindings()
        {
            Plugin.Logger.Debug("InstallBindings.");
#if DEBUG
            Container.BindInterfacesAndSelfTo<Testing.TestClass>().AsSingle();
#endif
            //https://github.com/modesttree/Zenject/blob/master/Documentation/CheatSheet.md
            //I am not using Zenject for these classes because I couldn't be bothered to resolve
            //the issues I was having with it not being injected into the server classes.
            //Container.BindInterfacesAndSelfTo<MapData>().AsSingle();
            //Container.BindInterfacesAndSelfTo<LiveData>().AsSingle();
        }
    }
}
