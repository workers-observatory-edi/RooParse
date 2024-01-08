using Material.Colors;
using Material.Styles.Themes;
using Material.Styles.Themes.Base;
using System.Diagnostics;
using System.Threading.Tasks;
using ReactiveUI;
using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;


namespace RiderParse.Avalonia
{
    public static class GlobalCommand
    {
        public static void OpenBrowserForVisitSite(string link)
        {
            var param = new ProcessStartInfo
            {
                FileName = link,
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(param);
        }

    }
}
