using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State
{
    public virtual void Enter(Manager manager) { }
    public virtual void Execute(Manager manager) { }
    public virtual void Exit(Manager manager) { }
}