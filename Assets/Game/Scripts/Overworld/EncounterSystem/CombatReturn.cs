using UnityEngine;

public class CombatReturn : MonoBehaviour
{
    // Appelée une seule fois au chargement de la carte.
    // Called only once when the map loads.
    void Start()
    {
        // Déplace le joueur aux coordonnées enregistrées
        // dans le carnet lorsque le joueur revient sur la carte.
        // Moves the player to the coordinates saved
        // in the notebook when the player returns to the map.
        transform.position = CombatData.instance.positionPlayer;

        // Si le combat s'est terminé par une victoire.
        // If the fight ended in a victory.
        if (CombatData.instance.fightResult == FightOutcome.Victory)
        {
            // Victoire du joueur : on retire l'ennemi de la map.
            // Rassemble tous les ennemis de la scène dans un tableau de GameObject.
            // Player victory: we remove the enemy from the map.
            // Gathers all the enemies of the scene in a GameObject array.
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Ennemy");

            // Affiche combien d'ennemis sont trouvés dans le tableau de la scène.
            // Displays how many enemies were found in the scene's array.
            Debug.Log("Ennemis trouvés : " + enemies.Length);

            // Pour chaque ennemi présent dans le tableau enemies.
            // For each enemy in the enemies array.
            foreach (GameObject enemy in enemies)
            {
                // Si la position de cet ennemi est celle notée dans le carnet : c'est l'ennemi battu.
                // If this enemy's position is the one saved in the notebook: it is the defeated enemy.
                if((Vector2)enemy.transform.position == CombatData.instance.positionEnemy)
                {
                    // Destruction du gameObject de l'ennemi.
                    // Destroys the enemy's gameObject.
                    Destroy(enemy);
                }
            }
        }

        // Si le résultat d'un combat est escape par le joueur lors d'un combat.
        // If the result of a fight is an escape by the player during a fight.
        if (CombatData.instance.fightResult == FightOutcome.Escape)
        {
            // On récupère le script CombatTrigger posé sur le Player.
            // We get the CombatTrigger script attached to the Player.
            CombatTrigger combatTrigger = GetComponent<CombatTrigger>();

            // On active l'invincibilité.
            // Activate invincibility.
            combatTrigger.StartInvincibility();
        }

        // On remet le résultat à zéro : le combat a été traité
        // We reset the result to zero: the fight has been handled.
        CombatData.instance.fightResult = FightOutcome.None;
    }
}
