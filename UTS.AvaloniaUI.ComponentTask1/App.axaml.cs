using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TickChartControl.Models;
using TickChartControl.Services.MarketData;
using UTS.AvalonaiUI.ComponentTask1.TickChartControl;
using UTS.AvaloniaUI.ComponentTask1.Views;

namespace UTS.AvaloniaUI.ComponentTask1
{
    public partial class App : Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var services = new ServiceCollection();

            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<MainWindow>();

            services.AddTransient<IPriceGenerator, MarketPriceGenerator>();
            services.AddTransient<ITickStorage>(provider => new CircularTickBuffer(500));
            services.AddTransient<ITickChart, TickChart>();

            ServiceProvider = services.BuildServiceProvider();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            }
            base.OnFrameworkInitializationCompleted();
        }

    }
}