using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour {

    void OnCollisionEnter(Collision other)
    {
        if (other.collider != this)
        {
            Destroy(this);
        }
    }
}
