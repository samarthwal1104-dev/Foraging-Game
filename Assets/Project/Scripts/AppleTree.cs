using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AppleTree : MonoBehaviour
{
    public List<GameObject> applePrefabs;
    public List<Transform> spawnPoints;

    [Header("Depletion schedule")]
    public float initialDropInterval = 2.0f;   // time for the first apple to drop
    public float depletionMultiplier = 1.4f;   // interval *= this after each drop

    private bool playerNearby = false;
    private bool running = false;
    private Coroutine dropCoroutine;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        // strongly recommended: add a kinematic rigidbody for CharacterController triggers
        var rb = GetComponent<Rigidbody>();
        if (rb == null) { rb = gameObject.AddComponent<Rigidbody>(); }
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = true;
        TryStartDropping();
    }

    private void OnTriggerStay(Collider other)
    {
        // This covers the case where the player was already inside
        if (!other.CompareTag("Player")) return;
        playerNearby = true;
        TryStartDropping();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerNearby = false;
        if (dropCoroutine != null)
        {
            StopCoroutine(dropCoroutine);
            dropCoroutine = null;
        }
        running = false;
    }

    private void TryStartDropping()
    {
        if (!playerNearby || running) return;
        running = true;
        dropCoroutine = StartCoroutine(DropApples());
    }

    private IEnumerator DropApples()
    {
        float currentInterval = initialDropInterval;

        while (playerNearby)
        {
            // wait with scaled time (game is unpaused during play)
            yield return new WaitForSeconds(currentInterval);

            SpawnApple();

            // exponentially increase the interval
            currentInterval *= Mathf.Max(1.0001f, depletionMultiplier);
        }

        running = false;
    }

    private void SpawnApple()
    {
        if (spawnPoints == null || spawnPoints.Count == 0) return;
        if (applePrefabs == null || applePrefabs.Count == 0) return;

        int spawnIndex = Random.Range(0, spawnPoints.Count);
        Transform spawnPoint = spawnPoints[spawnIndex];
        GameObject appleToSpawn = GetRandomApple();
        if (appleToSpawn != null)
        {
            Instantiate(appleToSpawn, spawnPoint.position, Quaternion.identity);
        }
    }

    private GameObject GetRandomApple()
    {
        if (applePrefabs.Count == 0) return null;

        // 10% chance to spawn Golden if available
        if (Random.value < 0.10f)
        {
            foreach (var a in applePrefabs)
                if (a != null && a.name.Contains("Golden")) return a;
        }
        // otherwise prefer Red
        foreach (var a in applePrefabs)
            if (a != null && a.name.Contains("Red")) return a;

        // fallback
        return applePrefabs[Random.Range(0, applePrefabs.Count)];
    }
}