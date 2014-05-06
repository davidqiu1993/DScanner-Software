using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Windows;

namespace DScanner_Software
{
    public partial class MainWindow : Window
    {
        private Bitmap _Bitmap = null; // The current displayed bitmap
        private FilterInfoCollection _VideoDevices = null; // The available video device list
        private VideoCaptureDevice _VideoSource = null; // The current connected video source
        private bool _ProcessingFlag = false; // The flag indicating if there is task being processed
        private bool _SnapshotFlag = false; // The flag indicating if snapshot takes place
        private bool _CalibrationFlag = false; // The flag indicating if chessboard calibration takes place
        private bool _ConsoleScrollToEndFlag = true; // The flag indicating if the console automatically scroll to end


        /// <summary>
        /// Print message to console.
        /// </summary>
        /// <param name="message">The message to print.</param>
        private void _ConsolePrint(string message)
        {
            txtConsole.Dispatcher.Invoke(new Action(() => {
                txtConsole.Text += message;
                if (txtConsole.LineCount > 100) txtConsole.Text = txtConsole.Text.Substring(txtConsole.GetCharacterIndexFromLineIndex(txtConsole.LineCount - 100));
                if (_ConsoleScrollToEndFlag) txtConsole.ScrollToEnd();
            }));
        }

        /// <summary>
        /// Print message to console with new line at the end.
        /// </summary>
        /// <param name="message">The message to print</param>
        private void _ConsolePrintLine(string message)
        {
            _ConsolePrint(message + "\n");
        }


        /// <summary>
        /// Sum up the RGB values of a pixel.
        /// </summary>
        /// <param name="pix">The target pixel.</param>
        /// <returns></returns>
        private int _SumUpRGB(Color pix)
        {
            return (pix.R + pix.G + pix.B);
        }

        /// <summary>
        /// Process the captured image.
        /// </summary>
        /// <param name="bmp">The image as a bitmap to process.</param>
        private void _ProcessCapturedImage(ref Bitmap bmp)
        {
            if (_CalibrationFlag)
            {
                const int accuracy = 10;
                const int accuracy_detect = 15;
                const int spread = 90;
                int min = 765;
                int max = 0;
                for (int i = 0; i < bmp.Size.Height; i += accuracy)
                {
                    for (int j = 0; j < bmp.Size.Width; j += accuracy)
                    {
                        int psum = _SumUpRGB(bmp.GetPixel(j, i));
                        if (psum < min) min = psum;
                        if (psum > max) max = psum;
                    }
                }
                _ConsolePrintLine("(min, max) = (" + min.ToString() + ", " + max.ToString() + ")");

                min += spread;
                max -= spread;
                for (int i = 0; i < bmp.Size.Height - accuracy_detect; i += accuracy)
                {
                    for (int j = 0; j < bmp.Size.Width - accuracy_detect; j += accuracy)
                    {
                        if (_SumUpRGB(bmp.GetPixel(j, i)) > min) continue;
                        if (_SumUpRGB(bmp.GetPixel(j + accuracy_detect, i)) < max) continue;
                        if (_SumUpRGB(bmp.GetPixel(j, i + accuracy_detect)) < max) continue;
                        if (_SumUpRGB(bmp.GetPixel(j + accuracy_detect, i + accuracy_detect)) > min) continue;

                        Graphics g = Graphics.FromImage(bmp);
                        g.DrawEllipse(Pens.Red, j, i, accuracy_detect, accuracy_detect);
                    }
                }
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
            if (_Bitmap != null) _Bitmap.Dispose();

            // Get new frame
            _Bitmap = (Bitmap)eventArgs.Frame.Clone();

            // Process the frame
            _ProcessCapturedImage(ref _Bitmap);

            // Check if snapshot required
            if (_SnapshotFlag)
            {
                _Bitmap.Save("Snapshot_" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".bmp");
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

        // Event: Main window closed
        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
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

        // Event: Click menu on "Switches -> Calibration (on)" 
        private void menu_Switches_Calibration_Checked(object sender, RoutedEventArgs e)
        {
            _CalibrationFlag = true;
        }

        // Event: Click menu on "Switches -> Calibration (off)"
        private void menu_Switches_Calibration_Unchecked(object sender, RoutedEventArgs e)
        {
            _CalibrationFlag = false;
        }

        // Event: Click menu on "Switches -> Console Autoscroll (on)"
        private void menu_Switches_Checked(object sender, RoutedEventArgs e)
        {
            _ConsoleScrollToEndFlag = true;
        }

        // Event: Click menu on "Switches -> Console Autoscroll (off)"
        private void menu_Switches_Unloaded(object sender, RoutedEventArgs e)
        {
            _ConsoleScrollToEndFlag = false;
        }
    }
}
