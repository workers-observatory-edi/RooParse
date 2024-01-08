using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RiderParse.Avalonia.Views;
using ReactiveUI;
using RiderParseMvvm.ViewModels;
using Splat;
using System.Reflection;

namespace RiderParse.Avalonia
{
    public class App : Application, IViewFor<AppViewModel>
    {

        public App(): base()
        {
            
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            ViewModel = new AppViewModel();
            DataContext = ViewModel;

        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

        public static readonly StyledProperty<AppViewModel> ViewModelProperty =
            AvaloniaProperty.Register<App, AppViewModel>(nameof(ViewModel));

        public AppViewModel ViewModel
        {
            get => (AppViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, (object)value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (AppViewModel)value;
        }


    }
}
