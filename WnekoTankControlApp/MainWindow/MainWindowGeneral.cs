using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using CommonsLibrary;
using WnekoTankControlApp.CommandControl;
using WnekoTankControlApp.CommandControl.ComDevices;

namespace WnekoTankControlApp
{
    /// <summary>
    /// Partial class for readability containing all button click handlers
    /// </summary>
    public partial class MainWindow
    {
        bool shouldQueue = false;
        private void Send(string msg)
        {
            if (shouldQueue)
            {
                commandList.Add(msg);
                return;
            }
            try
            {
                outQueue.SendMessage(msg);
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Connect first!", "Not connected!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw ex;
            }
        }

        private void SendEmergency(string msg)
        {
            try
            {
                outQueue.SendEmergencyMessage(msg);
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Connect first!", "Not connected!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw ex;
            }
        }
    }
}
