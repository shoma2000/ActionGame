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
            //�t�B�[���h���痎�����珉���̈ʒu�ɖ߂�
            playerPos.position = player.initialPos;
        }
    }
}
