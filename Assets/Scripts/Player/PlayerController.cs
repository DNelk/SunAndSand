using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Ink.Parsed;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.XR.WSA.Input;

[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour
{
    #region Variables
    
    [Header("Movement")]
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

    [Header("Interaction")] 
    public float InteractDistance = 10f;

    public float InteractRadius = 5f;
    public GameObject InteractingObj;
    
    //Components
    private CharacterController _charController;
    private Camera _myCamera;
    
    
   
    #endregion

    // Start is called before the first frame update
    private void Start()
    {
        //Get Components
        _charController = GetComponent<CharacterController>();
        
        //Set up Mouse Look
        _originalRotation = transform.localRotation;

        _myCamera = transform.GetComponentInChildren<Camera>();
        
        //Set up some events
        EventManager.Instance.AddHandler<ExitInteractEvent>(OnExitInteract);
    }
    
    // Update is called once per frame
    private void Update()
    {
        switch (GameManager.Instance.State)
        {
            case GameState.Exploring:
                ExplorationUpdate();
                break;
            case GameState.Dialogue:
                DialogueUpdate();
                break;
            case GameState.Interacting:
                InteractUpdate();
                break;
        }
    }

    private void ExplorationUpdate()
    {
        //Check for inputs
        ProcessMovement();
        
        //Check for Interaction
        CheckInteract();
    }

    private void DialogueUpdate()
    {
        if (DialogueManager.Instance.State == DialogueState.End)
            StartCoroutine(EndDialogue());
    }

    private Vector3 _lastMousePos;
    private void InteractUpdate()
    {
        if (Input.GetButtonDown("Interact"))
            StartCoroutine(EndInteract());
    }

    private void ProcessMovement()
    {
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

    private RaycastHit CheckInteract()
    {
        
        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, transform.TransformDirection(Vector3.forward), out hit,
            InteractDistance))
        {
            //NPCs
            if (hit.collider.CompareTag("NPC"))
            {
                NPC npc = hit.collider.GetComponent<NPC>();
                //Put outline on npc

                npc.Highlight = true;
                
                //Display tooltip
                UIManager.Instance.ShowInteractTip(npc.Name, npc.Verb);
                if (Input.GetButtonDown("Interact"))
                {
                    //Start Dialog
                    StartCoroutine(StartDialogue(npc.CurrentDialogue, npc.LookTarget));
                }
                
                //Debug.Log("hit npc");
            }

            if (hit.collider.CompareTag("InteractableObj"))
            {
                InteractableObj obj = hit.collider.GetComponent<InteractableObj>();

                obj.Highlight = true;
                
                //Display tooltip
                UIManager.Instance.ShowInteractTip(obj.Name, obj.Verb);
                if (Input.GetButtonDown("Interact"))
                {
                    //Start Interact
                    Debug.Log("pick up");
                    StartCoroutine(StartInteract(obj.gameObject));
                }
            }
        }
        
        return hit;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
       // Gizmos.DrawWireSphere(transform.position + transform.TransformDirection(Vector3.forward)*InteractDistance, InteractRadius);
        //Gizmos.DrawRay(Camera.main.transform.position, transform.TransformDirection(Vector3.forward)*InteractDistance);
    }

    private IEnumerator StartDialogue(TextAsset dialogue, Transform lookTarget)
    {
        UIManager.Instance.Reticle = false;
        GameManager.Instance.State = GameState.Transition;
        DialogueManager.Instance.LoadDialogue(dialogue);
        Tween tween;

        yield return new WaitForSeconds(UIManager.Instance.ToggleDialogue());
        
        Vector3 targetDirection = lookTarget.position - _myCamera.transform.position;
        Vector3 newDirection = Vector3.RotateTowards(_myCamera.transform.forward, targetDirection, 1f, 0f);
        tween =
            _myCamera.transform.DORotate(Quaternion.LookRotation(newDirection).eulerAngles,0.5f);
        yield return tween.WaitForCompletion();
        GameManager.Instance.State = GameState.Dialogue;
        DialogueManager.Instance.State = DialogueState.Printing;
    }
    
    private IEnumerator EndDialogue()
    {
        GameManager.Instance.State = GameState.Transition;
        Tween tween;

        yield return new WaitForSeconds(UIManager.Instance.ToggleDialogue());
        
        tween = _myCamera.transform.DOLocalRotate(Vector3.zero, 0.5f);
        yield return tween.WaitForCompletion();
        GameManager.Instance.State = GameState.Exploring;
        UIManager.Instance.Reticle = true;

    }

    private IEnumerator StartInteract(GameObject obj)
    {
        UIManager.Instance.Reticle = false;
        GameManager.Instance.State = GameState.Transition;

        yield return new WaitForSeconds(UIManager.Instance.ToggleInteract(obj));

        GameManager.Instance.State = GameState.Interacting;
    }

    //Helper method so we can end interact from other places
    private void OnExitInteract(GameEvent evt)
    {
        StartCoroutine(EndInteract());
    }
    
    private IEnumerator EndInteract()
    {
        GameManager.Instance.State = GameState.Transition;

        yield return new WaitForSeconds(UIManager.Instance.ToggleInteract());

        GameManager.Instance.State = GameState.Exploring;
        UIManager.Instance.Reticle = true;
    }
}
