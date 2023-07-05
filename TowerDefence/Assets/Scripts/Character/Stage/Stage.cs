using System;
using System.Collections;
using UnityAtoms;
using UnityEngine;

[System.Serializable]
public struct StageData
{
    public string ID;
    public WaveData[] WaveDatas;
}

public class Stage : MonoBehaviour
{
    [SerializeField, ReadOnly] private StageData _data;
    [SerializeField, ReadOnly] protected float _limitTime;
    [SerializeField, ReadOnly] private int _waveEnemyCount;
    [SerializeField, ReadOnly] private int _waveIndex;
    [SerializeField, ReadOnly] private bool _spawnable = true;

    public Action OnStageStart;
    public Action<bool> OnStageFinish;

    public Action OnWaveStart;
    public Action OnWaveFinish;

    private Spawner _spawner;
    private Transform _target;

    private IEnumerator _waveCoroutine;

    private void Update()
    {
        UpdateLifeTime();
    }
    private void UpdateLifeTime()
    {
        _limitTime -= Time.deltaTime;
        if (_limitTime <= 0f)
        {
            _limitTime = 0f;
            OnStageFinish?.Invoke(false);
        }
    }

    public void Initalization(StageTableData tableData, Transform target)
    {
        if (target == null) return;
        _target = target;

        _data = new StageData
        {
            ID = tableData.ID,
            WaveDatas = tableData.WaveDatas,
        };

        _limitTime = tableData.LimitTime;

        //spawner.Initialization(this, tower, tableData.SpawnInterval, tableData.SpawnDatas);
        //spawner.SetSpawnable(true);

        //OnStageFinish = null;
        //OnStageFinish += () => spawner.SetSpawnable(false);
    }
    public void SetSpawnable(bool spawnable) => _spawnable = spawnable;
    private IEnumerator _stageCoroutine;
    public void StartStage()
    {
        _stageCoroutine = StageCoroutine();
        StartCoroutine(_stageCoroutine);
    }
    public void EndStage(bool cleared = true)
    {
        if (_stageCoroutine != null)
            StopCoroutine(_stageCoroutine);
        _stageCoroutine = null;

        OnStageFinish?.Invoke(cleared);
    }
    private IEnumerator StageCoroutine()
    {
        _waveIndex = 0;
        _spawner ??= new();

        OnStageStart?.Invoke();

        var wfs = new WaitForSeconds(1f);
        while (_waveIndex <= _data.WaveDatas.Length - 1)
        {
            if (!_spawnable) yield return wfs;

            OnWaveStart?.Invoke();

            StartWave(_waveIndex);

            yield return new WaitWhile(() => _waveEnemyCount > 0);

            OnWaveFinish?.Invoke();

            yield return wfs;
            _waveIndex++;
        }

        OnStageFinish?.Invoke(true);
    }
    public void StartWave(int waveIndex)
    {
        if (_target == null) return;
        if (waveIndex < 0 || _waveIndex >= _data.WaveDatas.Length) return;

        var waveData = _data.WaveDatas[_waveIndex];
        for (int i = 0; i < waveData.SpawnDatas.Length; i++)
        {
            var spawnData = waveData.SpawnDatas[i];
            _spawner.Spawn(spawnData, _target, () => _waveEnemyCount--);
            _waveEnemyCount = spawnData.Amount;
        }
    }
}
