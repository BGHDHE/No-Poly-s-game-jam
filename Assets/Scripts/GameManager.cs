using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    private GridSystem gridSystem;
    [SerializeField] private GameObject playerPrefab;
    private List<Player> players = new List<Player>();

    private void Start() 
    {
        gridSystem = new GridSystem(10, 10, 2f);
        SpawnPlayers();
        UpdateSelectionVisuals();
    }

    private void SpawnPlayers() 
    {
        GridPosition[] startPositions = new GridPosition[] 
        {
            new GridPosition(0, 0),
            new GridPosition(0, 9),
            new GridPosition(9, 9)
        };

        for (int i = 0; i < 3; i++) 
        {
            GameObject go = Instantiate(playerPrefab);
            Player p = go.GetComponent<Player>();
            p.Setup(gridSystem, startPositions[i], i + 1);
            players.Add(p);
        }
    }

    public void HandleMoveInput(Vector2 input) 
    {
        Player activePlayer = players.Find(p => !p.HasMovedThisTurn);

        if (activePlayer == null) 
        {
            Debug.Log("Minden karakter lepett, nyomj gombot");
            return;
        }

        GridPosition offset = new GridPosition(Mathf.RoundToInt(input.x), Mathf.RoundToInt(input.y));
        
        if (activePlayer.TryMove(offset)) 
        {
            activePlayer.HasMovedThisTurn = true;
            UpdateSelectionVisuals();
            Debug.Log($"{activePlayer.name} lepett.");
        }
    }

    public void NextTurn() 
    {
        if (players.Any(p => !p.HasMovedThisTurn)) 
        {
            Debug.Log("Meg nem leptel minden karakterrel");
            return;
        }

        foreach (Player p in players) 
        {
            p.HasMovedThisTurn = false;
        }

        UpdateSelectionVisuals();
        Debug.Log("Uj kor kezdodott");
    }

    private void UpdateSelectionVisuals()
    {
        Player activePlayer = players.Find(p => !p.HasMovedThisTurn);

        foreach (Player p in players)
        {
            p.SetSelected(p == activePlayer);
        }
    }
    private void OnDrawGizmos() 
    {
        foreach (var p in players) 
        {
            if (p == null) continue;
            Gizmos.color = p.HasMovedThisTurn ? Color.gray : Color.green;
            Gizmos.DrawWireSphere(p.transform.position + Vector3.up * 4f, 0.3f);
        }
    }
}