using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomMsgType
{
    public static short Transform = MsgType.Highest + 1;
};


public class PlayerController : NetworkBehaviour
{
    public float m_linearSpeed = 5.0f;
    public float m_angularSpeed = 3.0f;
    public float m_jumpSpeed = 5.0f;
    public float FiringRate = 0.75f;

    public GameObject flagSlot;

    [SyncVar]
    public bool CanShoot = true;

    private NetworkStartPosition[] spawnPoints;

    public GameObject bulletPrefab;
    public Transform bulletSpawn;

    private Rigidbody m_rb = null;

    [SyncVar]
    private bool m_hasFlag = false;

    public bool HasFlag() {
        return m_hasFlag;
    }

    [Command]
    public void CmdPickUpFlag()
    {
        m_hasFlag = true;
        this.transform.GetChild(4).transform.localPosition = flagSlot.transform.localPosition;
        this.transform.GetChild(4).transform.localRotation = flagSlot.transform.localRotation;
    }  

    bool IsHost()
    {
        return isServer && isLocalPlayer;
    }
        
    void Start() 
    {
        m_rb = GetComponent<Rigidbody>();
        //Debug.Log("Start()");

        if (isLocalPlayer)
        {
            spawnPoints = FindObjectsOfType<NetworkStartPosition>();
        }
        Vector3 spawnPoint;
        ObjectSpawner.RandomPoint(this.transform.position, 10.0f, out spawnPoint);
        this.transform.position = spawnPoint;

        TrailRenderer tr = GetComponent<TrailRenderer>();
        tr.enabled = false;
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        //Debug.Log("OnStartAuthority()");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        //Debug.Log("OnStartClient()");
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        //Debug.Log("OnStartLocalPlayer()");
        GetComponent<MeshRenderer>().material.color = new Color(0.0f, 1.0f, 0.0f);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        //Debug.Log("OnStartServer()");
    }

    public void Jump()
    {
        Vector3 jumpVelocity = Vector3.up * m_jumpSpeed;
        m_rb.velocity += jumpVelocity;
        TrailRenderer tr = GetComponent<TrailRenderer>();
        tr.enabled = false;
    }

    [ClientRpc]
    public void RpcJump()
    {
        Jump();
    }

    [Command]
    public void CmdJump()
    {
        Jump();
        RpcJump();
    }

    [Command]
    void CmdFire()
    {
        CanShoot = false;

        StartCoroutine("ResetFiring");

        var bullet = (GameObject)Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);

        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 22;

        NetworkServer.Spawn(bullet);

        Destroy(bullet, 2.0f);
    }


    IEnumerator ResetFiring()
    {
        yield return new WaitForSeconds(FiringRate);
        CanShoot = true;
    }

    // Update is called once per frame
    void Update() {

        if (!isLocalPlayer)
        {
            return;
        }

        if (m_rb.velocity.y < Mathf.Epsilon) {
            TrailRenderer tr = GetComponent<TrailRenderer>();
            tr.enabled = false;
        }

        float rotationInput = Input.GetAxis("Horizontal");
        float forwardInput = Input.GetAxis("Vertical");

        Vector3 linearVelocity = this.transform.forward * (forwardInput * m_linearSpeed);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CmdJump();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            CmdPlayerDropFlag();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RpcRespawn();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (CanShoot)
            {
                CmdFire();
            }
        }

        float yVelocity = m_rb.velocity.y;


        linearVelocity.y = yVelocity;
        m_rb.velocity = linearVelocity;

        Vector3 angularVelocity = this.transform.up * (rotationInput * m_angularSpeed);
        m_rb.angularVelocity = angularVelocity;
    }

    [Command]
    public void CmdPlayerDropFlag()
    {
        Transform childTran = this.transform.GetChild(this.transform.childCount - 1);
        Flag flag = childTran.gameObject.GetComponent<Flag>();
		if (flag) 
        {
            flag.RpcDropFlag();
		}
    }
	public void OnCollisionEnter(Collision other)
	{
        if (!isLocalPlayer && other.collider.tag == "Bullet")
        {
            RpcRespawn();
        }
            
	}

    [ClientRpc]
    void RpcRespawn()
    {
        if (isLocalPlayer)
        {
            if (HasFlag()) 
            {
                Transform childTran = this.transform.GetChild(this.transform.childCount - 1);
                Flag flag = childTran.gameObject.GetComponent<Flag>();
                if (flag) 
                {
                    flag.RpcDropFlag();
                    flag.RpcResetFlag();
                }
            }

            Vector3 spawnPoint = Vector3.zero;

            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
            }

            transform.position = spawnPoint;
        }
    }

    [Command]
    public void CmdIncreaseSpeed()
    {
        m_linearSpeed = 10.0f;
        StartCoroutine("SpeedCooldown");
    }

    IEnumerator SpeedCooldown()
    {
        yield return new WaitForSeconds(5);

        m_linearSpeed = 5.0f;
    }

    [Command]
    public void CmdIncreaseFiringRate()
    {
        FiringRate = 0.2f;
        StartCoroutine("FiringCD");
    }

    IEnumerator FiringCD()
    {
        yield return new WaitForSeconds(5);

        FiringRate = 0.75f;
    }
}
