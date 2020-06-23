using System;
using Microsoft.Extensions.DependencyInjection;
using TinyZipper.Application.Core.Interfaces;
using TinyZipper.Application.Core.StatusUpdaters;

namespace TinyZipper.Application
{
    class Program
    {
        static int Main(string[] args)
        {
            using (var serviceProvider = ServiceProvider.GetServiceProvider())
            {
                var statusUpdateService = serviceProvider.GetService<IStatusUpdateService>();

                try
                {
                    var service = serviceProvider.GetService<ICompressionOrchestrationService>();
                    return service.Do(args) ? 0 : 1;
                }
                catch (Exception exception)
                {
                    statusUpdateService.Error(exception.Message);
                    return 1;
                }
            }
        }
    }
}
