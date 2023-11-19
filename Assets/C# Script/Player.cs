using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private float inputHorizontal;                   //�O��ړ�
    private float inputVertical;                     //���E�ړ�
    public float moveSpeed = 0.05f;                  //�v���C���[�̈ړ����x
    public float turnSmoothing = 5f;                 //�v���C���[�̉�]�̊��炩��
    public float jumpHeight = 1.5f;                  //�v���C���[�̃W�����v�̍���
    public int playerHP = 10;                        //�v���C���[��HP
    private float intervalPlayerDamage = 3.0f;       //�v���C���[���U�����󂯂�܂ł̊Ԋu

    private bool isDamagedPlayer;
    private bool isCollidedEnemy;
    private bool attackEnemy;
    private bool isGround;

    public EnemyData enemyData;
    private GameManager gameManager;

    private Transform mainCam;
    private Rigidbody playerRb;
    private Animator playerAnim;
    private Collider attackEnemyCollider;

    private Vector3 colExtents;                      //�v���C���[�̐ڒn�͈�
    public Vector3 initialPos;                       //�v���C���[�̏����ʒu

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        playerAnim = GetComponent<Animator>();
        colExtents = GetComponent<Collider>().bounds.extents;
        mainCam = GetComponentInChildren<Camera>().transform;

        attackEnemyCollider = GameObject.FindWithTag("Sword").GetComponent<Collider>();
        gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        attackEnemyCollider.enabled = false;

        initialPos = transform.position;
    }

    void Update()
    {
        if(!gameManager.gameOver && !gameManager.gameClear)
        {
            //�O�㍶�E����
            inputHorizontal = Input.GetAxis("Horizontal");
            inputVertical = Input.GetAxis("Vertical");


            AttackEnemyManagement();
            RunManagement();
            JumpManagement();

            StartCoroutine(DieManagement());

            StartCoroutine(PlayerDamegedTimer());

            isGround = IsGrounded();
        }

        if (gameManager.gameClear)
        {
            playerAnim.SetBool("Run", false);
        }
    }

    private void FixedUpdate()
    {
        if (!gameManager.gameOver && !gameManager.gameClear)
        {
            //�ړ���
            float moveX = inputHorizontal * moveSpeed;
            float moveZ = inputVertical * moveSpeed;

            //�J�����̕�������AX-Z���ʂ̒P�ʃx�N�g�����擾
            Vector3 cameraForward = Vector3.Scale(mainCam.transform.forward, new Vector3(1, 0, 1)).normalized;
            //�����L�[�̓��͒l�ƃJ�����̌�������A�ړ�����������
            Vector3 moveForward = cameraForward * moveZ + mainCam.transform.right * moveX;

            //�O�������Ă��Ȃ������犊�炩�ɑO�����ɉ�]����
            if (moveForward != Vector3.zero)
            {
                Quaternion rotateForward = Quaternion.LookRotation(moveForward);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotateForward, turnSmoothing * Time.deltaTime);
            }

            transform.position += moveForward;
        }
    }

    //�ڒn����
    public bool IsGrounded()
    {
        Ray ray = new Ray(this.transform.position + Vector3.up * (2 * colExtents.x), Vector3.down);
        return Physics.SphereCast(ray, colExtents.x, colExtents.x + 0.2f);
    }

    public void RunManagement()
    {
        //�ړ��L�[�����͂���Ă����瑖��A�j���[�V�����ɂ���
        if (inputHorizontal != 0 || inputVertical != 0)
        {
            playerAnim.SetBool("Run", true);
        }
        else
        {
            playerAnim.SetBool("Run", false);
        }
    }

    public void JumpManagement()
    {
        //�X�y�[�X�������n�ʂɂ���Ƃ��W�����v����
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded() && !playerAnim.GetBool("Jump"))
        {
            playerAnim.SetBool("Jump", true);

            //�W�����v�̍�������
            float jumpVelocity = 2f * Mathf.Abs(Physics.gravity.y) * jumpHeight;
            jumpVelocity = Mathf.Sqrt(jumpVelocity);
            playerRb.AddForce(Vector3.up * jumpVelocity, ForceMode.VelocityChange);
        }
        else if(playerAnim.GetBool("Jump"))
        {
            if (IsGrounded() && playerRb.velocity.y < 0)
            {
                playerAnim.SetBool("Jump", false);
            }

        }
    }

    public void AttackEnemyManagement()
    {
        //���N���b�N���n�ʂɂ���Ƃ����̓����蔻��on�ɂ��A�^�b�N
        if (Input.GetMouseButtonDown(0) && isGround)
        {
            playerAnim.SetTrigger("Attack");
            attackEnemy = true;
            attackEnemyCollider.enabled = true;
            Debug.Log("�A�^�b�N�����");
        }

        if (attackEnemy)
        {
            moveSpeed = 0f; //�A�^�b�N���Ă���Ƃ������Ȃ�

                if (playerAnim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                {
                    moveSpeed = 0.1f;
                    attackEnemy = false;
                    attackEnemyCollider.enabled = false;
                    Debug.Log("�A�^�b�N��");
                }
        }
    }

    //�v���C���[�̏��Őݒ�
    public IEnumerator DieManagement()
    {
        //HP��0�ɂȂ�����2�b��ɏ���
        if (playerHP <= 0)
        {
            gameManager.gameOver = true;
            playerAnim.SetTrigger("Die");

            yield return new WaitForSeconds(2);

            GameObject character = GameObject.FindWithTag("Character");
            Destroy(character);
        }
    }

    //�v���C���[�̃_���[�W����Ԋu
    public IEnumerator PlayerDamegedTimer()
    {
        if (isDamagedPlayer)
        {
            isDamagedPlayer = false;
            yield return new WaitForSeconds(intervalPlayerDamage);
            isCollidedEnemy = false;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isCollidedEnemy)
        {
            //�G�ɏՓ˂����Ƃ��v���C���[��HP���炷
            if (other.gameObject.CompareTag("EnemyAttack"))
            {
                isDamagedPlayer = true;
                isCollidedEnemy = true;
                playerAnim.SetTrigger("Hit");

                //�G��ID���擾���G���Ƃ�HP���炷
                Enemy enemy = other.GetComponentInParent<Enemy>();
                playerHP -= enemyData.DataList[enemy.enemyID].attack;
            }
        }
    }
}
