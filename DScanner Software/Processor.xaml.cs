using DScanner;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace DScanner_Software
{
    public partial class Processor : Window
    {
        private Bitmap _TargetImage = null;
        private string _TargetImagePath = null;

        private void _UpdateDisplay()
        {
            picDisplay.Image = _TargetImage;
        }


        // Event: On processor window loaded
        public Processor()
        {
            InitializeComponent();
        }

        // Event: Click open button
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "Bitmap (*.bmp)|*.bmp";
            if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _TargetImagePath = dialog.FileName;
                _TargetImage = new Bitmap(_TargetImagePath);
                _UpdateDisplay();
            }
        }

        // Event: Click binaryzation button
        private void btnBinaryzation_Click(object sender, RoutedEventArgs e)
        {
            int bias = 0;
            if (!(int.TryParse(txtBinaryzationBias.Text, out bias) && 0 <= bias && bias <= 255 * 3))
            {
                bias = 0;
                txtBinaryzationBias.Text = "0";
            }

            Stopwatch watch = new Stopwatch();
            watch.Start();
            //ImageProcessor.Binaryze(ref _TargetImage, bias, true);
            DScanner.Image.ImageProcessor.Binaryze(ref _TargetImage, bias);
            watch.Stop();
            System.Windows.MessageBox.Show("Time: " + watch.Elapsed.TotalMilliseconds.ToString() + "ms");
            _UpdateDisplay();
        }

        // Event: Click trim button
        private void btnTrim_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            //ImageProcessor.TrimBinaryLines(ref _TargetImage);
            DScanner.Image.ImageProcessor.Slim(ref _TargetImage);
            watch.Stop();
            System.Windows.MessageBox.Show("Time: " + watch.Elapsed.TotalMilliseconds.ToString() + "ms");
            _UpdateDisplay();
        }

        // Event: Click save button
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            picDisplay.Image.Save("SaveImage_" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".bmp");
        }

        // Event: Close the processor window
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        // Event: Click one step process button
        private void btnOneStepProcess_Click(object sender, RoutedEventArgs e)
        {
            int bias = 0;
            if (!(int.TryParse(txtBinaryzationBias.Text, out bias) && 0 <= bias && bias <= 255 * 3))
            {
                bias = 0;
                txtBinaryzationBias.Text = "0";
            }

            Stopwatch watch = new Stopwatch();
            watch.Start();
            DScanner.Image.ImageProcessor.OneStepProcess(ref _TargetImage, bias);
            watch.Stop();
            System.Windows.MessageBox.Show("Time: " + watch.Elapsed.TotalMilliseconds.ToString() + "ms");
            _UpdateDisplay();
        }
    }
}
