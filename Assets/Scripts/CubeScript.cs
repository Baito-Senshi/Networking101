using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CubeScript : NetworkBehaviour 
{
    public bool Up = true;
    public float Threshold;
	void Start () 
    {
		
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (Up)
        {
            transform.position += new Vector3(0.0f, 0.003f, 0.0f);
        }
        else
        {
            transform.position -= new Vector3(0.0f, 0.003f, 0.0f);
        }
        transform.Rotate(new Vector3 (0.0f, Time.deltaTime* 22f, 0.0f));

        Threshold += 0.25f;

        if (Threshold > 40)
        {
            if (Up)
            {
                Up = false;
                Threshold = 0;
            }
            else
            {
                Up = true;
                Threshold = 0;
            }
        }
	}
}
