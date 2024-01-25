using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void OnEnable()
    {
        StaticDestructable.onStaticDestroyed += OnStaticDestroy;
        DynamicDestructable.onDynamicDestroyed += OnDynamicDestroy;
        GooChamber.onGooRelease += OnAlarm;
        PointPickup.onPointPickup += OnPointGet;
        Collectable.onGenericCollectable += OnItemCollect;
        LineGenerator.OnRuneComplete += OnRuneComplete;
    }

    private void OnDisable()
    {
        StaticDestructable.onStaticDestroyed -= OnStaticDestroy;
        DynamicDestructable.onDynamicDestroyed -= OnDynamicDestroy;
        GooChamber.onGooRelease -= OnAlarm;
        PointPickup.onPointPickup -= OnPointGet;
        Collectable.onGenericCollectable -= OnItemCollect;
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
        if (alarmSound.isPlaying && alarmSound.time >= 4.0f)
        {
            alarmSound.volume -= 0.001f;
        }
    }

    private void OnStaticDestroy(ObjectScorePair pair, Vector2Int graphicalPos)
    {
        if (destroySound.isPlaying) destroySound.Stop();
        destroySound.Play();
    }

    private void OnDynamicDestroy(ObjectScorePair pair, Vector2Int graphicalPos)
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
        backgroundMusicLoop = postAlarmLoop;
        backgroundMusic.volume += 0.2f;
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
        pointCollectSound.pitch = 1.0f + (points/10000.0f);
        pointCollectSound.Play();
    }

    private void OnRuneComplete(RuneInfo info)
    {
        switch (info.type)
        {
            case RuneTypes.Ice:
                runeCastSound.clip = iceCastSound;
                runeCastSound.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                runeCastSound.Play();
                break;
            case RuneTypes.Fire:
                runeCastSound.clip = fireCastSound;
                runeCastSound.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                runeCastSound.Play();
                break;
        }
    }
}
