using UnityEngine;

public class TestSeed : MonoBehaviour
{
    // Déclare une variable publique qui pourra référencer un composant SeedManager.
    public SeedManager seedManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("TestSeed : Start fonctionne.");

        Debug.Log("Teste de la Seed : " + seedManager.Seed);

        // Appelle la méthode GetRandomNumber du composant SeedManager.
        // Lui transmet la limite de 10 et stocke le résultat dans resultTest.
        int resultTest = seedManager.GetRandomNumber(10);

        // Affiche le nombre pseudo-aléatoire reçu depuis SeedManager.
        Debug.Log("Le résultat des nombres pris au hasard et : " + resultTest);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
