using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationHandler : MonoBehaviour
{
    [SerializeField] Animator _animator;
    [SerializeField] CustomCharacterController _characterController;
    [SerializeField] Transform _bookTransform;

    public bool _isWalking = false;
    public bool _facingRight = true;

    private void Update()
    {

        _bookTransform.localRotation = Quaternion.Euler( new Vector3(_bookTransform.localRotation.x, _bookTransform.localRotation.y, _characterController._aimDirection+90));
        float horizontalInput = Input.GetAxis("Horizontal");
        if ( horizontalInput!= 0 || Input.GetAxis("Vertical") != 0)
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

        if (_characterController._aimDirection < 0)
        {
            _animator.SetBool("isFacingRight", true);
            if (!_facingRight)
            {
                _facingRight = true;
            }
        }
        else
        {
            _animator.SetBool("isFacingRight", false);
            if (_facingRight)
            {
                _facingRight = false;
            }
        }


        //set animation direction
        if(_facingRight && horizontalInput < 0)
        {
            _animator.SetFloat("direction", -1f);
        }
        else if(_facingRight && horizontalInput > 0)
        {
            _animator.SetFloat("direction", 1f);
        }
        else if (!_facingRight && horizontalInput > 0)
        {
            _animator.SetFloat("direction", -1f);
        }
        else if (!_facingRight && horizontalInput < 0)
        {
            _animator.SetFloat("direction", 1f);
        }
    }
}
