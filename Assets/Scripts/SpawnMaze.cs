using System.Collections.Generic;
using UnityEngine;

public class PrefabLoader : MonoBehaviour
{
    public List<GameObject> prefabList; 
    
    public Transform spawnPoint;

    public void Start()
    {
        //SpawnRandomPrefab();
    }

    public void SpawnRandomPrefab()
    {
        if (prefabList == null || prefabList.Count == 0)
        {
            Debug.LogWarning("A prefab lista üres!");
            return;
        }

        int randomIndex = Random.Range(0, prefabList.Count);
        GameObject selectedPrefab = prefabList[randomIndex];

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        Instantiate(selectedPrefab, pos, rot);
    }

    public void SpawnPrefabByIndex(int index)
    {
        if (index >= 0 && index < prefabList.Count)
        {
            Instantiate(prefabList[index], transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Érvénytelen index a prefab listában!");
        }
    }
}