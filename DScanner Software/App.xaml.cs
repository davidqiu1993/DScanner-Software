using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace DScanner_Software
{
    public partial class App : Application
    {
        public App()
        {
            WindowHoster.LoadWindows();
        }
    }

    public static class WindowHoster
    {
        private static Processor _Window_Processor = null;

        public static void ShowWindow_Processor()
        {
            _Window_Processor.Visibility = Visibility.Visible;
        }

        public static void LoadWindows()
        {
            // Load processor window
            if(_Window_Processor == null)
            {
                _Window_Processor = new Processor();
                _Window_Processor.Visibility = Visibility.Hidden;
            }
        }
    }
}
