using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DScannerServer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Local Assistance Structures

        /// <summary>
        /// Level of console message.
        /// </summary>
        private enum MessageLevel
        {
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3,
            Critical = 4
        }

        #endregion


        #region Local Configurations

        private bool _CrosshairFlag; // Display crosshair on the video screen
        private int _ConsoleMaxLines = 100; // Maximum number of lines displayed on the console (Preset)
        private bool _ConsoleAutoscrollFlag; // Automatically scroll the console to the end
        private MessageLevel _ConsoleDisplayLevel = MessageLevel.Info; // Lowest message level of the console to display (Preset)

        #endregion


        #region Assistant Functions for Console

        /// <summary>
        /// Print message to console.
        /// </summary>
        /// <param name="message">The message to print.</param>
        private void _ConsolePrint(string message)
        {
            txtConsole.Dispatcher.Invoke(new Action(() =>
            {
                txtConsole.Text += message;
                if (txtConsole.LineCount > _ConsoleMaxLines) txtConsole.Text = txtConsole.Text.Substring(txtConsole.GetCharacterIndexFromLineIndex(txtConsole.LineCount - _ConsoleMaxLines));
                if (_ConsoleAutoscrollFlag) txtConsole.ScrollToEnd();
            }));
        }

        /// <summary>
        /// Print message to console with new line at the end.
        /// </summary>
        /// <param name="message">The message to print.</param>
        private void _ConsolePrintLine(string message)
        {
            _ConsolePrint(message + "\n");
        }

        /// <summary>
        /// Print message to console with level at the front and new line at the end.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="level">The level of the message.</param>
        private void _ConsolePrintMessage(string message, MessageLevel level = MessageLevel.Info)
        {
            if (level >= _ConsoleDisplayLevel)
            {
                _ConsolePrint("[" + DateTime.Now + "] [" + level.ToString() + "] " + message + "\n");
            }
        }

        #endregion


        #region Main Window Logic

        // Event: Main window initialization
        public MainWindow()
        {
            // Initialize the components
            InitializeComponent();

            // Initialize the configuration of crosshair
            _CrosshairFlag = true;
            menu_Configurations_Assistance_Crosshair.IsChecked = true;

            // Initialize the configuration of console autoscroll
            _ConsoleAutoscrollFlag = true;
            menu_Configurations_Console_Autoscroll.IsChecked = true;

            // CONSOLE: Server initialized message
            _ConsolePrintMessage("DScanner server initialized.", MessageLevel.Info);
        }

        // Event: Main window closed
        private void winMain_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion


        #region Menu Logic

        // Event: Click menu on "File -> Exit"
        private void menu_File_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


        // Event: Click menu on "Configurations -> Assistance -> Crosshair"
        private void menu_Configurations_Assistance_Crosshair_Click(object sender, RoutedEventArgs e)
        {
            // Switch the check state
            menu_Configurations_Assistance_Crosshair.IsChecked = !menu_Configurations_Assistance_Crosshair.IsChecked;
        }

        // Event: Switch menu state on "Configurations -> Assistance -> Crosshair" to Checked
        private void menu_Configurations_Assistance_Crosshair_Checked(object sender, RoutedEventArgs e)
        {
            _CrosshairFlag = true;
            _ConsolePrintMessage("Crosshair configuration is enabled.", MessageLevel.Info);
        }

        // Event: Switch menu state on "Configurations -> Assistance -> Crosshair" to Unchecked
        private void menu_Configurations_Assistance_Crosshair_Unchecked(object sender, RoutedEventArgs e)
        {
            _CrosshairFlag = false;
            _ConsolePrintMessage("Crosshair configuration is disabled.", MessageLevel.Info);
        }


        // Event: Click menu on "Configurations -> Console -> Autoscroll"
        private void menu_Configurations_Console_Autoscroll_Click(object sender, RoutedEventArgs e)
        {
            // Switch the check state
            menu_Configurations_Console_Autoscroll.IsChecked = !menu_Configurations_Console_Autoscroll.IsChecked;
        }

        // Event: Switch menu state on "Configurations -> Console -> Autoscroll" to Checked
        private void menu_Configurations_Console_Autoscroll_Checked(object sender, RoutedEventArgs e)
        {
            _ConsoleAutoscrollFlag = true;
            _ConsolePrintMessage("Console autoscroll configuration is enabled.", MessageLevel.Info);
        }

        // Event: Switch menu state on "Configurations -> Console -> Autoscroll" to Unchecked
        private void menu_Configurations_Console_Autoscroll_Unchecked(object sender, RoutedEventArgs e)
        {
            _ConsoleAutoscrollFlag = false;
            _ConsolePrintMessage("Console autoscroll configutation is disabled.", MessageLevel.Info);
        }

        #endregion
    }
}
