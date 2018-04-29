using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FiringPickup : NetworkBehaviour
{
    [ClientRpc]
    public void RpcPowerUp(GameObject player)
    {
        PlayerController pc = player.GetComponent<PlayerController> ();
        if (pc) 
        {   
            pc.CmdIncreaseFiringRate();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            RpcPowerUp(other.gameObject);
            this.transform.position = new Vector3(-10.0f, -10.0f, -10.0f);
            StartCoroutine("RespawnItem");
        }
    }

    IEnumerator RespawnItem()
    {
        yield return new WaitForSeconds(3);

        RpcRespawn();
    }

    [ClientRpc]
    void RpcRespawn()
    {
        Vector3 spawnPoint;
        ObjectSpawner.RandomPoint(Vector3.zero, 30.0f, out spawnPoint);
        this.transform.position = spawnPoint;
    }
}
