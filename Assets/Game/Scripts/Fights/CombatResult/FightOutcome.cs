// Les quatre issues possibles d'un combat : rien, victoire, défaite ou fuite du joueur.
// Four possible outcomes of the player on a fight : none, victory, defeat, escape.
public enum FightOutcome
{
    // Aucun combat n'a encore eu lieu.
    None,

    // Cas de victoire dans un combat du joueur.
    Victory,

    // Cas de défaite dans un combat du joueur.
    Defeat,

    // Cas de fuite dans un combat du joueur.
    Escape
}