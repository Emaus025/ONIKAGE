using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int nivelActual = 1;
    public string climaActual = "Normal";

    public bool[] decisionesNivel1 = new bool[3];

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
    
}