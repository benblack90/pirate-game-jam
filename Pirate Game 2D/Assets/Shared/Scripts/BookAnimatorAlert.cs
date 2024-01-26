using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookAnimatorAlert : MonoBehaviour
{
    public bool isFinished;
    public void AnimationFinished()
    {
        isFinished = true;
        gameObject.GetComponent<Animator>().SetBool("_isFlipping", false);
    }

    public void AnimationStart()
    {
        isFinished = false;
    }
}
