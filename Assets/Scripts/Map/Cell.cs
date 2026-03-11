using UnityEngine;

public class MazeCell : MonoBehaviour
{
    public GameObject wallNorth;
    public GameObject wallEast;
    public GameObject wallSouth;
    public GameObject wallWest;

    public bool IsNorthOpen => wallNorth != null && !wallNorth.activeSelf;
    public bool IsEastOpen  => wallEast != null && !wallEast.activeSelf;
    public bool IsSouthOpen => wallSouth != null && !wallSouth.activeSelf;
    public bool IsWestOpen  => wallWest != null && !wallWest.activeSelf;

    public void RemoveNorth() { wallNorth.SetActive(false); }
    public void RemoveEast()  { wallEast.SetActive(false); }
    public void RemoveSouth() { wallSouth.SetActive(false); }
    public void RemoveWest()  { wallWest.SetActive(false); }

}