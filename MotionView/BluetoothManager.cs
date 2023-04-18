using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

using KDH_BluetoothPlugin;
using CWJ.Singleton;

public class BluetoothManager : DontDestroyObjectSingleton<BluetoothManager>
{
    private Bluetooth _bluetooth;
    public Bluetooth Bluetooth { get => _bluetooth; }

    [SerializeField] private char _startChar = '$';
    public char StartChar { get => _startChar; set => _startChar = value; }
    [SerializeField] private char _endChar = '&';
    public char EndChar { get => _endChar; set => _endChar = value; }
    private bool _isPacketedData = true;
    public bool IsPacketedData { get => _isPacketedData; set => _isPacketedData = value; }

    private List<BluetoothDevice> _deviceList;
    public List<BluetoothDevice> DeviceList { get => _deviceList; }

    private BluetoothDevice _connectedDevice;
    public BluetoothDevice ConnectedDevice { get => _connectedDevice; }

    private List<byte> _byteBufferList;
    private List<Action> _actionList = new List<Action>();
    private int _actionCount;
    private object _locker = new object();
    private IEnumerator _enableCoroutine;
    private IEnumerator _checkBufferCoroutine;
    private IEnumerator _connectCoroutine;
    private bool _isCheckBuffer = false;

    /// <summary>
    /// CONNECT_STATE_NONE = 0;            // NONE
    /// CONNECT_STATE_LISTEN = 1;          // ����
    /// CONNECT_STATE_CONNECTING = 2;      // �۽�
    /// CONNECT_STATE_CONNECTED = 3;       // ����
    /// </summary>
    private int _connectState;
    public int ConnectState { get => _connectState; }

    /// <summary>
    /// [������� On/Off]
    /// BLUETOOTH_STATE_OFF = 10;
    /// BLUETOOTH_STATE_TURNING_ON = 11;
    /// BLUETOOTH_STATE_ON = 12;
    /// BLUETOOTH_STATE_TURNING_OFF = 13;
    /// [���� On/Off]
    /// BLUETOOTH_ACL_DISCONNECTED = 100;
    /// BLUETOOTH_ACL_CONNECTED = 101;
    /// </summary>
    private int _actionState;
    public int ActionState { get => _actionState; }

    public bool IsEnabled { get => _bluetooth.IsEnabled(); }

    #region Define
    /// <summary>
    /// �������
    /// OnStateChanged�� ���� �Ѿ���� ��
    /// </summary>
    public const int CONNECT_STATE_NONE = 0;            // NONE
    public const int CONNECT_STATE_LISTEN = 1;          // ����
    public const int CONNECT_STATE_CONNECTING = 2;      // �۽�
    public const int CONNECT_STATE_CONNECTED = 3;       // ����

    /// <summary>
    /// ������� On/Off ����
    /// OnActionStateChanged�� ���� �Ѿ���� ��
    /// </summary>
    public const int BLUETOOTH_STATE_OFF = 10;
    public const int BLUETOOTH_STATE_TURNING_ON = 11;
    public const int BLUETOOTH_STATE_ON = 12;
    public const int BLUETOOTH_STATE_TURNING_OFF = 13;

    /// <summary>
    /// ���� On/Off ����
    /// OnActionStateChanged�� ���� �Ѿ���� ��
    /// </summary>
    public const int BLUETOOTH_ACL_DISCONNECTED = 100;
    public const int BLUETOOTH_ACL_CONNECTED = 101;
    #endregion

    #region Callback Method
    public Action<int> OnStateChanged;
    public Action<int> OnActionStateChanged;
    public Action<BluetoothDevice[]> OnReceiveBondedDevices;
    public Action<string> OnSendMessage;
    public Action<string> OnReadPacket;
    public Action OnStartDiscovery;
    public Action<BluetoothDevice> OnFoundDevice;
    public Action OnFinishDiscovery;
    #endregion

    #region LifeCycle Method
    private void Awake()
    {
        Debug.Log("::::::::::: BluetoothManager :::::::::::");

        _deviceList = new List<BluetoothDevice>();
        _byteBufferList = new List<byte>();

        _bluetooth = new Bluetooth();
        _bluetooth.StartPlugin();

        MessageHandler handler = new MessageHandler(
            ReceiveStateChanged, ReceiveActionStateChanged,
            ReceiveSendMessage, ReceiveReadMessage,
            ReceiveBondedDevices,
            ReceiveStartDiscovery, ReceiveFoundDevice, ReceiveFinishDiscovery
            );
        _bluetooth.SetMessageHandler(handler);
        _isCheckBuffer = false;
    }

    private void Start()
    {
        if (IsEnabled)
            AddCallback(OnStateChanged, BLUETOOTH_STATE_ON);

        OnStateChanged += (state) => { _connectState = state; };
        OnActionStateChanged += (state) => { _actionState = state; };
    }

    private void Update()
    {
        lock (_locker)
        {
            while (_actionList.Count > 0)
            {
                _actionList[0]?.Invoke();
                _actionList.RemoveAt(0);
            }
        }
    }

    private void OnDestroy()
    {
        _bluetooth.Destroy();
    }
    #endregion

    #region Receive Callback Method
    private void ReceiveStateChanged(int state)
    {
        //Debug.Log("ReceiveStateChanged =====> " + state.ToString());

        AddCallback(OnStateChanged, state);
    }

    private void ReceiveActionStateChanged(int state)
    {
        //Debug.Log("ReceiveActionStateChanged =====> " + state.ToString());

        AddCallback(OnActionStateChanged, state);
    }

    private void ReceiveSendMessage(string message)
    {
        //Debug.Log("ReceiveSendMessage =====> \n" + message);

        AddCallback(OnSendMessage, message);
    }

    private void ReceiveReadMessage(string message)
    {
        //Debug.Log("ReceiveReadMessage");
        byte[] temp = System.Text.Encoding.UTF8.GetBytes(message);

        _byteBufferList.AddRange(temp);

        if (!_isCheckBuffer)
        {
            if (_checkBufferCoroutine != null)
            {
                StopCoroutine(_checkBufferCoroutine);
                _checkBufferCoroutine = null;
            }
            _checkBufferCoroutine = CheckBufferCoroutine();
            StartCoroutine(_checkBufferCoroutine);
        }

        //Debug.Log("[BlueToothPlugin] - On Read Message : " + message);
    }

    private void ReceiveBondedDevices(string deviceInfo)
    {
        string logText = "ReceiveBondedDevices";

        // ��ϵ� ��� ����
        string[] splitInfo = deviceInfo.Split(',');
        BluetoothDevice[] devices = new BluetoothDevice[splitInfo.Length / 2];

        for (int i = 0; i < devices.Length; i++)
        {
            devices[i] = new BluetoothDevice(splitInfo[i * 2], splitInfo[(i * 2) + 1]);
            logText += $"��ϵ� ��� �̸� : {devices[i].Name} :: �ּ� : {devices[i].Address}";
        }

        //Debug.Log(logText);

        AddCallback(OnReceiveBondedDevices, devices);
    }

    private void ReceiveStartDiscovery()
    {
        //Debug.Log("ReceiveStartDiscovery");

        ClearDeviceList();
        AddCallback(OnStartDiscovery);
    }

    private void ReceiveFinishDiscovery()
    {
        //Debug.Log("ReceiveFinishDiscovery");

        AddCallback(OnFinishDiscovery);
    }

    private void ReceiveFoundDevice(string deviceInfo)
    {
        string[] splitInfo = deviceInfo.Split(',');

        BluetoothDevice device = new BluetoothDevice(splitInfo[0], splitInfo[1]);
        _deviceList.Add(device);

        //Debug.Log($"�˻��� ��� �̸� : {device.Name} :: �ּ� : {device.Address}");

        AddCallback(OnFoundDevice, device);
    }
    #endregion

    private void AddCallback(Action action)
    {
        lock (_locker)
        {
            _actionList.Add(action);
        }
    }

    private void AddCallback<T>(Action<T> action, T param)
    {
        Action temp = () => { action?.Invoke(param); };
        AddCallback(temp);
    }

    private IEnumerator CheckBufferCoroutine()
    {
        _isCheckBuffer = true;
        //Debug.Log("===== ��Ŷ �м� ���� =====");
        do
        {
            while (_byteBufferList.Count > 0 && _byteBufferList[0] != (byte)_startChar)
            {
                //Debug.Log("::::: ��Ŷ ���۹��� �ٸ� :::::" + System.Convert.ToChar(_byteBufferList[0]));
                _byteBufferList.RemoveAt(0);
            }

            int waitCount = 120;
            int endIndex = _byteBufferList.IndexOf((byte)_endChar);
            while (waitCount > 0 && endIndex == -1)
            {
                waitCount--;
                yield return new WaitForSeconds(0.1f);
                endIndex = _byteBufferList.IndexOf((byte)_endChar);
            }

            if (waitCount == 0)
            {
                List<byte> packets = _byteBufferList.GetRange(0, _byteBufferList.Count);
                string message = System.Text.Encoding.Default.GetString(packets.ToArray());
                //Debug.Log("::::: ��Ŷ ���� :::::" + message + " ::::: " + message.Length);
            }
            else
            {
                List<byte> packets = _byteBufferList.GetRange(0, endIndex + 1);
                string message = System.Text.Encoding.Default.GetString(packets.ToArray());

                //Debug.Log("::::: ��Ŷ ���� :::::" + message + " ::::: " + message.Length);
                message = message.Replace(StartChar.ToString(), "").Replace(EndChar.ToString(), "");
                AddCallback(OnReadPacket, message);
                _byteBufferList.RemoveRange(0, endIndex + 1);
            }
        } while (_byteBufferList.Count > 0);

        //Debug.Log("===== ��Ŷ �м� ���� =====");

        _isCheckBuffer = false;
    }

    private IEnumerator ConnectCoroutine(BluetoothDevice device, Action<bool> onFinish)
    {
        bool result = false;
        int connectState = 0;
        int actionState = 0;

        _connectedDevice = null;

        if (_connectState == CONNECT_STATE_CONNECTED)
        {
            Bluetooth.Disconnect();
            yield return new WaitForEndOfFrame();
        }
        
        Action<int> onStateChange = (state) => { connectState = state; };
        Action<int> onActionStateChange = (state) => { actionState = state; };

        OnStateChanged += onStateChange;
        OnActionStateChanged += onActionStateChange;

        bool tryConnect = _bluetooth.Connect($"{device?.Name},{device?.Address}");
        Debug.Log(tryConnect);
        while (tryConnect)
        {
            if (connectState == CONNECT_STATE_CONNECTED && actionState == BLUETOOTH_ACL_CONNECTED)
            {
                _connectedDevice = device;
                result = true;
                //Debug.Log("����");
                //Debug.Log($"ConnectedDevice =====> \n {_connectedDevice.Name}{_connectedDevice.Address}");
                break;
            }

            if (ConnectState == CONNECT_STATE_LISTEN && actionState != BLUETOOTH_ACL_CONNECTED)
            {
                result = false;
                //Debug.Log("����");
                break;
            }
            //Debug.Log("StateInfo\nConnectState ===> " + connectState + "\nActionState ===> " + actionState);
            yield return new WaitForSeconds(0.02f);
        }

        OnStateChanged -= onStateChange;
        OnActionStateChanged -= onActionStateChange;
        onFinish?.Invoke(result);
    }

    private string BytesToString(byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", "");
    }

    private byte GetHex(string value)
    {
        return Convert.ToByte(value, 16);
    }

    private void ClearDeviceList() { _deviceList.Clear(); }

    public void Connect(BluetoothDevice device, Action<bool> onFinish = null)
    {
        if (device == null)
            return;

        if (_connectCoroutine != null)
        {
            StopCoroutine(_connectCoroutine);
            _connectCoroutine = null;
        }
        _connectCoroutine = ConnectCoroutine(device, onFinish);
        StartCoroutine(_connectCoroutine);
    }

    public void Disconnect() { _bluetooth.Disconnect(); }

    public void SendPacket(string message)
    {
        string packet = _startChar + message + _endChar;
        //Debug.Log("Send Packet =====> " + packet);
        _bluetooth.SendMessage(packet);
    }

    public void EnableBluetooth(Action<bool> onFinish)
    {
        if (_enableCoroutine != null)
        {
            StopCoroutine(_enableCoroutine);
            _enableCoroutine = null;

            //Debug.Log($"�ڷ�ƾ ��������");
        }

        _enableCoroutine = EnableBluetoothCoroutine(onFinish);
        StartCoroutine(_enableCoroutine);
    }

    private IEnumerator EnableBluetoothCoroutine(Action<bool> onFinish)
    {
        //Debug.Log($"�ڷ�ƾ ����");
        bool result = true;

#if PLATFORM_ANDROID
        //Debug.Log($"�ڷ�ƾ --> ���ѿ�û");
        Bluetooth.PERMISSION_STATE permissionState = Bluetooth.PERMISSION_STATE.DENIED;
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            PermissionCallbacks callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += (state) =>
            {
                //Debug.Log($"���ѿ�û ���\nPermissionDenied : {state}");
                permissionState = Bluetooth.PERMISSION_STATE.DENIED;
            };
            callbacks.PermissionGranted += (state) =>
            {
                //Debug.Log($"���ѿ�û ���\nPermissionGranted : {state}");
                permissionState = Bluetooth.PERMISSION_STATE.GRANTED;
            };
            callbacks.PermissionDeniedAndDontAskAgain += (state) =>
            {
                //Debug.Log($"���ѿ�û ���\nPermissionDeniedAndDontAskAgain : {state}");
                permissionState = Bluetooth.PERMISSION_STATE.DENIED_DONTASK;
            };
            Permission.RequestUserPermission(Permission.FineLocation, callbacks);

            // ��� ���
            yield return new WaitForSeconds(0.2f);
            //Debug.Log($"�ڷ�ƾ --> ���ѿ�û ---> �����");
            // ��Ŀ�� Ȯ�� (��Ŀ���� ����Ƽ�� ���ƿö����� ���)
            yield return new WaitUntil(() => Application.isFocused == true);

            switch (permissionState)
            {
                case Bluetooth.PERMISSION_STATE.DENIED:
                    //Debug.Log($"�ڷ�ƾ --> ���ѿ�û ---> �ź�");
                    result = false;
                    goto FINISH;
                case Bluetooth.PERMISSION_STATE.GRANTED:
                    //Debug.Log($"�ڷ�ƾ --> ���ѿ�û ---> ���");
                    break;
                case Bluetooth.PERMISSION_STATE.DENIED_DONTASK:
                    //Debug.Log($"�ڷ�ƾ --> ���ѿ�û ---> �ź�(�ٽù�������)");
                    _bluetooth.ShowLongToast("��ġ ���� �׼��� ���� ��û�� �źεǾ����ϴ�.\n�� �������� ������ Ȯ�����ּ���.");

                    // ���ѿ�û�� �źεǾ� 1�� ����� App Setting���� �Ѿ
                    yield return new WaitForSeconds(1f);
                    _bluetooth.ShowAppSetting();
                    // App Setting ȭ�� ȣ�� �� 0.2�� ���
                    yield return new WaitForSeconds(0.2f);
                    // ��Ŀ�� Ȯ��
                    yield return new WaitUntil(() => Application.isFocused == true);
                    // �������� ��Ȯ��
                    if (!_bluetooth.CheckPermission())
                    {
                        // ������ ������ �źεǾ��ٸ� Finish
                        result = false;
                        goto FINISH;
                    }
                    break;
            }
            yield return new WaitForSeconds(0.2f);
        }
#endif

        if (!IsEnabled)
        {
            // ������� ���� ��û
            _bluetooth.RequestEnable();
            //Debug.Log($"�ڷ�ƾ --> ���������û");
            yield return new WaitForSeconds(0.2f);
            //Debug.Log($"�ڷ�ƾ --> ���������û ---> �����");
            yield return new WaitUntil(() => Application.isFocused == true);
            if (!IsEnabled)
            {
                //Debug.Log($"�ڷ�ƾ --> ���������û ---> �ź�");
                result = false;
                goto FINISH;
            }

            //Debug.Log($"�ڷ�ƾ --> ���������û ---> ���");
            yield return new WaitForSeconds(0.2f);
        }

    FINISH:
        //Debug.Log($"�ڷ�ƾ ����");
        onFinish?.Invoke(result);
    }
}