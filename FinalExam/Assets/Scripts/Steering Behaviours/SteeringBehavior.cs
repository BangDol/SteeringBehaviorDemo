using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringBehavior : MonoBehaviour
{
    [Header ("General")]
    public float MaxSpeed = 2.0f;
    public float deceleration;
    //public bool enableVectorGizmo;

    [Header ("Steering Behaviours Options")]
    public bool seek;
    public bool flee;
    public bool arrive;
    public bool pursuit;
    public bool evade;
    public bool wander;
    public bool offsetpursuit;
    public bool interpose;
    public bool wallAvoidance;

    private Rigidbody2D rigid;
    private Camera camera;

    [Header ("Target Object")]
    public GameObject targetObj;
    private Vector3 targetPos;

    // for Flee
    [Header ("For Flee")]
    [Range(1f, 15f)]
    public float panicDist = 0f;
    private GameObject[] evadeTargets;
    private Vector2[] evadeTargetPos;
    private int playerAmount;

    // for Prey Pursuit
    [Header("For Prey Pursuit")]
    public bool preyPursuit;
    public bool enableLineToPrey;
    public float detectRange;
    private GameObject[] pursuitTargets;
    private Vector2[] pursuitTargetPos;
    private int pursuitTargetAmount;

    // for Wander
    [Header ("For Wander")]
    public float m_dWanderJitter = 1.0f;
    public float m_dWanderRadius = 1.0f;
    public float m_dWanderDistance = 1.0f;
    private Vector2 m_vWanderTarget;
    private Vector3 Target;
    public bool enableCircleForWander;
    public int segments = 100;

    // for Offset Pursuit
    [Header("For Offset Pursuit")]
    public GameObject leader;
    private Vector2 offset;
    public float maxPrediction = 1f;

    // for Interpose
    [Header("For Interpose")]
    public GameObject agentA;
    public GameObject agentB;

    // for Wall Aviodance
    [Header("For Wall Aviodance")]
    [Range(1f, 15f)]
    public float rayLength;
    [SerializeField]
    Transform wall;
    public float wallHitForce;
    public bool enableWallAvoidanceRay;

    void Start()
    {
        camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        rigid = GetComponent<Rigidbody2D>();
        rigid.gravityScale = 0.0f;

        // For Evade
        evadeTargets = GameObject.FindGameObjectsWithTag("leader");
        playerAmount = evadeTargets.Length;
        evadeTargetPos = new Vector2[playerAmount];

        for (int i = 0; i < playerAmount; i++)
        {
            evadeTargetPos[i] = evadeTargets[i].transform.position;
        }

        // For Prey Pursuit
        pursuitTargets = GameObject.FindGameObjectsWithTag("prey");
        pursuitTargetAmount = pursuitTargets.Length;
        pursuitTargetPos = new Vector2[pursuitTargetAmount];

        for (int i = 0; i < pursuitTargetAmount; i++)
        {
            pursuitTargetPos[i] = pursuitTargets[i].transform.position;
        }

        // 타켓 지정 안하면 0으로 초기화
        // 지정했으면 지정한 오브젝트의 위치로 초기화
        if (!targetObj)
        {
            targetPos = Vector3.zero;
        }
        else
        {
            targetPos = targetObj.transform.position;
        }

        // for Wander
        float angle = 360 * Mathf.Deg2Rad;
        m_vWanderTarget = new Vector2(m_dWanderRadius * Mathf.Cos(angle), m_dWanderRadius * Mathf.Sin(angle));

        // for Offset Pursuit
        if (offsetpursuit)
        {
            offset = transform.position - leader.transform.position;
            offset = Quaternion.Inverse(leader.transform.rotation) * offset;
        }
    }

    void Update()
    {
        UpdateVariousTargets();
        ApplyForceToAgent();
    }

    // Steering Behaviours 끄고 켜기
    private void ApplyForceToAgent()
    {
        // 방향은 전방
        transform.up = rigid.velocity;

        // 힘 적용
        if (seek)
            rigid.AddForce(Seek(targetPos) * Time.deltaTime);
        if (flee)
            rigid.AddForce(Flee(targetPos) * Time.deltaTime);
        if (arrive)
            rigid.AddForce(Arrive(targetPos, deceleration) * Time.deltaTime);
        if (pursuit)
            rigid.AddForce(Pursuit(targetPos) * Time.deltaTime);
        if (evade)
        {
            for (int i = 0; i < playerAmount; i++)
            {
                rigid.AddForce(Evade(evadeTargetPos[i]) * Time.deltaTime);
            }
        }
        if (wander)
        {
            rigid.AddForce(Wander() * Time.deltaTime);
            if (enableCircleForWander)
            {
                DrawCircleforWander();
            }
        }
        if (offsetpursuit)
            rigid.AddForce(OffsetPursuit(leader, offset) * Time.deltaTime);
        if (interpose)
            rigid.AddForce(Interpose(agentA, agentB) * Time.deltaTime);
        if (wallAvoidance)
        {
            WallAvoidance();
            EnableWallAvoidanceRay();
        }
        if (preyPursuit)
        {
            UpdatePreyPursuit();
            rigid.AddForce(PreyPursuit(pursuitTargetPos, pursuitTargets) * Time.deltaTime);
        }
    }

    // 여러 종류의 타겟의 위치 업데이트
    private void UpdateVariousTargets()
    {
        // 타켓으로 지정된 오브젝트 없으면 마우스의 위치로 대체
        if (!targetObj)
        {
            if (Input.GetMouseButton(1))
            {
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;


                if (Physics.Raycast(ray, out hit, 10000f))
                {
                    targetPos = hit.point;
                }
            }
        }
        // 타켓이 있으면 해당 오브젝트의 위치로 설정
        else
        {
            targetPos = targetObj.transform.position;
        }

        // Evade 타겟의 위치 업데이트
        for (int i = 0; i < evadeTargets.Length; i++)
        {
            evadeTargetPos[i] = evadeTargets[i].transform.position;
        }
    }

    // 새로운 Prey가 생성될 때마다 Prey 개수 업데이트
    private void UpdatePreyPursuit()
    {
        pursuitTargets = GameObject.FindGameObjectsWithTag("prey");
        pursuitTargetAmount = pursuitTargets.Length;
        pursuitTargetPos = new Vector2[pursuitTargetAmount];

        for (int i = 0; i < pursuitTargetAmount; i++)
        {
            pursuitTargetPos[i] = pursuitTargets[i].transform.position;
        }
    }

    // For Debug ---------------------------------

    // Draw Circle
    private void DrawDebugCircle(Vector3 pos, float radius, Color color)
    {
        Vector3 start = new Vector3();
        start.x = Mathf.Sin(0f);
        start.y = Mathf.Cos(0f);

        Vector3 end = new Vector3();
        for (int i = 0; i <= segments; i++)
        {
            end.x = Mathf.Sin(2f * Mathf.PI * i / segments);
            end.y = Mathf.Cos(2f * Mathf.PI * i / segments);
            Debug.DrawLine(start * radius + pos, end * radius + pos, color);
            start = end;
        }
    }

    // Draw Debugging Circle for "Wander"
    private void DrawCircleforWander()
    {
        DrawDebugCircle(Target, 0.1f, Color.red);
        DrawDebugCircle(transform.position + transform.up * m_dWanderDistance, m_dWanderRadius, Color.blue);
    }

    // Draw Detection Line for "Wall Avoidance"
    private void EnableWallAvoidanceRay()
    {
        if (enableWallAvoidanceRay)
        {
            Debug.DrawRay(transform.position, rayLength * transform.up, Color.black);
            Debug.DrawRay(transform.position, Quaternion.Euler(0f, 0f, 20f) * transform.up * rayLength, Color.black);
            Debug.DrawRay(transform.position, Quaternion.Euler(0f, 0f, -20f) * transform.up * rayLength, Color.black);
        }
    }

    // Draw Line To Prey for "Prey Pursuit"
    private void DrawLineToPrey(Vector2 closestPreyPos)
    {
        if(enableLineToPrey)
            Debug.DrawLine(transform.position, closestPreyPos, Color.red);
    }




    // Steering Behaviours -----------------------------------------------------------------------------

    // Seek -------------------------------------
    Vector2 Seek(Vector3 targetPos)
    {
        Vector2 DesiredVelocity = Vector3.Normalize(targetPos - transform.position) * MaxSpeed;
        return (DesiredVelocity - rigid.velocity);
    }

    // Flee -------------------------------------
    Vector2 Flee(Vector3 targetPos)
    {
        Vector2 DesiredVelocity = Vector3.Normalize(transform.position - targetPos) * MaxSpeed;
        float DistanceFromTarget = Vector3.Distance(transform.position, targetPos);

        if (DistanceFromTarget < panicDist)
        {
            return (DesiredVelocity - rigid.velocity);
        }

        return Vector3.zero;
    }

    // Arrive -------------------------------------
    Vector2 Arrive(Vector3 targetPos, float deceleration)
    {
        Vector2 ToTarget = targetPos - transform.position;
        float dist = ToTarget.magnitude;

        if(dist > 0)
        {
            float speed = dist / (float)deceleration;
            speed = Mathf.Min(speed, MaxSpeed);
            Vector2 DesiredVelocity = ToTarget * speed / dist;
            return (DesiredVelocity - rigid.velocity);
        }

        return new Vector2(0f, 0f);
    }

    // Pursuit -------------------------------------
    Vector2 Pursuit(Vector3 targetPos)
    {
        Vector2 ToEvader = targetPos - transform.position;

        float lookAheadTime = ToEvader.magnitude / (MaxSpeed + targetPos.magnitude);
        return Seek(targetPos + (targetPos.normalized * lookAheadTime));
    }

    // Evade
    Vector2 Evade(Vector3 targetPos)
    {
        Vector2 ToEvader = targetPos - transform.position;

        float lookAheadTime = ToEvader.magnitude / (MaxSpeed + targetPos.magnitude);

        return Flee(targetPos + (targetPos.normalized* lookAheadTime));
    }

    // Wander -------------------------------------
    Vector2 Wander()
    {
        m_vWanderTarget.x += Random.Range(-1f, 1f) * m_dWanderJitter;
        m_vWanderTarget.y += Random.Range(-1f, 1f) * m_dWanderJitter;

        m_vWanderTarget.Normalize();

        m_vWanderTarget *= m_dWanderRadius;

        Vector3 localTarget = m_vWanderTarget + new Vector2(0, m_dWanderDistance);
        Target = Matrix4x4.Rotate(transform.rotation) * localTarget;
        Target += transform.position;

        return Seek(Target);
    }

    // Offset Pursuit -------------------------------------
    Vector2 OffsetPursuit(GameObject leader, Vector2 offset)
    {
        Vector2 worldOffsetPos = leader.transform.position + leader.transform.TransformDirection(offset);
        Vector2 displacement = worldOffsetPos - (Vector2)transform.position;
        float distance = displacement.magnitude;

        float speed = rigid.velocity.magnitude;

        float prediction;
        if (speed <= distance / maxPrediction)
        {
            prediction = maxPrediction;
        }
        else
        {
            prediction = distance / speed;
        }

        Vector2 TargetPos = worldOffsetPos + leader.GetComponent<Rigidbody2D>().velocity;
        return Arrive(TargetPos, deceleration);
    }

    // Interpose -------------------------------------
    Vector2 Interpose(GameObject AgentA, GameObject AgentB)
    {
        Vector2 MidPoint = (AgentA.transform.position + AgentB.transform.position) / 2.0f;

        float TimeToReachMidPoint = Vector2.Distance(transform.position, MidPoint) / MaxSpeed;

        Vector2 Apos = (Vector2)AgentA.transform.position + AgentA.GetComponent<Rigidbody2D>().velocity * TimeToReachMidPoint;
        Vector2 Bpos = (Vector2)AgentB.transform.position + AgentB.GetComponent<Rigidbody2D>().velocity * TimeToReachMidPoint;

        MidPoint = (Apos + Bpos) / 2.0f;

        return Arrive(MidPoint, deceleration);
    }

    // Wall Avoidance -------------------------------------
    private void WallAvoidance()
    {
        RaycastHit2D hit2D = Physics2D.Raycast(transform.position, transform.up, rayLength);
        RaycastHit2D hit2DLeft = Physics2D.Raycast(transform.position, Quaternion.Euler(0f, 0f, 20f) * transform.up, rayLength);
        RaycastHit2D hit2DRight = Physics2D.Raycast(transform.position, Quaternion.Euler(0f, 0f, -20f) * transform.up, rayLength);

        if (hit2D)
        {
            Vector2 normalForce = (1f / hit2D.distance) * hit2D.normal;
            Vector2 steeringForce = wallHitForce * normalForce * Time.deltaTime;

            // 예외 처리
            if (steeringForce.x == Vector2.positiveInfinity.x || steeringForce.x == Vector2.negativeInfinity.x
                || steeringForce.y == Vector2.positiveInfinity.y || steeringForce.y == Vector2.negativeInfinity.y
                || float.IsNaN(steeringForce.x) || float.IsNaN(steeringForce.y))
                return;
            rigid.AddForce(wallHitForce * normalForce * Time.deltaTime);
        }
        if (hit2DLeft)
        {
            Vector2 normalForce = (1f / hit2DLeft.distance) * hit2DLeft.normal;
            Vector2 steeringForce = wallHitForce * normalForce * Time.deltaTime;

            // 예외 처리
            if (steeringForce.x == Vector2.positiveInfinity.x || steeringForce.x == Vector2.negativeInfinity.x
                || steeringForce.y == Vector2.positiveInfinity.y || steeringForce.y == Vector2.negativeInfinity.y
                || float.IsNaN(steeringForce.x) || float.IsNaN(steeringForce.y))
                return;
            rigid.AddForce(wallHitForce * normalForce * Time.deltaTime);
        }
        if (hit2DRight)
        {
            Vector2 normalForce = (1f / hit2DRight.distance) * hit2DRight.normal;
            Vector2 steeringForce = wallHitForce * normalForce * Time.deltaTime;

            // 예외 처리
            if (steeringForce.x == Vector2.positiveInfinity.x || steeringForce.x == Vector2.negativeInfinity.x
                || steeringForce.y == Vector2.positiveInfinity.y || steeringForce.y == Vector2.negativeInfinity.y
                || float.IsNaN(steeringForce.x) || float.IsNaN(steeringForce.y))
                return;
            rigid.AddForce(wallHitForce * normalForce * Time.deltaTime);
        }
    }

    // Prey Pursuit -------------------------------------------
    Vector2 PreyPursuit(Vector2[] TargetPos, GameObject[] Targets)
    {
        GameObject closestTarget = Targets[0];
        Vector2 closestTargetPos = Targets[0].transform.position;

        for(int i = 0; i < pursuitTargetAmount; i++)
        {
            // 거리 비교하여 가장 가까운 Prey의 벡터 구하기
            if (Vector2.Distance(transform.position, closestTarget.transform.position) > Vector2.Distance(transform.position, Targets[i].transform.position))
            {
                // 똑같은 색이면 가장 가까이 있어도 무시
                if (Targets[i].GetComponent<SpriteRenderer>().color
                    == gameObject.GetComponent<SpriteRenderer>().color)
                {
                    //Debug.Log("Same Color. Ignore this to continue");
                    break;
                }

                closestTarget = Targets[i];
                closestTargetPos = Targets[i].transform.position;
            }
        }

        if (detectRange > Vector2.Distance(transform.position, closestTarget.transform.position))
        {
            closestTarget.gameObject.GetComponent<SpriteRenderer>().color = gameObject.GetComponent<SpriteRenderer>().color;
        }

        DrawLineToPrey(closestTargetPos);

        Vector2 ToEvader = closestTargetPos - (Vector2)transform.position;

        float lookAheadTime = ToEvader.magnitude / (MaxSpeed + closestTargetPos.magnitude);

        return Seek(closestTargetPos + (closestTargetPos.normalized * lookAheadTime));
    }
}