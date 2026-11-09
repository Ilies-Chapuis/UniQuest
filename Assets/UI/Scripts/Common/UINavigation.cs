using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UniQuest.UI
{
    /// <summary>
    /// Câble la navigation clavier des boutons créés par script.
    /// La navigation "Automatic" d'Unity peut sauter vers un bouton d'un AUTRE panneau :
    /// avec ces helpers, les flèches restent dans la liste et bouclent proprement.
    /// </summary>
    public static class UINavigation
    {
        /// <summary>Liste verticale : haut/bas, avec retour au début en bas de liste.</summary>
        public static void Vertical(IList<Selectable> items, bool wrap = true)
        {
            Grid(items, 1, wrap);
        }

        /// <summary>Liste horizontale (onglets) : gauche/droite.</summary>
        public static void Horizontal(IList<Selectable> items, bool wrap = true)
        {
            var list = Interactables(items);
            int n = list.Count;
            for (int i = 0; i < n; i++)
            {
                var nav = new Navigation { mode = Navigation.Mode.Explicit };
                nav.selectOnLeft = i > 0 ? list[i - 1] : (wrap ? list[n - 1] : null);
                nav.selectOnRight = i < n - 1 ? list[i + 1] : (wrap ? list[0] : null);
                list[i].navigation = nav;
            }
        }

        /// <summary>Grille (inventaire, menu d'action en 2 colonnes).</summary>
        public static void Grid(IList<Selectable> items, int columns, bool wrap = true)
        {
            var list = Interactables(items);
            int n = list.Count;
            if (n == 0 || columns < 1) return;

            for (int i = 0; i < n; i++)
            {
                int col = i % columns;
                var nav = new Navigation { mode = Navigation.Mode.Explicit };

                // Gauche / droite dans la même ligne
                if (columns > 1)
                {
                    nav.selectOnLeft = col > 0 ? list[i - 1] : (wrap ? list[Mathf.Min(i + columns - 1, n - 1)] : null);
                    nav.selectOnRight = (col < columns - 1 && i + 1 < n) ? list[i + 1]
                                        : (wrap ? list[i - col] : null);
                }

                // Haut / bas dans la même colonne
                int up = i - columns;
                int down = i + columns;
                if (up >= 0) nav.selectOnUp = list[up];
                else if (wrap) nav.selectOnUp = list[LastInColumn(col, columns, n)];

                if (down < n) nav.selectOnDown = list[down];
                else if (wrap) nav.selectOnDown = list[col < n ? col : 0];
                else if (i / columns < (n - 1) / columns) nav.selectOnDown = list[n - 1]; // dernière ligne incomplète

                list[i].navigation = nav;
            }
        }

        private static int LastInColumn(int col, int columns, int count)
        {
            int last = col;
            while (last + columns < count) last += columns;
            return last < count ? last : count - 1;
        }

        private static List<Selectable> Interactables(IList<Selectable> items)
        {
            var result = new List<Selectable>();
            foreach (var s in items)
                if (s != null && s.interactable && s.gameObject.activeInHierarchy) result.Add(s);

            // Les boutons désactivés ne doivent pas piéger la navigation
            foreach (var s in items)
                if (s != null && !result.Contains(s)) s.navigation = new Navigation { mode = Navigation.Mode.None };
            return result;
        }

        /// <summary>Détruit tous les enfants d'un conteneur (avant de reconstruire une liste).</summary>
        public static void ClearChildren(Transform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                var child = container.GetChild(i).gameObject;
                child.SetActive(false);            // disparaît tout de suite, Destroy n'agit qu'en fin de frame
                Object.Destroy(child);
            }
        }
    }
}
