// Script utilisé pour détecter quand on doit passer en mode combat quand un ennemi touche le player.
// Script use to switch to combat mode when an enemy touches the player

// Pour charger une scène on utilise (using UnityEngine.SceneManagement):
// To load a scene, we use using UnityEngine.SceneManagement.
using UnityEngine.SceneManagement;
using UnityEngine;

public class CombatTrigger : MonoBehaviour
{
    // Pour savoir si le joueur est en état d'invincibilité (pour l'option de la fuite dans un combat).
    // To know if the player is on invincible state (for the escape option on a fight).
    public bool isInvincible = false;

    // Temps d'invincibilité du joueur après avoir pris la fuite.
    // Invincibility time of the player after escaping a battle.
    public float invincibleTime = 3f;

    // Temps d'invincibilité restant (il descend jusqu'à zéro).
    // Remaining invincibility time (it counts down to zero).
    private float invincibleTimer = 0f;

    // Appelée automatiquement par Unity quand le collider du Player entre dans un collider en trigger.
    // Reçoit en paramètre le collider de l'objet touché.
    // Called automatically by Unity when the Player's collider enters a trigger collider.
    // Receives the collider of the touched object as a parameter.
    void OnTriggerEnter2D(Collider2D other)
    {
        // On vérifie si le tag qui entre en contact avec le joueur c'est l'ennemi.
        // Check if the object touched has enemy tag.
        // On vérifie que le joueur n'est pas en état d'invincibilité.
        // We check if the player is not on the invincibility state.
        if (!isInvincible && other.CompareTag("Ennemy"))
        {
            // Affichage console pour montrer qu'il y a contact entre player et ennemi.
            // Print a message on console something to show that the ennemy is on contact with player.
            Debug.Log("En contact avec un ennemi");

            // Récupère la position du joueur pour la stocker dans le carnet.
            // Gets the player's position to store it in the notebook.
            CombatData.instance.positionPlayer = transform.position;

            // Récupère la position de l'ennemi pour la stocker dans le carnet.
            // Gets the enemy's position to store it in the notebook.
            CombatData.instance.positionEnemy = other.transform.position;

            // Chargement de la scène de combat.
            // Loads the combat scene.
            SceneManager.LoadScene("Combat");
        }
    }

    void Update()
    {
        // Enlève le temps écoulé depuis la dernière image au temps restant
        // Removes the time elapsed since the last frame from the remaining time.
        // Regarde si le joueur est en état invincible.
        // Check if the player is in an invincible state.
        if(isInvincible)
        {
            // Enlève le temps d'invincibilité au joueur s'il est en état d'invincibilité.
            // Remove time from the player's invincibility if the player is invincible.
            invincibleTimer -= Time.deltaTime;

            // Si le temps d'invincibilité atteint zéro.
            // If the invincibility time reaches zero.
            if(invincibleTimer <= 0f)
            {
                // Le délai passé le joueur n'est plus invincible.
                // Once the delay has passed, the player is no longer invincible.
                isInvincible = false;
            }
        }
    }

    // Lance l'invincibilité : pas de combat pendant invincibleTime secondes.
    // Start invincibility: no fight for invincibleTime seconds.
    public void StartInvincibility()
    {
        // On active l'invincibilité en cas de fuite du joueur durant un combat.
        // Activate invincibility in case of escape by the player during a battle.
        isInvincible = true;

        // On active le timer pour enlever l'invincibilité.
        // Activate the timer to cut off the invincibility state.
        invincibleTimer = invincibleTime;
    }
}
