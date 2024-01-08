using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RiderParse.Avalonia.Views;
using RiderParseMvvm.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Avalonia.ReactiveUI;

namespace RiderParse.Avalonia.Pages
{
    public class Analysis : ReactiveUserControl<AnalysisViewModel>
    {

        public Analysis()
        {

            this.InitializeComponent();
            this.WhenActivated(disposables => { /* Handle interactions etc. */ });
            DataContext = this;

        }

        private void InitializeComponent()
        {          
            AvaloniaXamlLoader.Load(this);
        }

    }
}
