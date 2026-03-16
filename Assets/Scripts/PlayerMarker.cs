using UnityEngine;

public sealed class PlayerMarker : MonoBehaviour
{
    public Vector2Int GridPos;
    public int StepRange = 1;
    public int GhostCharges = 0;

    public void SetGridPosition(int x, int y)
    {
        GridPos = new Vector2Int(x, y);
    }
}