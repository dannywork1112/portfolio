package com.kdh.BluetoothPlugin;

import android.Manifest;
import android.app.Activity;
import android.app.Application;
import android.app.Fragment;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.PackageManager;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.provider.Settings;
import android.util.Log;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;

//import com.unity3d.player.UnityPlayer;

import java.util.ArrayList;
import java.util.List;
import java.util.Set;

public class BluetoothActivity
{
    private final String TAG = ":: KDH_Activity_Log ::";
    private static final String TARGET = "BluetoothModel";

    public static final int MESSAGE_STATE_CHANGE = 1;
    public static final int MESSAGE_READ = 2;
    public static final int MESSAGE_WRITE = 3;
    public static final int MESSAGE_DEVICE_NAME = 4;
    public static final int MESSAGE_TOAST = 5;

    private final int MULTIPLE_PERMISSIONS = 100;

    private Activity _activity;
    private BluetoothAdapter _adapter;
    private BluetoothService _bluetoothService;
    private StringBuffer _outStringBuffer;

    private String[] _permissions = {
            Manifest.permission.BLUETOOTH,
            Manifest.permission.BLUETOOTH_ADMIN,
            Manifest.permission.ACCESS_COARSE_LOCATION,
            Manifest.permission.ACCESS_FINE_LOCATION,
    };
    private List _permissionList;

    private String _connectedDeviceName = null;

    private IBluetoothMessageHandler _messageHandler;

    //private BluetoothStateBroadcastReceive _receiver;

    // Handler
    private final Handler _handler = new Handler(new Handler.Callback() {
        //public boolean handleMessage(@NonNull Message msg) {
        @Override
        public boolean handleMessage(Message msg) {
            switch(msg.what) {
                case MESSAGE_STATE_CHANGE:
                    Log.d(TAG, "MESSAGE_STATE_CHANGE ==> " + String.valueOf(msg.arg1));

                    if (_messageHandler != null)
                        _messageHandler.OnStateChanged(msg.arg1);

                    //UnityPlayer.UnitySendMessage(TARGET, "ReceiveStateChanged", Integer.toString(msg.arg1));
                    break;
                case MESSAGE_READ:
                    byte[] readBuf = (byte[])msg.obj;
                    String readMessage = new String(readBuf, 0, msg.arg1);
                    Log.d(TAG, "MESSAGE_READ ==> " + msg.arg1);
                    Log.d(TAG, "MESSAGE_READ ==> " + new String(readBuf));

                    if (_messageHandler != null)
                        _messageHandler.OnReadMessage(readMessage);

                    //UnityPlayer.UnitySendMessage(TARGET, "ReceiveReadMessage", readMessage);
                    break;
                case MESSAGE_WRITE:
                    byte[] writeBuf = (byte[])msg.obj;
                    String writeMessage = new String(writeBuf);
                    Log.d(TAG, "MESSAGE_WRITE ==> " + writeMessage);

                    if (_messageHandler != null)
                        _messageHandler.OnSendMessage(writeMessage);

                    //UnityPlayer.UnitySendMessage(TARGET, "ReceiveSendMessage", writeMessage);
                    break;
                case MESSAGE_DEVICE_NAME:
                    _connectedDeviceName = msg.getData().getString("device_name");
                    Log.d(TAG, "MESSAGE_DEVICE_NAME ==> " + _connectedDeviceName);
                    ShowShortToast("Connected to " + _connectedDeviceName);
                    break;
                case MESSAGE_TOAST:
                    ShowLongToast(msg.getData().getString("toast"));
                    break;
            }
            return true;
        }
    });

    private BroadcastReceiver _receiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            final String action = intent.getAction();
            Log.d(TAG, "=====onReceive=====");
            Log.d(TAG, "Bluetooth action" + action);
            switch (action) {
                case BluetoothDevice.ACTION_ACL_DISCONNECTED:
                    if (_messageHandler != null)
                        _messageHandler.OnActionStateChange(100);
                    break;
                case BluetoothDevice.ACTION_ACL_CONNECTED:
                    if (_messageHandler != null)
                        _messageHandler.OnActionStateChange(101);
                    break;

                case BluetoothAdapter.ACTION_STATE_CHANGED: //블루투스의 연결 상태 변경
                    Log.d(TAG, "ACTION_STATE_CHANGED! " + action);

                    final int state = intent.getIntExtra(BluetoothAdapter.EXTRA_STATE, BluetoothAdapter.ERROR);
                    switch (state) {
                        case BluetoothAdapter.STATE_OFF:
                            Log.d(TAG, "블루투스 Off");

                            if (_messageHandler != null)
                                _messageHandler.OnActionStateChange(BluetoothAdapter.STATE_OFF);

                            //UnitySendMessage("ReceiveActionStateChanged", Integer.toString(BluetoothAdapter.STATE_OFF));
                            break;
                        case BluetoothAdapter.STATE_ON:
                            Log.d(TAG, "블루투스 On");

                            if (_messageHandler != null)
                                _messageHandler.OnActionStateChange(BluetoothAdapter.STATE_ON);

                            //UnitySendMessage("ReceiveActionStateChanged", Integer.toString(BluetoothAdapter.STATE_ON));
                            break;
                        case BluetoothAdapter.STATE_TURNING_OFF:
                            Log.d(TAG, "블루투스 Turning Off");

                            if (_messageHandler != null)
                                _messageHandler.OnActionStateChange(BluetoothAdapter.STATE_TURNING_OFF);

                            //UnitySendMessage("ReceiveActionStateChanged", Integer.toString(BluetoothAdapter.STATE_TURNING_OFF));
                            break;
                        case BluetoothAdapter.STATE_TURNING_ON:
                            Log.d(TAG, "블루투스 Turning On");

                            if (_messageHandler != null)
                                _messageHandler.OnActionStateChange(BluetoothAdapter.STATE_TURNING_ON);

                            //UnitySendMessage("ReceiveActionStateChanged", Integer.toString(BluetoothAdapter.STATE_TURNING_ON));
                            break;
                    }
                    break;
                case BluetoothAdapter.ACTION_DISCOVERY_STARTED: //블루투스 기기 검색 시작
                    Log.d(TAG, "블루투스 기기 검색 시작!!");

                    if (_messageHandler != null)
                        _messageHandler.OnDiscoveryStart();

                    //UnitySendMessage("ReceiveStartDiscovery", "");
                    break;
                case BluetoothDevice.ACTION_FOUND:  //블루투스 기기 검색 됨, 블루투스 기기가 근처에서 검색될 때마다 수행됨
                    final BluetoothDevice device = intent.getParcelableExtra(BluetoothDevice.EXTRA_DEVICE);
                    Log.d(TAG, "검색된 디바이스 :: 이름 : " + device.getName() + " 주소 : " + device.getAddress());

                    if (_messageHandler != null)
                        _messageHandler.OnDeviceFound(device.getName() + "," + device.getAddress());

                    //UnitySendMessage("ReceiveFoundDevice", device.getName() + "," + device.getAddress());
                    break;
                case BluetoothAdapter.ACTION_DISCOVERY_FINISHED:    //블루투스 기기 검색 종료
                    Log.d(TAG, "블루투스 기기 검색 종료!!");

                    if (_messageHandler != null)
                        _messageHandler.OnDiscoveryFinish();

                    //UnitySendMessage("ReceiveFinishDiscovery", "");
                    break;
                case BluetoothDevice.ACTION_PAIRING_REQUEST:
                    break;
            }

        }
    };

    private void OnDestroy()
    {
        UnregisterBluetoothReceiver();
        if (_bluetoothService != null)
            _bluetoothService.stop();
        if (_adapter != null && _adapter.isDiscovering())
            _adapter.cancelDiscovery();
    }

    private void ShowToast(String msg, int duration)
    {
        _activity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                Toast.makeText(_activity, msg, duration).show();
            }
        });
    }

    private void ShowShortToast(String msg)
    {
        ShowToast(msg, Toast.LENGTH_SHORT);
    }

    private void ShowLongToast(String msg)
    {
        ShowToast(msg, Toast.LENGTH_LONG);
    }

    private int GetState()
    {
        int state = BluetoothService.STATE_NONE;
        if (_bluetoothService != null)
            state= _bluetoothService.GetState();
        return state;
    }

    /*
    private void UnitySendMessage(String method, String msg)
    {
        UnityPlayer.UnitySendMessage(TARGET, method, msg);
    }
    */

    private void StartPlugin(Activity activity)
    {
        Log.d(TAG, "=====StartPlugin=====");
        if(Looper.myLooper() == null)
        {
            Looper.prepare();
        }

        _activity = activity;
        SetupPlugin();
    }

    private void SetupPlugin()
    {
        Log.d(TAG, "=====SetupPlugin=====");
        _adapter = BluetoothAdapter.getDefaultAdapter();
        //_activity = GetActivity();
        if (_adapter == null)
        {
            ShowShortToast("블루투스를 지원하지 않는 디바이스입니다.");
            return;
        }

        if (_bluetoothService == null)
        {
            _bluetoothService = new BluetoothService(_handler);
            _outStringBuffer = new StringBuffer();
        }

        _permissionList = new ArrayList();
    }

    private void SetMessageHandler(IBluetoothMessageHandler messageHandler)
    {
        _messageHandler = messageHandler;
    }

    //private void

    private void RegisterBluetoothReceiver()
    {
        Log.d(TAG, "=====RegisterBluetoothReceiver=====");
        IntentFilter intentFilter = new IntentFilter();
        intentFilter.addAction(BluetoothAdapter.ACTION_STATE_CHANGED); //BluetoothAdapter.ACTION_STATE_CHANGED : 블루투스 상태변화 액션
        intentFilter.addAction(BluetoothAdapter.ACTION_CONNECTION_STATE_CHANGED);
        intentFilter.addAction(BluetoothDevice.ACTION_ACL_DISCONNECTED); //연결 끊김 확인
        intentFilter.addAction(BluetoothDevice.ACTION_ACL_CONNECTED); //연결 확인
        intentFilter.addAction(BluetoothDevice.ACTION_BOND_STATE_CHANGED);
        intentFilter.addAction(BluetoothDevice.ACTION_FOUND);    //기기 검색됨
        intentFilter.addAction(BluetoothAdapter.ACTION_DISCOVERY_STARTED);   //기기 검색 시작
        intentFilter.addAction(BluetoothAdapter.ACTION_DISCOVERY_FINISHED);  //기기 검색 종료
        intentFilter.addAction(BluetoothDevice.ACTION_PAIRING_REQUEST);
        _activity.registerReceiver(_receiver, intentFilter);
    }

    private void UnregisterBluetoothReceiver()
    {
        Log.d(TAG, "=====UnregisterBluetoothReceiver=====");
        if (_receiver != null)
            _activity.unregisterReceiver(_receiver);
    }

    private boolean CheckPermission()
    {
        int result;
        _permissionList.clear();
        for (String permission : _permissions)
        {
            result = _activity.checkSelfPermission(permission);
            if (result != PackageManager.PERMISSION_GRANTED)
                _permissionList.add(permission);
            Log.d(TAG, "권한요청 결과 : " + permission + " - " + result + " ..... " + _permissionList.size());
        }

        // 남은 권한이 있을 경우
        if (!_permissionList.isEmpty())
            return false;
        return true;
    }

    private void RequestPermission()
    {
        Log.d(TAG, "=====RequestPermission=====");
        if (!CheckPermission())
            _activity.requestPermissions((String[])_permissionList.toArray(new String[_permissionList.size()]), MULTIPLE_PERMISSIONS);
    }

    private void ShowAppSetting()
    {
        Intent intent = new Intent(Settings.ACTION_APPLICATION_SETTINGS);
        intent.addCategory(Intent.CATEGORY_DEFAULT);
        intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        _activity.startActivity(intent);
    }

    private boolean IsEnabled() { return _adapter.isEnabled(); }

    private void Enable()
    {
        if (IsEnabled())
            return;
        _adapter.enable();
        return;
    }

    private void Disable()
    {
        if (!IsEnabled())
            return;
        else
        {
            if (_adapter != null)
            {
                _adapter.cancelDiscovery();
                Log.d(TAG, "=====검색취소=====");
            }
            _adapter.disable();
        }
    }

    private void RequestEnable()
    {
        Log.d(TAG, "=====RequestEnable=====");
        Intent intent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
        _activity.startActivity(intent);
    }

    private void GetBondedDevices()
    {
        Log.d(TAG, "=====GetBondedDevices=====");
        Set<BluetoothDevice> pairedDevices = _adapter.getBondedDevices();
        if (pairedDevices.size() > 0)
        {
            for (BluetoothDevice device : pairedDevices)
            {
                if (_messageHandler != null)
                    _messageHandler.OnReceiveBondedDevices(device.getName() + "," + device.getAddress());

                //UnitySendMessage("ReceiveBondedDevices", device.getName() + "," + device.getAddress());
                //Log.d(TAG, "===> " + device.getName() + "," + device.getAddress());
            }
        }
    }

    private void RequestDiscoverable()
    {
        Log.d(TAG, "=====RequestDiscoverable=====");
        if (IsEnabled() && _adapter.getScanMode() != BluetoothAdapter.SCAN_MODE_CONNECTABLE_DISCOVERABLE)
        {
            Intent discoverableIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_DISCOVERABLE);
            discoverableIntent.putExtra(BluetoothAdapter.EXTRA_DISCOVERABLE_DURATION, 120);
            _activity.startActivity(discoverableIntent);
        }
    }

    private void Discovery()
    {
        Log.d(TAG, "=====Discovery=====");
        if (_adapter.isDiscovering())
            _adapter.cancelDiscovery();
        _adapter.startDiscovery();
    }

    private void StopThread()
    {
        Log.d(TAG, "=====StopThread=====");
        if (_bluetoothService != null)
        {
            _bluetoothService.stop();
            _bluetoothService = null;
        }

        SetupPlugin();
    }

    private void Connect(String target)
    {
        Log.d(TAG, "=====Connect=====");
        if (_adapter.isDiscovering())
            _adapter.cancelDiscovery();

        String[] deviceInfo = target.split(",");
        Log.d(TAG, "연결기기 정보 : " + target);
        _connectedDeviceName = deviceInfo[0];
        BluetoothDevice device = _adapter.getRemoteDevice(deviceInfo[1]);

        _bluetoothService.connect(device);
    }

    public boolean IsConnected()
    {
        return _bluetoothService.GetState() == BluetoothService.STATE_CONNECTING;
    }

    private boolean SendMessage(String message)
    {
        if (!_adapter.isEnabled())
            return false;
        else if (_bluetoothService.GetState() != BluetoothService.STATE_CONNECTED)
            return false;
        else
        {
            if (message.length() > 0)
            {
                byte[] bytes = message.getBytes();
                _bluetoothService.write(bytes);
                _outStringBuffer.setLength(0);
                Log.d(TAG, "SendMessage ===> " + message);
            }
            return true;
        }
    }

/*
    private Activity GetActivity()
    {
        if(null == _activity)
        {
            try
            {
                Class<?> classtype = Class.forName("com.unity3d.player.UnityPlayer");
                Activity activity = (Activity) classtype.getDeclaredField("currentActivity").get(classtype);
                _activity = UnityPlayer.currentActivity;
            }
            catch (ClassNotFoundException e){
                e.printStackTrace();
            }catch (IllegalAccessException e){
                e.printStackTrace();
            }catch (NoSuchFieldException e){
                e.printStackTrace();
            }
        }
        return _activity;
    }
 */

    /*
    class BluetoothStateBroadcastReceive extends BroadcastReceiver
    {
        @Override
        public void onReceive(Context context, Intent intent) {
            String action = intent.getAction();
            Log.d(TAG, "Context : " + context + " --- " + "action : " + action);
            BluetoothDevice device = intent.getParcelableExtra(BluetoothDevice.EXTRA_DEVICE);
            switch (action)
            {
                case BluetoothDevice.ACTION_ACL_CONNECTED:
                    Log.d(TAG, "onReceive: "+"블루투스장치 => " + device.getName() + "연결됨");
                    break;
                case BluetoothDevice.ACTION_ACL_DISCONNECTED:
                    Log.d(TAG, "onReceive: "+"블루투스장치 => " + device.getName() + "연결끊김");
                    break;
                case BluetoothAdapter.ACTION_STATE_CHANGED:
                    int bluetoothState = intent.getIntExtra(BluetoothAdapter.EXTRA_STATE, 0);
                    switch (bluetoothState)
                    {
                        case BluetoothAdapter.STATE_OFF:
                            Log.d(TAG, "onReceive: "+"블루투스가 꺼져있습니다." );
                            _bluetoothStateInterface.onBluetoothStateOFF();
                            break;
                        case BluetoothAdapter.STATE_ON:
                            Log.i(TAG, "onReceive: "+"블루투스가 켜져 있습니다.");
                            _bluetoothStateInterface.onBluetoothStateON();
                            break;
                    }
                    break;
            }
        }
    }*/
}
