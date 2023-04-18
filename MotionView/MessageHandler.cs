using System;
using UnityEngine;

namespace KDH_BluetoothPlugin
{
    public class MessageHandler : AndroidJavaProxy
    {
        private Action<int> _onStateChanged;
        private Action<int> _onActionStateChange;
        private Action<string> _onSendMessage;
        private Action<string> _onReadMessage;
        private Action<string> _onReceiveBondedDevices;
        private Action _onDiscoveryStart;
        private Action<string> _onDeviceFound;
        private Action _onDiscoveryFinish;

        public MessageHandler(
            Action<int> onStateChanged, Action<int> onActionStateChange,
            Action<string> onSendMessage, Action<string> onReadMessage,
            Action<string> onReceiveBondedDevices,
            Action onDiscoveryStart, Action<string> onDeviceFound, Action onDiscoveryFinish
            ) : base("com.kdh.BluetoothPlugin.IBluetoothMessageHandler")
        {
            _onStateChanged = onStateChanged;
            _onActionStateChange = onActionStateChange;
            _onSendMessage = onSendMessage;
            _onReadMessage = onReadMessage;
            _onReceiveBondedDevices = onReceiveBondedDevices;
            _onDiscoveryStart = onDiscoveryStart;
            _onDeviceFound = onDeviceFound;
            _onDiscoveryFinish = onDiscoveryFinish;
        }

        public void OnStateChanged(int state)
        {
            _onStateChanged?.Invoke(state);
        }

        public void OnActionStateChange(int state)
        {
            _onActionStateChange?.Invoke(state);
        }

        public void OnSendMessage(string message)
        {
            _onSendMessage?.Invoke(message);
        }

        public void OnReadMessage(string message)
        {
            _onReadMessage?.Invoke(message);
        }

        public void OnReceiveBondedDevices(string deviceInfo)
        {
            _onReceiveBondedDevices?.Invoke(deviceInfo);
        }

        public void OnDiscoveryStart()
        {
            _onDiscoveryStart?.Invoke();
        }

        public void OnDeviceFound(string deviceInfo)
        {
            _onDeviceFound?.Invoke(deviceInfo);
        }

        public void OnDiscoveryFinish()
        {
            _onDiscoveryFinish?.Invoke();
        }
    }
}