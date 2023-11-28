using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Transform cameraTransform;
    private ChunkLoader chunkLoader;
    public Toolbar toolbar;

    [SerializeField]
    public GameObject debugScreen;

    [SerializeField]
    public Transform highlightBlock;

    protected Vector3 placeBlockPos;

    [SerializeField]
    public float walkSpeed = 3f;
    [SerializeField]
    public float sprintSpeed = 6f;
    [SerializeField]
    public float jumpForce = 5f;
    [SerializeField]
    public float reach = 8;

    private float gravity;

    public float playerWidthRadius = 0.5f;
    public float playerHeight = 1.8f;

    private float horizontal;
    private float vertical;

    private float yaw = 0;
    private float pitch = 0;

    private Vector3 velocity;
    private float verticalMomentum;

    private float sensitivity = 1.0f;

    public float checkIncrement;

    [HideInInspector]
    public int selectedBlockIndex = 1;

    public bool isGrounded;
    public bool isSprinting;

    private bool toJump;
    private void Start()
    {
        cameraTransform = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;

        gravity = WorldManager.gravity;
        chunkLoader = WorldManager.instance.gameObject.GetComponent<ChunkLoader>();

        isSprinting = false;
        isGrounded = false;
        toJump = false;
    }
    private void Update()
    {
        Inputs();
        UpdateInteractPos();

        transform.Rotate(Vector3.up * yaw);
        cameraTransform.Rotate(Vector3.right * -pitch);
    }

    private void FixedUpdate()
    {
        GetVelocity();

        if (toJump == true)
        {
            Jump();
        }

        transform.Translate(velocity, Space.World);
    }

    private void UpdateInteractPos()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while(step < reach)
        {
            Vector3 pos = cameraTransform.position + (cameraTransform.forward * step);

            if(chunkLoader.CheckForVoxel(pos))
            {
                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeBlockPos = lastPos;

                highlightBlock.gameObject.SetActive(true);

                return;
            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            step += checkIncrement;
        }

        highlightBlock.gameObject.SetActive(false);
    }
    private void GetVelocity()
    {
        if(verticalMomentum > gravity)
        {
            verticalMomentum += Time.fixedDeltaTime * gravity;
        }

        if(isSprinting == true)
        {
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)).normalized * Time.fixedDeltaTime * sprintSpeed;
        }
        else
        {
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;
        }

        if ((velocity.z > 0 && front == true) || (velocity.z < 0 && back == true))
        {
            velocity.z = 0;
        }

        if ((velocity.x > 0 && right == true) || (velocity.x < 0 && left == true))
        {
            velocity.x = 0;
        }

        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if (velocity.y < 0)
        {
            velocity.y = CheckIfGrounded(velocity.y);
        }
        else if (velocity.y > 0)
        {
            velocity.y = CheckAbove(velocity.y);
        }
    }
    private void Inputs()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        yaw = sensitivity * Input.GetAxis("Mouse X");
        pitch = sensitivity * Input.GetAxis("Mouse Y");
        pitch = Mathf.Clamp(pitch, -90, 90);

        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            isSprinting = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isSprinting = false;
        }

        if(isGrounded == true && Input.GetKey(KeyCode.Space))
        {
            toJump = true;
        }

        if(Input.GetKeyDown(KeyCode.F3))
        {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }

        if(highlightBlock.gameObject.activeSelf == false)
        {
            return;
        }

        if(Input.GetMouseButtonDown(0))
        {
            chunkLoader.GetChunkFromVector3(highlightBlock.position).EditVoxel(highlightBlock.position, 0);
        }

        if(selectedBlockIndex == 0)
        {
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            int blockX = Mathf.FloorToInt(placeBlockPos.x);
            int playerX = Mathf.FloorToInt(transform.position.x);

            int blockZ = Mathf.FloorToInt(placeBlockPos.z);
            int playerZ = Mathf.FloorToInt(transform.position.z);

            if(blockX == playerX && blockZ == playerZ)
            {
                int blockY = Mathf.FloorToInt(placeBlockPos.y);
                int playerY = Mathf.FloorToInt(transform.position.y);

                if (blockY == playerY || blockY == playerY - 1)
                {
                    return;
                }
            }

            chunkLoader.GetChunkFromVector3(placeBlockPos).EditVoxel(placeBlockPos, selectedBlockIndex);
            toolbar.RemoveItem(selectedBlockIndex);
        }
    }

    private void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        toJump = false;
    }
    private float CheckIfGrounded(float downSpeed)
    {
        if 
            (
            chunkLoader.CheckForVoxel(new Vector3(transform.position.x - playerWidthRadius, transform.position.y - playerHeight, transform.position.z - playerWidthRadius)) && (left == false && back == false) ||
            chunkLoader.CheckForVoxel(new Vector3(transform.position.x + playerWidthRadius, transform.position.y - playerHeight, transform.position.z - playerWidthRadius)) && (right == false && back == false) ||
            chunkLoader.CheckForVoxel(new Vector3(transform.position.x + playerWidthRadius, transform.position.y - playerHeight, transform.position.z + playerWidthRadius)) && (right == false && front == false) ||
            chunkLoader.CheckForVoxel(new Vector3(transform.position.x - playerWidthRadius, transform.position.y - playerHeight, transform.position.z + playerWidthRadius)) && (left == false && front == false)
            )
        {
            isGrounded = true;
            return 0;
        }

        isGrounded = false;
        return downSpeed;
    }
    private float CheckAbove(float upSpeed)
    {
        if
            (
            chunkLoader.CheckForVoxel(new Vector3(transform.position.x - playerWidthRadius, transform.position.y + upSpeed + 0.2f, transform.position.z - playerWidthRadius)) && (left == false && back == false) ||
            chunkLoader.CheckForVoxel(new Vector3(transform.position.x + playerWidthRadius, transform.position.y + upSpeed + 0.2f, transform.position.z - playerWidthRadius)) && (right == false && back == false) ||
            chunkLoader.CheckForVoxel(new Vector3(transform.position.x + playerWidthRadius, transform.position.y + upSpeed + 0.2f, transform.position.z + playerWidthRadius)) && (right == false && front == false) ||
            chunkLoader.CheckForVoxel(new Vector3(transform.position.x - playerWidthRadius, transform.position.y + upSpeed + 0.2f, transform.position.z + playerWidthRadius)) && (left == false && front == false)
            )
        {
            verticalMomentum = 0;
            return 0;
        }

        return upSpeed;
    }

    public bool front
    {
        get 
        {
            if(
               chunkLoader.CheckForVoxel(new Vector3(transform.position.x, transform.position.y - 1, transform.position.z + playerWidthRadius)) ||
               chunkLoader.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidthRadius))
              )
            {
                return true;
            }

            return false;
        }
    }
    public bool back
    {
        get
        {
            if (
               chunkLoader.CheckForVoxel(new Vector3(transform.position.x, transform.position.y - 1, transform.position.z - playerWidthRadius)) ||
               chunkLoader.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidthRadius))
              )
            {
                return true;
            }

            return false;
        }
    }
    public bool left
    {
        get
        {
            if (
               chunkLoader.CheckForVoxel(new Vector3(transform.position.x - playerWidthRadius, transform.position.y - 1, transform.position.z)) ||
               chunkLoader.CheckForVoxel(new Vector3(transform.position.x - playerWidthRadius, transform.position.y, transform.position.z))
              )
            {
                return true;
            }

            return false;
        }
    }
    public bool right
    {
        get
        {
            if (
               chunkLoader.CheckForVoxel(new Vector3(transform.position.x + playerWidthRadius, transform.position.y - 1, transform.position.z)) ||
               chunkLoader.CheckForVoxel(new Vector3(transform.position.x + playerWidthRadius, transform.position.y, transform.position.z))
              )
            {
                return true;
            }

            return false;
        }
    }
}
