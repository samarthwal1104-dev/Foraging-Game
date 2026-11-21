using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ApplePatch : MonoBehaviour
{
    [Tooltip("Unique name for the data log (e.g., NorthPatch)")]
    public string patchName = "Patch_1";

    private bool hasBeenVisited = false;

    private void Start()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (DataLogger.Instance != null)
            {
                // Tell the logger the player is currently standing in this patch
                DataLogger.Instance.SetCurrentPatch(patchName);

                // Only log the discovery event once
                if (!hasBeenVisited)
                {
                    hasBeenVisited = true;
                    DataLogger.Instance.LogPatchVisited(patchName);
                    Debug.Log($"Player discovered patch: {patchName}");
                }
            }
        }
    }

    // --- ADD THIS NEW METHOD ---
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (DataLogger.Instance != null)
            {
                // Tell the logger we left this patch
                DataLogger.Instance.ClearCurrentPatch(patchName);
            }
        }
    }
}
