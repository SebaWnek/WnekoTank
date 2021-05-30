using MjpegProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WnekoTankControlApp
{
    public partial class MainWindow
    {
        MjpegDecoder mjpegLeft;
        MjpegDecoder mjpegRight;

        private void MjpegRight_Error(object sender, MjpegProcessor.ErrorEventArgs e)
        {
            MessageBox.Show(e.Message);
        }

        private void MjpegRight_FrameReady(object sender, FrameReadyEventArgs e)
        {
            rightCameraImage.Source = e.BitmapImage;
        }

        private void MjpegLeft_Error(object sender, MjpegProcessor.ErrorEventArgs e)
        {
            MessageBox.Show(e.Message);
        }

        private void MjpegLeft_FrameReady(object sender, FrameReadyEventArgs e)
        {
            leftCameraImage.Source = e.BitmapImage;
        }

        private void browserRightButton_Click(object sender, RoutedEventArgs e)
        {
            mjpegRight.ParseStream(new Uri(browserRightAddress.Text));
        }

        private void browserLeftButton_Click(object sender, RoutedEventArgs e)
        {
            mjpegLeft.ParseStream(new Uri(browserLeftAddress.Text));
        }

        private void browserLeftStopButton_Click(object sender, RoutedEventArgs e)
        {
            mjpegLeft.StopStream();
        }

        private void browserRightStopButton_Click(object sender, RoutedEventArgs e)
        {
            mjpegRight.StopStream();
        }
    }
}
