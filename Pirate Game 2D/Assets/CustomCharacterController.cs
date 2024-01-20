using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using UnityEngine.Rendering;
using System.Drawing;
using Color = UnityEngine.Color;

public class CustomCharacterController : MonoBehaviour
{
    // Start is called before the first frame update

    
    [Header("Controller Settings")]
    public float _characeterSpeed = 1.0f;
    [Range(0.0f, 100.0f)]
    public float _castRange = 5.0f;
    [Range(0.0f,360.0f)]
    public float _castFieldOfView = 90.0f;

    [Header("Camera Settings")]
    public Camera _camera;
    [Range(0f, 1.0f)]
    public float _dampening = 0.9f;
    [Range(0f, 0.5f)]
    public float _cameraMouseRatio = 0.15f;
    public bool _rotatePlayer = false;

    private Rigidbody2D _rb;
    private Vector2 _playerCameraHalf;
    private Vector2 _mousePosition;
    private bool lockCamera = false;
    private float _aimDirection = 0.0f;

    private int SPELL_HOT = 0;
    private int SPELL_COLD = 1;


    private GameObject castArea;
    void Start()
    {
        _rb = this.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        lockCamera = (Input.GetMouseButton(1));
        int spellAccuracyTest = Input.GetKey(KeyCode.LeftShift) ? 50 : 100;
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Casting hot spell, accuracy: " + spellAccuracyTest.ToString());
            CastSpell(SPELL_HOT, spellAccuracyTest);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Casting cold spell, accuracy: " + spellAccuracyTest.ToString());
            CastSpell(SPELL_COLD, spellAccuracyTest);
        }


        PlayerCastArea();
    }

    private void OnEnable()
    {
        LineGenerator.OnRuneComplete += CastRune;
    }

    private void OnDisable()
    {
        LineGenerator.OnRuneComplete -= CastRune;
    }

    public void CastRune(RuneInfo info)
    {
        Debug.Log("LET'S FUCKING GOOOOOOOOO");
        Debug.Log("ACCURACY: " + info.accuracy.ToString());
        switch (info.type)
        {
            case RuneTypes.Ice:
                Debug.Log("ICE");
                break;
            case RuneTypes.Fire:
                Debug.Log("FIRE");
                break;
            case RuneTypes.Invalid:
                Debug.Log("INVALID");
                break;
        }
    }


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


        if (!lockCamera)
        {
            _mousePosition = mousePos;
            if (_rotatePlayer) transform.up = direction;
            _aimDirection = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg * -1;
        }
        Vector2 trans2D = new Vector2(this.transform.position.x, this.transform.position.y);
        _playerCameraHalf = trans2D - (trans2D - _mousePosition) * _cameraMouseRatio;
    }

    void PlayerCastArea()
    {
        Color color = new Color(1, 0, 0, 1);
        float width = 0.05f;
        int vertexCount = 32;
        
        GameObject.Destroy(castArea);
        castArea = new GameObject();
        castArea.AddComponent<LineRenderer>();
        LineRenderer lr = castArea.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        lr.SetVertexCount(vertexCount);
        lr.SetColors(color, color);
        lr.SetWidth(width, width);
        lr.SetPosition(0, this.transform.position);
        lr.loop = true;
        for (int i = 0; i < vertexCount - 2; i++)
        {
            float angle = (_aimDirection + 90.0f - _castFieldOfView/2 + _castFieldOfView / (vertexCount - 2 - 1) * i) * Mathf.Deg2Rad;
            Vector3 position = this.transform.position;
            position.x += _castRange * Mathf.Cos(angle);
            position.y += _castRange * Mathf.Sin(angle);
            lr.SetPosition(i + 1, position);
        }
        lr.SetPosition(vertexCount-1, this.transform.position);

        lr.sortingLayerName = "Lines";

    }

    void CameraMove()
    {

        Vector3 targetPosition = _playerCameraHalf;


        Vector2 cameraXY = _camera.transform.position * _dampening + targetPosition * (1.0f - _dampening);

        _camera.transform.position = new Vector3(cameraXY.x, cameraXY.y, _camera.transform.position.z);
    }

    public void CastSpell(int spellId, int spellAccuracy)
    {

    }
}
