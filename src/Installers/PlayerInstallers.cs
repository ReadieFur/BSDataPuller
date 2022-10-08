using Zenject;
using DataPuller.Core;

namespace DataPuller.Installers
{
    internal class PlayerInstallers : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<MapEvents>().AsSingle();
        }
    }
}
