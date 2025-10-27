using UnityEngine;

public class FireflyController : MonoBehaviour
{
    private int flashCount = 0;
    private float dirTimer;
    private Vector2 moveDir;

    [Header("Movement")]
    public float moveSpeed = 1f;
    public Vector2 dirChangeRange = new Vector2(1f, 3f);
    public float attractSpeed = 2f;
    public float attractRadius = 3f;

    [Header("Flash")]
    public int maxFlashes = 3;
    public Color normalColor = Color.yellow;
    public Color flashColor = Color.white;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = normalColor;
        PickNewDirection();
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            float dist = Vector2.Distance(transform.position, mousePos);

            if (dist < attractRadius)
            {
                Vector3 dir = (mousePos - transform.position).normalized;
                transform.position += dir * attractSpeed * Time.deltaTime;
            }
            else
            {
                Wander();
            }
        }
        else
        {
            Wander();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            int roll = Random.Range(1, 4); // 1-3 chance
            if (roll == 1)
            {
                Flash();
            }
        }
    }

    void Wander()
    {
        transform.position += (Vector3)(moveDir * moveSpeed * Time.deltaTime);
        dirTimer -= Time.deltaTime;
        if (dirTimer <= 0f) PickNewDirection();
    }

    void PickNewDirection()
    {
        moveDir = Random.insideUnitCircle.normalized;
        dirTimer = Random.Range(dirChangeRange.x, dirChangeRange.y);
    }

    void Flash()
    {
        flashCount++;
        StartCoroutine(BlinkTwice());

        if (flashCount >= maxFlashes)
        {
            Destroy(gameObject);
        }
    }

    System.Collections.IEnumerator BlinkTwice()
    {
        for (int i = 0; i < 2; i++)
        {
            sr.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            sr.color = normalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
