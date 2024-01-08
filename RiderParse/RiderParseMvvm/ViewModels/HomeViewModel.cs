using System;
using System.Collections.Generic;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace RiderParseMvvm.ViewModels
{
    public class HomeViewModel : PageViewModelBase
    {

        public ReactiveCommand<string, Unit> GoToPage { get; }

        public HomeViewModel(IScreen screen): base(screen)
        {

            UrlPathSegment = "HomeViewModel";
            PageName = "Home";

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
