# MotionView
> ### [Bluetooth.cs](https://github.com/dannywork1112/portfolio/blob/main/MotionView/Bluetooth.cs)
* 플러그인을 실행하고 기능을 호출

#### Android Native App Setting
```c#
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
```
* 앱 사용에 필요한 권한을 획득 할 수 있음.
---
> ### [BluetoothManager.cs](https://github.com/dannywork1112/portfolio/blob/main/MotionView/BluetoothManager.cs)
* 블루투스 연결, 해제, 권한요청 등
* 플러그인의 메시지를 `MessageHandler`를 통해 전달받아 처리

#### 콜백 등록
 * [`AddCallback`][AddCallback].

[AddCallback]:https://github.com/dannywork1112/portfolio/blob/47d2f3b403f9cfbd7b76716d9f6c24216b0eaa75/MotionView/BluetoothManager.cs#L237 "콜백 등록"

#### 메시지 분석

* [`CheckBufferCoroutine`][CheckBufferCoroutine].

[CheckBufferCoroutine]:https://github.com/dannywork1112/portfolio/blob/47d2f3b403f9cfbd7b76716d9f6c24216b0eaa75/MotionView/BluetoothManager.cs#L251"패킷분석"

```c#
while (_byteBufferList.Count > 0 && _byteBufferList[0] != (byte)_startChar)
{
    _byteBufferList.RemoveAt(0);
}
```
* 버퍼에 시작문자가 패킷 시작문자와 동일하도록.
```c#
List<byte> packets = _byteBufferList.GetRange(0, endIndex + 1);
string message = System.Text.Encoding.Default.GetString(packets.ToArray());

message = message.Replace(StartChar.ToString(), "").Replace(EndChar.ToString(), "");
AddCallback(OnReadPacket, message);
_byteBufferList.RemoveRange(0, endIndex + 1);
```
* 종료 문자가 전달될 때까지 반복적으로 메시지를 저장.

#### 콜백수행
```c#
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
```
* 전달받은 명령을 안전하게 실행하기 위해 `Update`에서 수행.
---
> ### [블루투스 플러그인](https://github.com/dannywork1112/portfolio/tree/main/MotionView/main)
* Unity에서 전용 디바이스를 컨트롤하기 위한 플러그인
* AndroidStudio로 작성
* 블루투스 검색, 연결, 해제, 상태확인, 메시지 전달 등의 기능