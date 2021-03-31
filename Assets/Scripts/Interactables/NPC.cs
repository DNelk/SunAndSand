using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : Interactable
{
    private void Reset()
    {
        Verb = "Talk";
    }

    public TextAsset CurrentDialogue;
    public Transform LookTarget;
}
