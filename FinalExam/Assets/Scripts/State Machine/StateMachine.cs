using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    Manager manager;

    private State m_pPreviousState;
    private State m_pCurrentState;
    private State m_pGlobalState;

    public StateMachine(Manager _manager)
    {
        manager = _manager;
        m_pPreviousState = null;
        m_pCurrentState = null;
        m_pGlobalState = null;
    }

    public void SetPreviousState(State m) { m_pPreviousState = m; }
    public void SetCurrentState(State m) { m_pCurrentState = m; }
    public void SetGlobalState(State m) { m_pGlobalState = m; }

    public void Update()
    {
        m_pGlobalState.Execute(manager);

        m_pCurrentState.Execute(manager);
    }

    public void ChangeState(State pNewState)
    {
        m_pPreviousState = m_pCurrentState;

        m_pCurrentState.Exit(manager);

        m_pCurrentState = pNewState;

        m_pCurrentState.Enter(manager);
    }

    public void RevertToPreviousState()
    {
        ChangeState(m_pPreviousState);
    }
}
