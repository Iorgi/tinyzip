using Autofac;
using Autofac.Extensions.DependencyInjection;
using TinyZipper.Application.ClientOptions;
using TinyZipper.Application.Compressing;
using TinyZipper.Application.Core;
using TinyZipper.Application.Core.Interfaces;
using TinyZipper.Application.Readers;
using TinyZipper.Application.Settings;
using TinyZipper.Application.StatusUpdaters;
using TinyZipper.Application.Writers;

namespace TinyZipper.Application
{
    public class ServiceProvider
    {
        public static AutofacServiceProvider GetServiceProvider()
        {
            var containerBuilder = new ContainerBuilder();
            Configure(containerBuilder);
            var container = containerBuilder.Build();
            return new AutofacServiceProvider(container);
        }

        private static void Configure(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<ClientOptionsService>().As<IClientOptionsService>();
            containerBuilder.RegisterType<GzipCompressionService>().As<ICompressionService>();
            containerBuilder.RegisterType<ParallelCompressionOrchestrationService>().As<ICompressionOrchestrationService>();
            containerBuilder.RegisterType<DestinationStreamWriter>().As<IDestinationWriter>();
            containerBuilder.RegisterType<StreamToQueueReader>().As<ISourceReader>();
            containerBuilder.RegisterType<ParallelCompressionService>().As<IParallelCompressionService>();
            containerBuilder.RegisterType<ConsoleStatusUpdateService>().As<IStatusUpdateService>();
            containerBuilder.RegisterType<OutcomeService>().As<IOutcomeService>();
            containerBuilder.RegisterType<FileService>().As<IFileService>();
            containerBuilder.RegisterType<FileSourceService>().As<ISourceStreamService>();
            containerBuilder.RegisterType<StreamUtilsService>().As<IStreamUtilsService>();
            containerBuilder.RegisterType<FileDestinationService>().As<IDestinationStreamService>();
            containerBuilder.RegisterType<ThreadService>().As<IThreadService>();

            var settings = new DefaultSettings();
            containerBuilder.RegisterInstance(settings).As<IInputOverflowControlSettings>();
            containerBuilder.RegisterInstance(settings).As<IOutputOverflowControlSettings>();
            containerBuilder.RegisterInstance(settings).As<ICompressionSettings>();
        }
    }
}