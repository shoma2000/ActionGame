using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallManager : MonoBehaviour
{
    public Player player;
    private Transform playerPos;

    void Start()
    {
        playerPos = GameObject.FindWithTag("Player").GetComponent<Transform>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //フィールドから落ちたら初期の位置に戻す
            playerPos.position = player.initialPos;
        }
    }
}
