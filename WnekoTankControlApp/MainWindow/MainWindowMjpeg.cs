using MjpegProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WnekoTankControlApp
{
    public partial class MainWindow
    {
        MjpegDecoder mjpegLeft;
        HttpClient client;
        string streamSuffix = ":81/stream";
        string controlSuffix = "/control?";
        string var = "var=";
        string val = "&val=";
        string framesize = "framesize";
        string quality = "quality";
        string contrast = "contrast";
        string brightness = "brightness";
        string saturation = "saturation";
        string wb_mode = "wb_mode";
        //string[] wb_modes =
        //{
        //    "Auto",
        //    "Sunny",
        //    "Cloudy",
        //    "Office",
        //    "Home"
        //};

        string baseAddress;

        private void MjpegLeft_Error(object sender, MjpegProcessor.ErrorEventArgs e)
        {
            MessageBox.Show(e.Message);
        }

        private void MjpegLeft_FrameReady(object sender, FrameReadyEventArgs e)
        {
            leftCameraImage.Source = e.BitmapImage;
        }

        private void browserLeftButton_Click(object sender, RoutedEventArgs e)
        {
            baseAddress = browserLeftAddress.Text;
            client = new HttpClient();
            string address = baseAddress + streamSuffix;
            mjpegLeft.ParseStream(new Uri(address));
        }

        private void browserLeftStopButton_Click(object sender, RoutedEventArgs e)
        {
            client.Dispose();
            wbListBox.ItemsSource = null;
            mjpegLeft.StopStream();
        }

        private void resolutionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            string address = baseAddress + controlSuffix + var + framesize + val + value;
            client?.GetAsync(address);
        }

        private void qualitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            string address = baseAddress + controlSuffix + var + quality + val + value;
            client?.GetAsync(address);
        }

        private void saturationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            string address = baseAddress + controlSuffix + var + saturation + val + value;
            client?.GetAsync(address);
        }

        private void brightnesSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            string address = baseAddress + controlSuffix + var + brightness + val + value;
            client?.GetAsync(address);
        }

        private void contrastSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            string address = baseAddress + controlSuffix + var + contrast + val + value;
            client?.GetAsync(address);
        }

        private void wbListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int value = wbListBox.SelectedIndex;
            string address = baseAddress + controlSuffix + var + wb_mode + val + value;
            client?.GetAsync(address);
        }
    }
}
