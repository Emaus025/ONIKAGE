using UnityEngine;

public class KillGate : MonoBehaviour
{
    public PlayerController player;
    public int killThreshold = 5;
    public bool destroyOnUnlock = false;

    private void OnEnable()
    {
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.GetComponent<PlayerController>();
        }
        if (player != null)
        {
            player.OnKillCountChanged += HandleKillChanged;
            HandleKillChanged(player.killCount);
        }
    }

    private void OnDisable()
    {
        if (player != null)
            player.OnKillCountChanged -= HandleKillChanged;
    }

    private void HandleKillChanged(int count)
    {
        if (count >= killThreshold)
        {
            if (destroyOnUnlock) Destroy(gameObject);
            else gameObject.SetActive(false);
            if (player != null) player.OnKillCountChanged -= HandleKillChanged;
        }
    }
}