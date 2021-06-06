using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;
using CommonsLibrary;

namespace WnekoTankControlApp
{
    public partial class MainWindow
    {
        bool continuous = false;
        int delay = 300;
        DispatcherTimer timer = new DispatcherTimer();
        ObservableCollection<Position> predefinedPositionsList = new ObservableCollection<Position>();

        double horRangeCanvas;
        double verRangeCanvas;
        double minX;
        double maxX;
        double minY;
        double maxY;
        double horRange;
        double verRange;
        double horLSB;
        double verLSB;

        string previousX = "", previousY = "";

        public ObservableCollection<Position> PredefinedPositionsList { get => predefinedPositionsList; set => predefinedPositionsList = value; }

        private void PrepareCameraCanvas()
        {
            horRangeCanvas = CameraTargetCanvas.ActualWidth - CameraTarget.ActualWidth;
            verRangeCanvas = CameraTargetCanvas.ActualHeight - CameraTarget.ActualHeight;
            minX = gimbalHorAngSlider.Minimum;
            maxX = gimbalHorAngSlider.Maximum;
            minY = gimbalVerAngSlider.Minimum;
            maxY = gimbalVerAngSlider.Maximum;
            horRange = maxX - minX;
            verRange = maxY - minY;
            horLSB = horRange / horRangeCanvas;
            verLSB = verRange / verRangeCanvas;

            timer.Interval = TimeSpan.FromMilliseconds(delay);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            string currentX = gimbalHorAngCanvasBox.Text;
            string currentY = gimbalVerAngCanvasBox.Text;

            if (currentX != previousX && currentY != previousY)
            {
                string msg = CommandList.emergencyPrefix + CommandList.setGimbalAngle;
                msg += gimbalVerAngCanvasBox.Text + ";" + gimbalHorAngCanvasBox.Text;
                Send(msg);
                previousX = currentX;
                previousY = currentY;
            }
        }

        private void CameraTarget_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(CameraTargetCanvas);
            double left = p.X - CameraTarget.Width / 2;
            double top = p.Y - CameraTarget.Height / 2;
            int[] angles = GetAngleFromPosition(p);
            if (left > 0 && left < CameraTargetCanvas.ActualWidth - CameraTarget.Width)
            {
                Canvas.SetLeft(CameraTarget, left);
                gimbalHorAngCanvasBox.Text = angles[0].ToString();
            }
            if (top > 0 && top < CameraTargetCanvas.ActualHeight - CameraTarget.Height)
            {
                Canvas.SetTop(CameraTarget, top);
                gimbalVerAngCanvasBox.Text = angles[1].ToString();
            }
        }

        private void CameraTarget_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Ellipse)
            {
                (sender as Ellipse).CaptureMouse();
                CameraTarget.MouseMove += CameraTarget_MouseMove;
                if (continuous)
                {
                    timer.Start();
                }
            }
        }

        private void CameraTarget_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Ellipse)
            {
                Point p = e.GetPosition(CameraTargetCanvas);
                (sender as Ellipse).ReleaseMouseCapture();
                CameraTarget.MouseMove -= CameraTarget_MouseMove;

                int[] angles = GetAngleFromPosition(p);
                gimbalHorAngCanvasBox.Text = angles[0].ToString();
                gimbalVerAngCanvasBox.Text = angles[1].ToString();
                gimbalDefineXBox.Text = angles[0].ToString();
                gimbalDefineYBox.Text = angles[1].ToString();

                string msg = CommandList.emergencyPrefix + CommandList.setGimbalAngle;
                msg += angles[1] + ";" + angles[0];
                Send(msg);
                if (continuous)
                {
                    timer.Stop();
                }
            }
        }

        private int[] GetAngleFromPosition(Point p)
        {
            double left = p.X - CameraTarget.Width / 2;
            double top = p.Y - CameraTarget.Height / 2;
            double horAngle = left * horLSB - horRange / 2;
            double verAngle = -1 * (top * verLSB - verRange / 2);
            if (horAngle < minX) horAngle = minX;
            if (horAngle > maxX) horAngle = maxX;
            if (verAngle < minY) verAngle = minY;
            if (verAngle > maxY) verAngle = maxY;
            return new int[] { (int)Math.Round(horAngle, 0), (int)Math.Round(verAngle, 0) };
        }

        private void resetCameraCanvas_Click(object sender, RoutedEventArgs e)
        {
            double left = CameraTargetCanvas.ActualWidth / 2 - CameraTarget.ActualWidth / 2;
            double top = CameraTargetCanvas.ActualHeight / 2 - CameraTarget.ActualHeight / 2;
            Canvas.SetLeft(CameraTarget, left);
            Canvas.SetTop(CameraTarget, top);
            gimbalHorAngCanvasBox.Text = "0";
            gimbalVerAngCanvasBox.Text = "0";

            string msg = CommandList.emergencyPrefix + CommandList.setGimbalAngle;
            msg += 0 + ";" + 0;
            Send(msg);
        }

        private void cameraCanvasContinuous_Checked(object sender, RoutedEventArgs e)
        {
            continuous = true;
        }

        private void cameraCanvasContinuous_Unchecked(object sender, RoutedEventArgs e)
        {
            continuous = false;
        }

        private void CameraTarget_Loaded(object sender, RoutedEventArgs e)
        {
            PrepareCameraCanvas();
            Canvas.SetLeft(CameraTarget, CameraTargetCanvas.ActualWidth / 2 - CameraTarget.ActualWidth / 2);
            Canvas.SetTop(CameraTarget, CameraTargetCanvas.ActualHeight / 2 - CameraTarget.ActualHeight / 2);
        }

        private void gimbalDefineAddButton_Click(object sender, RoutedEventArgs e)
        {
            int x = 0;
            int y = 0;
            try
            {
                x = int.Parse(gimbalDefineXBox.Text);
                y = int.Parse(gimbalDefineYBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (x < minX || x > maxX || y < minY || y > maxY)
            {
                MessageBox.Show($"Angles out of range!\nHorizontal: {minX} to {maxX}\nVertical: {minY} to {maxY}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string name = gimbalDefineNameBox.Text;
            PredefinedPositionsList.Add(new Position(x, y, name));
        }

        private void predefinedDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Position selectedPosition = predefinedDataGrid.SelectedItem as Position;
            string msg = CommandList.emergencyPrefix + CommandList.setGimbalAngle;
            msg += selectedPosition.Y + ";" + selectedPosition.X;
            Send(msg);
        }

        public class Position
        {
            public int X { get; set; }
            public int Y { get; set; }
            public string Name { get; set; }

            public Position(int x, int y, string name)
            {
                X = x;
                Y = y;
                Name = name;
            }
        }
    }
}
