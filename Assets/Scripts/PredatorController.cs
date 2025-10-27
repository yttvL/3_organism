using UnityEngine;
using System.Collections;

public class PredatorController : MonoBehaviour
{
    private enum PredatorState
    {
        Rest,
        Hunt,
        Dying
    }

    PredatorState currentState = PredatorState.Rest;

    [Header("Timing")]
    public float restDuration = 2f;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float attackRange = 0.4f;

    [Header("Death")]
    public int killsToDie = 2;
    public float shrinkSpeed = 1f;

    private int kills = 0;
    private float restTimer = 0f;

    private Transform currentTarget;
    private float currentScale = 1f;
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        currentScale = 0.7f;
        transform.localScale = Vector3.one * currentScale;

        currentState = PredatorState.Rest;
        restTimer = restDuration;
    }

    void Update()
    {
        switch (currentState)
        {
            case PredatorState.Rest:
                UpdateRest();
                break;

            case PredatorState.Hunt:
                UpdateHunt();
                break;

            case PredatorState.Dying:
                UpdateDying();
                break;
        }
    }

    void UpdateRest()
    {
        sr.color = Color.black;
        restTimer -= Time.deltaTime;
        if (restTimer <= 0f)
        {
            PickTarget();
            if (currentTarget != null)
            {
                currentState = PredatorState.Hunt;
            }
            else
            {
                restTimer = restDuration;
            }
        }
    }

    void PickTarget()
    {
        PreyController[] preys = Object.FindObjectsByType<PreyController>(FindObjectsSortMode.None);
        if (preys.Length == 0)
        {
            currentTarget = null;
            return;
        }

        PreyController nearest = null;
        float nearestDist = Mathf.Infinity;

        foreach (PreyController p in preys)
        {
            if (p == null) continue;
            float dist = Vector2.Distance(transform.position, p.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = p;
            }
        }

        if (nearest != null)
        {
            currentTarget = nearest.transform;
        }
        else
        {
            currentTarget = null;
        }
    }

    void UpdateHunt()
    {
        sr.color = Color.red;
        if (currentTarget == null)
        {
            GoRestAgain();
            return;
        }

        Vector3 dir = (currentTarget.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;

        float dist = Vector2.Distance(transform.position, currentTarget.position);
        if (dist <= attackRange)
        {
            KillTarget();
        }
    }

    void KillTarget()
    {
        if (currentTarget != null)
        {
            Destroy(currentTarget.gameObject);
        }

        kills += 1;
        currentTarget = null;

        if (kills >= killsToDie)
        {
            currentState = PredatorState.Dying;
        }
        else
        {
            GoRestAgain();
        }
    }

    void GoRestAgain()
    {
        currentState = PredatorState.Rest;
        restTimer = restDuration;
    }

    void UpdateDying()
    {
        currentScale -= shrinkSpeed * Time.deltaTime;
        if (currentScale <= 0.05f)
        {
            Destroy(gameObject);
            return;
        }

        transform.localScale = Vector3.one * currentScale;
    }
}
