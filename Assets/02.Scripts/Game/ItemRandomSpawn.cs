using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem.LowLevel;
using Photon.Pun;
using Photon.Realtime;

public class ItemRandomSpawn : MonoBehaviourPun
{
    public GameObject spawnRange;
    private BoxCollider rangeCollider;

    public List<GameObject> itemPrefabs;
    public List<GameObject> groundItems;

    public int itemSpawnCount = 50;
    LayerMask layer;

    private void Awake()
    {
        rangeCollider = spawnRange.GetComponent<BoxCollider>();
        layer = LayerMask.NameToLayer("GROUND");

    }
    public void GameStartSpawn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < itemSpawnCount;)
            {
                Vector3 randPos = GetRandSpawnPos();
                //int idx = Random.Range(0, itemPrefabs.Count);
                if (ItemSpawn(randPos))
                {
                    photonView.RPC("ItemSpawn", RpcTarget.OthersBuffered, randPos);
                    i++;
                }
            }
        }
    }
    private Vector3 GetRandSpawnPos()
    {
        Vector3 rangePosition = spawnRange.transform.position;

        float colX = rangeCollider.bounds.size.x;
        float colZ = rangeCollider.bounds.size.x;

        float randomX = Random.Range((colX / 2) * -1, (colX / 2));
        float randomZ = Random.Range((colZ / 2) * -1, (colZ / 2));

        Vector3 randomPosition = new Vector3(randomX,0, randomZ);

        Vector3 respawnPosition = rangePosition + randomPosition;
        return respawnPosition;
    }

    [PunRPC]
    private bool ItemSpawn(Vector3 randPos)
    {
        RaycastHit[] results = new RaycastHit[10];
        int hits = Physics.RaycastNonAlloc(randPos, Vector3.down, results, 100.0f);
        if(hits == 1)
        {
            if(results[0].transform.gameObject.layer == layer)
            {
                groundItems.Add(Instantiate(itemPrefabs[0], results[0].point + new Vector3(0, 0.1f, 0),Quaternion.identity));
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
}
