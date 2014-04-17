using DScanner;
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
            Stopwatch watch = new Stopwatch();
            watch.Start();
            ImageProcessor.Binaryze(ref _TargetImage, 1, true);
            watch.Stop();
            System.Windows.MessageBox.Show("Time: " + watch.Elapsed.TotalMilliseconds.ToString() + "ms");
            _UpdateDisplay();
        }

        private void btnTrim_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            ImageProcessor.TrimBinaryLines(ref _TargetImage);
            watch.Stop();
            System.Windows.MessageBox.Show("Time: " + watch.Elapsed.TotalMilliseconds.ToString() + "ms");
            _UpdateDisplay();
        }
    }
}
