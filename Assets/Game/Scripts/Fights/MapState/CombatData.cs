using UnityEngine;

public class CombatData : MonoBehaviour
{
    // Position du joueur sur la carte juste avant le combat.
    public Vector2 positionPlayer;

    // Position de l'ennemi sur la carte juste avant le combat.
    public Vector2 positionEnemy;

    // Contient le résultat du combat déclenché par le joueur ou l'ennemi.
    public FightOutcome fightResult;

    // Le seul carnet qui existe dans le jeu,
    // accessible depuis n'importe quel script avec CombatData.instance.
    public static CombatData instance;

    // Appelée automatiquement par Unity à la création de l'objet, avant Start()
    void Awake()
    {
        // Vérifie s'il existe déjà un carnet
        if (instance == null)
        {
            // Premier carnet: on le range dans la case et on le protège du changement de scène.
            instance = this;
            
            // Protège cet objet : il survit au changement de scène, ce qui garde les positions et le résultat du combat.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Un carnet existe déjà: celui-ci est en trop, on le supprime.
            Destroy(gameObject);
        }
    }
}
