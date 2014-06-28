using AForge.Video;
using AForge.Video.DirectShow;
using DScanner.Communication;
using DScanner.Image;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.IO.Ports;

namespace DScannerServer
{
    /// <summary>
    /// Interaction logic of the main window.
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

        private string[] _SerialPortList = null; // The available serail port list
        private StepperController _Stepper = null; // The controller of stepper

        #endregion


        #region Local Configurations

        private bool _RawFrameFlag; // Display raw frame from the video stream
        private bool _CrosshairFlag; // Display crosshair on the video screen

        private int _ConsoleMaxLines = 100; // Maximum number of lines displayed on the console (Preset)
        private bool _ConsoleAutoscrollFlag; // Automatically scroll the console to the end
        private MessageLevel _ConsoleDisplayLevel = MessageLevel.Info; // Lowest message level of the console to display (Preset)

        private bool _ProcessingFlag = false; // There is task being processed
        private bool _SnapshotFlag = false; // Taking snapshot

        private int _StepperBaudRate = 57600; // Baud rate of the stepper serial port

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
        /// Append crosshair on the image.
        /// </summary>
        /// <param name="bmp">Image to append crosshair.</param>
        private void _AppendCrosshair(ref Bitmap bmp)
        {
            // Append crosshair
            if (_CrosshairFlag)
            {
                int halfWidth = bmp.Width / 2;
                int halfHeight = bmp.Height / 2;
                for (int i = 0; i < bmp.Height; ++i)
                {
                    bmp.SetPixel(halfWidth, i, Color.YellowGreen);
                }
                for (int i = 0; i < bmp.Width; ++i)
                {
                    bmp.SetPixel(i, halfHeight, Color.YellowGreen);
                }
            }
        }

        /// <summary>
        /// Process the captured image.
        /// </summary>
        /// <param name="bmp">The image as a bitmap to process.</param>
        private void _ProcessCapturedImage(ref Bitmap bmp)
        {
            // Check if display raw frame
            if(!_RawFrameFlag)
            {
                // Image processing
                ImageProcessor.OneStepProcess(ref bmp, 5);
            }
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
            //if (_Bitmap != null) _Bitmap.Dispose();

            // Get new frame
            _Bitmap = (Bitmap)eventArgs.Frame.Clone();

            // Process the frame
            _ProcessCapturedImage(ref _Bitmap);

            // Append crosshair
            _AppendCrosshair(ref _Bitmap);

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
            // Clear the old list
            if (menu_Devices_Camera.Items.Count > 0)
            {
                menu_Devices_Camera.Items.Clear();
            }

            // Obtain the list of all video input devices
            _VideoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            // Check if input devices exist
            if (_VideoDevices.Count == 0)
            {
                MenuItem item = new MenuItem();
                item.Header = "(No Camera)";
                item.IsEnabled = false;
                menu_Devices_Camera.Items.Add(item);
                _VideoDevices = null;
            }

            // Append all available devices to the device list
            for (int i = 0; i < _VideoDevices.Count; ++i)
            {
                MenuItem item = new MenuItem();
                item.Header = _VideoDevices[i].Name;
                item.Name = "ConnectCamera_" + i;
                item.IsChecked = false;
                item.Click += menu_Devices_Camera_ConnectCamera_Click;
                menu_Devices_Camera.Items.Add(item);
            }
        }

        #endregion


        #region Assistance Functions for Stepper

        /// <summary>
        /// Handler for the operation failed event of the stepper.
        /// </summary>
        private void _StepperOperationFailedEventHandler()
        {
            // Inform the operation failed message
            _ConsolePrintMessage("Stepper operation failed.", MessageLevel.Warning);
        }

        /// <summary>
        /// Handler for the rotation finished event of the stepper.
        /// </summary>
        private void _StepperRotationFinishedEventHandler()
        {
            // Inform the rotation finished message
            _ConsolePrintMessage("Stepper finished rotation.", MessageLevel.Info);
        }

        /// <summary>
        /// Connect to the specific serial port of the stepper. If no 
        /// availale serial port exists, nothing will happen.
        /// </summary>
        /// <param name="index">Index of the serial port to connect to.</param>
        private void _ConnectStepper(int index)
        {
            // Check if there is available serial port
            if (_SerialPortList == null || _SerialPortList.Length == 0) return;

            // Ensure the previous stepper stopped
            _DisconnectStepper();

            // Select the specific serial port
            try
            {
                _Stepper = new StepperController(_SerialPortList[index]);
            }
            catch (Exception e)
            {
                _ConsolePrintMessage("private void _ConnectStepper(int): " + e.Message, MessageLevel.Error);
                return;
            }

            // Set the baud rate
            _Stepper.BaudRate = _StepperBaudRate;

            // Append handlers to the events
            _Stepper.RotationFinished += _StepperRotationFinishedEventHandler;
            _Stepper.OperationFailed += _StepperOperationFailedEventHandler;

            // Connect to the stepper port
            _Stepper.Open();

            // CONSOLE: Connect stepper message
            _ConsolePrintMessage("Successfully connect to the stepper on " + _Stepper.PortName + ".", MessageLevel.Info);
        }

        /// <summary>
        /// Disconnect the current stepper. If stepper has been stopped 
        /// or no serial port exists, nothing will happen.
        /// </summary>
        private void _DisconnectStepper()
        {
            // Check if stpper exists
            if (_Stepper == null) return;

            // Close the stepper
            if (_Stepper.IsOpen)
            {
                _Stepper.Close();
                _ConsolePrintMessage("Stepper on " + _Stepper.PortName + " is closed.", MessageLevel.Info);
            }
            else
            {
                _ConsolePrintMessage("Stepper serial port is closed.", MessageLevel.Info);
            }

            // Reset the reference to the stepper
            _Stepper = null;
        }

        /// <summary>
        /// Rotate the stepper by specific steps
        /// </summary>
        /// <param name="steps">The number of steps to rotate.</param>
        private void _RotateStepper(int steps)
        {
            if(_Stepper != null)
            {
                // Inform the rotation message
                _ConsolePrintMessage("Stepper rotates for " + steps.ToString() + " steps.", MessageLevel.Info);

                // Rotate the stepper
                _Stepper.Rotate(steps);
            }
            else
            {
                // Inform the no stepper opens message
                _ConsolePrintMessage("No stepper is connected.", MessageLevel.Error);
            }
        }

        /// <summary>
        /// Initialze the available serial port list for stepper.
        /// </summary>
        private void _InitializeStepperSerialPortList()
        {
            // Clear the old list
            if (menu_Devices_Stepper_Port.Items.Count > 0)
            {
                menu_Devices_Stepper_Port.Items.Clear();
            }

            // Obtain the list of all available serial ports
            _SerialPortList = SerialPort.GetPortNames();

            // Check if serial port devices exist
            if (_SerialPortList.Length == 0)
            {
                MenuItem item = new MenuItem();
                item.Header = "(No Serial Port)";
                item.IsEnabled = false;
                menu_Devices_Stepper_Port.Items.Add(item);
            }

            // Append all available devices to the serial port list
            for (int i = 0; i < _SerialPortList.Length; ++i)
            {
                MenuItem item = new MenuItem();
                item.Header = _SerialPortList[i];
                item.Name = "ConnectStepperOnPort_" + i;
                item.IsChecked = false;
                item.Click += menu_Devices_Stepper_Port_ConnectStepperOnPort_Click;
                menu_Devices_Stepper_Port.Items.Add(item);
            }
        }

        #endregion


        #region Main Window Logic

        // Event: Main window initialization
        public MainWindow()
        {
            // Initialize the components
            InitializeComponent();

            // Initialize the device list
            _InitializeVideoDeviceList();

            // Initialize the serial port list for stepper
            _InitializeStepperSerialPortList();

            // Initialize the configuration of raw frame
            _RawFrameFlag = false;
            menu_Configurations_VideoStream_RawFrame.IsChecked = false;

            // Initialize the configuration of crosshair
            _CrosshairFlag = true;
            menu_Configurations_VideoStream_Crosshair.IsChecked = true;

            // Initialize the configuration of console autoscroll
            _ConsoleAutoscrollFlag = true;
            menu_Configurations_Console_Autoscroll.IsChecked = true;

            // CONSOLE: Server initialized message
            _ConsolePrintMessage("DScanner server initialized.", MessageLevel.Info);
        }

        // Event: Main window closed
        private void winMain_Closed(object sender, EventArgs e)
        {
            // Disconnect camera device
            _DisconnectCamera();

            // Disconnect stepper
            _DisconnectStepper();

            // Shutdown the application
            Application.Current.Shutdown();
        }

        #endregion


        #region Menu Logic

        // Event: Click menu on "File -> Exit"
        private void menu_File_Exit_Click(object sender, RoutedEventArgs e)
        {
            _DisconnectCamera();
            Application.Current.Shutdown();
        }


        // Event: Click menu on "Devices -> Camera -> ConnectCamera_[X]"
        private void menu_Devices_Camera_ConnectCamera_Click(object sender, RoutedEventArgs e)
        {
            // Obtain the sender MenuItem
            MenuItem item = (MenuItem)sender;

            // Traverse the all menu items
            foreach (object traversalItem in menu_Devices_Camera.Items)
            {
                MenuItem traversalMenuItem = (MenuItem)traversalItem;
                if (traversalMenuItem.Name == item.Name)
                {
                    if (traversalMenuItem.IsChecked)
                    {
                        // Disconnect the camera
                        _DisconnectCamera();

                        // Uncheck the menu item
                        traversalMenuItem.IsChecked = false;
                    }
                    else
                    {
                        // Obtain the camera index
                        int index = int.Parse(item.Name.Substring(14)); // ConnectCamera_[X]

                        // Connect to the selected camera
                        _ConnectCamera(index);

                        // Check the menu item
                        traversalMenuItem.IsChecked = true;
                    }
                }
                else
                {
                    // Uncheck other menu items
                    if (traversalMenuItem.IsChecked) traversalMenuItem.IsChecked = false;
                }
            }
        }


        // Event: Click menu on "Devices -> Stepper -> Port -> ConnectStepperOnPort_[X]"
        private void menu_Devices_Stepper_Port_ConnectStepperOnPort_Click(object sender, RoutedEventArgs e)
        {
            // Obtain the sender MenuItem
            MenuItem item = (MenuItem)sender;

            // Traverse the all menu items
            foreach (object traversalItem in menu_Devices_Stepper_Port.Items)
            {
                MenuItem traversalMenuItem = (MenuItem)traversalItem;
                if (traversalMenuItem.Name == item.Name)
                {
                    if (traversalMenuItem.IsChecked)
                    {
                        // Disconnect the serial port
                        _DisconnectStepper();

                        // Uncheck the menu item
                        traversalMenuItem.IsChecked = false;
                    }
                    else
                    {
                        // Obtain the serial port index
                        int index = int.Parse(item.Name.Substring(21)); // ConnectStepperOnPort_[X]

                        // Connect to the selected port
                        _ConnectStepper(index);

                        // Check the menu item
                        traversalMenuItem.IsChecked = true;
                    }
                }
                else
                {
                    // Uncheck other menu items
                    if (traversalMenuItem.IsChecked) traversalMenuItem.IsChecked = false;
                }
            }
        }


        // Event: Click menu on "Devices -> Stepper -> Run Test"
        private void menu_Devices_Stepper_RunTest_Click(object sender, RoutedEventArgs e)
        {
            _RotateStepper(512);
        }


        // Event: Click menu on "Devices -> Refresh"
        private void menu_Devices_Refresh_Click(object sender, RoutedEventArgs e)
        {
            // Reinitialzie video device list
            _InitializeVideoDeviceList();

            // Reinitialize serial port list
            _InitializeStepperSerialPortList();
        }


        // Event: Click menu on "Video Stream -> Snapshot"
        private void menu_VideoStream_Snapshot_Click(object sender, RoutedEventArgs e)
        {
            _SnapshotFlag = true;
        }


        // Event: Click menu on "Configurations -> Video Stream -> Raw Frame"
        private void menu_Configurations_VideoStream_RawFrame_Click(object sender, RoutedEventArgs e)
        {
            // Toggle the raw frame display state
            menu_Configurations_VideoStream_RawFrame.IsChecked = !menu_Configurations_VideoStream_RawFrame.IsChecked;
        }

        // Event: Switch menu state on "Configurations -> Video Stream -> Raw Frame" to Checked
        private void menu_Configurations_VideoStream_RawFrame_Checked(object sender, RoutedEventArgs e)
        {
            _RawFrameFlag = true;
            _ConsolePrintMessage("Raw frame display configuration is enabled.", MessageLevel.Info);
        }

        // Event: Switch menu state on "Configurations -> Video Stream -> Raw Frame" to Unchecked
        private void menu_Configurations_VideoStream_RawFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            _RawFrameFlag = false;
            _ConsolePrintMessage("Raw frame display configuration is disabled.", MessageLevel.Info);
        }


        // Event: Click menu on "Configurations -> Video Stream -> Crosshair"
        private void menu_Configurations_VideoStream_Crosshair_Click(object sender, RoutedEventArgs e)
        {
            // Switch the check state
            menu_Configurations_VideoStream_Crosshair.IsChecked = !menu_Configurations_VideoStream_Crosshair.IsChecked;
        }

        // Event: Switch menu state on "Configurations -> Video Stream -> Crosshair" to Checked
        private void menu_Configurations_VideoStream_Crosshair_Checked(object sender, RoutedEventArgs e)
        {
            _CrosshairFlag = true;
            _ConsolePrintMessage("Crosshair configuration is enabled.", MessageLevel.Info);
        }

        // Event: Switch menu state on "Configurations -> Video Stream -> Crosshair" to Unchecked
        private void menu_Configurations_VideoStream_Crosshair_Unchecked(object sender, RoutedEventArgs e)
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
