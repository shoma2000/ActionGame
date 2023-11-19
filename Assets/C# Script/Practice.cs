using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Practice : MonoBehaviour
{

    public float inputHorizontal;
    public float inputVertical;
    public float moveSpeed;

    public float turnSmoothing = 0.6f;

    private Transform mainCam;
    private Rigidbody playerRb;

    public Animator playerAnimator;
    private bool isRun;
    public bool isJump;

    public float jumpHeight = 1.5f;

    public Vector3 colExtents;

    void Start()
    {
        mainCam = GetComponentInChildren<Camera>().transform;
        playerRb = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
        colExtents = GetComponent<Collider>().bounds.extents;
    }

    void Update()
    {
        playerAnimator.SetBool("Run", false);
        //�O�㍶�E����
        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");

        //�ړ���
        float moveX = inputHorizontal * moveSpeed;
        float moveZ = inputVertical * moveSpeed;

        // �J�����̕�������AX-Z���ʂ̒P�ʃx�N�g�����擾
        Vector3 cameraForward = Vector3.Scale(mainCam.transform.forward, new Vector3(1, 0, 1)).normalized;
        // �����L�[�̓��͒l�ƃJ�����̌�������A�ړ�����������
        Vector3 moveForward = cameraForward * moveZ + mainCam.transform.right * moveX;

        //�O�������Ă��Ȃ������犊�炩�ɑO�����ɉ�]����
        if (moveForward != Vector3.zero)
        {
            Quaternion rotateForward = Quaternion.LookRotation(moveForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotateForward, turnSmoothing * Time.deltaTime);
        }

        transform.position += moveForward;

        
        if (inputHorizontal != 0 || inputVertical != 0)
        {
            playerAnimator.SetBool("Run", true);
        }
        
    }
}
