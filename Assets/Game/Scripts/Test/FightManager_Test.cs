using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Temporaire : simule la fin d'un combat avec le clavier (V, D, E).
// Temporary : simulates the end of a fight with the keyboard (V, D, E).
// A supprimer quand le vrai système de combat existera.
// To delete once the real combat system exists.

public class FightManager_Test : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // Si on appuie sur V le résultat du combat est Victoire.
        if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            EndFight(FightOutcome.Victory);
        }

        // Si on appuie sur D le résultat du combat passe en mode Défaite.
        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            EndFight(FightOutcome.Defeat);
        }

        // Si on appuie sur E le résultat du combat est en mode échappé du combat.
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            EndFight(FightOutcome.Escape);
        }
    }

    // Ecrit le résultat dans le carnet, puis recharge la carte.
    // Writes the result in the notebook, then reloads the map.
    void EndFight(FightOutcome outcome)
    {
        // Enregistre le résultat qui s'est passé lors du combat dans le carnet.
        CombatData.instance.fightResult = outcome;

        // On charge la scène de base car le combat est fini.
        SceneManager.LoadScene("TestGameplay");
    }
}
