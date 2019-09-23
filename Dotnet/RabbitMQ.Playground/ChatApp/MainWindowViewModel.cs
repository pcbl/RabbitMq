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
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        public MainWindowViewModel()
        {
            _isConnected = false;
            Alias = Environment.UserName;
            
            Connect = new RelayCommand(ConnectAction);
            Send = new RelayCommand(SendAction);
            Channels = Enum.GetValues(typeof(MessageChannel)).Cast<MessageChannel>();
            Channel = MessageChannel.Business;
            ChatManager = new ChatManager();
        }

        public ChatManager ChatManager { get; set; }

        private bool _isConnected;

        public IEnumerable<MessageChannel> Channels { get; set; }

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

        public bool IsDisconnected
        {
            get { return !_isConnected; }
        }

        public string Alias { get; set; }

        private string _messageToSend;

        public string MessageToSend
        {
            get { return _messageToSend; }
            set {
                _messageToSend = value;
                RaisePropertyChanged("MessageToSend");
            }
        }


        public ICommand Connect { get; set; }

        void ConnectAction()
        {
            IsConnected = !IsConnected;
            if(IsConnected)
            {                
                ChatManager.ListenChannel(Channel);
                ChatManager.Send(new ChatMessage("Just connected!", Channel, Alias));
            }
            else
            {
                ChatManager.StopChannel(Channel);
            }

        }
        void SendAction()
        {
            ChatManager.Send(new ChatMessage(MessageToSend, Channel, Alias));
            MessageToSend = string.Empty;
        }

        public void Dispose()
        {
            ChatManager.Dispose();
        }

        public ICommand Send { get; set; }

        public MessageChannel Channel { get; set; }


    }
}
