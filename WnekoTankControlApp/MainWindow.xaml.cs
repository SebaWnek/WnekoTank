using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        bool autoScroll = true;
        ComPortCommunication comPort;
        Thread sender;
        BlockingCollection<string> outbox = new BlockingCollection<string>();
        AutoResetEvent waiter = new AutoResetEvent(false);  
        public MainWindow()
        {
            InitializeComponent();
            comPort = new ComPortCommunication();
            comPort.SubscrideToMessages(Port_DataReceived);
            sender = new Thread(SendMessages);
            sender.Start();
        }

        private void SendMessages()
        {
            while (true)
            {
                string msg = outbox.Take();
                Dispatcher.Invoke(() => outputBox.Text += $"Sending {msg} \r\n");
                comPort.SendMessage(msg);
                waiter.WaitOne();
            }
        }

        private void Port_DataReceived(object sender, ComMessageEventArgs e)
        {
            Dispatcher.Invoke(() => outputBox.Text += e.Message + "\r\n");
            if (e.Message.StartsWith("ACK")) waiter.Set();
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
            outbox.Add(msg);
        }

        private void gear2btn_Click(object sender, RoutedEventArgs e)
        {
            string msg = "GEA-020";
            outbox.Add(msg);
        }

        private void angleSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            int angle = (int)angleSlider.Value;
            string msg = newGear + (angle >= 0 ? "+" : "") + angle.ToString("D3");
            outbox.Add(msg);
        }

        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            //speedSlider.Value = 0;
            //turnSlider.Value = 0;
            outbox = new BlockingCollection<string>();
            waiter.Set();
            outbox.Add("STP0"); //sends outside of queue as emergency message
        }

        private void speedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int speed = (int)e.NewValue;
            string msg = newSpeed + (speed >= 0 ? "+" : "") + speed.ToString("D3");
            outbox.Add(msg);
        }

        private void turnSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int turn = (int)e.NewValue;
            string msg = newTurn + (turn >= 0 ? "+" : "") + turn.ToString("D3");
            outbox.Add(msg);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            outbox.Dispose();
            comPort.Close();
            Environment.Exit(1);
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scroll = sender as ScrollViewer;
            if (e.ExtentHeightChange == 0)
            {
                if (scroll.VerticalOffset == scroll.ScrollableHeight)
                {
                    autoScroll = true;
                }
                else
                {
                    autoScroll = false;
                }
            }
            if (autoScroll && e.ExtentHeightChange != 0)
            {
                scroll.ScrollToEnd();
            }

        }
    }
}
