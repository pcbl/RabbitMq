using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace ChatApp
{
    /// <summary>
    /// View Model to Bind to the MainWindow
    /// </summary>
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        public MainWindowViewModel()
        {
            _isConnected = false;
            Alias = Environment.UserName;
            
            Connect = new RelayCommand(ConnectAction);
            Send = new RelayCommand(SendAction);
            Channels = Enum.GetValues(typeof(MessageChannel)).Cast<MessageChannel>();
            CurrentChannel = MessageChannel.Business;
            ChatManager = new ChatManager();
        }

        /// <summary>
        /// Chat manager, handles all communication with RabbitMQ
        /// </summary>
        public ChatManager ChatManager { get; set; }

        /// <summary>
        /// Channels available to post message
        /// </summary>
        public IEnumerable<MessageChannel> Channels { get; set; }

        /// <summary>
        /// Current Channel to Post Messages to
        /// </summary>
        public MessageChannel CurrentChannel { get; set; }

        /// <summary>
        /// Indicates if we are connected to the RabbitMQ
        /// </summary>
        private bool _isConnected;

        /// <summary>
        /// Indicates if we are connected to the RabbitMQ
        /// </summary>
        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                RaisePropertyChanged("IsConnected");
                RaisePropertyChanged("IsDisconnected");
            }
        }

        /// <summary>
        /// Indicates if we are Disconnected from WPF
        /// </summary>
        public bool IsDisconnected
        {
            get { return !_isConnected; }
        }

        /// <summary>
        /// Alias to USe when posting messages
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Message To send
        /// </summary>
        private string _messageToSend;

        /// <summary>
        /// Message To send
        /// </summary>
        public string MessageToSend
        {
            get { return _messageToSend; }
            set {
                _messageToSend = value;
                RaisePropertyChanged("MessageToSend");
            }
        }

        /// <summary>
        /// Connect Command
        /// </summary>
        public ICommand Connect { get; set; }

        /// <summary>
        /// Connect Command Action
        /// </summary>
        void ConnectAction()
        {
            IsConnected = !IsConnected;            
            if(IsConnected)
            {                
                //When connected, we add a listener on the target channel
                ChatManager.StartChannelListening(CurrentChannel);
                //And we send a message informing we are connected
                ChatManager.SendChatMessage(new ChatMessage("Just connected!", CurrentChannel, Alias));
            }
            else
            {
                //When disconnecting we just Stop Listening
                ChatManager.StopChannelListening(CurrentChannel);
            }
        }

        /// <summary>
        /// Send Command
        /// </summary>
        public ICommand Send { get; set; }

        /// <summary>
        /// Send Command Action
        /// </summary>
        void SendAction()
        {
            ChatManager.SendChatMessage(new ChatMessage(MessageToSend, CurrentChannel, Alias));
            MessageToSend = string.Empty;
        }

        /// <summary>
        /// When Disposing, freeing resources up
        /// </summary>
        public void Dispose()
        {
            ChatManager.Dispose();
        }
    }
}
