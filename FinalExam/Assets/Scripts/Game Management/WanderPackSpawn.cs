using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderPackSpawn : MonoBehaviour
{
    public float spawnTime = 1f;
    public float DestroyTime = 50f;
    private float timer = 0f;
    public GameObject prey;
    public int quantity = 1;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < quantity; i++)
        {
            GameObject _prey = Instantiate(prey);
            _prey.transform.position = transform.position;
            Destroy(_prey, DestroyTime);
        }
    }

    // 일정 시간마다 WanderPack 스폰
    // Update is called once per frame
    void Update()
    {
        if (timer > spawnTime)
        {
            for (int i = 0; i < quantity; i++)
            {
                GameObject _prey = Instantiate(prey);
                _prey.transform.position = transform.position;
                Destroy(_prey, DestroyTime);
            }
            timer = 0;
        }
        timer += Time.deltaTime;
    }
}
