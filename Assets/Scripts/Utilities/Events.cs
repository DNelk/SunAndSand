using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
// Large parts taken from Tim Miller's post on Type Safe Events in Unity 3D: http://www.willrmiller.com/?p=87

/// <summary>
/// Base class for events
/// </summary>
public abstract class GameEvent { }

public class EventManager
{
    //Singleton stuff
    private static EventManager _instance;

    public static EventManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new EventManager();
            return _instance;
        }
    }
    
    //Delegates
    public delegate void EventDelegate<T>(T e) where T : GameEvent;
    private delegate void EventDelegate(GameEvent e);
    
    //Registry of event types and their handlers
    private readonly Dictionary<Type, EventDelegate> _registeredHandlers = new Dictionary<Type, EventDelegate>();
    private readonly Dictionary<Delegate, EventDelegate> _delegateLookup = new Dictionary<Delegate, EventDelegate>();

    
    //Add handlers to events
    public void AddHandler<T>(EventDelegate<T> handler) where T : GameEvent
    {
        if(_delegateLookup.ContainsKey(handler)) 
            return;

        Type type = typeof(T);

        EventDelegate internalDel = (e) => handler((T) e);
        _delegateLookup[handler] = internalDel;
        
        EventDelegate tempDel;

        if (_registeredHandlers.TryGetValue(type, out tempDel))
            _registeredHandlers[type] = tempDel += internalDel;
        else
            _registeredHandlers[type] = internalDel;
    }
    
    //Remove a handler from an event type
    public void RemoveHandler<T>(EventDelegate<T> handler) where T : GameEvent
    {
        Type type = typeof(T);

        EventDelegate internalDel;
        if (_delegateLookup.TryGetValue(handler, out internalDel))
        {
            EventDelegate tempDel;
            if (_registeredHandlers.TryGetValue(type, out tempDel))
            {
                tempDel -= internalDel;
                if (tempDel == null)
                    _registeredHandlers.Remove(type);
                else
                    _registeredHandlers[type] = tempDel;
            }
        }
    }
    
    //Call all an event's delegates
    public void Fire(GameEvent e)
    {
        Type type = e.GetType();
        EventDelegate handlers;

        if (_registeredHandlers.TryGetValue(type, out handlers))
            handlers(e);
    }
}

#region Events

public class ExitInteractEvent : GameEvent { }

#endregion
