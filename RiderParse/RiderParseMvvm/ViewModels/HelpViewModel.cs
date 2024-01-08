using System;
using System.Collections.Generic;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace RiderParseMvvm.ViewModels
{
    public class HelpViewModel : PageViewModelBase
    {
        public HelpViewModel(IScreen screen): base(screen)
        {

            void goToPage(string pageName) => MainViewModel.GoToPageFunc(pageName);

            ReactiveCommand<string, Unit> GoToPage;

            UrlPathSegment = "HelpViewModel";
            PageName = "Help";

            this.WhenActivated((CompositeDisposable disposables) =>
            {

                GoToPage = ReactiveCommand.Create<string>(goToPage);

                /* handle activation */
                Disposable
                    .Create(() => { /* handle deactivation */ })
                    .DisposeWith(disposables);
            });
        }
    }
}
