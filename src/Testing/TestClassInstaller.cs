using Zenject;

namespace DataPuller.Testing
{
    internal class TestInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<TestClass>().AsSingle();
        }
    }
}
