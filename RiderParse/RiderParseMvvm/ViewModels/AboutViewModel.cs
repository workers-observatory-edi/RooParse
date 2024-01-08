using System;
using System.Collections.Generic;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace RiderParseMvvm.ViewModels
{
    public class AboutViewModel : PageViewModelBase
    { 
        public AboutViewModel(IScreen screen): base(screen)
        {

            UrlPathSegment = "AboutViewModel";
            PageName = "About";

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
