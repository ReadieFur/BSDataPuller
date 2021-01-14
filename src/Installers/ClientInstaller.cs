using Zenject;
using DataPuller.Client;

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
