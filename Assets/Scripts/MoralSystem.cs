using UnityEngine;
using System.Collections.Generic;

public class MoralSystem : MonoBehaviour
{
    public static MoralSystem Instance;

    [Header("Puntos de Alineación")]
    public int luzPoints = 0;
    public int sombraPoints = 0;

    [Header("Umbrales")]
    public int umbralLuz = 50;
    public int umbralSombra = 50;

    [Header("Eventos")]
    public System.Action<int> OnLuzChanged;
    public System.Action<int> OnSombraChanged;
    public System.Action<string> OnAlignmentShift;

    private List<string> decisionesHistorial = new List<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddLuz(int points)
    {
        luzPoints += points;
        Debug.Log($"⚡ Luz +{points}. Total: {luzPoints}");

        OnLuzChanged?.Invoke(luzPoints);
        CheckAlignmentShift();

        // Registrar decisión
        decisionesHistorial.Add($"Luz +{points} - {System.DateTime.Now}");
    }

    public void AddSombra(int points)
    {
        sombraPoints += points;
        Debug.Log($"🌑 Sombra +{points}. Total: {sombraPoints}");

        OnSombraChanged?.Invoke(sombraPoints);
        CheckAlignmentShift();

        // Registrar decisión
        decisionesHistorial.Add($"Sombra +{points} - {System.DateTime.Now}");
    }

    private void CheckAlignmentShift()
    {
        if (luzPoints >= umbralLuz)
        {
            OnAlignmentShift?.Invoke("Luz");
            Debug.Log("¡Alineación cambiada a LUZ!");
        }
        else if (sombraPoints >= umbralSombra)
        {
            OnAlignmentShift?.Invoke("Sombra");
            Debug.Log("¡Alineación cambiada a SOMBRA!");
        }
    }

    public string GetCurrentAlignment()
    {
        if (luzPoints > sombraPoints) return "Luz";
        if (sombraPoints > luzPoints) return "Sombra";
        return "Equilibrio";
    }

    public int GetAlignmentScore()
    {
        return luzPoints - sombraPoints;
    }

    public void ResetSystem()
    {
        luzPoints = 0;
        sombraPoints = 0;
        decisionesHistorial.Clear();
    }

    // Para debug
    public void PrintHistorial()
    {
        Debug.Log("=== HISTORIAL DECISIONES ===");
        foreach (string decision in decisionesHistorial)
        {
            Debug.Log(decision);
        }
    }
}