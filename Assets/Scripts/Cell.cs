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


[SerializeField] private Renderer floorRenderer;
    private Color baseColor = Color.white; // Alapértelmezett, ha valami elromlana

    // Ezt hívjuk meg a MazeGenerator-ból a generáláskor
    public void SetBaseColor(Color color)
    {
        baseColor = color;
        if (floorRenderer != null)
        {
            // Létrehozunk egy példányt a materialból, hogy ne az összes cellát színezze
            floorRenderer.material.color = baseColor;
        }
    }

    public void SetHighlight(bool highlight, Color highlightColor)
    {
        if (floorRenderer == null) return;

        // Ha highlight hamis, akkor KÖTELEZŐEN a saját baseColor-ját használja
        if (highlight)
        {
            floorRenderer.material.color = highlightColor;
        }
        else
        {
            floorRenderer.material.color = baseColor;
        }
    }
}