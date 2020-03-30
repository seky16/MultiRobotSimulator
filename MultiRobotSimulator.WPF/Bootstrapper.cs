using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using MultiRobotSimulator.Core.Factories;
using MultiRobotSimulator.WPF.Pages;
using MultiRobotSimulator.WPF.Services;
using NLog.Extensions.Logging;
using Stylet;
using StyletIoC;

namespace MultiRobotSimulator.WPF
{
    internal class Bootstrapper : Bootstrapper<RootViewModel>
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        protected override void Configure()
        {
            _logger.Debug("Container loaded");
        }

        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            builder.Assemblies.Add(Assembly.Load("MultiRobotSimulator.Core"));

            // todo add plugin assemblies here, and add to Stylet IoC using .ToAllImplementations();

            // singletons
            builder.Bind<IIOService>().To<IOService>().InSingletonScope();
            builder.Bind<IMapFactory>().To<MapFactory>().InSingletonScope();

            // logging
            builder.Bind<ILoggerFactory>().To<NLogLoggerFactory>();
            builder.Bind(typeof(ILogger<>)).To(typeof(Logger<>));
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _logger.Info("App exit");
        }

        protected override void OnLaunch()
        {
        }

        protected override void OnStart()
        {
            // set invariant culture
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(System.Windows.Markup.XmlLanguage.GetLanguage(CultureInfo.InvariantCulture.IetfLanguageTag)));

            _logger.Info("App started");
        }

        protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = e.Exception;

            if (ex is TargetInvocationException tie && tie.InnerException != null)
                ex = tie.InnerException;

            _logger.Fatal(ex, "Unhandled exception");

            var msg = ex.Message;

#if DEBUG
            msg = ex.ToString();
#endif

            Container.Get<IWindowManager>().ShowMessageBox($"Unhandled Exception: {msg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            e.Handled = true;
        }
    }
}
