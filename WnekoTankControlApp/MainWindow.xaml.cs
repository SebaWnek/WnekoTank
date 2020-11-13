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
        ComPortCommunication comPort;
        public MainWindow()
        {
            InitializeComponent();
             comPort = new ComPortCommunication();
            comPort.SubscrideToMessages(Port_DataReceived);
        }

        private void Port_DataReceived(object sender, ComMessageEventArgs e)
        {
            Dispatcher.Invoke(() => outputBox.Text += e.Message + "\r\n");
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
            comPort.SendMessage(msg);
        }

        private void gear2btn_Click(object sender, RoutedEventArgs e)
        {
            string msg = "GEA-020";
            comPort.SendMessage(msg);
        }

        private void angleSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            int angle = (int)angleSlider.Value;
            string msg = newGear + (angle >= 0 ? "+" : "") + angle.ToString("D3");
            comPort.SendMessage(msg);
        }

        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            //speedSlider.Value = 0;
            //turnSlider.Value = 0;
            comPort.SendMessage("STP0");
        }

        private void speedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int speed = (int)e.NewValue;
            string msg = newSpeed + (speed >= 0 ? "+" : "") + speed.ToString("D3");
            comPort.SendMessage(msg);
        }

        private void turnSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int turn = (int)e.NewValue;
            string msg = newTurn + (turn >= 0 ? "+" : "") + turn.ToString("D3");
            comPort.SendMessage(msg);
        }
    }
}
