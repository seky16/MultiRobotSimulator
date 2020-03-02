using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using MultiRobotSimulator.WPF.Pages;
using MultiRobotSimulator.WPF.Services;
using Stylet;
using StyletIoC;

namespace MultiRobotSimulator.WPF
{
    internal class Bootstrapper : Bootstrapper<RootViewModel>
    {
        protected override void Configure()
        {
        }

        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            builder.Assemblies.Add(Assembly.Load("MultiRobotSimulator.Core"));

            builder.Bind<IIOService>().To<IOService>();
            builder.Bind<IMapService>().To<MapService>();

            //// TODO add logging
            //builder.Bind<ILoggerFactory>().ToAbstractFactory();
            //builder.Bind(typeof(ILogger<>)).To(typeof(Logger<>));
        }

        protected override void OnExit(ExitEventArgs e)
        {
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
        }

        protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = e.Exception;

            if (ex is TargetInvocationException tie && tie.InnerException != null)
                ex = tie.InnerException;

            //Container.Get<ILogger<App>>().LogCritical(ex, "Unhandled exception");

            var msg = ex.Message;

#if DEBUG
            msg = ex.ToString();
#endif

            Container.Get<IWindowManager>().ShowMessageBox($"Unhandled Exception: {msg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            e.Handled = true;
        }
    }
}
