using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource backgroundMusic;
    public AudioClip backgroundMusicLoop;
    public AudioClip postAlarmMusic;
    public AudioSource destroySound;
    public AudioSource alarmSound;
    public AudioSource itemCollectSound;
    public AudioSource pointCollectSound;
    public AudioSource runeCastSound;
    public AudioClip iceCastSound;
    public AudioClip fireCastSound;

    private void OnEnable()
    {
        StaticDestructable.onStaticDestroyed += OnStaticDestroy;
        GooChamber.onGooRelease += OnAlarm;
        PointPickup.onPointPickup += OnPointGet;
        LineGenerator.OnRuneComplete += OnRuneComplete;
    }

    private void OnDisable()
    {
        StaticDestructable.onStaticDestroyed -= OnStaticDestroy;
        GooChamber.onGooRelease -= OnAlarm;
        PointPickup.onPointPickup -= OnPointGet;
        LineGenerator.OnRuneComplete -= OnRuneComplete;
    }

    // Update is called once per frame
    void Update()
    {
        if (!backgroundMusic.isPlaying && !backgroundMusic.loop)
        {
            backgroundMusic.clip = backgroundMusicLoop;
            backgroundMusic.loop = true;
            backgroundMusic.Play();
        }
    }

    private void OnStaticDestroy(ObjectScorePair pair, Vector2Int graphicalPos)
    {
        if (destroySound.isPlaying) destroySound.Stop();
        destroySound.Play();
    }

    private void OnAlarm()
    {
        alarmSound.Play();
        backgroundMusic.Stop();
        backgroundMusic.loop = false;
        backgroundMusic.clip = postAlarmMusic;
        backgroundMusic.Play();
    }

    private void OnItemCollect()
    {
        if (itemCollectSound.isPlaying) itemCollectSound.Stop();
        itemCollectSound.Play();
    }

    private void OnPointGet(int points)
    {
        if (pointCollectSound.isPlaying) pointCollectSound.Stop();
        pointCollectSound.Play();
    }

    private void OnRuneComplete(RuneInfo info)
    {
        switch (info.type)
        {
            case RuneTypes.Ice:
                runeCastSound.clip = iceCastSound;
                runeCastSound.Play();
                break;
            case RuneTypes.Fire:
                runeCastSound.clip = fireCastSound;
                runeCastSound.Play();
                break;
        }
    }
}
