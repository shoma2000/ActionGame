using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHPBarManager : MonoBehaviour
{
    private Transform cam;

    private void Start()
    {
        cam = GameObject.FindWithTag("Camera").GetComponent<Transform>();
    }

    void Update()
    {
        //HPバーをカメラの向きに
        transform.rotation = cam.transform.rotation;
    }
}
