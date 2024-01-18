using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CustomCharacterController : MonoBehaviour
{
    // Start is called before the first frame update

    public Camera _camera;
    [Range(0f,1.0f)]
    public float _dampening;
    public float _characeterSpeed = 1.0f;
    [Range(0f, 0.5f)]
    public float _cameraMouseRatio = 0.15f;

    private Rigidbody2D _rb;
    private Vector2 _playerCameraHalf;

    void Start()
    {
        _rb = this.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        PlayerMove();
        PlayerLook();
        CameraMove();


    }

    void PlayerMove()
    {
        float horizontalSpeed = Input.GetAxis("Horizontal");
        float verticalSpeed = Input.GetAxis("Vertical");
        _rb.position += new Vector2(horizontalSpeed, verticalSpeed) * _characeterSpeed * Time.deltaTime;
    }

    void PlayerLook()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;
        transform.up = direction;

        Vector2 trans2D = new Vector2(this.transform.position.x, this.transform.position.y);
        _playerCameraHalf = trans2D  - (trans2D - mousePos) * _cameraMouseRatio;
    }

    void CameraMove()
    {

        Vector3 targetPosition = _playerCameraHalf;


        Vector2 cameraXY = _camera.transform.position * _dampening + targetPosition  * (1.0f-_dampening);

        _camera.transform.position = new Vector3(cameraXY.x, cameraXY.y, _camera.transform.position.z);
        //_camera.transform.position = this.transform.position;
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        //Debug.Log("aoiu");
    }
}
