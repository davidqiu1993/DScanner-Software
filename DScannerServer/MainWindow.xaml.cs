using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Windows;

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


        #region Local Variables

        private FilterInfoCollection _VideoDevices = null; // The available video device list
        private VideoCaptureDevice _VideoSource = null; // The current connected video source

        private Bitmap _Bitmap = null; // The current displayed bitmap
        
        #endregion


        #region Local Configurations

        private bool _CrosshairFlag; // Display crosshair on the video screen

        private int _ConsoleMaxLines = 100; // Maximum number of lines displayed on the console (Preset)
        private bool _ConsoleAutoscrollFlag; // Automatically scroll the console to the end
        private MessageLevel _ConsoleDisplayLevel = MessageLevel.Info; // Lowest message level of the console to display (Preset)

        private bool _ProcessingFlag = false; // There is task being processed
        private bool _SnapshotFlag = false; // Taking snapshot

        #endregion


        #region Assistance Functions for Console

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



        #region Assistance Functions for Video Stream

        /// <summary>
        /// Process the captured image.
        /// </summary>
        /// <param name="bmp">The image as a bitmap to process.</param>
        private void _ProcessCapturedImage(ref Bitmap bmp)
        {
            ;
        }

        /// <summary>
        /// Handler of the new frame event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The arguments of the event.</param>
        private void _NewFrameEventHandler(object sender, NewFrameEventArgs eventArgs)
        {
            // Check if there is task being processed
            if (_ProcessingFlag) return;

            // Update the processing flag indicating task begins
            _ProcessingFlag = true;

            // Release the previous bitmap
            if (_Bitmap != null) _Bitmap.Dispose();

            // Get new frame
            _Bitmap = (Bitmap)eventArgs.Frame.Clone();

            // Process the frame
            _ProcessCapturedImage(ref _Bitmap);

            // Check if snapshot required
            if (_SnapshotFlag)
            {
                string filename = "Snapshot_" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".bmp";
                _Bitmap.Save(filename);
                _ConsolePrintMessage("Snapshot is saved as \"" + filename + "\"", MessageLevel.Info);
                _SnapshotFlag = false;
            }

            // Display the captured image
            picDisplay.Image = _Bitmap;

            // Update the processing flag indicating task finished
            _ProcessingFlag = false;
        }

        /// <summary>
        /// Connect to the specific camera. If no availale camera exists, 
        /// nothing will happen.
        /// </summary>
        /// <param name="index">Index of the camera to connect to.</param>
        private void _ConnectCamera(int index)
        {
            // Check if there is available camera
            if (_VideoDevices == null) return;

            // Ensure the previous camera stopped
            _DisconnectCamera();

            // Clear state flags
            _ProcessingFlag = false;
            _SnapshotFlag = false;

            // Select the specific camera
            try
            {
                _VideoSource = new VideoCaptureDevice(_VideoDevices[index].MonikerString);
            }
            catch (Exception e)
            {
                _ConsolePrintMessage("private void _ConnectCamera(int): " + e.Message, MessageLevel.Error);
                return;
            }

            // Append handler to new frame event
            _VideoSource.NewFrame += new NewFrameEventHandler(_NewFrameEventHandler);

            // Connect to the camera
            _VideoSource.Start();

            // CONSOLE: Start camera message
            _ConsolePrintMessage("Successfully connect to the camera. Video capturing and processing begins.", MessageLevel.Info);
        }

        /// <summary>
        /// Disconnect the current camera. If camera has been stopped or 
        /// no camera exists, nothing will happen.
        /// </summary>
        private void _DisconnectCamera()
        {
            // Check if camera exists
            if (_VideoSource == null) return;

            // Signal the camera to stop
            _VideoSource.SignalToStop();
            _ConsolePrintMessage("Waiting for the video stream to stop.", MessageLevel.Info);

            // Wait for the camera until it stops
            _VideoSource.WaitForStop();
            _ConsolePrintMessage("Video device is stopped.", MessageLevel.Info);
        }

        /// <summary>
        /// Initialize the available video device list.
        /// </summary>
        private void _InitializeVideoDeviceList()
        {
            // Obtain the list of all video input devices
            _VideoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            // Check if input devices exist
            if (_VideoDevices.Count == 0)
            {
                cbxVideoDeviceList.Items.Add("No local capture devices.");
                cbxVideoDeviceList.IsEnabled = false;
                _VideoDevices = null;
            }

            // Append all available devices to the device list
            foreach (FilterInfo device in _VideoDevices)
            {
                cbxVideoDeviceList.Items.Add(device.Name);
            }

            // Set the default device
            cbxVideoDeviceList.SelectedIndex = 0;
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
