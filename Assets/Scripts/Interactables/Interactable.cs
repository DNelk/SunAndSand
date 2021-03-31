using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    private MeshRenderer _MR;
    private bool _highlighted;
    private float _highlightCooldown;

    public string Name;
    public string Verb = "Interact";
    
    private void Start()
    {
        _MR = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        if (_highlightCooldown <= 0)
            Highlight = false;
        else
            _highlightCooldown -= Time.deltaTime;
    }

    public bool Highlight
    {
        get => _highlighted;
        set => ToggleOutline(value);
    }

    private void ToggleOutline(bool newValue)
    {
        if (_highlighted == newValue)
        {
            _highlightCooldown = _highlighted ? 0.05f : 0f;
            return;
        }

        _highlighted = newValue;
        
        float outline = _highlighted ? 0.05f : 0f;
        _highlightCooldown = _highlighted ? 0.05f : 0f;
        foreach (Material m in _MR.materials)
        {
            m.SetFloat("_ASEOutlineWidth", outline);
        }
    }
}
