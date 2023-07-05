using UnityEngine;

[CreateAssetMenu(fileName = "GameConfiguration", menuName = "Game/GameConfiguration")]
public class GameConfiguration : ScriptableObject
{
    public string StageID;
    public string TowerID;
    public Vector2 SpawnDistance;
}
