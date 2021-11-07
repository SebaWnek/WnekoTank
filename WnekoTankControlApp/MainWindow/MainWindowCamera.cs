using CommonsLibrary;
using MjpegProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WnekoTankControlApp
{
    public partial class MainWindow
    {
        enum ClickMode
        {
            rotateCamera,
            rotateDevice,
            move
        }

        ClickMode clickMode = ClickMode.rotateCamera;
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
        int w,h;
        double horAngle = 128; //verAngle = 96;
        double angularResolution;

        string baseAddress;

        private void MjpegLeft_Error(object sender, MjpegProcessor.ErrorEventArgs e)
        {
            MessageBox.Show(e.Message);
        }

        private void MjpegLeft_FrameReady(object sender, FrameReadyEventArgs e)
        {
            leftCameraImage.Source = e.BitmapImage;
        }

        private async void BrowserLeftButton_Click(object sender, RoutedEventArgs e)
        {
            baseAddress = browserLeftAddress.Text;
            client = new HttpClient();
            string address = baseAddress + streamSuffix;
            mjpegLeft.ParseStream(new Uri(address));
            await Task.Delay(500);
            w = (int)leftCameraImage.ActualWidth;
            h = (int)leftCameraImage.ActualHeight;
            resolutionLabel.Content = $"{w}x{h}px";
        }

        private void BrowserLeftStopButton_Click(object sender, RoutedEventArgs e)
        {
            client.Dispose();
            wbListBox.ItemsSource = null;
            mjpegLeft.StopStream();
        }

        private async void ResolutionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            value = SelectResolution(value);
            string address = baseAddress + controlSuffix + var + framesize + val + value;
            client?.GetAsync(address);
            await Task.Delay(500);
            w = (int)leftCameraImage.ActualWidth;
            h = (int)leftCameraImage.ActualHeight;
            resolutionLabel.Content = $"{w}x{h}px";
        }

        private int SelectResolution(int value)
        {
            switch (value)
            {
                case 1: return 1;
                case 2: return 5;
                case 3: return 8;
                case 4: return 9;
                case 5: return 10;
                case 6: return 13;
                default: throw new ArgumentException("Unknown resolution!");
            }

        }

        private void QualitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            string address = baseAddress + controlSuffix + var + quality + val + value;
            client?.GetAsync(address);
        }

        private void SaturationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            string address = baseAddress + controlSuffix + var + saturation + val + value;
            client?.GetAsync(address);
        }

        private void BrightnesSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            string address = baseAddress + controlSuffix + var + brightness + val + value;
            client?.GetAsync(address);
        }

        private void ContrastSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            string address = baseAddress + controlSuffix + var + contrast + val + value;
            client?.GetAsync(address);
        }

        private void WbListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int value = wbListBox.SelectedIndex;
            string address = baseAddress + controlSuffix + var + wb_mode + val + value;
            client?.GetAsync(address);
        }

        private void LeftCameraImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            angularResolution = w / horAngle;
            string msg;
            switch (clickMode)
            {
                case ClickMode.rotateCamera:
                    msg = TurnCamera(e);
                    break;
                case ClickMode.rotateDevice:
                    msg = TurnDevice(e);
                    break;
                case ClickMode.move:
                    msg = MoveDevice(e);
                    break;
                default:
                    throw new ArgumentException("Unknown ClickMode!");

            }

            Send(msg);

            moveByClickButton.IsEnabled = true;
            turnByClickButton.IsEnabled = true;
            infoLabel.Content = "";
            clickMode = ClickMode.rotateCamera;
            imageArea.Cursor = Cursors.Arrow;

        }

        private string MoveDevice(MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(leftCameraImage);
            w = (int)leftCameraImage.ActualWidth;
            h = (int)leftCameraImage.ActualHeight;

            int dX = (int)((p.X - w / 2) / angularResolution);
            int dY = (int)(-1 * (p.Y - h / 2) / angularResolution);
            resolutionLabel.Content = $"{dX}, {dY}";
            string msg = TankCommandList.moveByCamera + dX + ";" + dY;
            return msg;
        }

        private string TurnDevice(MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(leftCameraImage);
            w = (int)leftCameraImage.ActualWidth;
            h = (int)leftCameraImage.ActualHeight;

            int dX = (int)((p.X - w / 2) / angularResolution);
            resolutionLabel.Content = $"{dX}";

            return TankCommandList.turnToByCamera + dX;
        }

        private string TurnCamera(MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(leftCameraImage);
            w = (int)leftCameraImage.ActualWidth;
            h = (int)leftCameraImage.ActualHeight;

            int dX = (int)((p.X - w / 2) / angularResolution);
            int dY = (int)(-1 * (p.Y - h / 2) / angularResolution);
            resolutionLabel.Content = $"{dX}, {dY}";

            string msg = TankCommandList.emergencyPrefix + TankCommandList.changeGimbalAngleBy;
            msg += dY + ";" + dX;
            return msg;
        }


        private void MoveByClickButton_Click(object sender, RoutedEventArgs e)
        {
            moveByClickButton.IsEnabled = false;
            turnByClickButton.IsEnabled = false;
            infoLabel.Content = "Click where to move";
            clickMode = ClickMode.move;
            imageArea.Cursor = Cursors.Cross;
        }

        private void TurnByClickButton_Click(object sender, RoutedEventArgs e)
        {
            moveByClickButton.IsEnabled = false;
            turnByClickButton.IsEnabled = false;
            infoLabel.Content = "Click where to turn to";
            clickMode = ClickMode.rotateDevice;
            imageArea.Cursor = Cursors.Cross;
        }

        private void CancelClickButton_Click(object sender, RoutedEventArgs e)
        {
            moveByClickButton.IsEnabled = true;
            turnByClickButton.IsEnabled = true;
            infoLabel.Content = "";
            clickMode = ClickMode.rotateCamera;
            imageArea.Cursor = Cursors.Arrow;
        }
    }
}
