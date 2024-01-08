using System;
using System.Collections.Generic;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace RiderParseMvvm.ViewModels
{
    public class MainViewModel : ReactiveObject, IActivatableViewModel, IScreen
    {
        public ViewModelActivator Activator { get; }

        #region Page navigation
        // The Router associated with this Screen.
        public RoutingState Router { get; } = new RoutingState();

        // CurrentPage string property
        readonly ObservableAsPropertyHelper<string> currentPage;
        public string CurrentPage => currentPage.Value;

        // Function to change the page using string PageName
        public void GoToPageFunc(string page) => Router.Navigate.Execute(Pages[page]).Subscribe();

        #endregion

        #region ReactiveCommand
        // Navigate to page using name string argument
        public ReactiveCommand<string, Unit> GoToPage { get; }

        // Navigate to previous page
        public ReactiveCommand<Unit, IRoutableViewModel> GoBack { get; }
        #endregion

        #region Page ViewModels

        public class PagesViewModels
        {

            public HomeViewModel HomeViewModel { get; }
            public AnalysisViewModel AnalysisViewModel { get; }

            public HelpViewModel HelpViewModel { get; }
            public SettingsViewModel SettingsViewModel { get; }

            public AboutViewModel AboutViewModel { get; }

            private Dictionary<string, INamedPage> indexingDictionary { get; }

            public INamedPage this[string pageName]
            {
                get => indexingDictionary[pageName];
            }

            public PagesViewModels(MainViewModel mainViewModel)
            {
                HomeViewModel = new(mainViewModel);
                AnalysisViewModel = new(mainViewModel);
                HelpViewModel = new(mainViewModel);
                SettingsViewModel = new(mainViewModel);
                AboutViewModel = new(mainViewModel);

                indexingDictionary = new();

                // Add to dictionary, PageName attribute as key
                foreach (INamedPage pageViewModel in new INamedPage[] {
                    HomeViewModel,
                    AnalysisViewModel,
                    HelpViewModel,
                    SettingsViewModel,
                    AboutViewModel
                })
                {
                    indexingDictionary.Add(pageViewModel.PageName, pageViewModel);
                    //ArgumentException will be thrown if multiple pages have the same PageName.
                }



            }
        }

        public readonly PagesViewModels Pages;

        #endregion

        public MainViewModel()
        {

            Pages = new(this);

            Activator = new ViewModelActivator();

            GoToPage = ReactiveCommand.Create<string>(GoToPageFunc);

            GoToPage.Execute("Home").Subscribe();

            GoBack = Router.NavigateBack;

            // Bind CurrentPage to Router.CurrentViewModel
            currentPage =
                this.Router.CurrentViewModel
                .Select(current => ((INamedPage)current).PageName)
                .ToProperty(this, x => x.CurrentPage);

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                /* handle activation */
                Disposable
                    .Create(() => { /* handle deactivation */ })
                    .DisposeWith(disposables);
            });

        }
    }
}

