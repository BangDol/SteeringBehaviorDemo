using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Manager manager;

    // Start is called before the first frame update
    void Start()
    {
        manager = new Manager();
    }

    // Update is called once per frame
    void Update()
    {
        manager.Update();
    }
}
