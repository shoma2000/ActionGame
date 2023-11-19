using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //�G�̃f�[�^�x�[�X������Ă��ꂼ��̓G���ƂɃp�����[�^�[��ݒ肵Enemy�X�N���v�g��ς��ăX�|�[��������ok
    //�G�̃X�|�[���͈�
    //���̓����蔻�蒲��
    //�G�̓|�������̃e�L�X�g�ς���ok
    //�b���A�X���C���|������{�X����

    public int[] enemyKillCount;
    public int[] maxEnemyCount;
    public bool[] enemyCleared;
    public bool gameOver;
    public bool gameClear;
    public bool[] changeText;

    public EnemyData enemyData;
    public Enemy enemy;

    public GameObject enemyCountPanel;
    private Text[] enemiesCountText;
    public GameObject gameOverText;
    public GameObject gameClearText;
    

    void Start()
    {
        enemiesCountText = new Text[enemyCountPanel.transform.childCount];
        for(int i = 0; i < enemyCountPanel.transform.childCount; i++)
        {
            GameObject enemies = enemyCountPanel.transform.GetChild(i).gameObject;
            enemiesCountText[i] = enemies.GetComponent<Text>();
        }

        enemyKillCount = new int[enemiesCountText.Length];
        maxEnemyCount = new int[enemiesCountText.Length];
        enemyCleared = new bool[enemiesCountText.Length];
        changeText = new bool[enemiesCountText.Length];

        maxEnemyCount[0] = 1;
        maxEnemyCount[1] = 1;

        gameOverText.SetActive(false);
        gameClearText.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        LockCursol();

        //�����Ƃ悭�ł���͂��I�I
        {
            if (enemyKillCount[0] < maxEnemyCount[0])
            {
                enemiesCountText[0].text = "�|�����X���C���̐��F" + enemyKillCount[0] + "/" + maxEnemyCount[0];
            }
            else  //�G��ڕW���|������Cleared�ɕύX���t�F�[�h�A�E�g
            {
                if (!enemyCleared[0] && !changeText[0])
                {
                    StartCoroutine(FadeOut(0));
                }
                else if (changeText[0])
                {
                    enemiesCountText[0].color = new Color(0f, 0.6f, 1f, 1f);
                    enemiesCountText[0].text = "�N���A�I";
                    enemiesCountText[0].alignment = TextAnchor.MiddleCenter;
                    changeText[0] = false;
                    enemyCleared[0] = true;
                }
            }

            if (enemyKillCount[1] < maxEnemyCount[1])
            {
                enemiesCountText[1].text = "�|�����g�Q������̐��F" + enemyKillCount[1] + "/" + maxEnemyCount[1];
            }
            else  //�G��ڕW���|������Cleared�ɕύX���t�F�[�h�A�E�g
            {
                if (!enemyCleared[1] && !changeText[1])
                {
                    StartCoroutine(FadeOut(1));
                }

                if (changeText[1])
                {
                    enemiesCountText[1].color = new Color(0f, 0.6f, 1f, 1f);
                    enemiesCountText[1].text = "�N���A�I";
                    enemiesCountText[1].alignment = TextAnchor.MiddleCenter;
                    changeText[1] = false;
                    enemyCleared[1] = true;
                }
            }
        }

        if (enemyCleared[0] && enemyCleared[1])
        {
            gameClear = true;
        }

        if (gameOver)
        {
            gameOverText.SetActive(true);
        }

        if (gameClear)
        {
            gameClearText.SetActive(true);
        }
    }

    void LockCursol()
    {
        if (!gameOver && !gameClear)
        {
            //��ʂ����N���b�N���ꂽ��J�[�\�������ɌŒ肵�ď���
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            //escape�L�[�ŕ\��
            if (Input.GetKeyDown("escape"))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    
    IEnumerator FadeOut(int i)
    {
        float transparency = 1.0f;  //�����x
        while (transparency > 0)
        {
            yield return null;
            enemiesCountText[i].color = new Color(1f, 1f, 1f, transparency);
            transparency -= 0.005f;
        }
        changeText[i] = true;
    }

    //���g���C�{�^���Ń��X�^�[�g
    public void Retry()
    {
        SceneManager.LoadScene("MainScene");
    }

    //��߂�{�^���ŃX�^�[�g���
    public void QuitGame()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
