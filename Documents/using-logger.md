# Logger

The ``Logger`` in this library exists for dependency injection.

There is no ``LoggerFactory`` or ``LogManager`` to create a ``Logger``.

The ``Logger`` in this library consumes an ``ILoggingService``, and there is currently only the ``TraceLogger`` with that interface.

```
log chain top down

-> ILogger                      (Logging.Logger.Default)
-> ILoggingService              (Logging.Logger)
-> Trace                        (System.Diagnostics)
-> Trace.Listeners              (System.Diagnostics)
-> AdapterTraceListener         (Logging.Tracing)
-> IAdapter                     (Logging.Tracing)
-> Console, File, Syslog        (Logging.Tracing.Adapters)
```

You can use the ``Trace.TraceInformation`` as is.

If u have a small application u can also use the ``ILoggingSerice`` from ``TraceLogger`` as is.

If u wants dependency injection u can use the ``ILogger``. 

```csharp
using Logging.Logger;
using Logging.Logger.Default;
using Logging.Logger.Default.Generic;
using Logging.Tracing;
using System.Diagnostics;

namespace ConsoleApp
{
    internal class Program
    {
        static readonly ILoggingService log = TraceLogger.Get(nameof(ConsoleApp));

        static void Main(string[] args)
        {
            // someone should trace to console
            TraceUtil.AddConsoleColorCodeToTrace();

            new Program().Run();
        }

        readonly ILogger logger;

        public Program()
        {
            logger = new Logger<Program>(log);
        }

        void Run()
        {
            Trace.TraceInformation("Hello World");
            // Output
            // INF [2023-02-12 15:27:31.155]:> Hello World

            log.Info("Hello Word");
            // Output
            // INF [2023-02-12 15:27:31.242]:> ConsoleApp:> Hello Word

            logger.Info("Hello World");
            // Output
            // INF [2023-02-12 15:27:31.248]:> ConsoleApp.Program:> Hello World
        }
    }
}
```

## Dependency Injection

This example shows u the base with ``Microsoft.Extensions.DependencyInjection``.

```csharp
using Logging.Logger;
using Logging.Logger.Default;
using Logging.Logger.Default.Generic;
using Logging.Tracing;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // someone should trace to console
            TraceUtil.AddConsoleColorCodeToTrace();

            // Demonstration
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<ILoggingService>(TraceLogger.Get("ConsoleApp"));
            services.AddScoped<ILogger, Logger>();
            services.AddScoped(typeof(ILogger<>), typeof(Logger<>));
            services.AddScoped<Program>();

            var provider = services.BuildServiceProvider();

            provider.GetRequiredService<Program>().Run();
        }

        readonly ILogger logger;

        public Program(ILogger<Program> logger)
        {
            this.logger = logger;
        }

        void Run()
        {
            logger.Info("Hello World");

            // Output
            // INF [2023-02-12 18:45:38.542]:> ConsoleApp.Program:> Hello World
        }
    }
}
```

This example shows u the base with ``Unity.Mvc``.

```csharp
// UnityConfig.cs
using Logging.Logger;
using Logging.Logger.Default;
using Logging.Logger.Default.Generic;
using System;
using Unity;
using Unity.Injection;

namespace WebApp
{
	public static class UnityConfig
    {
        private static Lazy<IUnityContainer> container =
          new Lazy<IUnityContainer>(() =>
          {
              var container = new UnityContainer();
              RegisterTypes(container);
              return container;
          });

        public static IUnityContainer Container => container.Value;

        public static void RegisterTypes(IUnityContainer container)
        {
            // base logging service
            container.RegisterInstance<ILoggingService>(TraceLogger.Get(nameof(WebApp)));
            container.RegisterType<ILogger, Logger>();
            container.RegisterType(typeof(ILogger<>), typeof(Logger<>));
        }
    }
}

...
// Global.asax.cs
using Logging.Tracing;
using System.Web;
using System.Web.Mvc;

namespace WebApp
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // syslog
            TraceUtil.AddSyslogToTrace("localhost");
        }
    }
}

...
// HomeController.cs
using Logging.Logger;
using Logging.Logger.Default;
using Logging.Logger.Default.Generic;
using System;
using System.Web.Mvc;

namespace WebApp
{
    [RoutePrefix("")]
    public class HomeController : MvcController
    {
		readonly ILogger logger;

        public HomeController(ILogger<HomeController> logger)
		{
			this.logger = logger;
			this.logger.Info("Hello World");
		}
    }
}
```