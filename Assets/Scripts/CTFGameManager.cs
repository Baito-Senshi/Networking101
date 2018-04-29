using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class CTFGameManager : NetworkBehaviour 
{
    public int m_numPlayers = 2;

    [SyncVar(hook = "DecreaseTime")]
    public float m_gameTime = 25.0f;

    public Text timeText;

    [SyncVar(hook = "SetScore")]
    public float ScoreOne = 0;

    [SyncVar(hook = "SetScore")]
    public float ScoreTwo = 0;

    public Text scoreOne;
    public Text scoreTwo;
    public Text EndText;

    public GameObject m_flag = null;
    public GameObject SpeedPickup;
    public GameObject FiringPickUp;

    public enum CTF_GameState
    {
        GS_WaitingForPlayers,
        GS_Ready,
        GS_InGame,
        GS_GameOver,
    }
        
    CTF_GameState m_gameState = CTF_GameState.GS_WaitingForPlayers;

    public bool SpawnFlag()
    {
		Vector3 spawnPoint;
		ObjectSpawner.RandomPoint(new Vector3(0, 0, 0), 0.0f, out spawnPoint);
		GameObject flag = Instantiate(m_flag, spawnPoint, new Quaternion());
        NetworkServer.Spawn(flag);
        return true;
    }

    public bool SpawnSpeedPickUp()
    {
        Vector3 spawnPoint;
        ObjectSpawner.RandomPoint(new Vector3(0, 0, 0), 30.0f, out spawnPoint);
        GameObject PickUp = Instantiate(SpeedPickup, spawnPoint, new Quaternion());
        NetworkServer.Spawn(PickUp);
        return true;
    }

    public bool SpawnFiringPickUp()
    {
        Vector3 spawnPoint;
        ObjectSpawner.RandomPoint(new Vector3(0, 0, 0), 30.0f, out spawnPoint);
        GameObject PickUp = Instantiate(FiringPickUp, spawnPoint, new Quaternion());
        NetworkServer.Spawn(PickUp);
        return true;
    }

    bool IsNumPlayersReached()
    {
        return CTFNetworkManager.singleton.numPlayers == m_numPlayers;
    }
        
	void Update ()
    {
	    if(isServer)
        {
            if(m_gameState == CTF_GameState.GS_WaitingForPlayers && IsNumPlayersReached())
            {
                m_gameState = CTF_GameState.GS_Ready;
            }
            if (m_gameState == CTF_GameState.GS_InGame)
            {
                if (m_gameTime <= 0)
                {
                    m_gameState = CTF_GameState.GS_GameOver;
                }
                    
            }
            if (m_gameState == CTF_GameState.GS_GameOver)
            {
                StopAllCoroutines();

                EndText.text = "Game is Over, The Winner is You!";
            }
        }

        UpdateGameState();
	}


    public void UpdateGameState()
    {
        if (m_gameState == CTF_GameState.GS_Ready)
        {
            //call whatever needs to be called
            if (isServer)
            {
                SpawnFlag();
                SpawnSpeedPickUp();
                SpawnFiringPickUp();
                StartCoroutine(StartCountdown());
                //change state to ingame
                m_gameState = CTF_GameState.GS_InGame;
            }
        }
    }

    IEnumerator StartCountdown()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            m_gameTime -= 1;
        }
    }
        
    void DecreaseTime(float time)
    {
        timeText.text = "Time Left: " + time.ToString();
    }

    void SetScore(float Score)
    {
        if (isServer)
        {
            scoreOne.text = "Player 1: " + Score.ToString();
        }
        else
        {
            scoreTwo.text = "Player 2: " + Score.ToString();
        }
    }
}
