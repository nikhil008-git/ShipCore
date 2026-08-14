using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShipCore.Reconciliation;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services => services.AddSingleton<InvoiceReconciler>())
    .Build();

host.Run();
