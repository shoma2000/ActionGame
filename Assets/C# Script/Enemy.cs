using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    private float distance;                       //�v���C���[�ƓG�̋���
    public float leaveDistance = 2.0f;            //�v���C���[�ɋ߂Â��~�܂鋗��
    private float trackingRange = 7.0f;           //�G���ǂ������n�߂鋗��
    private float quitRange = 10.0f;              //�G���ǂ��̂���߂鋗��
    public float attackDistance = 1.0f;           //�G�̍U���J�n����
    private float turnSmoothingEnemy = 5.0f;      //�G�̉�]�̊��炩��

    public int currentEnemyHP;                    //�G�̍���HP
    private int maxEnemyHP;                       //�G�̍ő�HP
    public float enemySpeed;                      //�G�̑��x
    private float enemyDamageInterval;            //�G���U�����󂯂�܂ł̊Ԋu
    public float enemyAttacInterval;              //�G���U������Ԋu
    public int enemyID;                           //�G��ID

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

        //�G�̃^�O����ID���擾
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

        //�G��ID���ƂɃp�����[�^�ݒ�
        currentEnemyHP = enemyData.DataList[enemyID].hP;
        enemySpeed = enemyData.DataList[enemyID].speed;
        enemyDamageInterval = enemyData.DataList[enemyID].damageInterval;
        enemyAttacInterval = enemyData.DataList[enemyID].attackInterval;
        maxEnemyHP = currentEnemyHP;

        //HP�o�[�̐ݒ�
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
                //�ǐՎ�����HP�����^���ȊO��HP�o�[�\��
                enemyHPUI.SetActive(true);
            }

            //�v���C���[�����̒P�ʃx�N�g�����擾���ړ�����������
            Vector3 dir = (playerPos - transform.position).normalized;
            //�G�̈ړ����x����
            Vector3 moveForwardEnemy = new Vector3(dir.x, 0, dir.z) * enemySpeed;

            //�v���C���[�����ɉ�]
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
        //�^�C�}�[��on�̎��U�����[�V����
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

        //�U�����[�V�����̎��U������
        if (attackPlayer)
        {
            enemySpeed = 0f; //�A�^�b�N���Ă���Ƃ������Ȃ�

            //�U�����[�V��������ς�����画�����
            if (enemyAnim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && enemyAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.98f)
            {
                enemySpeed = enemyData.DataList[enemyID].speed;
                attackPlayer = false;
                attackPlayerCollider.enabled = false;
            }
        }
    }

    //�G���Őݒ�
    public IEnumerator DieEnemyManagement()
    {
        //HP��0�ɂȂ�����2�b��ɏ���
        if (currentEnemyHP <= 0)
        {
            dieEnemy = true;
            enemyAnim.SetTrigger("Die");

            yield return new WaitForSeconds(2);

            Destroy(this.gameObject);
            gameManager.enemyKillCount[enemyID]++;
        }

        //�Q�[���N���A����������
        if (gameManager.enemyKillCount == gameManager.maxEnemyCount)
        {
            Destroy(this.gameObject);
        }
    }

    //�_���[�W�Ԋu�ݒ�
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
            //�G�ɏՓ˂����Ƃ��v���C���[��HP���炷
            if (other.gameObject.CompareTag("Sword"))
            {
                isDamagedEnemy = true;
                isCollidedPlayer = true;
                enemyAnim.SetTrigger("Hit");
                currentEnemyHP--;
                slider.value = currentEnemyHP;             //HP�o�[����������
            }
        }
    }
}
