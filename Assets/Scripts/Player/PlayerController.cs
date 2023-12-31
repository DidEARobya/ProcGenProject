using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    enum MovementState
    {
        SNEAKING,
        WALKING,
        SPRINTING
    }

    private PlayerBase playerBase;

    [SerializeField]
    public L_System lsystem;

    private Transform cameraTransform;
    private Vector3 cameraDefaultPos;
    private Vector3 cameraSneakPos;

    private WorldManager worldManager;
    private ChunkLoader chunkLoader;

    [SerializeField]
    public GameObject pauseMenu;

    [HideInInspector]
    public Toolbar toolbar;

    [SerializeField]
    public GameObject loadingScreen;
    private float loadDelay;
    private bool isSpawned;

    [SerializeField]
    public Transform highlightBlock;
    protected Vector3Int placeBlockPos;

    [SerializeField]
    public float sneakSpeed = 1f;
    [SerializeField]
    public float walkSpeed = 3f;
    [SerializeField]
    public float sprintSpeed = 6f;
    [SerializeField]
    public float jumpForce = 5f;
    [SerializeField]
    public float reach = 8;

    private MovementState movementState;

    private float gravity;
    private float fallingFrom;
    private float fallDmgMulti = 0.5f;

    public float playerWidthRadius = 0.5f;
    public float playerHeight = 1.8f;

    private float horizontal;
    private float vertical;

    private float sensitivity = 1.0f;
    private float yaw = 0;
    private float pitch = 0;

    private Vector3 velocity;
    private float verticalMomentum;

    public float checkIncrement;

    [HideInInspector]
    public int selectedBlockIndex = 0;
    [HideInInspector]
    public int toolbarIndex = 0;

    private int targetedBlockID;

    public bool isGrounded;
    private bool toJump;
    private bool toPlace;

    private float destroyTimer;
    Vector3 interactPos;

    private void Start()
    {
        playerBase = GetComponent<PlayerBase>();

        worldManager = WorldManager.instance;
        chunkLoader = ChunkLoader.instance;

        cameraTransform = Camera.main.transform;
        cameraDefaultPos = cameraTransform.localPosition;
        cameraSneakPos = cameraDefaultPos + new Vector3(0, -0.1f, 0.3f);

        Cursor.lockState = CursorLockMode.Locked;

        gravity = worldManager.gravity;

        movementState = MovementState.WALKING;

        isGrounded = false;
        toJump = false;
        toPlace = true;

        isSpawned = false;
        loadDelay = 0;

        fallingFrom = 0;
    }
    private void Update()
    {
        if(chunkLoader.isReady == true && isSpawned == false && isGrounded == true)
        {
            loadDelay += Time.deltaTime;

            if(loadDelay > 2f)
            {
                loadingScreen.SetActive(false);
                isSpawned = true;
            }
        }

        FallingLogic();
        Inputs();
        UpdateInteractPos();

        transform.eulerAngles += new Vector3(0, yaw, 0);
        cameraTransform.eulerAngles += new Vector3(-pitch, 0, 0);
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

    private void FallingLogic()
    {
        if(isSpawned == false)
        {
            return;
        }

        if (isGrounded == false)
        {
            if(fallingFrom == 0)
            {
                fallingFrom = transform.position.y;
            }
        }
        else
        {
            if (fallingFrom != 0)
            {
                float difference = fallingFrom - transform.position.y;

                int temp = Mathf.FloorToInt(difference / 3);

                if(difference > 0)
                {
                    playerBase.healthUpdate(-(temp * fallDmgMulti));
                }

                fallingFrom = 0;
            }
        }
    }
    private void UpdateInteractPos()
    {
        if(isSpawned == false)
        {
            return;
        }

        float step = checkIncrement;
        Vector3Int lastPos = new Vector3Int();

        while(step < reach)
        {
            Vector3Int pos = Vector3Int.FloorToInt(cameraTransform.position + (cameraTransform.forward * step));

            if(chunkLoader.CheckForVoxel(pos))
            {
                highlightBlock.position = new Vector3Int(pos.x, pos.y, pos.z);
                placeBlockPos = lastPos;

                targetedBlockID = chunkLoader.GetVoxelFromVector3Int(pos);

                highlightBlock.gameObject.SetActive(true);
                return;
            }

            lastPos = pos;
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

        switch(movementState)
        {
            case MovementState.SNEAKING:
                velocity = ((transform.forward * vertical) + (transform.right * horizontal)).normalized * Time.fixedDeltaTime * sneakSpeed;
                SneakCheck();
                break;
            case MovementState.WALKING:
                velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;
                break;
            case MovementState.SPRINTING:
                velocity = ((transform.forward * vertical) + (transform.right * horizontal)).normalized * Time.fixedDeltaTime * sprintSpeed;
                break;
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
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(isSpawned == false)
            {
                return;
            }

            pauseMenu.SetActive(true);
            Cursor.lockState = CursorLockMode.Confined;
            Time.timeScale = 0;
        }

        if(isSpawned == false || pauseMenu.activeInHierarchy == true)
        {
            return;
        }

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        yaw = sensitivity * Input.GetAxis("Mouse X");
        pitch = sensitivity * Input.GetAxis("Mouse Y");
        pitch = Mathf.Clamp(pitch, -90, 90);

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            movementState = MovementState.SPRINTING;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            movementState= MovementState.WALKING;
        }

        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            movementState = MovementState.SNEAKING;
            cameraTransform.localPosition = cameraSneakPos;
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            movementState = MovementState.WALKING;
            cameraTransform.localPosition = cameraDefaultPos;
        }

        if (isGrounded == true && Input.GetKey(KeyCode.Space))
        {
            toJump = true;
        }

        Interact();
    }

    private void Interact()
    {
        if (highlightBlock.gameObject.activeSelf == false)
        {
            return;
        }

        if(Input.GetKeyUp(KeyCode.L))
        {
            lsystem.SpawnLSystem(placeBlockPos);
        }

        if (Input.GetMouseButton(0))
        {
            if(targetedBlockID == 0 )
            {
                return;
            }

            if (interactPos != highlightBlock.transform.position)
            {
                interactPos = highlightBlock.transform.position;
                destroyTimer = 0;
            }

            destroyTimer += Time.deltaTime;

            if(destroyTimer > worldManager.blockData[targetedBlockID].destroyTime)
            {
                DestroyBlock();
            }
        }

        if(Input.GetMouseButtonUp(0))
        {
            destroyTimer = 0;
        }

        if (toPlace == true && Input.GetMouseButton(1))
        {
            toPlace = false;
            Invoke("PlaceBlock", 0.15f);
        }
    }
    private void DestroyBlock()
    {
        chunkLoader.GetChunkFromVector3(highlightBlock.position).EditVoxel(Vector3Int.FloorToInt(highlightBlock.position), 0);
        destroyTimer = 0;
    }
    private void PlaceBlock()
    {
        if (selectedBlockIndex == 0)
        {
            toPlace = true;
            return;
        }

        int blockX = placeBlockPos.x;
        int playerX = Mathf.FloorToInt(transform.position.x);

        int blockZ = placeBlockPos.z;
        int playerZ = Mathf.FloorToInt(transform.position.z);

        if (blockX == playerX && blockZ == playerZ)
        {
            int blockY = placeBlockPos.y;
            int playerY = Mathf.FloorToInt(transform.position.y);

            if (blockY == playerY || blockY == playerY - 1)
            {
                toPlace = true;
                return;
            }
        }

        bool isPlaced = chunkLoader.GetChunkFromVector3(placeBlockPos).EditVoxel(placeBlockPos, selectedBlockIndex);

        if (isPlaced == true)
        {
            toolbar.RemoveItemAtSlot(toolbarIndex, 1);
        }

        toPlace = true;
    }
    private void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        toJump = false;
    }
    private void SneakCheck()
    {
        if(isGrounded == false)
        {
            return;
        }

        if((velocity.x < 0 && sneakLeft == true) || velocity.x > 0 && sneakRight == true)
        {
            velocity.x = 0;
        }
        if ((velocity.z < 0 && sneakBack == true) || velocity.z > 0 && sneakFront == true)
        {
            velocity.z = 0;
        }
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
    public bool sneakFront
    {
        get
        {
            if(chunkLoader.CheckForVoxel(new Vector3(transform.position.x, transform.position.y - playerHeight, transform.position.z + 0.05f)) == false)
            {
                return true;
            }

            return false;
        }
    }
    public bool sneakBack
    {
        get
        {
            if(chunkLoader.CheckForVoxel(new Vector3(transform.position.x, transform.position.y - playerHeight, transform.position.z - 0.05f)) == false)
            {
                return true;
            }

            return false;
        }
    }
    public bool sneakLeft
    {
        get
        {
            if(chunkLoader.CheckForVoxel(new Vector3(transform.position.x - 0.05f, transform.position.y - playerHeight, transform.position.z)) == false)
            {
                return true;
            }

            return false;
        }
    }
    public bool sneakRight
    {
        get
        {
            if(chunkLoader.CheckForVoxel(new Vector3(transform.position.x + 0.05f, transform.position.y - playerHeight, transform.position.z)) == false)
            {
                return true;
            }

            return false;
        }
    }
}

