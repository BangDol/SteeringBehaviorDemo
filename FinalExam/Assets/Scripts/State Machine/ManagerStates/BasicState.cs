using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicState : State
{
    //public static bool isPause;
    public static BasicState instance = new BasicState();

    public override void Enter(Manager manager)
    {
        base.Enter(manager);
    }

    public override void Execute(Manager manager)
    {
        
    }

    public override void Exit(Manager manager)
    {
        base.Exit(manager);
    }
}
