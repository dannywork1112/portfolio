# Zeus
> ### [InputReader.cs](https://github.com/dannywork1112/portfolio/blob/main/Zeus/InputReader.cs)
* NewInputSystem�� ����Ͽ� Ű�� �����ϱ� ����
* �ݹ���, �׼Ǹ� Ȱ��ȭ/��Ȱ��ȭ, Ű �缳�� ��

#### �ݹ���
```c#
protected override void _OnAwake()
{
    if (_gameInput == null)
    {
        _gameInput = new GameInput();
        _gameInput.PlayerControls.SetCallbacks(this);
        _gameInput.PeaceMod.SetCallbacks(this);
        _gameInput.BattleMod.SetCallbacks(this);
        _gameInput.UI.SetCallbacks(this);
        _gameInput.QuickTab.SetCallbacks(this);

        Enable = true;
    }
}
```
* `GameInput.IPlayerControlsActions``GameInput.IPeaceModActions`...���� ��ӹ޾� �ݹ� ���

#### �׼Ǹ� Ȱ��ȭ/��Ȱ��ȭ
 * `GetActionMapNames(TypeInputActionMap actionMapType)` ȣ��
 * �ش� Ÿ���� �׼Ǹ��� �˻��Ͽ� Ȱ��ȭ

[EnableActionMap]:https://github.com/dannywork1112/portfolio/blob/47d2f3b403f9cfbd7b76716d9f6c24216b0eaa75/Zeus/InputReader.cs#L159 "�׼Ǹ� Ȱ��ȭ/��Ȱ��ȭ"
```c#
private void EnableActionMap(InputActionMap actionMap, bool enabled)
{
    if (enabled) actionMap.Enable();
    else actionMap.Disable();
}
```
* `EnableActionMap(actionMap, enable);` ȣ��� �ش� �׼Ǹ� Ȱ��ȭ/��Ȱ��ȭ
---
> ### [GameEventSO.cs](https://github.com/dannywork1112/portfolio/blob/main/Zeus/Event/GameEventSO.cs)
* ����Ʈ ����, ����, �Ϸ�, ������ ���� ���� �̺�Ʈ�� ScriptableObject�� ����
#### ���
 * [`�Լ�`][�Լ���ũ].

[�Լ���ũ]:��ũ "��ũ ����"

```c#
�ڵ峻��
```
* �ڵ� ����
---
> ### [QuestManager.cs](https://github.com/dannywork1112/portfolio/blob/main/Zeus/Quest/QuestManager.cs)
* ����Ʈ �߰�, ����, ������Ʈ
* ����Ʈ�� �������θ� �Ǵ��Ͽ� �ϷῩ�� �Ǵ�
#### ����Ʈ ������Ʈ
 * [`QuestProcess`][QuestProcess].
 * `UpdateStep(TypeQuestStep type, int targetID, int targetValue)`

[QuestProcess]:https://github.com/dannywork1112/portfolio/blob/47d2f3b403f9cfbd7b76716d9f6c24216b0eaa75/Zeus/Quest/QuestManager.cs#L178 "����Ʈ ������Ʈ"

 * [`QuestStepProcess`][QuestStepProcess].
 * `UpdateStep(TypeQuestStep type, int targetID, int targetValue)`

[QuestStepProcess]:https://github.com/dannywork1112/portfolio/blob/47d2f3b403f9cfbd7b76716d9f6c24216b0eaa75/Zeus/Quest/QuestManager.cs#L230 "����Ʈ ������Ʈ"
---