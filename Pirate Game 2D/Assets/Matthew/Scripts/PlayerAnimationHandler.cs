using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationHandler : MonoBehaviour
{
    [SerializeField] Animator _animator;
    bool _isWalking = false;
    bool _facingRight = true;

    private void Update()
    {
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            if (!_isWalking)
            {
                _animator.SetBool("IsWalking", true);
                _isWalking = true;
            }
            
        }
        else
        {
            if(_isWalking) 
            {
                _animator.SetBool("IsWalking", false);
                _isWalking = false;

            }
            
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (transform.position.x - mousePos.x < 0)
        {
            if (!_facingRight)
            {
                _facingRight = true;
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y,transform.localScale.z);
            }
        }
        else
        {
            if (_facingRight)
            {
                _facingRight = false;
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
            }
        }
    }
}
