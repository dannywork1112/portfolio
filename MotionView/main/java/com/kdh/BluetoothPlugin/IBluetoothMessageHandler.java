package com.kdh.BluetoothPlugin;

public interface IBluetoothMessageHandler
{
    void OnStateChanged(int state);
    void OnActionStateChange(int state);
    void OnSendMessage(String message);
    void OnReadMessage(String message);
    void OnReceiveBondedDevices(String deviceInfo);
    void OnDiscoveryStart();
    void OnDeviceFound(String deviceInfo);
    void OnDiscoveryFinish();
}
