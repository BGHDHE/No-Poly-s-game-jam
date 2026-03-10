using UnityEngine;

public class MazeCell : MonoBehaviour
{
    public GameObject wallNorth;
    public GameObject wallEast;
    public GameObject wallSouth;
    public GameObject wallWest;

    private bool northRemoved;
    private bool eastRemoved;
    private bool southRemoved;
    private bool westRemoved;

    public void RemoveNorth()
    {
        northRemoved = true;
        wallNorth.SetActive(false);
    }

    public void RemoveEast()
    {
        eastRemoved = true;
        wallEast.SetActive(false);
    }

    public void RemoveSouth()
    {
        southRemoved = true;
        wallSouth.SetActive(false);
    }

    public void RemoveWest()
    {
        westRemoved = true;
        wallWest.SetActive(false);
    }

    public void UpdateWalls()
    {
        if (wallNorth) wallNorth.SetActive(!northRemoved);
        if (wallEast)  wallEast.SetActive(!eastRemoved);
        if (wallSouth) wallSouth.SetActive(!southRemoved);
        if (wallWest)  wallWest.SetActive(!westRemoved);
    }
}