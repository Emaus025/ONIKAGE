using UnityEngine;

public class TempleAreaTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StoryNarrator.Instance.ShowProgressDialogue(
                "El templo guarda secretos ancestrales... observa con atención"
            );
        }
    }
}