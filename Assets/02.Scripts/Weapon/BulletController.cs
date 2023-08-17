using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class BulletController : MonoBehaviourPun
{
    private Rigidbody rb;
    public float bulletSpeed = 3000.0f;

    private Vector3 dir;
    public int _actorNum;
    public void SetDir(Vector3 target,int actorNum)
    {
        rb = GetComponent<Rigidbody>();
        _actorNum = actorNum;
        dir = target - transform.position;
        transform.LookAt(dir);
        rb.AddForce(dir.normalized * bulletSpeed);
        
    }
}
