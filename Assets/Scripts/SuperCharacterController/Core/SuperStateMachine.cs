// With a little help from UnityGems

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SuperStateMachine : MonoBehaviour
{
    private readonly Dictionary<Enum, Dictionary<string, Delegate>> _cache =
        new Dictionary<Enum, Dictionary<string, Delegate>>();

    [HideInInspector] public Enum lastState;

    public State state = new State();

    protected float timeEnteredState;

    public Enum currentState
    {
        get { return state.currentState; }
        set
        {
            if (state.currentState == value)
                return;

            ChangingState();
            state.currentState = value;
            ConfigureCurrentState();
        }
    }

    private void ChangingState()
    {
        lastState = state.currentState;
        timeEnteredState = Time.time;
    }

    private void ConfigureCurrentState()
    {
        if (state.exitState != null)
            state.exitState();

        //Now we need to configure all of the methods
        state.DoSuperUpdate = ConfigureDelegate<Action>("SuperUpdate", DoNothing);
        state.enterState = ConfigureDelegate<Action>("EnterState", DoNothing);
        state.exitState = ConfigureDelegate<Action>("ExitState", DoNothing);

        if (state.enterState != null)
            state.enterState();
    }

    private T ConfigureDelegate<T>(string methodRoot, T Default) where T : class
    {
        Dictionary<string, Delegate> lookup;
        if (!_cache.TryGetValue(state.currentState, out lookup))
            _cache[state.currentState] = lookup = new Dictionary<string, Delegate>();
        Delegate returnValue;
        if (!lookup.TryGetValue(methodRoot, out returnValue))
        {
            var mtd = GetType().GetMethod(state.currentState + "_" + methodRoot, BindingFlags.Instance
                                                                                 | BindingFlags.Public |
                                                                                 BindingFlags.NonPublic |
                                                                                 BindingFlags.InvokeMethod);

            if (mtd != null)
                returnValue = Delegate.CreateDelegate(typeof(T), this, mtd);
            else
                returnValue = Default as Delegate;
            lookup[methodRoot] = returnValue;
        }
        return returnValue as T;
    }

    private void SuperUpdate()
    {
        EarlyGlobalSuperUpdate();

        state.DoSuperUpdate();

        LateGlobalSuperUpdate();
    }

    protected virtual void EarlyGlobalSuperUpdate()
    {
    }

    protected virtual void LateGlobalSuperUpdate()
    {
    }

    private static void DoNothing()
    {
    }

    public class State
    {
        public Enum currentState;
        public Action DoSuperUpdate = DoNothing;
        public Action enterState = DoNothing;
        public Action exitState = DoNothing;
    }
}