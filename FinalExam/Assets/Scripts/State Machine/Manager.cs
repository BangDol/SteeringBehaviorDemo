using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager
{
    private StateMachine m_pStateMachine;

    public Manager()
    {
        m_pStateMachine = new StateMachine(this);
        m_pStateMachine.SetCurrentState(BasicState.instance);
        m_pStateMachine.SetGlobalState(Resume.instance);
    }

    public StateMachine GetFSM() { return m_pStateMachine; }

    public void Update()
    {
        m_pStateMachine.Update();
    }
}
