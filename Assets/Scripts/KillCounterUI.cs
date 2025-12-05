using UnityEngine;
using TMPro;

public class KillCounterUI : MonoBehaviour
{
    public PlayerController player;
    public TMP_Text label;
    public string prefix = "Kills: ";

    private void OnEnable()
    {
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.GetComponent<PlayerController>();
        }
        if (player != null)
        {
            player.OnKillCountChanged += UpdateLabel;
            UpdateLabel(player.killCount);
        }
    }

    private void OnDisable()
    {
        if (player != null)
            player.OnKillCountChanged -= UpdateLabel;
    }

    private void UpdateLabel(int count)
    {
        if (label != null) label.text = prefix + count;
    }
}