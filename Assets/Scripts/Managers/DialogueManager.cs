using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.PlayerLoop;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance = null;
    private InkParser _inkParser;
    public DialogueState State;
    
    //Text
    private string _currentText;
    private bool _textDone;
    [Range(0.01f, 0.1f)] public float TextSpeed = 0.05f;
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
        _inkParser = new InkParser();
    }
    
    private void Update()
    {
        if(GameManager.Instance.State == GameState.Dialogue)
        {
            switch (State)
            {
                case DialogueState.Printing:
                    PrintingUpdate();
                    break; 
                case DialogueState.Reading:
                    ReadingUpdate();
                    break;
            }
        }
    }

    private void PrintingUpdate()
    {
        if (_inkParser.CanContinue())
        {
            _currentText = _inkParser.NextLine();
            State = DialogueState.Reading;
            StartCoroutine(PrintDialogue());
        }
        else if(_inkParser.HasChoice())
        {
            State = DialogueState.Choice;
            LoadChoices();
        }
        else
        {
            State = DialogueState.End;
        }
    }

    private void ReadingUpdate()
    {
        if (_textDone && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            UIManager.Instance.FadeText();
            UIManager.Instance.SetDialogueText("");
            State = DialogueState.Printing;
            UIManager.Instance.SetDialogueColor(Color.white);
        }

    }
    
    public void LoadDialogue(TextAsset dialogue)
    {
        _inkParser.LoadNewStory(dialogue);
        State = DialogueState.Printing;
    }

    private IEnumerator PrintDialogue()
    {
        UIManager.Instance.FadeText(false);
        /*string[] words =  _currentText.Split(' ');
        string dialogue = "";
        for (int i = 0; i < words.Length; i++)
        {
            dialogue += words[i] + " ";
            UIManager.Instance.SetDialogueText(dialogue);
            if (currentCharName != "player")
                AudioManager.Instance.PlayClip("blip", characters[currentCharName].VoicePitch, characters[currentCharName].VoiceVolume);
            else
                AudioManager.Instance.PlayClip("blip", 0.7f);
            yield return new WaitForSeconds(TextSpeed);
        }*/
        UIManager.Instance.SetDialogueText(_currentText);
        yield return new WaitForSeconds(1f);
        _textDone = true;
    }

    private void LoadChoices()
    {
        UIManager.Instance.SetButtons(_inkParser.Choices().Select(x=>x.text).ToArray());
        UIManager.Instance.FadeText(false);
    }

    public void MakeChoice(int index)
    {
        _inkParser.MakeChoice(index);
        State = DialogueState.Printing;
        UIManager.Instance.SetDialogueColor(new Color(1f,0.8f,0f));
    }
}

public enum DialogueState
{
    Printing,
    Reading,
    Choice,
    End
}