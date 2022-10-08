using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace DataPuller.Installers
{
    internal class AppInstallers : MonoInstaller
    {
        public override void InstallBindings()
        {
#if DEBUG
            Container.BindInterfacesAndSelfTo<Testing.TestClass>().AsSingle();
#endif
        }
    }
}
