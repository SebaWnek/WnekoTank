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
using WnekoTankMeadow;

namespace WnekoTankControlApp
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CommandsList comList = new CommandsList();
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
            string msg = comList.GetCode("setGear") + "1";
            queue.SendMessage(msg);
        }

        private void gear2btn_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("setGear") +"2";
            queue.SendMessage(msg);
        }

        //private void angleSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        //{
        //    int angle = (int)angleSlider.Value;
        //    string msg = setGear + (angle >= 0 ? "+" : "") + angle.ToString("D3");
        //    queue.SendMessage(msg);
        //}

        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            //speedSlider.Value = 0;
            //turnSlider.Value = 0;
            string msg = comList.GetCode("emergencyPrefix") + comList.GetCode("stop") + "0";
            queue.SendEmergencyMessage(msg);
        }

        private void speedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int speed = (int)e.NewValue;
            string msg = comList.GetCode("setLinearSpeed") + speed.ToString();
            queue.SendMessage(msg);
        }

        private void turnSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int turn = (int)e.NewValue;
            string msg = comList.GetCode("setTurn") + turn.ToString();
            queue.SendMessage(msg);
        }

        private void queueStart_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("startInvoking") + "0";
            queue.SendMessage(msg);
        }

        private void queueStop_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("stopInvoking") + "0";
            queue.SendMessage(msg);
        }

        private void listQueue_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("enumerateQueue") + "0";
            queue.SendMessage(msg);
        }

        private void waitButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("wait") + waitBox.Text;
            queue.SendMessage(msg);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            communication?.ClosePort();
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
        private void clearQueue_Click(object sender, RoutedEventArgs e)
        {

        }

        //Based on: https://stackoverflow.com/questions/2984803/how-to-automatically-scroll-scrollviewer-only-if-the-user-did-not-change-scrol
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
    }
}
