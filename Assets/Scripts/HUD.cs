using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUD : MonoBehaviour
{
    public PlayerController player;
    public CombatManager combat;

    public Image healthFill;
    public Image expFill;
    public Image furiaFill;

    public TextMeshProUGUI livesText;
    public TextMeshProUGUI levelText;

    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;

    private void Update()
    {
        if (player != null)
        {
            float h = player.maxHealth > 0 ? (float)player.currentHealth / player.maxHealth : 0f;
            if (healthFill != null) healthFill.fillAmount = Mathf.Clamp01(h);

            int threshold = player.level * 100;
            float e = threshold > 0 ? (float)player.experience / threshold : 0f;
            if (expFill != null) expFill.fillAmount = Mathf.Clamp01(e);

            if (livesText != null) livesText.text = "Vidas: " + player.lives;
            if (levelText != null) levelText.text = "Nivel: " + player.level;

            // CORREGIDO: Solo activar Game Over si las vidas son 0, no solo si el script está desactivado
            if (gameOverPanel != null) 
            {
                bool isDead = player.lives <= 0;
                gameOverPanel.SetActive(isDead);
            }
        }

        if (combat != null && furiaFill != null)
        {
            float f = combat.maxFuria > 0 ? combat.furiaMeter / combat.maxFuria : 0f;
            furiaFill.fillAmount = Mathf.Clamp01(f);
        }
    }

    public void ShowLevelComplete(bool show)
    {
        if (levelCompletePanel != null) levelCompletePanel.SetActive(show);
    }
}