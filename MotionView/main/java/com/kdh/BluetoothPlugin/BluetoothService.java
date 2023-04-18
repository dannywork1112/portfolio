package com.kdh.BluetoothPlugin;

import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothServerSocket;
import android.bluetooth.BluetoothSocket;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.util.Log;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.LinkedList;
import java.util.Queue;
import java.util.UUID;

public class BluetoothService
{
    private static final String TAG = "::: KDH_Service_LOG :::";

    // 시리얼 통신
    private UUID MY_UUID = UUID.fromString("00001101-0000-1000-8000-00805F9B34FB");

    private BluetoothAdapter _adapter;
    private Handler _handler;

    // 스레드
    private ConnectThread _connectThread;
    private ConnectedThread _connectedThread;
    private AcceptThread _acceptThread;
    private int _state;

    // 연결상태
    public static final int STATE_NONE = 0;            // NONE
    public static final int STATE_LISTEN = 1;          // 수신
    public static final int STATE_CONNECTING = 2;      // 송신
    public static final int STATE_CONNECTED = 3;       // 연결

    public static final String DEVICE_NAME = "device_name";
    public static final String TOAST = "toast";

    public BluetoothService(Handler handler)
    {
        _handler = handler;
        _adapter = BluetoothAdapter.getDefaultAdapter();
    }

    private synchronized void SetState(int state)
    {
        _state = state;
        _handler.obtainMessage(BluetoothActivity.MESSAGE_STATE_CHANGE, state, -1).sendToTarget();
        Log.d(TAG, "SetState() " + _state + " -> " + state);
        _state = state;
    }

    public synchronized int GetState() { return _state; }

    public synchronized void start()
    {
        Log.d(TAG, "synchronized start");

        // 연결 시도하는 스레드 취소
        if (_connectThread != null)
        {
            _connectThread.cancel();
            _connectThread = null;
        }

        // 현재 연결중인 스레드 취소
        if (_connectedThread != null)
        {
            _connectedThread.cancel();
            _connectedThread = null;
        }

        // Accept 스레드가 null 인경우 생성 및 시작
        if (_acceptThread == null)
        {
            _acceptThread = new AcceptThread();
            _acceptThread.start();
        }

        SetState(STATE_LISTEN);
    }

    public synchronized void connect(BluetoothDevice device)
    {
        Log.d(TAG, "synchronized connect to: " + device);

        // 연결 시도하는 스레드 취소
        if (_state == STATE_CONNECTING)
        {
            Log.d(TAG, "연결 시도하는 스레드 취소");
            if (_connectThread != null)
            {
                _connectThread.cancel();
                _connectThread = null;
            }
        }

        // 현재 연결중인 스레드 취소
        if (_connectedThread != null)
        {
            Log.d(TAG, "현재 연결중인 스레드 취소");
            _connectedThread.cancel();
            _connectedThread = null;
        }

        // 장치와 연결하기위한 스레드 시작
        Log.d(TAG, "장치와 연결하기위한 스레드 시작");
        _connectThread = new ConnectThread(device);
        _connectThread.start();

        SetState(STATE_CONNECTING);
    }

    public synchronized void connected(BluetoothSocket socket, BluetoothDevice device)
    {
        Log.d(TAG, "synchronized connected");

        // 연결 완료한 스레드 취소
        if (_connectThread != null)
        {
            _connectThread.cancel();
            _connectThread = null;
        }

        // 현재 연결중인 스레드 취소
        if (_connectedThread != null)
        {
            _connectedThread.cancel();
            _connectedThread = null;
        }

        // Cancel
        if(_acceptThread != null)
        {
            _acceptThread.cancel();
            _acceptThread = null;
        }

        // 스레드를 시작하여 연결 관리 및 전송 수행
        _connectedThread = new ConnectedThread(socket);
        _connectedThread.start();
        Message msg = _handler.obtainMessage(BluetoothActivity.MESSAGE_DEVICE_NAME);
        Bundle bundle = new Bundle();
        bundle.putString(DEVICE_NAME, device.getName());
        msg.setData(bundle);
        _handler.sendMessage(msg);
        SetState(STATE_CONNECTED);
    }

    public synchronized void stop()
    {
        Log.d(TAG, "synchronized stop");

        if (_connectThread != null)
        {
            _connectThread.cancel();
            _connectThread = null;
        }

        if (_connectedThread != null)
        {
            _connectedThread.cancel();
            _connectedThread = null;
        }

        if(_acceptThread != null)
        {
            _acceptThread.cancel();
            _acceptThread = null;
        }

        SetState(STATE_NONE);
    }

    public void write(byte[] out)
    {
        ConnectedThread tempConnectedThread;
        synchronized (this)
        {
            if (_state != STATE_CONNECTED)
                return;
            tempConnectedThread = _connectedThread;
            tempConnectedThread.write(out);
        } // Perform the write unsynchronized r.write(out); }
    }

    private void ConnectionFailed()
    {
        SetState(STATE_LISTEN);
        Message msg = _handler.obtainMessage(BluetoothActivity.MESSAGE_TOAST);
        Bundle bundle = new Bundle();
        bundle.putString(TOAST, "장치에 연결할 수 없습니다");
        msg.setData(bundle);
        _handler.sendMessage(msg);
    }

    private void ConnectionLost()
    {
        SetState(STATE_LISTEN);
        Message msg = _handler.obtainMessage(BluetoothActivity.MESSAGE_TOAST);
        Bundle bundle = new Bundle();
        bundle.putString(TOAST, "장치 연결이 끊겼습니다");
        msg.setData(bundle);
        _handler.sendMessage(msg);
    }

    private class AcceptThread extends Thread
    {
        private final BluetoothServerSocket _serverSocket;

        public AcceptThread()
        {
            BluetoothServerSocket tmp = null;
            try
            {
                tmp = _adapter.listenUsingRfcommWithServiceRecord("BluetoothActivity", MY_UUID);
            }
            catch (IOException e)
            {
                Log.e(TAG, "listen() failed", e);
            }
            _serverSocket = tmp;
        }

        public void run()
        {
            Log.d(TAG, "Accept 스레드 시작");
            setName("AcceptThread");
            BluetoothSocket socket = null;

            while(_state != STATE_CONNECTED)
            {
                try
                {
                    socket = _serverSocket.accept();
                }
                catch (IOException e1)
                {
                    Log.e(TAG, "accept() failed", e1);
                    break;
                }

                if(socket != null)
                {
                    BluetoothService bluetoothService = BluetoothService.this;
                    synchronized(bluetoothService)
                    {
                        switch(_state)
                        {
                            case STATE_NONE:
                            case STATE_CONNECTED:
                                try
                                {
                                    socket.close();
                                }
                                catch (IOException e2)
                                {
                                    Log.e(TAG, "Could not close unwanted socket", e2);
                                }
                                break;
                            case STATE_LISTEN:
                            case STATE_CONNECTING:
                                BluetoothService.this.connected(socket, socket.getRemoteDevice());
                        }
                    }
                }
            }

            Log.i(TAG, "Accept 스레드 종료");
        }

        public void cancel()
        {
            Log.d(TAG, "cancel " + this);

            try
            {
                _serverSocket.close();
            }
            catch (IOException e)
            {
                Log.e(TAG, "close() of server failed", e);
            }
        }
    }

    private class ConnectThread extends Thread
    {
        private final BluetoothSocket _socket;
        private final BluetoothDevice _device;

        public ConnectThread(BluetoothDevice device)
        {
            Log.d(TAG, "ConnectThread 생성");
            _device = device;
            BluetoothSocket tmp = null;

            try
            {
                tmp = device.createRfcommSocketToServiceRecord(MY_UUID);
            }
            catch (IOException e)
            {
                Log.e(TAG, "create() failed", e);
            }
            _socket = tmp;
        }

        public void run()
        {
            Log.i(TAG, "Connect Thread 시작");
            setName("ConnectThread");
            _adapter.cancelDiscovery();

            try
            {
                _socket.connect();
                Log.d(TAG, "Connect 성공");
            }
            catch (IOException e)
            {
                ConnectionFailed();
                Log.d(TAG, "Connect 실패 ==========\n" + e.toString() + "\n==========");

                try
                {
                    _socket.close();
                }
                catch (IOException e2)
                {
                    Log.e(TAG, "unable to close() socket during connection failure", e2);
                }

                BluetoothService.this.start();
                return;
            }

            synchronized (BluetoothService.this)
            {
                _connectThread = null;
            }

            connected(_socket, _device);
        }

        public void cancel()
        {
            try
            {
                _socket.close();
            }
            catch (IOException e)
            {
                Log.e(TAG, "close() of connect socket failed", e);
            }
        }
    }

    private class ConnectedThread extends Thread
    {
        private final BluetoothSocket _socket;
        private final InputStream _inStream;
        private final OutputStream _outStream;

        public ConnectedThread(BluetoothSocket socket)
        {
            Log.d(TAG, "ConnectedThread 생성");
            _socket = socket;
            InputStream tmpIn = null;
            OutputStream tmpOut = null;

            try
            {
                tmpIn = socket.getInputStream();
                tmpOut = socket.getOutputStream();
            }
            catch (IOException e)
            {
                Log.e(TAG, "temp sockets not created", e);
            }

            _inStream = tmpIn;
            _outStream = tmpOut;
        }

        public void run()
        {
            Log.i(TAG, "Connected 스레드 시작");
            Queue<byte[]> _bufferQueue = new LinkedList<>();
            int bytes;

            while (true)
            {
                try
                {
                    _bufferQueue.offer(new byte[1024]);
                    bytes = _inStream.read(_bufferQueue.peek());
                    if (bytes > 0)
                    {
                        byte[] poolBytes = _bufferQueue.poll();
                        Log.d(TAG, "::::: 플러그인 패킷확인 ::::: " + bytes + " :: " + new String(poolBytes));

                        // 큐에 동적할당
                        //_bufferQueue.offer(new byte[bytes]);
                        // 버퍼를 카피
                        //System.arraycopy(buffer, 0, _bufferQueue.peek(), 0, bytes);
                        // 핸들러에 카피한 버퍼를 넘김

                        _handler.obtainMessage(BluetoothActivity.MESSAGE_READ, bytes, -1, poolBytes).sendToTarget();
                        //_handler.obtainMessage(BluetoothActivity.MESSAGE_READ, bytes, -1, buffer).sendToTarget();
                        //readMsg.sendToTarget();
                    }
                }
                catch (IOException e)
                {
                    Log.e(TAG, "disconnected", e);
                    ConnectionLost();
                    break;
                }
            }
        }

        public void write(byte[] buffer)
        {
            try
            {
                _outStream.write(buffer);
                _handler.obtainMessage(BluetoothActivity.MESSAGE_WRITE, -1, -1, buffer).sendToTarget();
            }
            catch (IOException e)
            {
                Log.e(TAG, "Exception during write", e);
            }
        }

        public void cancel()
        {
            try
            {
                _socket.close();
            }
            catch (IOException e)
            {
                Log.e(TAG, "close() of connect socket failed", e);
            }
        }
    }

    /*
    // 블루투스 연결 스레드
    private class ConnectThread extends Thread
    {
        private final BluetoothSocket _socket;
        private final BluetoothDevice _device;

        public ConnectThread(BluetoothDevice device)
        {
            BluetoothSocket tempSocket = null;
            _device = device;

            try
            {
                tempSocket = device.createRfcommSocketToServiceRecord(MY_UUID);
            }
            catch (IOException e)
            {
                Log.e(TAG, "소켓의 create()메소드 실패", e);
            }
            _socket = tempSocket;
        }

        public void run()
        {
            // 블루투스 검색 취소
            // 검색을 취소하지 않으면 느려짐
            Log.d(TAG, "=====Connect Thread Begin=====");
            _adapter.cancelDiscovery();

            try
            {
                // 장치연결
                _socket.connect();
                Log.d(TAG, "=====Connect Success=====");
            }
            catch (IOException connectException)
            {
                // 연결 할 수 없음. 소켓 닫음.
                Log.d(TAG, "=====Connect Fail=====");
                try
                {
                    _socket.close();
                    Log.d(TAG, "=====ConnectFail=====");
                }
                catch (IOException closeException)
                {
                    Log.e(TAG, "클라이언트 소켓을 닫을 수 없습니다.", closeException);
                }
                return;
            }

            // 연결성공
            // 별도의 스레드에서 연결
            //manageMyConnectedSocket(mmSocket);

            synchronized (BluetoothService.this)
            {
                _connectThread = null;
            }


        }

        // 클라이언트 소켓을 닫고 스레드 완료
        public void cancel()
        {
            try
            {
                _socket.close();
            }
            catch (IOException e)
            {
                Log.e(TAG, "클라이언트 소켓을 닫을 수 없습니다.", e);
            }
        }
    }
    */
}
