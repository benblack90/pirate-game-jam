using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource backgroundMusic;
    public AudioClip backgroundMusicLoop;
    public AudioSource destroySound;
    public AudioSource alarmSound;
    public AudioSource itemCollectSound;
    public AudioSource pointCollectSound;

    private void OnEnable()
    {
        StaticDestructable.onDestructableDestroyed += OnStaticDestroy;
        GooChamber.onGooRelease += OnAlarm;
        PointPickup.onPointPickup += OnPointGet;
    }

    private void OnDisable()
    {
        StaticDestructable.onDestructableDestroyed -= OnStaticDestroy;
        GooChamber.onGooRelease -= OnAlarm;
        PointPickup.onPointPickup -= OnPointGet;
    }

    // Update is called once per frame
    void Update()
    {
        if (!backgroundMusic.isPlaying)
        {
            backgroundMusic.clip = backgroundMusicLoop;
            backgroundMusic.Play();
        }
    }

    private void OnStaticDestroy(ObjectScorePair pair, Vector2Int graphicalPos, Vector2Int topRight)
    {
        if (destroySound.isPlaying) destroySound.Stop();
        destroySound.Play();
    }

    private void OnAlarm()
    {
        alarmSound.Play();
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
}
