using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Dialogs;
using Avalonia.Markup.Xaml;
using Material.Styles.Assists;
using System.Collections.ObjectModel;
using Material.Dialog;
using ReactiveUI;
using Avalonia.ReactiveUI;
using RiderParseMvvm.ViewModels;

namespace RiderParse.Avalonia.Pages
{
    public class Home : ReactiveUserControl<HomeViewModel>
    {

        public ReactiveCommand<string, IRoutableViewModel> GoToPage { get; }

        public Home()
        {

            this.InitializeComponent();
            this.WhenActivated(disposables => {
                /* Handle interactions etc. */

            });
            // AvaloniaXamlLoader.Load(this);
            DataContext = this;

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
