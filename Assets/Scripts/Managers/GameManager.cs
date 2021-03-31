using System.Collections;
using System.Collections.Generic;
using Ink.Parsed;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;
    public GameState State = GameState.Exploring;
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
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public enum GameState
{
    Exploring,
    Interacting,
    Dialogue,
    Paused,
    Transition
}
