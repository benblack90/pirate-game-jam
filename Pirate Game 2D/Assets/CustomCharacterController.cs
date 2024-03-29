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
    //
    
    [Header("Controller Settings")]
    public bool _characeterActive = true;
    public float _characeterSpeed = 600.0f;
    public float _characeterRunSpeed = 800.0f; 
    [Range(0.0f, 100.0f)]
    public float _castRange = 5.0f;
    [Range(0.0f, 0.5f)] public float _castStartRangeMultiplier = 0.0f;
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
    [Range(0f, 1.0f)]
    public float _globalShakeIntensity = 1.0f;

    [Header("Goo Settings")]
    public GooController _gooScript;
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

    [Header("Particle Settings")]
    public GameObject _particleSystem;

    [Header("Sound Settings")]
    public AudioSource footstepSound;

    private Rigidbody2D _rb;
    private Vector2 _playerCameraHalf;
    private Vector2 _mousePosition;
    private bool lockCamera = false;
    private Vector2 _cameraLockOffset;
    

    private const int SPELL_HOT = 0;
    private const int SPELL_COLD = 1;



    private GameObject castArea;
    private GameObject castAreaDeadZone;
    private Vector2 cameraShakeIntensity = new Vector2(0, 0);
    private float cameraInitialShakeTimer = 0.0f;
    private float cameraShakeTimer = 0.0f;
    private Vector3 preShakePosition = new Vector3(0,0,0);
    void Start()
    {
        preShakePosition = _camera.transform.position;  
        _rb = this.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_characeterActive)
        {
            lockCamera = (Input.GetMouseButton(1));
            int spellAccuracyTest = Input.GetKey(KeyCode.LeftShift) ? 50 : 100;

/*            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                CastSpell(SPELL_HOT, spellAccuracyTest);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                CastSpell(SPELL_COLD, spellAccuracyTest);
            }*/


            PlayerCastArea();
            PlayerCastAreaDead();
            if (cameraShakeTimer > 0.0f) ManageCameraShake();
            else
            {
                cameraShakeIntensity = Vector2.zero;
                cameraInitialShakeTimer = 0.0f;
            }
        }
    }
    void DestroyedObject(ObjectScorePair pair, Vector2Int graphicalPos)
    {
        CameraShake(new Vector2(1.0f,1.0f) * 0.0005f * pair.points, 0.5f);
    }
    public void CameraShake(Vector2 intensity, float timer)
    {
        cameraShakeIntensity = intensity;
        cameraInitialShakeTimer = timer;
        cameraShakeTimer = timer;   
    }
    void ManageCameraShake()
    {
        cameraShakeTimer = Mathf.Max(cameraShakeTimer - Time.deltaTime, 0.0f);
    }
    private void OnEnable()
    {
        LineGenerator.OnRuneComplete += CastRune;
        DynamicDestructable.onDynamicDestroyed += DestroyedObject;
    }

    private void OnDisable()
    {
        LineGenerator.OnRuneComplete -= CastRune;
        DynamicDestructable.onDynamicDestroyed -= DestroyedObject;
    }
    
    public void CastRune(RuneInfo info)
    {

        switch (info.type)
        {
            case RuneTypes.Ice:
                CastSpell(SPELL_COLD, info.accuracy);
                break;
            case RuneTypes.Fire:
                CastSpell(SPELL_HOT, info.accuracy);
                break;
            case RuneTypes.Invalid:
                break;
        }
    }

    public Vector2Int GetPlayerPos()
    {
        Vector2Int pos = new Vector2Int((int) this.transform.position.x, (int) this.transform.position.y);
        return pos;
    }

    void FixedUpdate()
    {
        if (_characeterActive)
        {
            PlayerMove();
            PlayerLook();

            CameraMove();
        }
    }

        void PlayerMove()
    {
        _rb.velocity = Vector2.zero;
        Vector2 inputs = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (inputs.magnitude > 1) inputs = inputs.normalized;

        _rb.velocity += inputs * (Input.GetKey(KeyCode.LeftShift) ? _characeterRunSpeed : _characeterSpeed) * Time.deltaTime;

        if(_rb.velocity.magnitude > 0.1f && !footstepSound.isPlaying)
        {
            footstepSound.Play();
        }
        else if(_rb.velocity.magnitude <= 0.3f && footstepSound.isPlaying)
        {
            footstepSound.Stop();
        }
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
        if (!lockCamera)
        {
            return;
        }
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
            float angle = (_aimDirection + 90.0f - _castFieldOfView / 2 + _castFieldOfView / (vertexCount - 2 - 1) * i) * Mathf.Deg2Rad;
            Vector3 position = this.transform.position;
            position.x += _castRange * Mathf.Cos(angle);
            position.y += _castRange * Mathf.Sin(angle);
            lr.SetPosition(i + 1, position);
        }
        lr.SetPosition(vertexCount - 1, this.transform.position);

        lr.sortingLayerName = "Lines";

    }

    void PlayerCastAreaDead()
    {
        Color color = new Color(1, 0, 0, 1);
        float width = 0.025f;
        int vertexCount = 32;

        GameObject.Destroy(castAreaDeadZone);
        castAreaDeadZone = new GameObject();

        if (!lockCamera)
        {
            return;
        }
        castAreaDeadZone.AddComponent<LineRenderer>();
        LineRenderer lr = castAreaDeadZone.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        lr.SetVertexCount(vertexCount);
        lr.SetColors(color, color);
        lr.SetWidth(width, width);
        lr.SetPosition(0, this.transform.position);
        lr.loop = true;
        for (int i = 0; i < vertexCount - 2; i++)
        {
            float angle = (_aimDirection + 90.0f - _castFieldOfView / 2 + _castFieldOfView / (vertexCount - 2 - 1) * i) * Mathf.Deg2Rad;
            Vector3 position = this.transform.position;
            position.x += _castRange * Mathf.Cos(angle) * _castStartRangeMultiplier;
            position.y += _castRange * Mathf.Sin(angle) * _castStartRangeMultiplier;
            lr.SetPosition(i + 1, position);
        }
        lr.SetPosition(vertexCount - 1, this.transform.position);

        lr.sortingLayerName = "Lines";

    }

    void CameraMove()
    {
        _camera.transform.position = preShakePosition;
        Vector3 targetPosition = _playerCameraHalf;
        Vector2 cameraXY = _camera.transform.position * _dampening + targetPosition * (1.0f - _dampening);

        _camera.transform.position = new Vector3(cameraXY.x, cameraXY.y, _camera.transform.position.z);
        preShakePosition = _camera.transform.position;
        if (cameraInitialShakeTimer > 0)
        {
            float timerScale = 1 / cameraInitialShakeTimer * cameraShakeTimer;
            _camera.transform.position += new Vector3(
                Random.Range(-cameraShakeIntensity.x, cameraShakeIntensity.x) * timerScale * _globalShakeIntensity,
                Random.Range(-cameraShakeIntensity.y, cameraShakeIntensity.y) * timerScale * _globalShakeIntensity,
                0);
        }
        
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
        Burst(spellId);
        tempType *= spellAccuracy / 100.0f;
        float castBurstRange = _castRange * _gooPlaneScaling;
        StartCoroutine(DelayedLineCast(castBurstRange, castBurstRange * _castRangeExtenderMultiplier, _spreadSpreadSpeed, tempType));
    }

    void Burst(int burstId)
    {

        GameObject spell = Instantiate(_particleSystem, this.transform.position, Quaternion.Euler(0,0,_aimDirection));
        ParticleSystem particleRef = spell.GetComponent<ParticleSystem>();

        Color color = new Color(1,1,1);

        switch (burstId)
        {
            case (SPELL_HOT):
                color = new Color(1,0.5f,0);
                break;
            case (SPELL_COLD):
                color = new Color(0, 0.5f, 1);
                break;
        }
        particleRef.startColor = color;
        particleRef.Play();
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
        float spreadTo = spreadMax * _castStartRangeMultiplier;
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
