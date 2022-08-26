using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : State
{
    public static bool isPause = false;
    public static Pause instance = new Pause();

    public override void Enter(Manager manager)
    {
        Debug.Log("Pause 모드");
        base.Enter(manager);
    }

    public override void Execute(Manager manager)
    {
        // 백스페이스 누르면 일시 정지 풀림
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (isPause)
            {
                Time.timeScale = 1;
                manager.GetFSM().RevertToPreviousState();
                isPause = false;
                return;
            }
        }
    }

    public override void Exit(Manager manager)
    {
        base.Exit(manager);
    }
}
