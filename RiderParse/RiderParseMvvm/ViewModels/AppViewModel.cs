using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Disposables;

namespace RiderParseMvvm.ViewModels
{
    public class AppViewModel : ReactiveObject, IActivatableViewModel
    {

        public ViewModelActivator Activator { get; }

        public AppViewModel()
        {
            Activator = new ViewModelActivator();
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
