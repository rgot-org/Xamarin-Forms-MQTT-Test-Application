using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AppMQTT
{
    public partial class MainPage : ContentPage
    {
        // Create a new MQTT client. voir https://github.com/chkr1011/MQTTnet/wiki/Client#preparation
        MqttFactory factory = new MqttFactory();
        IMqttClient mqttClient;
        public MainPage()
        {
            InitializeComponent();
            mqttClient = factory.CreateMqttClient();
            // declaration des listener
            mqttClient.UseConnectedHandler(ConnectedHandler); // une fois que le client est connecte appelle la fonction  "ConnectedHandler"
            mqttClient.UseApplicationMessageReceivedHandler(MessageReceivedHandler);// à la reception d'un message appelle la fonction "MessageReceivedHandler"
            mqttClient.UseDisconnectedHandler(DisconnectedHandler);// à la deconnexion apelle "DisconnectedHandler"
            Task.Run(async () => { await Connect(); }); // connexion initiale

        }

        public async Task Connect()
        {
            // voir https://github.com/chkr1011/MQTTnet/wiki/Client#tcp-connection 
            var options = new MqttClientOptionsBuilder() 
            .WithTcpServer("broker.hivemq.com", 1883)
            .WithCleanSession()
            .Build();
            try // essaye
            {
                // voir https://github.com/chkr1011/MQTTnet/wiki/Client#connecting
                await mqttClient.ConnectAsync(options, CancellationToken.None);
            }
            catch (Exception ex) // en cas d'echec
            {
                Console.WriteLine(ex);                
            }
        }
 
        private async void publishMessage(String value)
        {
            // voir https://github.com/chkr1011/MQTTnet/wiki/Client#publishing-messages
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("test/xamarin/publish")
                .WithPayload(value)
                .WithExactlyOnceQoS()
                .WithRetainFlag()
                .Build();
            await mqttClient.PublishAsync(message, CancellationToken.None); // Since 3.0.5 with CancellationToken
        }
        /*******************************************************************************/
        /*                     gestion des événements de l'IHM                         */
        /*******************************************************************************/
        private void btn_Publier_Clicked(object sender, EventArgs e)
        {
            publishMessage(Entry_MessageToSend.Text);
        }

        /*******************************************************************************/
        /*                   zone des handler (gestionnaires)                          */
        /*******************************************************************************/

        // à la deconnexion
        private async Task DisconnectedHandler(MqttClientDisconnectedEventArgs arg)
        {
            // voir https://github.com/chkr1011/MQTTnet/wiki/Client#reconnecting
            await Task.Delay(TimeSpan.FromSeconds(5));
            await Connect();
        }
        // à la reception d'un message
        private void MessageReceivedHandler(MqttApplicationMessageReceivedEventArgs arg)
        {
            // voir https://github.com/chkr1011/MQTTnet/wiki/Client#consuming-messages
            //recupération le la payload et conversion byte[]-> string
            String str = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
            //mise à jour du label sur l'écran principal
            Device.BeginInvokeOnMainThread(() =>
            {
                Label_MessageRecu.Text = str;
            });
        }
        // à la connexion au broker
        private async Task ConnectedHandler(MqttClientConnectedEventArgs arg)
        {
            await Device.InvokeOnMainThreadAsync(() =>
            {
                status.Text = "Connecté"; // mise à jour du label
            });
            // Subscribe to a topic
            // voir https://github.com/chkr1011/MQTTnet/wiki/Client#subscribing-to-a-topic
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("test/xamarin/abonne").Build());
        }
    }
}
