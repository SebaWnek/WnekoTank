using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WnekoTankControlApp
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string newSpeed = "SPD";
        string newTurn = "TRN";
        string newGear = "GEA";
        ICommunication communication;
        MessageQueue queue;
        private Boolean AutoScroll = true;
        public MainWindow()
        {
            InitializeComponent();
            //comPort.SubscribeToMessages(Port_DataReceived);
        }

        public void DisplayMessage(string msg)
        {
            Dispatcher.Invoke(() => outputBox.Text += msg + "\r\n");
        }

        private void turnSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            turnSlider.Value = 0;
        }

        private void speedSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            speedSlider.Value = 0;
        }

        private void gear1btn_Click(object sender, RoutedEventArgs e)
        {
            string msg = "GEA+055";
            queue.SendMessage(msg);
        }

        private void gear2btn_Click(object sender, RoutedEventArgs e)
        {
            string msg = "GEA-020";
            queue.SendMessage(msg);
        }

        private void angleSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            int angle = (int)angleSlider.Value;
            string msg = newGear + (angle >= 0 ? "+" : "") + angle.ToString("D3");
            queue.SendMessage(msg);
        }

        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            //speedSlider.Value = 0;
            //turnSlider.Value = 0;
            queue.SendEmergencyMessage("STP0");
        }

        private void speedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int speed = (int)e.NewValue;
            string msg = newSpeed + (speed >= 0 ? "+" : "") + speed.ToString("D3");
            queue.SendMessage(msg);
        }

        private void turnSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int turn = (int)e.NewValue;
            string msg = newTurn + (turn >= 0 ? "+" : "") + turn.ToString("D3");
            queue.SendMessage(msg);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            communication?.ClosePort();
        }

        //From: https://stackoverflow.com/questions/2984803/how-to-automatically-scroll-scrollviewer-only-if-the-user-did-not-change-scrol
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // User scroll event : set or unset auto-scroll mode
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : user scroll event
                if (ScrollView.VerticalOffset == ScrollView.ScrollableHeight)
                {   // Scroll bar is in bottom
                    // Set auto-scroll mode
                    AutoScroll = true;
                }
                else
                {   // Scroll bar isn't in bottom
                    // Unset auto-scroll mode
                    AutoScroll = false;
                }
            }

            // Content scroll event : auto-scroll eventually
            if (AutoScroll && e.ExtentHeightChange != 0)
            {   // Content changed and auto-scroll mode set
                // Autoscroll
                ScrollView.ScrollToVerticalOffset(ScrollView.ExtentHeight);
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            communication?.ClosePort();
            communication = null;
            queue = null;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            communication = new ComPortCommunication(PortBox.Text);
            queue = new MessageQueue(communication, DisplayMessage);
        }
    }
}
