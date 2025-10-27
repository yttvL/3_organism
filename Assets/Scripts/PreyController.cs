using UnityEngine;
using System.Collections;

public class PreyController : MonoBehaviour
{
    private enum PreyState
    {
        Wander,
        Pulse,
        Dying
    }

    private PreyState currentState = PreyState.Wander;


    [Header("Movement")]
    [SerializeField] private float moveSpeed = 0.4f;
    [SerializeField] private Vector2 dirDurationRange = new Vector2(1.5f, 3.0f);
    private Vector2 currentDir;
    private float dirTimer;

    [Header("Life / Size")]
    [SerializeField] private float startScale = 1.0f;
    [SerializeField] private float minScaleBeforeDeath = 0.3f;
    [SerializeField] private float shrinkRatePerSecond = 0.02f;
    [SerializeField] private float pulseScaleBoost = 0.05f;
    private float currentScale;

    [Header("Pulse Settings")]
    [SerializeField] private float pulseMaxScaleMultiplier = 3.0f;
    [SerializeField] private float pulseUpTime = 0.15f;
    [SerializeField] private float pulseHoldTime = 0.1f;
    [SerializeField] private float pulseDownTime = 0.15f;
    [SerializeField] private float pulseCooldownDuration = 1.0f;
    private float pulseCooldownTimer = 0f;
    private bool isPulsingAnim = false;


    [Header("Color")]
    [SerializeField] private Color deadGreyColor = new Color(0.2f, 0.2f, 0.25f, 1f);
    private Color baseColor;        
    private SpriteRenderer sr;
    private bool isDyingAnim = false;


    [Header("Death / Explosion")]
    [SerializeField] private float deathPopScaleMultiplier = 2.0f;
    [SerializeField] private float deathPopDuration = 0.3f;
    [SerializeField] private float deathFadeDuration = 0.3f;


    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        float h = Random.Range(0.55f, 0.75f);
        float s = Random.Range(0.6f, 1.0f);
        float v = Random.Range(0.7f, 1.0f);
        baseColor = Color.HSVToRGB(h, s, v);
        baseColor.a = 1f;

        currentScale = startScale;
        transform.localScale = Vector3.one * currentScale;

        PickNewDirection();
        UpdateColorFromScale();
    }

    void Update()
    {
        if (pulseCooldownTimer > 0f)
            pulseCooldownTimer -= Time.deltaTime;

        switch (currentState)
        {
            case PreyState.Wander:
                UpdateWander();
                break;

            case PreyState.Pulse:
                break;

            case PreyState.Dying:
                break;
        }

        if (currentState != PreyState.Dying && !isDyingAnim)
        {
            if (currentScale <= minScaleBeforeDeath)
            {
                EnterDyingState();
            }
        }

        UpdateColorFromScale();
    }

    void UpdateWander()
    {
        currentScale -= shrinkRatePerSecond * Time.deltaTime;
        if (currentScale < minScaleBeforeDeath)
            currentScale = minScaleBeforeDeath;

        transform.localScale = Vector3.one * currentScale;

        Vector3 move = new Vector3(currentDir.x, currentDir.y, 0f)
                        * moveSpeed
                        * Time.deltaTime;
        transform.position += move;

        dirTimer -= Time.deltaTime;
        if (dirTimer <= 0f)
        {
            PickNewDirection();
        }
    }

    void PickNewDirection()
    {
        currentDir = Random.insideUnitCircle.normalized;
        if (currentDir.sqrMagnitude < 0.001f)
            currentDir = Vector2.right;

        dirTimer = Random.Range(dirDurationRange.x, dirDurationRange.y);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleTrigger(other, isStay: false);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        HandleTrigger(other, isStay: true);
    }


    void HandleTrigger(Collider2D other, bool isStay)
    {
        if (other.CompareTag("BoundaryWall"))
        {
            currentDir = -currentDir;

            dirTimer = Random.Range(dirDurationRange.x, dirDurationRange.y);

            return;
        }

        PreyController otherPrey = other.GetComponent<PreyController>();
        if (otherPrey != null && otherPrey != this)
        {
            if (!isStay)
            {
                TryPulse();
            }
            else
            {
                TryPulse();
            }
        }
    }

    void TryPulse()
    {
        if (currentState == PreyState.Dying) return;
        if (isPulsingAnim) return;
        if (pulseCooldownTimer > 0f) return;

        StartCoroutine(CoPulse());
    }

    IEnumerator CoPulse()
    {
        currentState = PreyState.Pulse;
        isPulsingAnim = true;

        float startScaleBeforePulse = currentScale;
        float pulseTargetScale = startScaleBeforePulse * pulseMaxScaleMultiplier;
        float settleScale = startScaleBeforePulse + pulseScaleBoost;

        float t = 0f;
        while (t < pulseUpTime)
        {
            t += Time.deltaTime;
            float lerp01 = t / pulseUpTime;

            currentScale = Mathf.Lerp(startScaleBeforePulse, pulseTargetScale, lerp01);
            transform.localScale = Vector3.one * currentScale;

            UpdateColorFromScale();
            yield return null;
        }
        currentScale = pulseTargetScale;
        transform.localScale = Vector3.one * currentScale;
        UpdateColorFromScale();
        yield return new WaitForSeconds(pulseHoldTime);

        t = 0f;
        while (t < pulseDownTime)
        {
            t += Time.deltaTime;
            float lerp01 = t / pulseDownTime;

            currentScale = Mathf.Lerp(pulseTargetScale, settleScale, lerp01);
            transform.localScale = Vector3.one * currentScale;

            UpdateColorFromScale();
            yield return null;
        }

        currentScale = settleScale;
        transform.localScale = Vector3.one * currentScale;
        UpdateColorFromScale();

        pulseCooldownTimer = pulseCooldownDuration;
        isPulsingAnim = false;
        currentState = PreyState.Wander;
    }

    void UpdateColorFromScale()
    {
        float lifePercent = Mathf.InverseLerp(minScaleBeforeDeath, startScale, currentScale);
        lifePercent = Mathf.Clamp01(lifePercent);

        Color c = Color.Lerp(deadGreyColor, baseColor, lifePercent);

        if (!isDyingAnim)
        {
            c.a = 1f;
            sr.color = c;
        }
        else
        {
            Color dyingC = sr.color;
            dyingC.r = c.r;
            dyingC.g = c.g;
            dyingC.b = c.b;
            sr.color = dyingC;
        }
    }

    void EnterDyingState()
    {
        if (isDyingAnim) return;
        currentState = PreyState.Dying;
        StartCoroutine(CoDieExplosion());
    }

    IEnumerator CoDieExplosion()
    {
        isDyingAnim = true;
        float startS = currentScale;
        float popTargetScale = currentScale * deathPopScaleMultiplier;

        float t = 0f;
        while (t < deathPopDuration)
        {
            t += Time.deltaTime;
            float lerp01 = t / deathPopDuration;

            currentScale = Mathf.Lerp(startS, popTargetScale, lerp01);
            transform.localScale = Vector3.one * currentScale;

            UpdateColorFromScale();
            yield return null;
        }

        t = 0f;
        Color startColor = sr.color;
        while (t < deathFadeDuration)
        {
            t += Time.deltaTime;
            float lerp01 = t / deathFadeDuration;

            currentScale = popTargetScale;
            transform.localScale = Vector3.one * currentScale;

            UpdateColorFromScale();

            Color faded = sr.color;
            faded.a = Mathf.Lerp(1f, 0f, lerp01);
            sr.color = faded;

            yield return null;
        }

        Destroy(gameObject);
    }
}
