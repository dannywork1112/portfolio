# Zeus
> ### [InputReader.cs](https://github.com/dannywork1112/portfolio/blob/main/Zeus/InputReader.cs)
* NewInputSystem을 사용하여 키를 조작하기 위함
* 콜백등록, 액션맵 활성화/비활성화, 키 재설정 등

#### 콜백등록
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
* `GameInput.IPlayerControlsActions``GameInput.IPeaceModActions`...등을 상속받아 콜백 등록

#### 액션맵 활성화/비활성화
 * `GetActionMapNames(TypeInputActionMap actionMapType)` 호출
 * 해당 타입의 액션맵을 검색하여 활성화

[EnableActionMap]:https://github.com/dannywork1112/portfolio/blob/47d2f3b403f9cfbd7b76716d9f6c24216b0eaa75/Zeus/InputReader.cs#L159 "액션맵 활성화/비활성화"
```c#
private void EnableActionMap(InputActionMap actionMap, bool enabled)
{
    if (enabled) actionMap.Enable();
    else actionMap.Disable();
}
```
* `EnableActionMap(actionMap, enable);` 호출로 해당 액션맵 활성화/비활성화
---
> ### [GameEventSO.cs](https://github.com/dannywork1112/portfolio/blob/main/Zeus/Event/GameEventSO.cs)
* 퀘스트 수락, 진행, 완료, 레벨업 등의 게임 이벤트를 ScriptableObject로 관리
#### 기능
 * [`함수`][함수링크].

[함수링크]:링크 "링크 내용"

```c#
코드내용
```
* 코드 설명
---
> ### [QuestManager.cs](https://github.com/dannywork1112/portfolio/blob/main/Zeus/Quest/QuestManager.cs)
* 퀘스트 추가, 제거, 업데이트
* 퀘스트의 하위여부를 판단하여 완료여부 판단
#### 퀘스트 업데이트
 * [`QuestProcess`][QuestProcess].
 * `UpdateStep(TypeQuestStep type, int targetID, int targetValue)`

[QuestProcess]:https://github.com/dannywork1112/portfolio/blob/47d2f3b403f9cfbd7b76716d9f6c24216b0eaa75/Zeus/Quest/QuestManager.cs#L178 "퀘스트 업데이트"

 * [`QuestStepProcess`][QuestStepProcess].
 * `UpdateStep(TypeQuestStep type, int targetID, int targetValue)`

[QuestStepProcess]:https://github.com/dannywork1112/portfolio/blob/47d2f3b403f9cfbd7b76716d9f6c24216b0eaa75/Zeus/Quest/QuestManager.cs#L230 "퀘스트 업데이트"
---