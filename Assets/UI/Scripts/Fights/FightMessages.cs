using System.Collections.Generic;
using UniQuest.Contracts;

namespace UniQuest.UI
{
    /// <summary>
    /// Tous les textes de combat au même endroit : facile à relire, à traduire, et à rendre drôles.
    /// Le ton suit le jeu : sérieux pour les larves, absurde pour Trochu.
    /// Si le gameplay remplit ActionResult.CustomMessage, ce texte-là est utilisé à la place.
    /// </summary>
    public static class FightMessages
    {
        // Nom du héros "goofy" : ses actions ont des messages à part
        public const string GoofyHeroName = "Trochu";

        public static string Intro(ICombatant enemy, bool elite)
        {
            return elite
                ? $"{enemy.Name} se dresse devant vous. Ça sent très mauvais."
                : $"{enemy.Name} surgit des ténèbres !";
        }

        public static string Prompt(ICombatant player) => $"Que doit faire {player.Name} ?";

        /// <summary>Découpe une action en plusieurs messages affichés l'un après l'autre.</summary>
        public static List<string> Describe(ActionResult r)
        {
            var list = new List<string>();
            if (!string.IsNullOrEmpty(r.CustomMessage))
            {
                list.Add(r.CustomMessage);
                return list;
            }

            string actor = r.Actor != null ? r.Actor.Name : "???";
            string target = r.Target != null ? r.Target.Name : "???";

            if (actor == GoofyHeroName && r.ActionName == "Coup de casque")
                list.Add($"{actor} frappe avec son casque. Ça marche, étonnamment.");
            else
                list.Add($"{actor} utilise {r.ActionName} !");

            if (r.IsMiss)
            {
                list.Add(actor == GoofyHeroName ? "Il rate. Il fait semblant que c'était voulu." : "Mais ça rate.");
                return list;
            }

            if (r.IsCritical) list.Add("Coup critique !");
            if (r.Effectiveness == Effectiveness.SuperEffective) list.Add("C'est super efficace !");
            else if (r.Effectiveness == Effectiveness.NotVeryEffective) list.Add("Ce n'est pas très efficace...");

            if (r.Damage > 0) list.Add($"{target} perd {r.Damage} PV.");
            else if (r.Damage < 0) list.Add($"{target} récupère {-r.Damage} PV.");

            if (r.Target != null && r.Target.IsKO) list.Add(KO(r.Target));
            return list;
        }

        public static string KO(ICombatant c) =>
            c.Name == GoofyHeroName ? $"{c.Name} s'effondre en criant « C'ÉTAIT UN PIÈGE ! »" : $"{c.Name} est KO !";

        public static string SwitchIn(ICombatant c) => $"À toi, {c.Name} !";

        public static string Victory => "Victoire !";
        public static string Experience(int xp) => $"L'équipe gagne {xp} XP.";
        public static string Drop(IItemInfo item) => $"Butin : {item.Name}.";
        public static string Fled => "Vous prenez la fuite. Sans aucune honte.";
        public static string Defeat => "Toute l'équipe est KO...";
    }
}
