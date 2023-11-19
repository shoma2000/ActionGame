using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private float inputHorizontal;                   //前後移動
    private float inputVertical;                     //左右移動
    public float moveSpeed = 0.05f;                  //プレイヤーの移動速度
    public float turnSmoothing = 5f;                 //プレイヤーの回転の滑らかさ
    public float jumpHeight = 1.5f;                  //プレイヤーのジャンプの高さ
    public int playerHP = 10;                        //プレイヤーのHP
    private float intervalPlayerDamage = 3.0f;       //プレイヤーが攻撃を受けるまでの間隔

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

    private Vector3 colExtents;                      //プレイヤーの接地範囲
    public Vector3 initialPos;                       //プレイヤーの初期位置

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
            //前後左右入力
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
            //移動量
            float moveX = inputHorizontal * moveSpeed;
            float moveZ = inputVertical * moveSpeed;

            //カメラの方向から、X-Z平面の単位ベクトルを取得
            Vector3 cameraForward = Vector3.Scale(mainCam.transform.forward, new Vector3(1, 0, 1)).normalized;
            //方向キーの入力値とカメラの向きから、移動方向を決定
            Vector3 moveForward = cameraForward * moveZ + mainCam.transform.right * moveX;

            //前を向いていなかったら滑らかに前向きに回転する
            if (moveForward != Vector3.zero)
            {
                Quaternion rotateForward = Quaternion.LookRotation(moveForward);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotateForward, turnSmoothing * Time.deltaTime);
            }

            transform.position += moveForward;
        }
    }

    //接地判定
    public bool IsGrounded()
    {
        Ray ray = new Ray(this.transform.position + Vector3.up * (2 * colExtents.x), Vector3.down);
        return Physics.SphereCast(ray, colExtents.x, colExtents.x + 0.2f);
    }

    public void RunManagement()
    {
        //移動キーが入力されていたら走るアニメーションにする
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
        //スペース押すかつ地面にいるときジャンプする
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded() && !playerAnim.GetBool("Jump"))
        {
            playerAnim.SetBool("Jump", true);

            //ジャンプの高さ調整
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
        //左クリックかつ地面にいるとき剣の当たり判定onにしアタック
        if (Input.GetMouseButtonDown(0) && isGround)
        {
            playerAnim.SetTrigger("Attack");
            attackEnemy = true;
            attackEnemyCollider.enabled = true;
            Debug.Log("アタックおわり");
        }

        if (attackEnemy)
        {
            moveSpeed = 0f; //アタックしているとき動かない

                if (playerAnim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                {
                    moveSpeed = 0.1f;
                    attackEnemy = false;
                    attackEnemyCollider.enabled = false;
                    Debug.Log("アタック中");
                }
        }
    }

    //プレイヤーの消滅設定
    public IEnumerator DieManagement()
    {
        //HPが0になったら2秒後に消滅
        if (playerHP <= 0)
        {
            gameManager.gameOver = true;
            playerAnim.SetTrigger("Die");

            yield return new WaitForSeconds(2);

            GameObject character = GameObject.FindWithTag("Character");
            Destroy(character);
        }
    }

    //プレイヤーのダメージ判定間隔
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
            //敵に衝突したときプレイヤーのHP減らす
            if (other.gameObject.CompareTag("EnemyAttack"))
            {
                isDamagedPlayer = true;
                isCollidedEnemy = true;
                playerAnim.SetTrigger("Hit");

                //敵のIDを取得し敵ごとにHP減らす
                Enemy enemy = other.GetComponentInParent<Enemy>();
                playerHP -= enemyData.DataList[enemy.enemyID].attack;
            }
        }
    }
}
