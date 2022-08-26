using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resume : State
{
    public static Resume instance = new Resume();

    public override void Enter(Manager manager)
    {
        Debug.Log("Resume 모드");
        base.Enter(manager);
    }

    public override void Execute(Manager manager)
    {
        // ESC 누르면 일시 정지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!Pause.isPause)
            {
                Time.timeScale = 0;
                manager.GetFSM().ChangeState(Pause.instance);
                Pause.isPause = true;
                return;
            }
        }
    }

    public override void Exit(Manager manager)
    {
        base.Exit(manager);
    }
}

