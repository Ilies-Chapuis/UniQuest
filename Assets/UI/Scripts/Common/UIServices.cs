using UnityEngine;

namespace UniQuest.UI
{
    /// <summary>
    /// Unity ne sait pas afficher un champ de type interface dans l'Inspector.
    /// On déclare donc un champ MonoBehaviour (où l'on glisse le FightManager, l'inventaire...),
    /// et ce helper le convertit vers l'interface attendue.
    /// Si le champ est vide, il cherche automatiquement dans la scène.
    /// </summary>
    public static class UIServices
    {
        public static T Resolve<T>(MonoBehaviour source) where T : class
        {
            if (source != null)
            {
                var direct = source as T;
                if (direct != null) return direct;
                var onSameObject = source.GetComponent<T>();
                if (onSameObject != null) return onSameObject;
                Debug.LogWarning($"[UI] {source.name} n'implémente pas {typeof(T).Name}.", source);
            }

            foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                var found = mb as T;
                if (found != null) return found;
            }
            return null;
        }
    }
}
