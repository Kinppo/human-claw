using System;
using System.Collections;
using System.Collections.Generic;
using PathCreation;
using UnityEngine;

public enum PlayerState
{
    Idle,
    Rotating,
    Descending,
    Collecting,
    Ascending,
    Running,
    Pausing
}

public class Player : MonoBehaviour
{
    public static Player Instance { get; protected set; }
    public PlayerState playerState;
    public float speed, rotateSpeed, rushSpeed;
    public LayerMask touchPointMask;
    public LayerMask objectLayer;
    public SkinnedMeshRenderer renderer;
    public Transform child;
    public CapsuleCollider collider;
    public Rigidbody rb;
    public List<BodyPart> bodyParts = new List<BodyPart>();
    public List<Rigidbody> objects = new List<Rigidbody>();
    private BodyPart selectedBodyPart;
    private Vector3 offset, lastPos, leftFootRot, rightFootRot;
    private float zCoord, dstTravelled;
    [HideInInspector] public PathCreator path;
    private Transform newTransform;
    private bool isFalling, collectingInvoked;
    private int objCounter;
    [HideInInspector] public float initialSpeed;
    [HideInInspector] public int reward;

    private void Awake()
    {
        Instance = this;
        playerState = PlayerState.Idle;
        initialSpeed = speed;
    }

    private void Start()
    {
        path = GameManager.Instance.selectedLevel.path;
        newTransform = new GameObject().transform;
        newTransform.parent = transform;
    }

    private void Update()
    {
        if (GameManager.gameState is GameState.Win or GameState.Lose) return;

        if (GameManager.gameState == GameState.Rotate && playerState == PlayerState.Idle)
            MovePlayerToBox();
        else if (playerState == PlayerState.Rotating)
        {
            CheckRotateClickDownEvent();
            CheckRotateClickHoldEvent();
            CheckRotateClickUpEvent();
        }
        else if (playerState == PlayerState.Descending)
            DescendBody();
        else if (playerState == PlayerState.Collecting)
            RotateBodyParts();
        else if (playerState == PlayerState.Ascending)
            AscendBody();
        else if (playerState == PlayerState.Pausing)
            CheckForRunStart();

        if (playerState == PlayerState.Running)
        {
            FollowPath();
            SetUpSpeedRushEffect();
            CheckRunClickDownEvent();
            CheckRunClickUpEvent();
        }
    }

    private void FixedUpdate()
    {
        if (playerState == PlayerState.Running)
        {
            CheckRunClickHoldEvent();
        }
    }

    private void CheckRotateClickDownEvent()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        RaycastHit hit;
        Ray ray = GameManager.Instance.cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, touchPointMask))
        {
            selectedBodyPart = hit.collider.GetComponent<BodyPart>();
            zCoord = GameManager.Instance.cam.WorldToScreenPoint(transform.position).z;
            offset = transform.position - GetMousePos();
        }
    }

    private void CheckRotateClickHoldEvent()
    {
        if (!Input.GetMouseButton(0) || selectedBodyPart == null) return;

        var mousePos = Input.mousePosition;
        var objectPos = GameManager.Instance.cam.WorldToScreenPoint(selectedBodyPart.bone.transform.position);
        mousePos.x = mousePos.x - objectPos.x;
        mousePos.z = mousePos.y - objectPos.y;
        mousePos.y = 0;

        float angle = Mathf.Atan2(mousePos.z, mousePos.x) * Mathf.Rad2Deg;

        if (selectedBodyPart.bodyPartName == BodyPartName.RightArm)
        {
            angle += 270;
            if (angle is < 360 and > 180)
                selectedBodyPart.bone.transform.rotation = Quaternion.Euler(new Vector3(90, 0, angle));
        }
        else if (selectedBodyPart.bodyPartName == BodyPartName.LeftArm)
        {
            angle += 270;
            if (angle is < 180 or > 360)
                selectedBodyPart.bone.transform.rotation = Quaternion.Euler(new Vector3(90, 0, angle));
        }
        else if (selectedBodyPart.bodyPartName == BodyPartName.RightFoot)
        {
            angle += 270;

            if (angle is < 270 and > 180)
            {
                selectedBodyPart.bone.transform.rotation = Quaternion.Euler(new Vector3(90, 0, angle));
                var percent = (angle - 180) * 100 / 180;
                renderer.SetBlendShapeWeight(1, percent);
            }
        }
        else if (selectedBodyPart.bodyPartName == BodyPartName.LeftFoot)
        {
            angle += 270;
            if (angle is < 180 and > 90)
            {
                selectedBodyPart.bone.transform.rotation = Quaternion.Euler(new Vector3(90, 0, angle));
                var percent = (angle - 90) * 100 / 90;
                renderer.SetBlendShapeWeight(0, 100 - percent);
            }
        }
        else if (selectedBodyPart.bodyPartName == BodyPartName.Body)
        {
            var nextPos = GetMousePos() + offset;
            if (nextPos.x > -0.25f && nextPos.x < 0.25f && nextPos.z < -9.3f && nextPos.z > -10.6f)
                transform.position = nextPos;
        }
    }

    private void CheckRotateClickUpEvent()
    {
        if (!Input.GetMouseButtonUp(0) || selectedBodyPart == null) return;
        selectedBodyPart = null;
    }

    private Vector3 GetMousePos()
    {
        var mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        return GameManager.Instance.cam.ScreenToWorldPoint(mousePoint);
    }

    private void RotateBodyParts()
    {
        foreach (var bodyPart in bodyParts)
        {
            var rot = bodyPart.bone.transform.localEulerAngles;

            switch (bodyPart.bodyPartName)
            {
                case BodyPartName.LeftArm or BodyPartName.RightArm:
                    bodyPart.bone.transform.localEulerAngles = Vector3.Lerp(rot, new Vector3(90, rot.y,
                        rot.z), Time.deltaTime * 2);
                    break;
                case BodyPartName.RightFoot:
                {
                    if (rightFootRot == Vector3.zero)
                        rightFootRot = bodyPart.bone.transform.localEulerAngles;

                    var angle = Mathf.LerpAngle(bodyPart.bone.transform.localEulerAngles.x, -90, Time.deltaTime * 2);
                    bodyPart.bone.transform.localEulerAngles = new Vector3(angle, 0, rightFootRot.z);
                    bodyPart.childBone.transform.localEulerAngles = new Vector3(angle, 0, 0);
                    break;
                }
                case BodyPartName.LeftFoot:
                {
                    if (leftFootRot == Vector3.zero)
                        leftFootRot = bodyPart.bone.transform.localEulerAngles;

                    var angle = Mathf.LerpAngle(bodyPart.bone.transform.localEulerAngles.x, -90, Time.deltaTime * 2);
                    bodyPart.bone.transform.localEulerAngles = new Vector3(angle, 0, leftFootRot.z);
                    bodyPart.childBone.transform.localEulerAngles = new Vector3(angle, 0, 0);
                    break;
                }
            }
        }

        if (!collectingInvoked)
        {
            collectingInvoked = true;
            Invoke(nameof(FinishCollecting), 1.5f);
        }
    }

    private void FinishCollecting()
    {
        var colls = new Collider[50];
        Physics.OverlapBoxNonAlloc(transform.position + new Vector3(0, -0.22f, 0.2f),
            new Vector3(0.15f, 0.2f, 0.225f), colls, Quaternion.identity, objectLayer);
        for (int i = 0; i < colls.Length; i++)
        {
            if (colls[i] != null)
            {
                var obj = colls[i].transform.GetComponent<Rigidbody>();
                objects.Add(obj);
                obj.isKinematic = true;
                obj.transform.parent = child;
            }
        }

        playerState = PlayerState.Ascending;
    }

    private void MovePlayerToBox()
    {
        var target = GameManager.Instance.instantiatePoint.position + new Vector3(0, -0.5f, 0f);
        transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * 4);

        if (Vector3.Distance(transform.position, target) <= 0)
            playerState = PlayerState.Rotating;
    }

    private void DescendBody()
    {
        var target = new Vector3(transform.position.x, 0.7f, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, target
            , Time.deltaTime * 2);

        if (Vector3.Distance(transform.position, target) <= 0.05f)
            playerState = PlayerState.Collecting;
    }

    private void AscendBody()
    {
        var target = new Vector3(transform.position.x, 1.9f, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, target
            , Time.deltaTime * 2);

        if (Vector3.Distance(transform.position, target) <= 0.05f)
        {
            playerState = PlayerState.Pausing;
            UIManager.Instance.ActivateFadePanel();
        }
    }

    private void CheckForRunStart()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        playerState = PlayerState.Running;
    }

    private void FollowPath()
    {
        dstTravelled += speed * Time.deltaTime;

        transform.position = path.path.GetPointAtDistance(dstTravelled, EndOfPathInstruction.Stop) +
                             new Vector3(0, -0.75f, 0);
        GameManager.Instance.wire.position = path.path.GetPointAtDistance(dstTravelled, EndOfPathInstruction.Stop) +
                                             new Vector3(0, -0.33f, 0);
        GameManager.Instance.camPoint.position = transform.position + new Vector3(0, 0.25f, 0);


        transform.rotation = path.path.GetRotationAtDistance(dstTravelled, EndOfPathInstruction.Stop);
        GameManager.Instance.camPoint.rotation =
            path.path.GetRotationAtDistance(dstTravelled, EndOfPathInstruction.Stop);

        if (path.path.length - dstTravelled < 2.3f)
        {
            GameManager.gameState = GameState.Win;
            StartCoroutine(FinishGame());
        }
    }

    private void CheckRunClickDownEvent()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        lastPos = Input.mousePosition;
    }

    private void CheckRunClickHoldEvent()
    {
        if (!Input.GetMouseButton(0)) return;
        var x = Input.mousePosition.x - lastPos.x;
        lastPos = Input.mousePosition;

        var limitedLeft = child.rotation.z > 0.42;
        var limitedRight = child.rotation.z < -0.42;
        newTransform.position = child.position;

        if (x > 0 && !limitedLeft)
        {
            newTransform.localEulerAngles += new Vector3(0, 0f, rotateSpeed * Time.deltaTime * 100);
            child.rotation = Quaternion.Lerp(child.rotation, newTransform.rotation, 35 * Time.deltaTime);
        }
        else if (x < 0 && !limitedRight)
        {
            newTransform.localEulerAngles += new Vector3(0, 0f, -rotateSpeed * Time.deltaTime * 100);
            child.rotation = Quaternion.Lerp(child.rotation, newTransform.rotation, 35 * Time.deltaTime);
        }
    }

    private void CheckRunClickUpEvent()
    {
        if (!Input.GetMouseButtonUp(0)) return;
    }

    public void DropObject()
    {
        if (isFalling) return;
        objCounter++;
        if (objects.Count < objCounter) return;
        objects[^objCounter].isKinematic = false;
        objects[^objCounter].AddForce(new Vector3(0,-100,0));
        objects[^objCounter].transform.parent = null;
        speed = 0.5f;
        StartCoroutine(ResetSpeed());
        isFalling = true;
    }

    private IEnumerator ResetSpeed()
    {
        yield return new WaitForSeconds(0.15f);
        if (GameManager.gameState == GameState.SpeedRush) yield break;
        speed = initialSpeed;
        isFalling = false;
    }

    public void Fall()
    {
        GameManager.gameState = GameState.Lose;
        foreach (var obj in objects)
        {
            obj.isKinematic = false;
            obj.transform.parent = null;
        }

        StartCoroutine(RemoveRbConstraint());
    }

    private IEnumerator RemoveRbConstraint()
    {
        yield return new WaitForSeconds(0.1f);
        rb.constraints = RigidbodyConstraints.None;
        GameManager.Instance.Lose();
    }

    private IEnumerator FinishGame()
    {
        yield return new WaitForSeconds(0.5f);
        foreach (var obj in objects)
        {
            obj.gameObject.layer = 16;
            obj.isKinematic = false;
            obj.AddForce(new Vector3(0,-100,0));
            obj.transform.parent = null;
            reward++;
        }

        GameManager.Instance.Win();
    }

    public IEnumerator SetUpSpeedRushEffect()
    {
        yield return new WaitForSeconds(0.01f);
        var effect = Instantiate(GameManager.Instance.speedRushEffect, transform.position + transform.position,
            transform.rotation).GetComponent<SpeedRushEffect>();
        effect.dstTravelled = dstTravelled;

        if (GameManager.gameState == GameState.SpeedRush)
            StartCoroutine(SetUpSpeedRushEffect());
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + new Vector3(0, -0.22f, 0.2f), new Vector3(0.3f, 0.4f, 0.45f));
    }
}