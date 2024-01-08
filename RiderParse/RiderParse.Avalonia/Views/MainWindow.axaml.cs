using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Material.Styles;
using ReactiveUI;
using Avalonia.ReactiveUI;
using RiderParseMvvm.ViewModels;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Material.Colors;
using Material.Styles.Themes;
using Material.Styles.Themes.Base;
using System.Linq;

namespace RiderParse.Avalonia.Views
{

    public class MainWindow : ReactiveWindow<MainViewModel>
    {

        #region Control fields
        private ToggleButton NavDrawerSwitch;
        private ListBox DrawerList;
        // private Carousel PageCarousel;
        private ScrollViewer mainScroller;
        private RoutedViewHost RoutedViewHost;
        public TextBlock SelectedPageTextBlock { get; set; }
        #endregion

        #region Theme
        public ReactiveCommand<bool, Unit> SetTheme { get; }

        void SetThemeFunc(bool isDarkTheme)
        {

            if (isDarkTheme)
            {
                var paletteHelper = new PaletteHelper();

                ITheme theme = paletteHelper.GetTheme();

                theme.SetPrimaryColor(SwatchHelper.Lookup[MaterialColor.LightBlue300]);
                theme.SetSecondaryColor(SwatchHelper.Lookup[MaterialColor.Indigo400]);
                theme.SetBaseTheme(BaseThemeMode.Dark.GetBaseTheme());

                paletteHelper.SetTheme(theme);
            }
            else
            {
                var paletteHelper = new PaletteHelper();

                ITheme theme = paletteHelper.GetTheme();

                theme.SetPrimaryColor(SwatchHelper.Lookup[MaterialColor.Indigo400]);
                theme.SetSecondaryColor(SwatchHelper.Lookup[MaterialColor.LightBlue300]);
                theme.SetBaseTheme(BaseThemeMode.Light.GetBaseTheme());

                paletteHelper.SetTheme(theme);
            }
        }
        #endregion

        public MainWindow()
        {

            //App = (App)Application.Current;
            //var appDataContext = App.DataContext;

            InitializeComponent();
            ViewModel = new MainViewModel();

            // Only this view needs to initialise ViewModel, routed ones do it automatically
            DataContext = ViewModel;

            SetTheme = ReactiveCommand.Create<bool>(SetThemeFunc);

            this.WhenActivated(disposables =>
            {
                // Bind the view model router to RoutedViewHost.Router property.
                this.OneWayBind(ViewModel, x => x.Router, x => x.RoutedViewHost.Router)
                    .DisposeWith(disposables);
                // Sync selected item in menu to ViewModel reactive property
                //this.WhenAnyValue(x => x.ViewModel.CurrentPage)
                //    .Where(x => x != null)
                //    .Subscribe(x => SelectedPageTextBlock = this.Get<TextBlock>(x));
                this.WhenAnyValue(x => x.ViewModel.Pages.SettingsViewModel.IsDarkTheme)
                    .Subscribe(x => SetTheme.Execute(x).Subscribe());
            });

            DataContextChanged += (object sender, EventArgs wat) =>
            {
                // here, this.DataContext will be your MainViewModel
            };
            // Continue from here

        }

        private void InitializeComponent()
        {

            this.WhenActivated(disposables => { /* Handle interactions etc. */ });
            AvaloniaXamlLoader.Load(this);

            #region Control getter and event binding
            NavDrawerSwitch = this.Get<ToggleButton>(nameof(NavDrawerSwitch));

            DrawerList = this.Get<ListBox>(nameof(DrawerList));
            DrawerList.PointerReleased += DrawerSelectionChanged;
            DrawerList.KeyUp += DrawerList_KeyUp;

            // PageCarousel = this.Get<Carousel>(nameof(PageCarousel));

            RoutedViewHost = this.Get<RoutedViewHost>(nameof(RoutedViewHost));
            mainScroller = this.Get<ScrollViewer>(nameof(mainScroller));

            // Starting selected page on Menu
            // TODO
            #endregion

            this.AttachDevTools(KeyGesture.Parse("Shift+F12"));

        }

        private void DrawerList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Enter)
                DrawerSelectionChanged(sender, null);
        }

        public void DrawerSelectionChanged(object sender, RoutedEventArgs args)
        {
            var listBox = sender as ListBox;
            if (!listBox.IsFocused && !listBox.IsKeyboardFocusWithin)
                return;
            try
            {
                SelectedPageTextBlock = (TextBlock)listBox.SelectedItem;
                this.ViewModel.GoToPage.Execute(SelectedPageTextBlock.Name).Subscribe();
                mainScroller.Offset = Vector.Zero;
                mainScroller.VerticalScrollBarVisibility =
                    listBox.SelectedIndex == 5 ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto;

            }
            catch
            {
            }
            NavDrawerSwitch.IsChecked = false;
        }

        public void OpenHelp()
        {
            //var selected_item = PageCarousel.SelectedItem;

            //var command = ReactiveCommand.Create(() => { selected_item = null; }
            //);
            //command.Execute().Subscribe();
            
        }
    }
}
