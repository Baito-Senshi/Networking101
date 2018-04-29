using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Flag : NetworkBehaviour 
{

    public GameObject firePrefab;

	public enum State
	{
		Available,
		Dropped,
		Possessed
	};

	public float droppedTime = 1.0f;

	private float m_droppedTimer = 1.0f;

	[SyncVar]
	State m_state;

	public State GetState() {
		return m_state;
	}

	// Use this for initialization
	void Start () 
    {
        //Vector3 spawnPoint;
        //ObjectSpawner.RandomPoint(this.transform.position, 10.0f, out spawnPoint);
        //this.transform.position = spawnPoint;
        //GetComponent<MeshRenderer> ().enabled = false;
        m_state = State.Available;
		m_droppedTimer = droppedTime;

        SpawnEmitter();
    }

    [ClientRpc]
    public void RpcPickUpFlag(GameObject player)
    {
        AttachFlagToGameObject(player);

        if (firePrefab != null)
        {
            gameObject.GetComponentInChildren<ParticleSystem>().Stop();
        }
    }

    [ClientRpc]
	public void RpcDropFlag()
	{
        this.transform.position = this.transform.parent.position - this.transform.parent.forward * 4.0f;
        this.transform.rotation = Quaternion.identity;
        this.transform.parent = null;
		m_state = State.Dropped;

        if (firePrefab != null)
        {
            gameObject.GetComponentInChildren<ParticleSystem>().Play();
        }
	}

    public void AttachFlagToGameObject(GameObject obj)
    {
		PlayerController pc =obj.GetComponent<PlayerController> ();
		if (pc) 
        {
			this.transform.parent = obj.transform;

            pc.CmdPickUpFlag();
		}
    }

    void OnTriggerEnter(Collider other)
    {
        if(!isServer || other.tag != "Player")
        {
            return;
        }

		//make this player drop the flag, start a cooldown for pickup
	 	if (m_state == State.Available) 
        {
			m_state = State.Possessed;
			AttachFlagToGameObject (other.gameObject);
            RpcPickUpFlag(other.gameObject);
		}
    }

    [ClientRpc]
    public void RpcResetFlag()
    {
        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update () 
    {
        if (!isServer)
        {	
			return;
		}

		if (m_state == State.Dropped)
        {
			this.transform.parent = null;
			m_droppedTimer -= Time.deltaTime;


			if (m_droppedTimer < 0.0f) 
            {
				m_state = State.Available;
				m_droppedTimer = droppedTime;
			}
		}
	}

    void SpawnEmitter()
    {
        var fireSpawn = Instantiate (firePrefab) as GameObject;
        fireSpawn.transform.parent = gameObject.transform;
        fireSpawn.transform.localScale = new Vector3Int(1, 1, 1);
        fireSpawn.transform.localPosition = new Vector3(4.0f, 0.7f, 0.0f);
        fireSpawn.transform.localRotation = Quaternion.Euler(new Vector3(-90.0f, 0.0f, 0.0f));
    }
}
