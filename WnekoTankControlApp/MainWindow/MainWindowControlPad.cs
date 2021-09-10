using CommonsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WnekoTankControlApp
{
    public partial class MainWindow
    {
        int padDelay = 250;
        DispatcherTimer padTimer = new DispatcherTimer();

        int speedAccuracy = 10;
        int turnAccuracy = 5;

        string previousSpeed = "";
        string previousTurn = "";
        double padHorRangeCanvas;
        double padVerRangeCanvas;
        double minTurn = -52;
        double maxTurn = 52;
        double minSpeed = -104;
        double maxSpeed = 104;
        double padHorRange;
        double padVerRange;
        double padHorLSB;
        double padVerLSB;
        private void ControlTarget_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Ellipse)
            {
                (sender as Ellipse).CaptureMouse();
                ControlTarget.MouseMove += ControlTarget_MouseMove;
                padTimer.Start();
            }
        }

        private void ControlTarget_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(ControlCanvas);
            double left = p.X - ControlTarget.Width / 2;
            double top = p.Y - ControlTarget.Height / 2;
            int[] speedTurn = GetSpeedTurnFromPosition(p);
            if (left > 0 && left < ControlCanvas.ActualWidth - ControlTarget.Width)
            {
                Canvas.SetLeft(ControlTarget, left);
                ControlPadTurnBox.Text = speedTurn[0].ToString();
            }
            if (top > 0 && top < ControlCanvas.ActualHeight - ControlTarget.Height)
            {
                Canvas.SetTop(ControlTarget, top);
                ControlPadSpeedBox.Text = speedTurn[1].ToString();
            }
        }

        private int[] GetSpeedTurnFromPosition(Point p)
        {
            double left = p.X - ControlTarget.Width / 2;
            double top = p.Y - ControlTarget.Height / 2;
            double horAngle = left * padHorLSB - padHorRange / 2;
            double verAngle = -1 * (top * padVerLSB - padVerRange / 2);
            if (horAngle < minTurn) horAngle = minTurn;
            if (horAngle > maxTurn) horAngle = maxTurn;
            if (verAngle < minSpeed) verAngle = minSpeed;
            if (verAngle > maxSpeed) verAngle = maxSpeed;
            return new int[] { turnAccuracy * (int)Math.Round(horAngle / turnAccuracy, 0), speedAccuracy * (int)Math.Round(verAngle / speedAccuracy, 0) };
        }

        private void ControlTarget_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            (sender as Ellipse).ReleaseMouseCapture();
            ControlTarget.MouseMove -= ControlTarget_MouseMove;
            padTimer.Stop();
            Canvas.SetLeft(ControlTarget, ControlCanvas.ActualWidth / 2 - ControlTarget.ActualWidth / 2);
            Canvas.SetTop(ControlTarget, ControlCanvas.ActualHeight / 2 - ControlTarget.ActualHeight / 2);
            ControlPadSpeedBox.Text = "0";
            ControlPadTurnBox.Text = "0";
            string msg = TankCommandList.emergencyPrefix + TankCommandList.setSpeedWithTurn + "0;0";
            Send(msg);
        }

        private void ControlTarget_Loaded(object sender, RoutedEventArgs e)
        {
            PrepareControlCanvas();
            Canvas.SetLeft(ControlTarget, ControlCanvas.ActualWidth / 2 - ControlTarget.ActualWidth / 2);
            Canvas.SetTop(ControlTarget, ControlCanvas.ActualHeight / 2 - ControlTarget.ActualHeight / 2);
        }

        private void PrepareControlCanvas()
        {
            padHorRangeCanvas = CameraTargetCanvas.ActualWidth - ControlTarget.ActualWidth;
            padVerRangeCanvas = CameraTargetCanvas.ActualHeight - ControlTarget.ActualHeight;
            padHorRange = maxTurn - minTurn;
            padVerRange = maxSpeed - minSpeed;
            padHorLSB = padHorRange / padHorRangeCanvas;
            padVerLSB = padVerRange / padVerRangeCanvas;

            padTimer.Interval = TimeSpan.FromMilliseconds(padDelay);
            padTimer.Tick += PadTimer_Tick;
        }

        private void PadTimer_Tick(object sender, EventArgs e)
        {
            string currentSpeed = ControlPadSpeedBox.Text;
            string currentTurn = ControlPadTurnBox.Text;

            if (currentSpeed!= previousSpeed || currentTurn != previousTurn)
            {
                string msg = TankCommandList.emergencyPrefix + TankCommandList.setSpeedWithTurn;
                msg += currentSpeed + ";" + currentTurn;
                Send(msg);
                previousSpeed = currentSpeed;
                previousTurn = currentTurn;
            }
        }
    }
}
