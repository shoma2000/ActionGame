using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    int maxEnemy = 3;
    public float maxPosX = 5.0f;                    //x���̍ő�ʒu
    public float maxPosZ = 5.0f;                    //z���̍ő�ʒu
    public float minPosX = 3.0f;                    //x���̍ŏ��ʒu
    public float minPosZ = 3.0f;                    //z���̍ŏ��ʒu
    public float maxSpawnInterval = 10.0f;           //�X�|�[���ő�Ԋu
    public float minSpawnInterval = 8.0f;           //�X�|�[���ŏ��Ԋu
    public bool isSpawn;                            //�X�|�[������

    public GameObject[] enemies;
    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
    }


    private void Update()
    {
        if (!gameManager.gameOver && !gameManager.gameClear)
        {
            StartCoroutine(SpawnTimer());
        }

    }

    IEnumerator SpawnTimer()
    {
        if (!isSpawn)
        {
            if (SpawnEnemy())
            {
                isSpawn = true;

                //�X�|�[���Ԋu�ݒ�
                float interval = Random.Range(minSpawnInterval, maxSpawnInterval);
                yield return new WaitForSeconds(interval);

                isSpawn = false;
            }
            else
            {
                yield return null;
            }
        }

        yield return null;
    }

    bool SpawnEnemy()
    {
        GameObject[] spawnedSlime = GameObject.FindGameObjectsWithTag("Slime");
        GameObject[] spawnedShell = GameObject.FindGameObjectsWithTag("Shell");

        int spawnedEnemies = spawnedSlime.Length + spawnedShell.Length;

        if (spawnedEnemies >= maxEnemy)
        {
            return false;
        }
        else
        {
            //�X�|�[���ʒu�ݒ�
            float spawnPosX = Random.Range(minPosX, maxPosX);
            float spawnPosZ = Random.Range(minPosZ, maxPosZ);
            Vector3 spawnEnemy = new Vector3(transform.position.x + spawnPosX, transform.position.y, transform.position.z + spawnPosZ);

            //�X�|�[������G�ݒ�
            GameObject spawnenemy = enemies[Random.Range(0, enemies.Length)];
            Instantiate(spawnenemy, spawnEnemy, Quaternion.identity);

            return true;
        }
    }
}
