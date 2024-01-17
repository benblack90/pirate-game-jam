using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    public GameObject playerModel;
    short hitPoints = 100;
    short destructPoints = 0;
    short[] collectableIDs;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = ReadMovementInput();
        char rune = ReadRuneInput();
        ProcessMovement(dir);
        ProcessRuneCast(rune);
    }

    Vector3 ReadMovementInput()
    {
        Vector3 dir = new Vector3();
        if (Input.GetKey(KeyCode.W)) dir.z = 1;
        if (Input.GetKey(KeyCode.S)) dir.z = -1;
        if (Input.GetKey(KeyCode.D)) dir.x = 1;
        if (Input.GetKey(KeyCode.A)) dir.x = -1;
        return dir;
    }

    void ProcessMovement(Vector3 dir)
    { 
        playerModel.transform.position += 2 * dir * Time.deltaTime;
    }

    char ReadRuneInput()
    {
        return '0';
    }

    void ProcessRuneCast(char rune)
    {

    }


    public void SetStartPos(Vector3 pos)
    {
        playerModel.transform.position = Vector3.zero;
    }
}
