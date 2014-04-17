using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Windows;

namespace DScanner_Software
{
    public partial class MainWindow : Window
    {
        private FilterInfoCollection _VideoDevices = null; // The available video device list
        private VideoCaptureDevice _VideoSource = null; // The current connected video source
        private bool _ProcessingFlag = false; // The flag indicating if there is task being processed
        private bool _SnapshotFlag = false; // The flag indicating if snapshot takes place


        /// <summary>
        /// Process the captured image.
        /// </summary>
        /// <param name="bmp">The image as a bitmap to process.</param>
        private void _ProcessCapturedImage(ref Bitmap bmp)
        {
            return;
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

            // Get new frame
            Bitmap bmp = (Bitmap)eventArgs.Frame.Clone();

            // Process the frame
            _ProcessCapturedImage(ref bmp);

            // Display the captured image
            picDisplay.Image = bmp;

            // Check if snapshot required
            if (_SnapshotFlag)
            {
                bmp.Save("Snapshot_" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".bmp");
                _SnapshotFlag = false;
            }

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
            catch(Exception e)
            {
                return;
            }

            // Append handler to new frame event
            _VideoSource.NewFrame += new NewFrameEventHandler(_NewFrameEventHandler);

            // Connect to the camera
            _VideoSource.Start();
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

            // Wait for the camera until it stops
            _VideoSource.WaitForStop();
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


        // Event: On main window loaded
        public MainWindow()
        {
            // Initialize window components
            InitializeComponent();

            // Initialize video device list
            _InitializeVideoDeviceList();
        }

        // Event: Click connect button
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            _ConnectCamera(cbxVideoDeviceList.SelectedIndex);
        }

        // Event: Click disconnect button
        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            _DisconnectCamera();
        }

        // Event: Click snapshot button
        private void btnSnapshot_Click(object sender, RoutedEventArgs e)
        {
            _SnapshotFlag = true;
        }

        // Event: Click menu on "file -> exit"
        private void menu_File_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Event: Click menu on "processor"
        private void menu_Processor_Click(object sender, RoutedEventArgs e)
        {
            WindowHoster.ShowWindow_Processor();
        }
    }
}
