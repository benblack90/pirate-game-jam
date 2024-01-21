using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    public List<Level> levelList;
    int currentLevel = 0;


    // Start is called before the first frame update
    void Start()
    {
        levelList.Add(new Level());
        InitCamera();
        InitLevel();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitCamera()
    {

    }
    void InitLevel()
    {
        levelList[currentLevel].InitLevel();
    }

  

   

    
}


