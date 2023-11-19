using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamManager : MonoBehaviour
{
    public Transform player;

    private Transform cam;
    public Vector3 camPosY = new Vector3(0.0f, 1.5f, 0.0f);
    public Vector3 camPosZ = new Vector3(0.0f, 0.0f, -3.0f);
    public float smooth = 10f;
    public float amingHSpeed = 6f;
    public float amingVSpeed = 6f;
    public float minVAngle = -60f;
    public float maxVAngle = 60f;

    private float angleH = 0;
    private float angleV = 0;
    private Vector3 smoothCamPosY;
    private Vector3 smoothCamPosZ;

    private GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();

        cam = transform;

        cam.position = player.position + Quaternion.identity * camPosY + Quaternion.identity * camPosZ;
        cam.rotation = Quaternion.identity;

        smoothCamPosY = camPosY;
        smoothCamPosZ = camPosZ;
    }

    void Update()
    {
        if (!gameManager.gameOver && !gameManager.gameClear)
        {
            angleH += Mathf.Clamp(Input.GetAxis("Mouse X"), -1, 1) * amingHSpeed;
            angleV += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1, 1) * amingVSpeed;
            angleV = Mathf.Clamp(angleV, minVAngle, maxVAngle);

            Quaternion camYRotation = Quaternion.Euler(0, angleH, 0);
            Quaternion aimRotation = Quaternion.Euler(-angleV, angleH, 0);
            cam.rotation = aimRotation;

            smoothCamPosY = Vector3.Lerp(smoothCamPosY, camPosY, smooth * Time.deltaTime);
            smoothCamPosZ = Vector3.Lerp(smoothCamPosZ, camPosZ, smooth * Time.deltaTime);
            cam.position = player.position + camYRotation * smoothCamPosY + aimRotation * smoothCamPosZ;
        }
    }
}
