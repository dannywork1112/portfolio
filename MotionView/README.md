# MotionView
> ### [Bluetooth.cs](https://github.com/dannywork1112/portfolio/blob/main/MotionView/Bluetooth.cs)
* �÷������� �����ϰ� ����� ȣ��

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
* �� ��뿡 �ʿ��� ������ ȹ�� �� �� ����.
---
> ### [BluetoothManager.cs](https://github.com/dannywork1112/portfolio/blob/main/MotionView/BluetoothManager.cs)
* ������� ����, ����, ���ѿ�û ��
* �÷������� �޽����� `MessageHandler`�� ���� ���޹޾� ó��

#### �ݹ� ���
 * [`AddCallback`][AddCallback].

[AddCallback]:https://github.com/dannywork1112/portfolio/blob/47d2f3b403f9cfbd7b76716d9f6c24216b0eaa75/MotionView/BluetoothManager.cs#L237 "�ݹ� ���"

#### �޽��� �м�

* [`CheckBufferCoroutine`][CheckBufferCoroutine].

[CheckBufferCoroutine]:https://github.com/dannywork1112/portfolio/blob/47d2f3b403f9cfbd7b76716d9f6c24216b0eaa75/MotionView/BluetoothManager.cs#L251"��Ŷ�м�"

```c#
while (_byteBufferList.Count > 0 && _byteBufferList[0] != (byte)_startChar)
{
    _byteBufferList.RemoveAt(0);
}
```
* ���ۿ� ���۹��ڰ� ��Ŷ ���۹��ڿ� �����ϵ���.
```c#
List<byte> packets = _byteBufferList.GetRange(0, endIndex + 1);
string message = System.Text.Encoding.Default.GetString(packets.ToArray());

message = message.Replace(StartChar.ToString(), "").Replace(EndChar.ToString(), "");
AddCallback(OnReadPacket, message);
_byteBufferList.RemoveRange(0, endIndex + 1);
```
* ���� ���ڰ� ���޵� ������ �ݺ������� �޽����� ����.

#### �ݹ����
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
* ���޹��� ����� �����ϰ� �����ϱ� ���� `Update`���� ����.
---
> ### [������� �÷�����](https://github.com/dannywork1112/portfolio/tree/main/MotionView/main)
* Unity���� ���� ����̽��� ��Ʈ���ϱ� ���� �÷�����
* AndroidStudio�� �ۼ�
* ������� �˻�, ����, ����, ����Ȯ��, �޽��� ���� ���� ���