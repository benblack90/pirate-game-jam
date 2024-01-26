using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AudioManager : MonoBehaviour
{
    public AudioSource backgroundMusic;
    public AudioClip backgroundMusicLoop;
    public AudioClip postAlarmMusic;
    public AudioClip postAlarmLoop;
    public AudioSource destroySound;
    public AudioSource alarmSound;
    public AudioSource itemCollectSound;
    public AudioSource pointCollectSound;
    public AudioSource runeCastSound;
    public AudioClip iceCastSound;
    public AudioClip fireCastSound;
    public AudioClip invalidCastSound;
    public AudioSource doorOperateSound;
    public AudioClip doorOpenSound;
    public AudioClip doorCloseSound;
    public AudioSource loseSound;
    public AudioSource winSound;

    private float pointComboMax = 1.5f;
    private float pointComboTimer = 0.0f;
    private int comboCounter = 0;

    private void OnEnable()
    {
        StaticDestructable.onStaticDestroyed += OnStaticDestroy;
        DynamicDestructable.onDynamicDestroyed += OnDynamicDestroy;
        GooChamber.onGooRelease += OnAlarm;
        Collectable.onGenericCollectable += OnItemCollect;
        LineGenerator.OnRuneComplete += OnRuneComplete;
        DoorBase.onDoorOpenClose += OnDoorOpenClose;
        LevelExit.onLevelOver += OnWin;
        Level.onLose += OnLose;
    }

    private void OnDisable()
    {
        StaticDestructable.onStaticDestroyed -= OnStaticDestroy;
        DynamicDestructable.onDynamicDestroyed -= OnDynamicDestroy;
        GooChamber.onGooRelease -= OnAlarm;
        Collectable.onGenericCollectable -= OnItemCollect;
        LineGenerator.OnRuneComplete -= OnRuneComplete;
        DoorBase.onDoorOpenClose -= OnDoorOpenClose;
        LevelExit.onLevelOver -= OnWin;
        Level.onLose -= OnLose;
    }

    // Update is called once per frame
    void Update()
    {
        if(pointComboTimer > 0.0f)
        {
            pointComboTimer -= Time.deltaTime;
            if (pointComboTimer <= 0.0f) comboCounter = 0;
        }
        if (!backgroundMusic.isPlaying && !backgroundMusic.loop)
        {
            backgroundMusic.clip = backgroundMusicLoop;
            backgroundMusic.loop = true;
            backgroundMusic.Play();
        }
        if (alarmSound.isPlaying && alarmSound.time >= 4.0f)
        {
            alarmSound.volume -= 0.0003f;
            if(alarmSound.volume == 0)
            {
                alarmSound.Stop();
            }
        }
    }

    private void OnStaticDestroy(ObjectScorePair pair, Vector2Int graphicalPos)
    {
        
        pointComboTimer = pointComboMax;
        comboCounter = Mathf.Min(50, comboCounter + 1);
        if (destroySound.isPlaying) destroySound.Stop();
        destroySound.Play();
        OnPointGet(pair.points);
    }

    private void OnDynamicDestroy(ObjectScorePair pair, Vector2Int graphicalPos)
    {
        pointComboTimer = pointComboMax;
        comboCounter = Mathf.Min(50, comboCounter + 1);
        if (destroySound.isPlaying) destroySound.Stop();
        destroySound.Play();
        OnPointGet(pair.points);
    }

    private void OnAlarm()
    {
        alarmSound.Play();
        backgroundMusic.Stop();
        backgroundMusic.loop = false;
        backgroundMusic.clip = postAlarmMusic;
        backgroundMusicLoop = postAlarmLoop;
        backgroundMusic.volume += 0.1f;
        backgroundMusic.Play();
    }

    private void OnItemCollect(string name)
    {
        if (itemCollectSound.isPlaying) itemCollectSound.Stop();
        itemCollectSound.Play();
    }

    private void OnPointGet(int points)
    {
        if (pointCollectSound.isPlaying) pointCollectSound.Stop();
        pointCollectSound.pitch = 1.0f + (points/200.0f) + (comboCounter/50.0f);
        pointCollectSound.Play();
    }

    private void OnRuneComplete(RuneInfo info)
    {
        switch (info.type)
        {
            case RuneTypes.Ice:
                runeCastSound.clip = iceCastSound;
                runeCastSound.volume = 0.187f;
                break;
            case RuneTypes.Fire:
                runeCastSound.clip = fireCastSound;
                runeCastSound.volume = 0.2f;
                break;
            case RuneTypes.Invalid:
                runeCastSound.clip = invalidCastSound;
                runeCastSound.volume = 0.5f;
                break;
        }
        runeCastSound.pitch = Random.Range(0.8f, 1.2f);
        runeCastSound.Play();
    }

    private void OnDoorOpenClose(Dictionary<Vector2Int, TileBase> doorPositions, GridTileType gridType)
    {
        switch (gridType)
        {
            case GridTileType.BLANK:
                doorOperateSound.clip = doorOpenSound;
                break;
            case GridTileType.STATIC:
                doorOperateSound.clip = doorCloseSound;
                break;
        }
        doorOperateSound.Play();
    }

    private void OnWin()
    {
        winSound.Play();
    }

    private void OnLose()
    {
        loseSound.Play();
    }
}
