using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour
{
    #region Variables
    
    //Movement Vars
    [Range(0,10)]
    public float MoveSpeed = 1; //Maximum movement speed

    public float Gravity = -9.81f; //Gravity scalar

    public float JumpHeight = 1;
    
    [Header("Mouse Look")]
    public Vector2 MouseSensitivity = new Vector2(15f,15f);
    public Vector2 MinimumRotation = new Vector2(-360f,-60f);
    public Vector2 MaximumRotation = new Vector2(360f, 60f);
    private Vector2 _rotation = new Vector2(0f, 0f);
    private Quaternion _originalRotation;

    private Vector3 _velocity = new Vector3();
    
    //Components
    private CharacterController _charController;
    
    
   
    #endregion

    // Start is called before the first frame update
    private void Start()
    {
        //Get Components
        _charController = GetComponent<CharacterController>();
        
        //Set up Mouse Look
        _originalRotation = transform.localRotation;
    }

    // Update is called once per frame
    private void Update()
    {
       ProcessInput();
    }

    private void ProcessInput()
    {
        //Moving
        float h = Input.GetAxis("Horizontal") * MoveSpeed;
        float v = Input.GetAxis("Vertical") * MoveSpeed;

        Vector3 move = (transform.right * h + transform.forward * v) * Time.deltaTime;
        
        //Mouse Looking
        //Read Mouse input
        _rotation.x += Input.GetAxis("Mouse X") * MouseSensitivity.x;
        _rotation.y += Input.GetAxis("Mouse Y") * MouseSensitivity.y;
        _rotation.x = Utils.ClampAngle(_rotation.x, MinimumRotation.x, MaximumRotation.x);
        _rotation.y = Utils.ClampAngle(_rotation.y, MinimumRotation.y, MaximumRotation.y);
        Quaternion xQuaternion = Quaternion.AngleAxis(_rotation.x, Vector3.up);
        Quaternion yQuaternion = Quaternion.AngleAxis(_rotation.y, -Vector3.right);

        transform.localRotation = _originalRotation * xQuaternion * yQuaternion;
        
        //Jump
        if (Input.GetAxis("Jump") > 0 && _charController.isGrounded)
        {
            _velocity.y += Mathf.Sqrt(JumpHeight * -3.0f * Gravity);
        }
        
        //Apply Gravity
        _velocity.y += Gravity * Time.deltaTime;
        
        //Don't sink below the ground
        if (_charController.isGrounded  && _velocity.y < 0)
        {
            _velocity.y = 0f;
            
        }
        
        _charController.Move(move + _velocity * Time.deltaTime);
    }
}
