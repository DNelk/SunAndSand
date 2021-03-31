using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance = null;

    private Image _reticle;

    //Tooltips
    private GameObject _tooltips;
    private Image _interactTip;
    private TMP_Text[] _interactTexts;
    private float _interactCooldown;
    
    //Dialogue
    private Animator _dialogueAnim;
    private TMP_Text _dialogueText;
    private TMP_Text[] _buttonTexts;
    private Button[] _dialogueButtons;
    
    //Interaction
    private CanvasGroup _inspectCG;
    private RotateInteractObj _inspectImg;
    private TMP_Text _inspectName;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        Init();
    }

    private void Init()
    {
        _reticle = GameObject.Find("Reticle").GetComponent<Image>();
        //Tooltips
        _tooltips = GameObject.Find("Tooltips");
        _interactTip = _tooltips.transform.Find("Interact").GetComponent<Image>();
        _interactTexts = _interactTip.GetComponentsInChildren<TMP_Text>();
        //Dialogue
        _dialogueAnim = transform.Find("Dialogue").GetComponent<Animator>();
        _dialogueText = GameObject.Find("DialogueText").GetComponent<TMP_Text>();
        _dialogueButtons = GameObject.FindGameObjectsWithTag("DialogueButton").Select(x => x.GetComponent<Button>())
            .ToArray();
        _buttonTexts = new TMP_Text[_dialogueButtons.Length];
        for (int i = 0; i < _dialogueButtons.Length; i++)
        {
            _buttonTexts[i] = _dialogueButtons[i].gameObject.GetComponentInChildren<TMP_Text>();
            _buttonTexts[i].text = "";
            _dialogueButtons[i].gameObject.SetActive(false);
        }
        //Interact
        _inspectCG = transform.Find("Inspection").GetComponent<CanvasGroup>();
        _inspectImg = _inspectCG.GetComponentInChildren<RotateInteractObj>();
        _inspectImg.gameObject.SetActive(false);
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        _interactTip.gameObject.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        switch (GameManager.Instance.State)
        {
            case GameState.Exploring:
                if (_interactCooldown < 0 && _interactTip.IsActive())
                    _interactTip.gameObject.SetActive(false);
                else
                    _interactCooldown -= Time.deltaTime;
                break;
            case GameState.Dialogue:
                if (DialogueManager.Instance.State == DialogueState.Choice)
                {
                    bool choiceMade = false;
                    for (int i = 0; i < _dialogueButtons.Length && !choiceMade; i++)
                    {
                        if(!_dialogueButtons[i].gameObject.activeSelf)
                            continue;
                        if (Input.GetKeyDown((i + 1) + ""))
                        {
                            MakeChoice(i);
                            choiceMade = true;
                        }
                    
                    }
                }
                break;
            case GameState.Interacting:
                if (Input.GetButtonDown("Exit"))
                {
                    CloseInteract();
                }
                break;
        }
    }

    public void ShowInteractTip(string name, string verb)
    {
        if(!_interactTip.IsActive())
            _interactTip.gameObject.SetActive(true);

        _interactTexts[0].text = name;
        _interactTexts[1].text = "Press [E] to " + verb;
        _interactCooldown = 0.05f;
    }

    public bool Reticle
    {
        get => _reticle.IsActive();
        set => _reticle.gameObject.SetActive(value);
    }
    
    #region Dialogue
    public float ToggleDialogue()
    {
        _dialogueAnim.SetTrigger("ToggleDialogue");
        return 0.167f;
    }

    public void SetDialogueText(string dialogue)
    {
        _dialogueText.text = dialogue;
    }
    public void SetDialogueColor(Color color)
    {
        _dialogueText.color = color;
    }

    public void SetButtons(string[] choices)
    {
        for (int i = 0; i < choices.Length; i++)
        {
            _dialogueButtons[i].gameObject.SetActive(true);
            _buttonTexts[i].text = (i+1) + ": " + choices[i];
        }
    }

    public void FadeText(bool fadeOut = true)
    {
        float alpha = fadeOut ? 0f : 1f;
        _dialogueText.DOFade(alpha, 0.1f);
        foreach (var b in _buttonTexts)
        {
            b.DOFade(alpha, 0.1f);
        }
    }

    public void MakeChoice(int index)
    {
//        Debug.Log(index);
        DialogueManager.Instance.MakeChoice(index);
        FadeText();
        foreach (var b in _dialogueButtons)
        {
            b.gameObject.SetActive(false);
        }
    }
    #endregion

    #region Interaction

    public float ToggleInteract(GameObject obj = null)
    {
        float animTime;
        var RTCam = GameObject.FindWithTag("RTCam");
        if (_inspectCG.alpha == 0)
        {
            //Add object to camera
            var go = Instantiate(obj, RTCam.transform);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localPosition = Vector3.zero + (Vector3.forward*2f);
            
            //Enable RT
            _inspectImg.InteractObj = go;
            _inspectImg.gameObject.SetActive(true);
            //Fade in
            animTime = 0.5f;
            _inspectCG.DOFade(1f, animTime);
            _inspectCG.interactable = true;
        }
        else
        {
            //Remove object from camera
            Destroy(RTCam.transform.GetChild(0).gameObject);
            //Disable RT
            _inspectImg.Reset();
            _inspectImg.gameObject.SetActive(false);
            //Fade out
            animTime = 0.2f;
            _inspectCG.DOFade(0f, animTime);
            _inspectCG.interactable = false;
        }

       
        return animTime;
    }

    public void CloseInteract()
    {
        EventManager.Instance.Fire(new ExitInteractEvent());
    }

    #endregion
}
