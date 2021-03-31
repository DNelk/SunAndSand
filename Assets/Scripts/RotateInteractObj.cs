using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class RotateInteractObj : EventTrigger
{
    public GameObject InteractObj;
    private bool _dragging = false;
    private float _rotationSpeed = 10f;
    
    //Rotation
    private readonly Vector2 _mouseSensitivity = new Vector2(15f,15f);
    private readonly Vector2 _minimumRotation = new Vector2(-360f,-360f);
    private readonly Vector2 _maximumRotation = new Vector2(360f, 360f);
    private Vector2 _rotation = new Vector2(0f, 0f);
    private Quaternion _originalRotation = Quaternion.identity;
    private Vector3 xVector = Vector3.down;
    private Vector3 yVector = Vector3.right;

    private void Update()
    {
        if (GameManager.Instance.State == GameState.Interacting)
        {
            Quaternion xQuaternion;
            Quaternion yQuaternion;
            
            /*ake rotation over x work when |y| is > 90 & < 180
            float x = Mathf.Abs(_rotation.x);
            if(x > 45f && x < 135f)
                yVector = _rotation.x < 0 ? Vector3.forward : Vector3.back;
            else if(x >= 135f && x < 225f)
                yVector = Vector3.left;
            else if(x >= 225f && x < 315f)
                yVector = _rotation.x < 0 ? Vector3.back : Vector3.forward;
            else
                yVector = Vector3.right;
            
            float y = Mathf.Abs(_rotation.y);
            if(y > 45f && y < 135f)
                xVector = _rotation.y < 0 ? Vector3.forward : Vector3.back;
            else if(y >= 135f && y < 225f)
                xVector = Vector3.up;
            else if(y >= 225f && y < 315f)
                xVector = _rotation.y < 0 ? Vector3.back : Vector3.forward;
            else
                xVector = Vector3.down;
            
            */
            if (_dragging)
            {
                _rotation.x += Input.GetAxis("Mouse X") * _mouseSensitivity.x;
                _rotation.y += Input.GetAxis("Mouse Y") * _mouseSensitivity.y;
                _rotation.x = Utils.ClampAngle(_rotation.x, _minimumRotation.x, _maximumRotation.x);
                _rotation.y = Utils.ClampAngle(_rotation.y, _minimumRotation.y, _maximumRotation.y);
                
                
                xQuaternion = Quaternion.AngleAxis(_rotation.x, xVector);
                yQuaternion = Quaternion.AngleAxis(_rotation.y, yVector);

                InteractObj.transform.localRotation = _originalRotation * xQuaternion * yQuaternion;
            }
            
            float h = Input.GetAxis("Horizontal") * _rotationSpeed;
            float v = Input.GetAxis("Vertical") * _rotationSpeed;
            
            _rotation.x += h;
            _rotation.y += v;
            _rotation.x = Utils.ClampAngle(_rotation.x, _minimumRotation.x, _maximumRotation.x);
            _rotation.y = Utils.ClampAngle(_rotation.y, _minimumRotation.y, _maximumRotation.y);
                
                
            xQuaternion = Quaternion.AngleAxis(_rotation.x, xVector);
            yQuaternion = Quaternion.AngleAxis(_rotation.y, yVector);

            InteractObj.transform.localRotation = _originalRotation * xQuaternion * yQuaternion;      
            
//            Debug.Log(_rotation);
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if(GameManager.Instance.State == GameState.Interacting) _dragging = true;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if(GameManager.Instance.State == GameState.Interacting) _dragging = false;
    }

    public void Reset()
    {
        _rotation = Vector2.zero;
    }
}
