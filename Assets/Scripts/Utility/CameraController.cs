using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    [SerializeField]
    public GameObject orientation;

    float speed = 1;
    float moveSpeed = 10;

    float yaw;
    float pitch;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    // Update is called once per frame
    void Update()
    {
        MoveCamera();
    }

    private void MoveCamera()
    {
        yaw += speed * Input.GetAxis("Mouse X");
        pitch -= speed * Input.GetAxis("Mouse Y");

        transform.eulerAngles = new Vector3(pitch, yaw, 0);

        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * Time.deltaTime * moveSpeed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * Time.deltaTime * moveSpeed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= transform.right * Time.deltaTime * moveSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * Time.deltaTime * moveSpeed;
        }
    }
    public Chunk CheckCurrentChunk()
    {
        RaycastHit hit;

        if (Physics.Raycast(orientation.transform.position, Vector3.down, out hit, Mathf.Infinity))
        {
            //if(hit.collider.GetComponentInParent<Chunk>())
            //{
            //    return hit.collider.GetComponentInParent<Chunk>();
            //}
        }
        return null;
    }
}
