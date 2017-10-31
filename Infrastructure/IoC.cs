namespace Infrastructure
{
    using DCCClientLib.Interfaces;
    using DCCClientLib.Workers;
    using DCCDiscoveryService;
    using DCCDiscoveryService.Interfaces;
    using DCCNodeLib;
    using DCCNodeLib.Interfaces;
    using DCCNodeLib.Workers;
    using Ninject;

    public class IoC
    {
        private static readonly IKernel Kernel = new StandardKernel();

        public static void RegisterAll()
        {
            Kernel.Bind<IDiscoveryService>()
                .To<DiscoveryService>();

            Kernel.Bind<IDCCXmlClientWorker>()
                .To<DCCXmlClientWorker>();

            Kernel.Bind<IDCCJsonClientWorker>()
                .To<DCCJsonClientWorker>();

            Kernel.Bind<IDCCNodeWorker>()
                .To<DCCNodeWorker>();
        }

        public static T Resolve<T>() => Kernel.Get<T>();
    }
}