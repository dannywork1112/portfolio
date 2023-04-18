using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

namespace KDH_BluetoothPlugin
{
    public class BluetoothDevice
    {
        public BluetoothDevice(string name, string address)
        {
            Name = name;
            Address = address;
        }
        public string Name;
        public string Address;
    }

    public class Bluetooth
    {
        public enum PERMISSION_STATE
        {
            DENIED = -1,
            GRANTED = 0,
            DENIED_DONTASK = 1,
        }

        private AndroidJavaObject _pluginObject;
        private AndroidJavaObject _currentActivity;

        public void StartPlugin()
        {
            Debug.Log("::::::::::: Bluetooth :::::::::::");
            _pluginObject = new AndroidJavaObject("com.kdh.BluetoothPlugin.BluetoothActivity");
#if !UNITY_EDITOR && UNITY_ANDROID
            try
            {
                var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                _currentActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
                Call("StartPlugin", _currentActivity);
                RegisterBluetoothReceiver();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
#endif
        }

        private bool Call(string methodName, params object[] args)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            try
            {
                _pluginObject.Call(methodName, args);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
#endif
            return false;
        }

        private bool Call<T>(string methodName, out T reward, params object[] args)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            try
            {
                reward = _pluginObject.Call<T>(methodName, args);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
#endif
            reward = default(T);
            return false;
        }

        public void Destroy()
        {
            Call("OnDestroy");
        }

        public void ShowShortToast(string message)
        {
            Call("ShowShortToast", message);
        }

        public void ShowLongToast(string message)
        {
            Call("ShowLongToast", message);
        }

        public void SetupPlugin()
        {
            Call("SetupPlugin");
        }

        public void SetMessageHandler(MessageHandler handler)
        {
            Call("SetMessageHandler", handler);
        }

        public bool CheckPermission()
        {
            Call("CheckPermission", out bool isDone);
            return isDone;
        }

        public void RequestPermission()
        {
            Call("RequestPermission");
        }

        public void ShouldShowRequestPermissionRationale()
        {
            Call("ShouldShowRequestPermissionRationale");
        }

        public bool IsEnabled()
        {
            Call("IsEnabled", out bool isDone);
            //Debug.Log("Bluetooth Enabled is " + isDone);
            return isDone;
        }

        public void Enable()
        {
            Call("Enable");
        }

        public void Disable()
        {
            Call("Disable");
        }

        public void RequestEnable()
        {
            Call("RequestEnable");
        }

        public void RegisterBluetoothReceiver()
        {
            Call("RegisterBluetoothReceiver");
        }

        public void UnregisterBluetoothReceiver()
        {
            Call("UnregisterBluetoothReceiver");
        }

        public void RequestBondedDevices()
        {
            Call("GetBondedDevices");
        }

        public void RequestDiscoverable()
        {
            Call("RequestDiscoverable");
        }

        public void Discovery()
        {
            Call("Discovery");
        }

        public bool Connect(string target)
        {
            return Call("Connect", target);
        }

        public void Disconnect()
        {
            Call("StopThread");
        }

        public int GetState()
        {
            int state;
            Call("GetState", out state);
            return state;
        }

        public bool SendMessage(string message)
        {
            Call("SendMessage", out bool isDone, message);
            return isDone;
        }

        public void ShowAppSetting()
        {
            try
            {
#if !UNITY_EDITOR && UNITY_ANDROID

                if (_currentActivity == null)
                    return;

                string packageName = _currentActivity.Call<string>("getPackageName");

                using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                using (AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", packageName, null))
                using (var intentObject = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS", uriObject))
                {
                    intentObject.Call<AndroidJavaObject>("addCategory", "android.intent.category.DEFAULT");
                    intentObject.Call<AndroidJavaObject>("setFlags", 0x10000000);
                    _currentActivity.Call("startActivity", intentObject);
                }
#endif
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}