using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using UnityEngine.Rendering;
using System.Drawing;
using Color = UnityEngine.Color;
using UnityEngine.UIElements;
using System.Xml;
using System.Linq;
using static UnityEngine.EventSystems.EventTrigger;

public class CustomCharacterController : MonoBehaviour
{
    // Start is called before the first frame update

    
    [Header("Controller Settings")]
    public float _characeterSpeed = 1.0f;
    [Range(0.0f, 100.0f)]
    public float _castRange = 5.0f;
    [Range(0.0f,360.0f)]
    public float _castFieldOfView = 90.0f;
    public float _aimDirection = 0.0f;

    [Header("Camera Settings")]
    public Camera _camera;
    [Range(0f, 1.0f)]
    public float _dampening = 0.9f;
    [Range(0f, 0.5f)]
    public float _cameraMouseRatio = 0.15f;
    public bool _rotatePlayer = false;

    [Header("Goo Settings")]
    public PracticeComputeScript _gooScript;
    public int _gooPlaneScaling = 8;

    [Header("Spell Settings")]
    [Range(0,100)]
    public int _hotSpellDefaultPower = 50;
    [Range(0, 100)]
    public int _coldSpellDefaultPower = 50;
    public float _castRangeExtenderMultiplier = 0.5f;
    [Range(0.0f, 1.0f)]
    public float _spreadDecayRate = 0.5f;
    public float _coneQuality = 1.0f;
    public int _spreadSpreadSpeed = 10;

    private Rigidbody2D _rb;
    private Vector2 _playerCameraHalf;
    private Vector2 _mousePosition;
    private bool lockCamera = false;
    private Vector2 _cameraLockOffset;
    

    private const int SPELL_HOT = 0;
    private const int SPELL_COLD = 1;


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
            //Debug.Log("Casting hot spell, accuracy: " + spellAccuracyTest.ToString());
            CastSpell(SPELL_HOT, spellAccuracyTest);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            //Debug.Log("Casting cold spell, accuracy: " + spellAccuracyTest.ToString());
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
        //Debug.Log("LET'S FUCKING GOOOOOOOOO");
        //Debug.Log("ACCURACY: " + info.accuracy.ToString());
        switch (info.type)
        {
            case RuneTypes.Ice:
                //Debug.Log("ICE");
                CastSpell(SPELL_COLD, info.accuracy);
                break;
            case RuneTypes.Fire:
                //Debug.Log("FIRE");
                CastSpell(SPELL_HOT, info.accuracy);
                break;
            case RuneTypes.Invalid:
                //Debug.Log("INVALID");
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
        _rb.velocity = Vector2.zero;
        Vector2 inputs = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (inputs.magnitude > 1) inputs = inputs.normalized;

        _rb.velocity += inputs * _characeterSpeed * Time.deltaTime;
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
            Vector2 trans2D = new Vector2(this.transform.position.x, this.transform.position.y);
            _cameraLockOffset = (trans2D - _mousePosition) * _cameraMouseRatio;
        }
        

        _playerCameraHalf = this.transform.position - new Vector3(_cameraLockOffset.x, _cameraLockOffset.y,0);
    }

    void PlayerCastArea()
    {
        Color color = new Color(1, 0, 0, 1);
        float width = 0.025f;
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
        float tempType = 0;
        switch (spellId)
        {
            case (SPELL_HOT):
                tempType = _hotSpellDefaultPower;
                break;
            case (SPELL_COLD):
                tempType = -_coldSpellDefaultPower;
                break;
        }
        tempType *= spellAccuracy / 100.0f;
        float castBurstRange = _castRange * _gooPlaneScaling;
        StartCoroutine(DelayedLineCast(castBurstRange, castBurstRange * _castRangeExtenderMultiplier, _spreadSpreadSpeed, tempType));
    }
    // This doesn't work if cast radius is >180 for some reason.
    IEnumerator DelayedLineCast(float spreadMax, float spreadBonus, int growTimeScale, float temperatureChange)
    {
        WaitForEndOfFrame wff = new WaitForEndOfFrame();
        float timeInSeconds = 1.0f/60;
        float counter = 0.0f;

        int totalLines = Mathf.RoundToInt(_castFieldOfView * _coneQuality);
        int spreadDist = Mathf.RoundToInt(_castRange * _gooPlaneScaling);
        

        
        Vector2Int playerPos = new Vector2Int(Mathf.RoundToInt(this.transform.position.x * _gooPlaneScaling), Mathf.RoundToInt(this.transform.position.y * _gooPlaneScaling));

        //Debug.Log("Start");
        float castedDirection = _aimDirection;
        float spreadTo = 0;
        bool hasFinished = false;

        Dictionary<float, float> anglesToWorkOn = new Dictionary<float, float>();

        Dictionary<float, float> angleRefCos = new Dictionary<float, float>();
        Dictionary<float, float> angleRefSin = new Dictionary<float, float>();
        for (int angleDir = 0; angleDir < totalLines; angleDir++)
        {
            float angle = (castedDirection + 90.0f - _castFieldOfView / 2 + _castFieldOfView / (totalLines - 1) * angleDir) * Mathf.Deg2Rad;
            anglesToWorkOn.Add(angle, temperatureChange * Mathf.Sin((180/ (totalLines-1) * angleDir) * Mathf.Deg2Rad));

            float cosRef = Mathf.Cos(angle);
            float sinRef = Mathf.Sin(angle);
            angleRefCos.Add(angle, cosRef);
            angleRefSin.Add(angle, sinRef);
        }

        while (!hasFinished)
        {
           
            for (int growLoop = 0; growLoop < growTimeScale && !hasFinished; growLoop++)
            {
                hasFinished = spreadTo >= (spreadMax + spreadBonus);
                foreach(KeyValuePair<float, float> entry in anglesToWorkOn)
                {
                    int x = Mathf.RoundToInt(playerPos.x + spreadTo * angleRefCos[entry.Key]);
                    int y = Mathf.RoundToInt(playerPos.y + spreadTo * angleRefSin[entry.Key]);
                    _gooScript.AddTemperatureToTile(x,y, entry.Value);
                }
                if (spreadTo >= spreadMax)
                {
                    foreach (float entry in anglesToWorkOn.Keys.ToList())
                    {
                        anglesToWorkOn[entry] = Mathf.Lerp(anglesToWorkOn[entry], 0.0f, _spreadDecayRate);
                        if (Mathf.Abs(anglesToWorkOn[entry]) <= 0.1f) anglesToWorkOn.Remove(entry);
                    }
                }
                
                spreadTo++;
            }
            _gooScript.SendTexToGPU();
            while (counter < timeInSeconds)
            {
                counter += Time.deltaTime;
                yield return wff;
            }
            counter = 0;
        }
        

        //Debug.Log("End");
    }
}
