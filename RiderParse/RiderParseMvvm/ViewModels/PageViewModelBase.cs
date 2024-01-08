using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace RiderParseMvvm.ViewModels
{
    public abstract class PageViewModelBase : ReactiveObject, IActivatableViewModel, INamedPage
    {
        public ViewModelActivator Activator { get; }

        // Reference to IScreen that owns the routable view model.
        public IScreen HostScreen { get; }

        // Unique identifier for the routable view model.
        public virtual string UrlPathSegment { get; protected set; }

        public virtual string PageName { get; protected set; }


        public readonly MainViewModel MainViewModel;


        public PageViewModelBase(IScreen screen)
        {
            HostScreen = screen;

            MainViewModel = (MainViewModel)HostScreen;

            Activator = new ViewModelActivator();
        }
    }

    public interface INamedPage: IRoutableViewModel
    {
        public string PageName { get; }

    }
}

