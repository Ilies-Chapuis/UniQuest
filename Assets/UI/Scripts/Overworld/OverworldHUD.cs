using System.Collections;
using UnityEngine;
using TMPro;
using UniQuest.Contracts;

namespace UniQuest.UI
{
    /// <summary>
    /// HUD de la carte (écran 1) : nom de la zone + PV du chef d'équipe, et rappel "Échap : Menu".
    /// Le HUD se cache pendant les dialogues et les menus pour ne pas surcharger l'écran.
    ///
    /// Quand le joueur entre dans une zone (trigger posé sur la tilemap), appelle :
    ///     overworldHUD.ShowZone("Forêt des larves");
    /// </summary>
    public class OverworldHUD : MonoBehaviour
    {
        [Tooltip("Le FightManager / GameManager qui implémente IParty. Vide = recherche automatique.")]
        [SerializeField] private MonoBehaviour partySource;

        [SerializeField] private CanvasGroup hudGroup;
        [SerializeField] private TMP_Text zoneText;
        [SerializeField] private CharacterHUD leaderHUD;

        [Header("Bandeau d'entrée de zone (optionnel)")]
        [SerializeField] private CanvasGroup zoneBanner;
        [SerializeField] private TMP_Text zoneBannerText;
        [SerializeField] private float bannerTime = 2f;

        private IParty party;
        private Coroutine bannerRoutine;

        private void Start()
        {
            party = UIServices.Resolve<IParty>(partySource);
            if (party != null)
            {
                party.OnPartyChanged += BindLeader;
                BindLeader();
            }
            if (zoneBanner != null) zoneBanner.alpha = 0f;
        }

        private void OnEnable() => UIState.OnBlockingChanged += HandleBlocking;

        private void OnDisable() => UIState.OnBlockingChanged -= HandleBlocking;

        private void OnDestroy()
        {
            if (party != null) party.OnPartyChanged -= BindLeader;
        }

        private void HandleBlocking(bool blocked)
        {
            if (hudGroup != null) hudGroup.alpha = blocked ? 0f : 1f;
        }

        /// <summary>Le premier membre vivant de l'équipe s'affiche dans le HUD.</summary>
        private void BindLeader()
        {
            if (leaderHUD == null || party == null) return;
            ICombatant leader = null;
            foreach (var m in party.Members)
                if (!m.IsKO) { leader = m; break; }
            if (leader == null && party.Members.Count > 0) leader = party.Members[0];
            leaderHUD.Bind(leader);
        }

        public void ShowZone(string zoneName)
        {
            if (zoneText != null) zoneText.text = zoneName;
            if (zoneBanner == null) return;

            if (bannerRoutine != null) StopCoroutine(bannerRoutine);
            bannerRoutine = StartCoroutine(Banner(zoneName));
        }

        private IEnumerator Banner(string zoneName)
        {
            if (zoneBannerText != null) zoneBannerText.text = zoneName;
            for (float t = 0f; t < 0.3f; t += Time.unscaledDeltaTime) { zoneBanner.alpha = t / 0.3f; yield return null; }
            zoneBanner.alpha = 1f;
            yield return new WaitForSecondsRealtime(bannerTime);
            for (float t = 0f; t < 0.5f; t += Time.unscaledDeltaTime) { zoneBanner.alpha = 1f - t / 0.5f; yield return null; }
            zoneBanner.alpha = 0f;
            bannerRoutine = null;
        }
    }
}
