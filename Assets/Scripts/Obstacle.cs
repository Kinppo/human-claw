using UnityEngine;

public enum ObstacleType
{
    Yellow,
    Red,
}

public class Obstacle : MonoBehaviour
{
    public ObstacleType obstacleType;
}