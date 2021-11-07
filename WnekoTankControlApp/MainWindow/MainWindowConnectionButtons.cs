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
        List<Button> connectButtons;
        List<IPAddress> iPAddresses;

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Button btn in connectButtons) btn.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
            communication?.ClosePort();
            communication = null;
            outQueue = null;
            inQueue = null;
            wifiUdpConnect.IsEnabled = true;
            wifiListBox.IsEnabled = true;
            localPortBox.IsEnabled = true;
            wifiListRefresh.IsEnabled = true;
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string port = PortBox.Text;
            try
            {
                if ((bool)usbComRadio.IsChecked) communication = new ComPortCommunication(port);
                else communication = new HC12Communication(port);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            await Connect();
        }

        private async void MockConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                communication = new MockCommunication();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            await Connect();
        }

        private async Task Connect()
        {
            foreach (Button btn in connectButtons) btn.IsEnabled = false;
            DisconnectButton.IsEnabled = true;
            outputBox.Text += "Connecting... \r\n";
            outQueue = new OutgoingMessageQueue(communication, DisplayMessage);
            inQueue = new IncommingMessageQueue();
            communication.SubscribeToMessages(inQueue.IncommingMessageHandler);
            RegisterMethods();

            await Task.Delay(1000);
            Send(TankCommandList.handshake);
            await Task.Delay(1000);
            Send(TankCommandList.handshake);
            await Task.Delay(500);
            Send(TankCommandList.hello + DateTime.Now.ToString());
        }

        private void wifiListRefresh_Click(object sender, RoutedEventArgs e)
        {
            iPAddresses = Dns.GetHostAddresses(Dns.GetHostName()).
                Where(x => ((bool)ipv4box.IsChecked && x.AddressFamily == AddressFamily.InterNetwork) ||
                ((bool)ipv6box.IsChecked && x.AddressFamily == AddressFamily.InterNetworkV6)).ToList();
            wifiListBox.ItemsSource = iPAddresses;
            if (iPAddresses.Count > 0) wifiListBox.SelectedItem = wifiListBox.Items[0];
            else MessageBox.Show("No adapter found!", "Can't connect!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void wifiUdpConnect_Click(object sender, RoutedEventArgs e)
        {
            int localPort;
            bool parsed = int.TryParse(localPortBox.Text, out localPort);
            if (parsed && wifiListBox.Items.Count > 0)
            {
                string msg = TankCommandList.connectUdp + wifiListBox.SelectedItem.ToString() + ";" + localPort;
                Send(msg);
                wifiUdpConnect.IsEnabled = false;
                wifiListBox.IsEnabled = false;
                localPortBox.IsEnabled = false;
                wifiListRefresh.IsEnabled = false;
            }
            else if (wifiListBox.Items.Count == 0) MessageBox.Show("No IP selected!", "Can't connect!", MessageBoxButton.OK, MessageBoxImage.Error);
            else MessageBox.Show("Incorrect port value!", "Can't connect!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
