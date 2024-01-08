using System;
using System.Collections.Generic;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace RiderParseMvvm.ViewModels
{
    public class AnalysisViewModel : PageViewModelBase
    {

        public AnalysisViewModel(IScreen screen): base(screen)
        {

            UrlPathSegment = "AnalysisViewModel";
            PageName = "Analysis";

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
