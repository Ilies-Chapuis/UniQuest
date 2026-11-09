// ==========================================
// Seed automatique
// ==========================================

using UnityEngine;

public class SeedManager : MonoBehaviour
{
    // Déclaration de la seed basée sur le temps écoulé depuis le démarrage du système.
    private int seed = System.Environment.TickCount;

    // Permet au autre script de pouvoir accéder à la valeur de la seed sans la modifier.
    public int Seed
    {
        get
        {
            // On va chercher la valeur de la seed et on la renvoie la valeur de la seed pour pouvoir la lire.
            return seed;
        }
    }

    // Génère un nombre décimal entre 0 inclus et 1 exclus.
    public double GetRandomValue()
    {
        // Utilise NextDouble() pour générer un nombre décimal pseudo-aléatoire entre 0 et 1.
        // Le résultat est directement renvoyé au script qui appelle cette méthode.
        return randomGenerator.NextDouble();
    }

    // Génère un entier entre une limite minimal incluse et maximale exclue.
    public int GetRandomRange(int limiteMin, int limiteMax)
    {
        // Génère un entier pseudo-aléatoire entre limiteMin (incluse) et limiteMax (exclue).
        // Le résultat est directement renvoyé au script qui appelle cette méthode.
        return randomGenerator.Next(limiteMin, limiteMax);
    }

    // Vérifie si une probabilité exprimée en poucentage est réussie.
    public bool RollChance(float pourcentage)
    {
        // Vérifie que le pourcentage est compris entre 0 et 100.
        pourcentage = Mathf.Clamp(pourcentage, 0f, 100f);

        // Génère un nombre décimal entre 0 et 1, puis le multiplie par 100.
        // Compare le résultat au pourcentage demandé.
        // Retourne true si la probabilité est réussie, sinon false.
        // Effectue le tirage aléatoire et retourne le résultat.
        return randomGenerator.NextDouble() * 100 < pourcentage;
    }

    // System.Random : c'est le type de la boîte.
    // Elle peut contenir un générateur de nombres pseudo-aléatoires.
    // randomGenerator : c'est le nom que tu donnes à cette boîte.
    private System.Random randomGenerator;

    // Méthode publique permettant aux autres scripts de demander un nombre pseudo-aléatoire.
    // Elle reçoit une limite maximale et renvoie un nombre entier.
    public int GetRandomNumber(int limiteMax)
    {
        // Génère un nombre pseudo-aléatoire entre 0 (inclus) et limitMax (exclu).
        // Le nombre généré est temporairement stocké dans la variable result.
        int result = randomGenerator.Next(limiteMax);

        // Renvoie le nombre généré au script qui a appelé cette méthode.
        return result;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Affiche la valeur de la seed.
        Debug.Log("Valeur de la seed : " + seed);       
    }

    // Awake est exécutée au chargement du composant, avant les méthodes Start().
    void Awake()
    {
        // Crée un générateur de nombres pseudo-aléatoires à partir de la seed.
        // Le générateur est stocké dans la variable randomGenerator.
        // Initialise le générateur pseudo-aléatoire à partir de la seed.
        randomGenerator = new System.Random(seed);
    }
}
