using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    private float distance;                       //プレイヤーと敵の距離
    public float leaveDistance = 2.0f;            //プレイヤーに近づき止まる距離
    private float trackingRange = 7.0f;           //敵が追いかけ始める距離
    private float quitRange = 10.0f;              //敵が追うのをやめる距離
    public float attackDistance = 1.0f;           //敵の攻撃開始距離
    private float turnSmoothingEnemy = 5.0f;      //敵の回転の滑らかさ

    public int currentEnemyHP;                    //敵の今のHP
    private int maxEnemyHP;                       //敵の最大HP
    public float enemySpeed;                      //敵の速度
    private float enemyDamageInterval;            //敵が攻撃を受けるまでの間隔
    public float enemyAttacInterval;              //敵が攻撃する間隔
    public int enemyID;                           //敵のID

    private bool isDamagedEnemy;
    private bool isCollidedPlayer;
 
    private float timerEnemyAttack;             
    private bool enemyAttackTimerOn;
    public bool attackPlayer;
    private bool dieEnemy;
    private bool tracking;

    public EnemyData enemyData;
    private GameManager gameManager;

    private GameObject player;
    public GameObject enemyHPUI;
    private Animator enemyAnim;
    public Collider attackPlayerCollider;
    private Slider slider;

    private Vector3 playerPos;


    void Start()
    {
        enemyAnim = GetComponent<Animator>();

        player = GameObject.FindWithTag("Player");
        gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        slider = GameObject.FindWithTag("HPBar").GetComponent<Slider>();

        //敵のタグからIDを取得
        string gameObjectTagName = this.gameObject.tag;
        switch (gameObjectTagName)
        {
            case "Slime":
                {
                    enemyID = enemyData.DataList[0].iD;
                    break;
                }
            case "Shell":
                {
                    enemyID = enemyData.DataList[1].iD;
                    break;
                }
        }

        //敵のIDごとにパラメータ設定
        currentEnemyHP = enemyData.DataList[enemyID].hP;
        enemySpeed = enemyData.DataList[enemyID].speed;
        enemyDamageInterval = enemyData.DataList[enemyID].damageInterval;
        enemyAttacInterval = enemyData.DataList[enemyID].attackInterval;
        maxEnemyHP = currentEnemyHP;

        //HPバーの設定
        slider.maxValue = maxEnemyHP;
        slider.value = maxEnemyHP;

        attackPlayerCollider.enabled = false;
        enemyHPUI.SetActive(false);
    }

    void Update()
    {
        if (!dieEnemy)
        {
            TrackPlayerManagement();
            AttackPlayerManagement();

            StartCoroutine(DieEnemyManagement());

            StartCoroutine(EnemyDamagedTimer());
        }
    }

    public void TrackPlayerManagement()
    {
        playerPos = player.transform.position;
        distance = Vector3.Distance(transform.position, playerPos);

        if (tracking)
        {
            if (distance > quitRange)
            {
                tracking = false;
                enemyAnim.SetBool("Walk", false);
                enemyHPUI.SetActive(false);
            }

            if (currentEnemyHP != maxEnemyHP)
            {
                //追跡時かつHPが満タン以外でHPバー表示
                enemyHPUI.SetActive(true);
            }

            //プレイヤー方向の単位ベクトルを取得し移動方向を決定
            Vector3 dir = (playerPos - transform.position).normalized;
            //敵の移動速度調整
            Vector3 moveForwardEnemy = new Vector3(dir.x, 0, dir.z) * enemySpeed;

            //プレイヤー方向に回転
            Quaternion rotateForward = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotateForward, turnSmoothingEnemy * Time.deltaTime);

            if(distance > leaveDistance)
            {
                transform.position += moveForwardEnemy;
                enemyAnim.SetBool("Walk", false);
            }
            else
            {
                enemyAnim.SetBool("Walk", true);
            }
            
        }
        else
        {
            if (distance < trackingRange)
            {
                tracking = true;
                enemyAnim.SetBool("Walk", true);
            }
        }
    }

    public void AttackPlayerManagement()
    {
        //タイマーがonの時攻撃モーション
        if (enemyAttackTimerOn)
        {
            timerEnemyAttack += Time.deltaTime;
            if (timerEnemyAttack >= enemyAttacInterval)
            {
                enemyAttackTimerOn = false;
                timerEnemyAttack = 0;
            }
        }
        else if (!enemyAttackTimerOn)
        {
            if (distance < attackDistance)
            {
                enemyAnim.SetTrigger("Attack");
                enemyAttackTimerOn = true;
                attackPlayer = true;
                attackPlayerCollider.enabled = true;
            }
        }

        //攻撃モーションの時攻撃判定
        if (attackPlayer)
        {
            enemySpeed = 0f; //アタックしているとき動かない

            //攻撃モーションから変わったら判定消す
            if (enemyAnim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && enemyAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.98f)
            {
                enemySpeed = enemyData.DataList[enemyID].speed;
                attackPlayer = false;
                attackPlayerCollider.enabled = false;
            }
        }
    }

    //敵消滅設定
    public IEnumerator DieEnemyManagement()
    {
        //HPが0になったら2秒後に消滅
        if (currentEnemyHP <= 0)
        {
            dieEnemy = true;
            enemyAnim.SetTrigger("Die");

            yield return new WaitForSeconds(2);

            Destroy(this.gameObject);
            gameManager.enemyKillCount[enemyID]++;
        }

        //ゲームクリア時すぐ消滅
        if (gameManager.enemyKillCount == gameManager.maxEnemyCount)
        {
            Destroy(this.gameObject);
        }
    }

    //ダメージ間隔設定
    public IEnumerator EnemyDamagedTimer()
    {
        if (isDamagedEnemy)
        {
            isDamagedEnemy = false;
            yield return new WaitForSeconds(enemyDamageInterval);
            isCollidedPlayer = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isCollidedPlayer)
        {
            //敵に衝突したときプレイヤーのHP減らす
            if (other.gameObject.CompareTag("Sword"))
            {
                isDamagedEnemy = true;
                isCollidedPlayer = true;
                enemyAnim.SetTrigger("Hit");
                currentEnemyHP--;
                slider.value = currentEnemyHP;             //HPバー減少させる
            }
        }
    }
}
