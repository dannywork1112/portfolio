using System;
using System.Collections.Generic;

public enum TypeFSM_Step
{
    IDLE,
    ENTER,
    UPDATE,
    EXIT,
}
public class FSM_Step
{
    private TypeFSM_Step _fromType;
    private TypeFSM_Step _toType;
    private Func<bool> _condition;

    public TypeFSM_Step From => _fromType;
    public TypeFSM_Step To => _toType;
    public Func<bool> Condition => _condition;

    public FSM_Step(TypeFSM_Step fromType, TypeFSM_Step toType, Func<bool> condition)
    {
        _fromType = fromType;
        _toType = toType;
        _condition = condition;
    }

    public bool IsDone() => _condition == null || _condition.Invoke();
}

public enum TypeFSM_State
{
    NONE = -1,
    IDLE,
    CHASE,
    ATTACK,
    HIT,
    DIE,
}
public struct FSM_TransitionData
{
    public Func<bool> Condition;
    public TypeFSM_State Type;
}
public class FSM_State
{
    private TypeFSM_State _curStateType;
    private List<FSM_TransitionData> _transitions;

    public Action OnEnterEvent;
    public Action OnUpdateEvent;
    public Action OnExitEvent;
    public Action<TypeFSM_State> OnChangeEvent;

    //private TypeFSM_Step _curStepType;
    //private Dictionary<TypeFSM_Step, FSM_Step> _steps;

    public void Initialization(TypeFSM_State type, Action onEnter, Action onUpdate, Action onExit, Action<TypeFSM_State> onChange)
    {
        OnEnterEvent = onEnter;
        OnUpdateEvent = onUpdate;
        OnExitEvent = onExit;
        OnChangeEvent = onChange;

        _curStateType = type;

        _transitions ??= new();

        //_states ??= new();
        //_states.Add(TypeFSM_Step.IDLE, new FSM_Step(TypeFSM_Step.IDLE, TypeFSM_Step.ENTER, null));
        //_states.Add(TypeFSM_Step.ENTER, new FSM_Step(TypeFSM_Step.ENTER, TypeFSM_Step.UPDATE, onEnter));
        //_states.Add(TypeFSM_Step.UPDATE, new FSM_Step(TypeFSM_Step.UPDATE, TypeFSM_Step.EXIT, onUpdate));
        //_states.Add(TypeFSM_Step.EXIT, new FSM_Step(TypeFSM_Step.EXIT, TypeFSM_Step.IDLE, onFinish));
    }
    public void AddTransition(TypeFSM_State stateType, Func<bool> condition)
    {
        if (condition == null) return;
        if (HasTransition(stateType)) return;

        _transitions.Add(new FSM_TransitionData
        {
            Condition = condition,
            Type = stateType,
        });
    }
    public bool HasTransition(TypeFSM_State stateType)
    {
        return _transitions.Exists(x => x.Type == stateType);
    }
    public void OnEnter()
    {
        //_curStepType = TypeFSM_Step.IDLE;
        OnEnterEvent?.Invoke();
    }
    public void OnUpdate()
    {
        //if (_curStepType == TypeFSM_Step.IDLE) return;

        //if (!_steps.TryGetValue(_curStepType, out var state)) return;

        //if (state.IsDone())
        //{
        //    _curStepType = state.To;
        //}

        for (int i = 0; i < _transitions.Count; i++)
        {
            var transition = _transitions[i];
            var result = transition.Condition.Invoke();
            if (result)
            {
                OnExit();
                OnChangeEvent?.Invoke(transition.Type);
                break;
            }
        }
        OnUpdateEvent?.Invoke();

        //if (_stateType == TypeFSM_State.IDLE) return;

        //if (_states.TryGetValue(_stateType, out var state))
        //{
        //    var result = state.Condition == null || state.Condition.Invoke();
        //    if (result) _stateType = state.To;
        //}

        //if (_stateType == TypeFSM_State.ENTER)
        //{
        //    var enterCondition = OnEnterEvent == null || OnEnterEvent.Invoke();
        //    if (enterCondition) _stateType = TypeFSM_State.UPDATE;
        //}
        //if (_stateType == TypeFSM_State.UPDATE)
        //{
        //    for (int i = 0; i < _transitions.Count; i++)
        //    {
        //        var transition = _transitions[i];
        //        var isChange = transition.Condition.Invoke();
        //        if (isChange)
        //        {
        //            OnFinishEvent?.Invoke(transition.Type);
        //            _stateType = TypeFSM_State.FINISH;
        //        }
        //        else OnUpdateEvent?.Invoke();
        //    }
        //}
        //if (_stateType == TypeFSM_State.FINISH)
        //{
        //    _stateType = TypeFSM_State.IDLE;
        //}
    }
    public void OnExit()
    {
        OnExitEvent?.Invoke();
    }
}
public class SimpleFSM
{
    private TypeFSM_State _curStateType;
    private FSM_State _curState;

    private Dictionary<TypeFSM_State, FSM_State> _states;

    public TypeFSM_State CurStateType => _curStateType;

    public void Initialization()
    {
        _states ??= new();
        _curStateType = TypeFSM_State.NONE;
    }
    public FSM_State AddState(TypeFSM_State stateType, Action onEnter, Action onUpdate, Action onFinish)
    {
        if (_states.TryGetValue(stateType, out var transition)) return transition;
        
        transition = new FSM_State();
        transition.Initialization(stateType, onEnter, onUpdate, onFinish, ChangeState);
        _states.Add(stateType, transition);
        return transition;
    }
    public void ChangeState(TypeFSM_State stateType)
    {
        if (stateType == TypeFSM_State.NONE) return;
        if (_curStateType == stateType) return;

        // 현재 state에 해당 type이 transition가능한지 체크
        if (_curState != null && !_curState.HasTransition(stateType)) return;

        // 아니면 현재 함수는 강제 Change로 남겨두고
        // 해당 state에서 정상적으로 transition가능한 함수 작성?
        if (!_states.TryGetValue(stateType, out var transition)) return;

        if (_curState != null) _curState.OnExit();

        _curStateType = stateType;
        _curState = transition;
        _curState.OnEnter();
    }
    public void OnUpdate()
    {
        if (_curState == null) return;

        _curState.OnUpdate();
    }
}