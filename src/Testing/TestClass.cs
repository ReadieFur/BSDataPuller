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
            Plugin.Logger.Info("Initialize TestClass.");
        }

        public void Dispose()
        {
            Plugin.Logger.Info("Dispose TestClass.");
        }
    }
}
#endif
