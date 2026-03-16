using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public Vector2Int GridPos { get; private set; }

    public void Setup(Vector2Int pos)
    {
        GridPos = pos;
    }

    public void Collect()
    {
        Destroy(gameObject);
    }
}