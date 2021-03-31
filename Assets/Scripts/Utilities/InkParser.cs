using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

public class InkParser
{
    private Story _story; //Ink object generated 

    private TextAsset _file; //Actual file

    //Constructors
    public InkParser(TextAsset file)
    {
        _file = file;
        LoadStory();
    }

    public InkParser()
    {
        _file = null;
        _story = null;
    }

    //Create story object from file
    private void LoadStory()
    {
        _story = new Story(_file.text);
        Debug.Log("Story Loaded");
    }

    //Get the next line of the story
    public string NextLine()
    {
        if (_story.canContinue)
            return _story.Continue();
        return "";
    }
    
    //Check if story can continue
    public bool CanContinue()
    {
        return _story.canContinue;
    }
    
    //Check if current line has choices
    public bool HasChoice()
    {
        return _story.currentChoices.Count > 0;
    }
    
    //Get a variable of any type from the story
    public T GetVar<T>(string varName)
    {
        T variable = default(T);
        foreach (string var in _story.variablesState) //Loop thru and get the var we want
        {
            if (varName == var)
                variable = (T) _story.variablesState[var];
        }

        return (T) variable;
    }

    //Change our file and reload story object
    public void LoadNewStory(TextAsset file)
    {
        _file = file;
        LoadStory();
    }

    //Choose a choice from current choices
    public void MakeChoice(int index)
    {
        if (index < _story.currentChoices.Count)
            _story.ChooseChoiceIndex(index);
    }

    //Change the value of a variable in the story
    public void ChangeVariable(string varName, string newData)
    {
        foreach (string var in _story.variablesState)
        {
            if (var == varName)
                _story.variablesState[var] = newData;
        }
    }

    public List<Choice> Choices()
    {
        return _story.currentChoices;
    }
}
