using UnityEngine;

public sealed class PlayerMarker : MonoBehaviour
{
    public Vector2Int GridPos;
    public int StepRange = 1;
    public int GhostCharges = 0;
     public GameObject activePersistentGhostEffect;
    public void SetGridPosition(int x, int y)
    {
        GridPos = new Vector2Int(x, y);
    }
    public void ResetProperties()
    {
        GhostCharges = 0;
        StepRange = 1;
    
        if (activePersistentGhostEffect != null)
        {
            Destroy(activePersistentGhostEffect);
            activePersistentGhostEffect = null;
        }
    }
}