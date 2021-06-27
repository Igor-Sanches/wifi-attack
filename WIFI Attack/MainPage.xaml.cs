using Perfect_Scan_UWP.Materias;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks; 
using Windows.Devices.WiFi;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x416

namespace WIFI_Attack
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static MainPage Main;
        public static MainPage Current
        {
            get { return Main; }
        }
        private WiFiAdapter firstAdapter;
        private string savedProfileName = null;
        public MainPage()
        {
            this.InitializeComponent();
            Main = this;
        }

        public ObservableCollection<WiFiNetworkDisplay> ResultCollection
        {
            get;
            private set;
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            ResultCollection = new ObservableCollection<WiFiNetworkDisplay>();
           

            // RequestAccessAsync must have been called at least once by the app before using the API
            // Calling it multiple times is fine but not necessary
            // RequestAccessAsync must be called from the UI thread
            var access = await WiFiAdapter.RequestAccessAsync();
            if (access != WiFiAccessStatus.Allowed)
            {
                NotifyUser("Access denied" );
            }
            else
            {
                DataContext = this;

                var result = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
                if (result.Count >= 1)
                { 
                    NotifyUser(result[0].Id);
                    firstAdapter = await WiFiAdapter.FromIdAsync(result[0].Id);

                    var scanButton = new Button();
                    scanButton.Content = string.Format("Scan");
                    scanButton.Click += ScanButton_Click;
                    Buttons.Children.Add(scanButton);

                    var disconnectButton = new Button();
                    disconnectButton.Content = string.Format("Disconnect");
                    disconnectButton.Click += DisconnectButton_Click; ;
                    Buttons.Children.Add(disconnectButton);

                    // Monitor network status changes
                    await UpdateConnectivityStatusAsync();
                    NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
                }
                else
                {
                    NotifyUser("No WiFi Adapters detected on this machine" );
                }
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            firstAdapter.Disconnect();
        }

        private async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            await UpdateConnectivityStatusAsync();

            // Update the connectivity level displayed for each
            foreach (var network in ResultCollection)
            {
                await network.UpdateConnectivityLevel();
            }
        }

        private async Task UpdateConnectivityStatusAsync()
        {
            var connectedProfile = await firstAdapter.NetworkAdapter.GetConnectedProfileAsync();
            if (connectedProfile != null && !connectedProfile.ProfileName.Equals(savedProfileName))
            {
                savedProfileName = connectedProfile.ProfileName;
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    NotifyUser(string.Format("WiFi adapter connected to: {0} ({1})", connectedProfile.ProfileName, connectedProfile.GetNetworkConnectivityLevel()) );
                });
            }
            else if (connectedProfile == null && savedProfileName != null)
            {
                savedProfileName = null;
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                   NotifyUser("WiFi adapter disconnected" );
                });
            }
        }

        private async void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            await firstAdapter.ScanAsync();
            ConnectionBar.Visibility = Visibility.Collapsed;

            DisplayNetworkReport(firstAdapter.NetworkReport);
        }




        "https://www.perfect-scan.com/UUID_USER/trueEdit/UUID_CODIGO"

        private async void DisplayNetworkReport(WiFiNetworkReport report)
        {
         
            ResultCollection.Clear();

            foreach (var network in report.AvailableNetworks)
            {
                var networkDisplay = new WiFiNetworkDisplay(network, firstAdapter);
                await networkDisplay.UpdateConnectivityLevel();
                ResultCollection.Add(networkDisplay);
            }
            foreach(var red in ResultCollection)
            {
                String ssid2 = red.Ssid;
                if(ssid.Text.Equals(ssid2)){
                     
                    if (red == null || firstAdapter == null)
                    {
                        NotifyUser("Network not selcted");
                        return;
                    }
                    WiFiReconnectionKind reconnectionKind = WiFiReconnectionKind.Manual;
                    //if (IsAutomaticReconnection.IsChecked.HasValue && IsAutomaticReconnection.IsChecked == true)
                    //{
                    //    reconnectionKind = WiFiReconnectionKind.Automatic;
                    //}

                    WiFiConnectionResult result;
                    var credential = new PasswordCredential();
                    if (red.AvailableNetwork.SecuritySettings.NetworkAuthenticationType == NetworkAuthenticationType.Open80211 &&
                        red.AvailableNetwork.SecuritySettings.NetworkEncryptionType == NetworkEncryptionType.None)
                    {
                        result = await firstAdapter.ConnectAsync(red.AvailableNetwork, reconnectionKind);
                    }
                    else
                    {
                        // Only the password portion of the credential need to be supplied

                        // Make sure Credential.Password property is not set to an empty string. 
                        // Otherwise, a System.ArgumentException will be thrown.
                        // The default empty password string will still be passed to the ConnectAsync method,
                        // which should return an "InvalidCredential" error

                        credential.Password = "row_connect2020";// await Passwords(1);


                        result = await firstAdapter.ConnectAsync(red.AvailableNetwork, reconnectionKind, credential);
                    }

                    if (result.ConnectionStatus == WiFiConnectionStatus.Success)
                    {
                        NotifyUser(string.Format("Successfully connected to {0}.", red.Ssid));
                        Bundle.PutInt("Key", Bundle.GetInt("Key", 1));
                    }
                    else
                    {
                        Bundle.PutInt("Key", Bundle.GetInt("Key", 1) + 1);
                        credential.Password = await Passwords(Bundle.GetInt("Key", 1));
                    }

                }
                
            }
        }

        private void ResultsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedNetwork = ResultsListView.SelectedItem as WiFiNetworkDisplay;
            if (selectedNetwork == null)
            {
                return;
            }

            // Show the connection bar
            ConnectionBar.Visibility = Visibility.Visible;

            // Only show the password box if needed
            if (selectedNetwork.AvailableNetwork.SecuritySettings.NetworkAuthenticationType == NetworkAuthenticationType.Open80211 &&
                selectedNetwork.AvailableNetwork.SecuritySettings.NetworkEncryptionType == NetworkEncryptionType.None)
            {
                //NetworkKeyInfo.Visibility = Visibility.Collapsed;
            }
            else
            {
                //NetworkKeyInfo.Visibility = Visibility.Visible;
            }
        }
        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedNetwork = ResultsListView.SelectedItem as WiFiNetworkDisplay;
            if (selectedNetwork == null || firstAdapter == null)
            {
                NotifyUser("Network not selcted");
                return;
            }
            WiFiReconnectionKind reconnectionKind = WiFiReconnectionKind.Manual;
            //if (IsAutomaticReconnection.IsChecked.HasValue && IsAutomaticReconnection.IsChecked == true)
            //{
            //    reconnectionKind = WiFiReconnectionKind.Automatic;
            //}

            WiFiConnectionResult result;
            var credential = new PasswordCredential();
            if (selectedNetwork.AvailableNetwork.SecuritySettings.NetworkAuthenticationType == NetworkAuthenticationType.Open80211 &&
                selectedNetwork.AvailableNetwork.SecuritySettings.NetworkEncryptionType == NetworkEncryptionType.None)
            {
                result = await firstAdapter.ConnectAsync(selectedNetwork.AvailableNetwork, reconnectionKind);
            }
            else
            {
                // Only the password portion of the credential need to be supplied
         
                // Make sure Credential.Password property is not set to an empty string. 
                // Otherwise, a System.ArgumentException will be thrown.
                // The default empty password string will still be passed to the ConnectAsync method,
                // which should return an "InvalidCredential" error
                
                credential.Password = await Passwords(1);
              

                result = await firstAdapter.ConnectAsync(selectedNetwork.AvailableNetwork, reconnectionKind, credential);
            }

            if (result.ConnectionStatus == WiFiConnectionStatus.Success)
            {
                NotifyUser(string.Format("Successfully connected to {0}.", selectedNetwork.Ssid));
                Bundle.PutInt("Key", Bundle.GetInt("Key", 1));
            }
            else
            {
                Bundle.PutInt("Key", Bundle.GetInt("Key", 1) + 1);
                credential.Password = await Passwords(Bundle.GetInt("Key", 1));
            }
        }

        private async Task<string> Passwords(int Lines)
        {
            try
            {
                var RootFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Senhas");
                var RootFile = await RootFolder.GetFileAsync("Senhas.txt");
                var Senhas = File.ReadLines(RootFile.Path); 
                return "fjfj";
            }
            catch
            {
                return "0"; 
            }
        }

        private async void NotifyUser(string v)
        {
            var Dialogo = new ContentDialog()
            {
                Content = v,
                RequestedTheme = ElementTheme.Dark
            };
            Dialogo.CloseButtonText = "Fecha";
            await Dialogo.ShowAsync();
        }
    }
}
