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
        char rune = ReadRuneInput();
        ProcessRuneCast(rune);
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
