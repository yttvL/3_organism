using UnityEngine;

public class FireflyController : MonoBehaviour
{
    private enum FireflyState
    {
        Wander,
        Flash,
        Chase
    }
    FireflyState currentState = FireflyState.Wander;

    private int flashCount = 0;
    private float dirTimer;
    private Vector2 moveDir;

    [Header("Wander")]
    public float moveSpeed = 1.5f;
    public Vector2 dirChangeRange = new Vector2(0.5f, 1f);

    [Header("Flash")]
    public int maxFlashes = 3;
    public Color normalColor = Color.yellow;
    public Color flashColor = Color.white;
    public float flashInterval = 6f;
    private float flashTimer;

    [Header("Chase")]
    public float chaseSpeed = 3f;
    public float attractRadius = 10f;
    public float followDist = 0.5f;
    public Color chaseColor = Color.white;
    private Transform currentTarget;

    private SpriteRenderer sr;
    private bool flashingNow = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = normalColor;

        PickNewDirection();

        flashTimer = flashInterval;
    }

    void Update()
    {
        switch (currentState)
        {
            case FireflyState.Wander:
                sr.color = normalColor;
                UpdateWander();
                break;

            case FireflyState.Chase:
                sr.color = chaseColor;
                UpdateChase();
                break;

            case FireflyState.Flash:
                break;
        }
    }

    void UpdateWander()
    {
        transform.position += (Vector3)(moveDir * moveSpeed * Time.deltaTime);

        dirTimer -= Time.deltaTime;
        if (dirTimer <= 0f)
            PickNewDirection();

        flashTimer -= Time.deltaTime;
        if (flashTimer <= 0f && !flashingNow)
        {
            int roll = Random.Range(1, 4);
            if (roll == 1)
            {
                StartCoroutine(BlinkAndMaybeDie());
                return;
            }
            else
            {
                flashTimer = flashInterval;
            }
        }

        PickTarget();
        if (currentTarget != null)
        {
            currentState = FireflyState.Chase;
            return;
        }
    }

    void PickNewDirection()
    {
        moveDir = Random.insideUnitCircle.normalized;
        if (moveDir.sqrMagnitude < 0.001f)
            moveDir = Vector2.right;

        dirTimer = Random.Range(dirChangeRange.x, dirChangeRange.y);
    }

    System.Collections.IEnumerator BlinkAndMaybeDie()
    {
        flashingNow = true;
        currentState = FireflyState.Flash;

        for (int i = 0; i < 2; i++)
        {
            sr.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            sr.color = normalColor;
            yield return new WaitForSeconds(0.1f);
        }

        flashCount++;
        flashTimer = flashInterval;

        if (flashCount >= maxFlashes)
        {
            Destroy(gameObject);
            yield break;
        }

        flashingNow = false;
        currentState = FireflyState.Wander;
    }


    void UpdateChase()
    {
        if (currentTarget == null)
        {
            currentState = FireflyState.Wander;
            return;
        }

        float dist = Vector2.Distance(currentTarget.position, transform.position);

        PredatorController pc = currentTarget.GetComponent<PredatorController>();
        if (pc == null ||
            !pc.isHunting ||
            dist > attractRadius)
        {
            currentTarget = null;
            currentState = FireflyState.Wander;
            return;
        }

        if (dist > followDist)
        {
            Vector3 chaseDir = (currentTarget.position - transform.position).normalized;
            transform.position += chaseDir * chaseSpeed * Time.deltaTime;
        }
    }


    void PickTarget()
    {
        currentTarget = null;
        PredatorController[] predators = Object.FindObjectsByType<PredatorController>(FindObjectsSortMode.None);

        if (predators.Length == 0) return;

        PredatorController nearest = null;
        float nearestDist = Mathf.Infinity;

        foreach (PredatorController p in predators)
        {
            if (p == null) continue;
            if (!p.isHunting)
                continue;

            float dist = Vector2.Distance(transform.position, p.transform.position);
            if (dist < nearestDist && dist <= attractRadius)
            {
                nearestDist = dist;
                nearest = p;
            }
        }

        if (nearest != null)
        {
            currentTarget = nearest.transform;
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("BoundaryWall"))
        {
            moveDir = -moveDir;
            dirTimer = Random.Range(dirChangeRange.x, dirChangeRange.y);
        }
    }
}
