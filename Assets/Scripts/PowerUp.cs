using UnityEngine;

public enum PowerUpType { Range, Ghost }

public class PowerUp : MonoBehaviour
{
    public PowerUpType type;
    public Vector2Int GridPos { get; private set; }

    public void Setup(UnityEngine.Vector2Int pos)
    {
        GridPos = pos;
    }

    public void Collect()
    {
        Destroy(gameObject);
    }
}