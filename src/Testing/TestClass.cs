#if DEBUG
using System;
using Zenject;

#nullable enable
namespace DataPuller.Testing
{
    class TestClass : IInitializable, IDisposable
    {
        public TestClass()
        {
        }

        public void Initialize()
        {
            Plugin.logger.Info("Initialize TestClass.cs");
        }

        public void Dispose()
        {
            Plugin.logger.Info("Dispose TestClass.cs");
        }
    }
}
#endif
