using System.Collections;
using UnityEngine;

public class GrowObstacle : MonoBehaviour
{
    // These settings remain the same, adjustable in the Inspector
    [SerializeField] private Vector3 targetScale = new Vector3(2f, 2f, 1f);
    [SerializeField] private float duration = 2.0f;
    [SerializeField] private bool loop = false;

    private Vector3 initialScale;
    private bool hasStartedScaling = false; // Flag to ensure it only runs once

    void Awake()
    {
        // Store the initial scale when the object is created
        initialScale = transform.localScale;
    }

    // This function is called automatically by Unity when the object is seen by a camera
    void OnBecameVisible()
    {
        // If we haven't started scaling yet, start now!
        if (!hasStartedScaling)
        {
            hasStartedScaling = true; // Set the flag so this won't run again
            StartCoroutine(ScaleRoutine());
        }
    }

    private IEnumerator ScaleRoutine()
    {
        // The delay is removed as the trigger is now visibility, not time
        while (true) 
        {
            // Grow to target scale
            yield return StartCoroutine(ScaleToTarget(targetScale, duration));

            if (!loop)
            {
                yield break; // Stop if not looping
            }

            // Shrink back to initial scale
            yield return StartCoroutine(ScaleToTarget(initialScale, duration));
        }
    }

    // This helper coroutine is unchanged
    private IEnumerator ScaleToTarget(Vector3 endScale, float time)
    {
        float elapsedTime = 0;
        Vector3 startingScale = transform.localScale;

        while (elapsedTime < time)
        {
            transform.localScale = Vector3.Lerp(startingScale, endScale, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.localScale = endScale;
    }
}