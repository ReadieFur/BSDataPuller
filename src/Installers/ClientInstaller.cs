using Zenject;
using DataPuller.Core;

namespace DataPuller.Installers
{
    class ClientInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<MapEvents>().AsSingle();
        }
    }
}
