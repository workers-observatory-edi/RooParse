using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive.Disposables;

namespace RiderParseMvvm.ViewModels
{
    public class SettingsViewModel : PageViewModelBase
    {

        [Reactive] public bool IsDarkTheme { get; private set; } = true;

        public SettingsViewModel(IScreen screen): base(screen)
        {

            UrlPathSegment = "SettingsViewModel";
            PageName = "Settings";

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
