using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WnekoTankControlApp
{
    /// <summary>
    /// Partial class for readability containing all slider handlers
    /// </summary>
    public partial class MainWindow
    {
        private void turnSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            turnSlider.Value = 0;
        }

        private void speedSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            speedSlider.Value = 0;
        }

        private void speedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int speed = (int)e.NewValue;
            string msg = comList.GetCode("setLinearSpeed") + speed.ToString();
            Send(msg);
        }

        private void turnSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int turn = (int)e.NewValue;
            string msg = comList.GetCode("setTurn") + turn.ToString();
            Send(msg);
        }

    }
}
