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
    /// CONNECT_STATE_LISTEN = 1;          // 수신
    /// CONNECT_STATE_CONNECTING = 2;      // 송신
    /// CONNECT_STATE_CONNECTED = 3;       // 연결
    /// </summary>
    private int _connectState;
    public int ConnectState { get => _connectState; }

    /// <summary>
    /// [블루투스 On/Off]
    /// BLUETOOTH_STATE_OFF = 10;
    /// BLUETOOTH_STATE_TURNING_ON = 11;
    /// BLUETOOTH_STATE_ON = 12;
    /// BLUETOOTH_STATE_TURNING_OFF = 13;
    /// [연결 On/Off]
    /// BLUETOOTH_ACL_DISCONNECTED = 100;
    /// BLUETOOTH_ACL_CONNECTED = 101;
    /// </summary>
    private int _actionState;
    public int ActionState { get => _actionState; }

    public bool IsEnabled { get => _bluetooth.IsEnabled(); }

    #region Define
    /// <summary>
    /// 연결상태
    /// OnStateChanged를 통해 넘어오는 값
    /// </summary>
    public const int CONNECT_STATE_NONE = 0;            // NONE
    public const int CONNECT_STATE_LISTEN = 1;          // 수신
    public const int CONNECT_STATE_CONNECTING = 2;      // 송신
    public const int CONNECT_STATE_CONNECTED = 3;       // 연결

    /// <summary>
    /// 블루투스 On/Off 상태
    /// OnActionStateChanged를 통해 넘어오는 값
    /// </summary>
    public const int BLUETOOTH_STATE_OFF = 10;
    public const int BLUETOOTH_STATE_TURNING_ON = 11;
    public const int BLUETOOTH_STATE_ON = 12;
    public const int BLUETOOTH_STATE_TURNING_OFF = 13;

    /// <summary>
    /// 연결 On/Off 상태
    /// OnActionStateChanged를 통해 넘어오는 값
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

        // 등록된 기기 받음
        string[] splitInfo = deviceInfo.Split(',');
        BluetoothDevice[] devices = new BluetoothDevice[splitInfo.Length / 2];

        for (int i = 0; i < devices.Length; i++)
        {
            devices[i] = new BluetoothDevice(splitInfo[i * 2], splitInfo[(i * 2) + 1]);
            logText += $"등록된 기기 이름 : {devices[i].Name} :: 주소 : {devices[i].Address}";
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

        //Debug.Log($"검색된 기기 이름 : {device.Name} :: 주소 : {device.Address}");

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
        //Debug.Log("===== 패킷 분석 시작 =====");
        do
        {
            while (_byteBufferList.Count > 0 && _byteBufferList[0] != (byte)_startChar)
            {
                //Debug.Log("::::: 패킷 시작문자 다름 :::::" + System.Convert.ToChar(_byteBufferList[0]));
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
                //Debug.Log("::::: 패킷 오류 :::::" + message + " ::::: " + message.Length);
            }
            else
            {
                List<byte> packets = _byteBufferList.GetRange(0, endIndex + 1);
                string message = System.Text.Encoding.Default.GetString(packets.ToArray());

                //Debug.Log("::::: 패킷 받음 :::::" + message + " ::::: " + message.Length);
                message = message.Replace(StartChar.ToString(), "").Replace(EndChar.ToString(), "");
                AddCallback(OnReadPacket, message);
                _byteBufferList.RemoveRange(0, endIndex + 1);
            }
        } while (_byteBufferList.Count > 0);

        //Debug.Log("===== 패킷 분석 종료 =====");

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
                //Debug.Log("성공");
                //Debug.Log($"ConnectedDevice =====> \n {_connectedDevice.Name}{_connectedDevice.Address}");
                break;
            }

            if (ConnectState == CONNECT_STATE_LISTEN && actionState != BLUETOOTH_ACL_CONNECTED)
            {
                result = false;
                //Debug.Log("실패");
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

            //Debug.Log($"코루틴 강제종료");
        }

        _enableCoroutine = EnableBluetoothCoroutine(onFinish);
        StartCoroutine(_enableCoroutine);
    }

    private IEnumerator EnableBluetoothCoroutine(Action<bool> onFinish)
    {
        //Debug.Log($"코루틴 시작");
        bool result = true;

#if PLATFORM_ANDROID
        //Debug.Log($"코루틴 --> 권한요청");
        Bluetooth.PERMISSION_STATE permissionState = Bluetooth.PERMISSION_STATE.DENIED;
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            PermissionCallbacks callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += (state) =>
            {
                //Debug.Log($"권한요청 결과\nPermissionDenied : {state}");
                permissionState = Bluetooth.PERMISSION_STATE.DENIED;
            };
            callbacks.PermissionGranted += (state) =>
            {
                //Debug.Log($"권한요청 결과\nPermissionGranted : {state}");
                permissionState = Bluetooth.PERMISSION_STATE.GRANTED;
            };
            callbacks.PermissionDeniedAndDontAskAgain += (state) =>
            {
                //Debug.Log($"권한요청 결과\nPermissionDeniedAndDontAskAgain : {state}");
                permissionState = Bluetooth.PERMISSION_STATE.DENIED_DONTASK;
            };
            Permission.RequestUserPermission(Permission.FineLocation, callbacks);

            // 잠시 대기
            yield return new WaitForSeconds(0.2f);
            //Debug.Log($"코루틴 --> 권한요청 ---> 대기중");
            // 포커스 확인 (포커스가 유니티로 돌아올때까지 대기)
            yield return new WaitUntil(() => Application.isFocused == true);

            switch (permissionState)
            {
                case Bluetooth.PERMISSION_STATE.DENIED:
                    //Debug.Log($"코루틴 --> 권한요청 ---> 거부");
                    result = false;
                    goto FINISH;
                case Bluetooth.PERMISSION_STATE.GRANTED:
                    //Debug.Log($"코루틴 --> 권한요청 ---> 허용");
                    break;
                case Bluetooth.PERMISSION_STATE.DENIED_DONTASK:
                    //Debug.Log($"코루틴 --> 권한요청 ---> 거부(다시묻지않음)");
                    _bluetooth.ShowLongToast("위치 정보 액세스 권한 요청이 거부되었습니다.\n앱 설정에서 권한을 확인해주세요.");

                    // 권한요청이 거부되어 1초 대기후 App Setting으로 넘어감
                    yield return new WaitForSeconds(1f);
                    _bluetooth.ShowAppSetting();
                    // App Setting 화면 호출 후 0.2초 대기
                    yield return new WaitForSeconds(0.2f);
                    // 포커스 확인
                    yield return new WaitUntil(() => Application.isFocused == true);
                    // 권한정보 재확인
                    if (!_bluetooth.CheckPermission())
                    {
                        // 권한이 여전히 거부되었다면 Finish
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
            // 블루투스 권한 요청
            _bluetooth.RequestEnable();
            //Debug.Log($"코루틴 --> 블루투스요청");
            yield return new WaitForSeconds(0.2f);
            //Debug.Log($"코루틴 --> 블루투스요청 ---> 대기중");
            yield return new WaitUntil(() => Application.isFocused == true);
            if (!IsEnabled)
            {
                //Debug.Log($"코루틴 --> 블루투스요청 ---> 거부");
                result = false;
                goto FINISH;
            }

            //Debug.Log($"코루틴 --> 블루투스요청 ---> 허용");
            yield return new WaitForSeconds(0.2f);
        }

    FINISH:
        //Debug.Log($"코루틴 종료");
        onFinish?.Invoke(result);
    }
}