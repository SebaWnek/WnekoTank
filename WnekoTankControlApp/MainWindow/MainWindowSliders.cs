﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using CommonsLibrary;

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
            string msg = TankCommandList.setLinearSpeed + speed.ToString();
            Send(msg);
        }

        private void turnSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int turn = (int)e.NewValue;
            string msg = TankCommandList.setTurn + turn.ToString();
            Send(msg);
        }

        private void gimbalSlider_ValueChanged(object sender, DragCompletedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.setGimbalAngle;
            msg += gimbalVerAngSlider.Value + ";" + gimbalHorAngSlider.Value;
            Send(msg);
        }


        private void wideLightSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.ledWidePower;
            msg += wideLightSlider.Value;
            Send(msg);
        }

        private void narrowLightSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.ledNarrowPower;
            msg += narrowLightSlider.Value;
            Send(msg);
        }
    }
}
