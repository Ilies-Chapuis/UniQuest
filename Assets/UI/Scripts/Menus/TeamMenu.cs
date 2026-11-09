using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniQuest.Contracts;

namespace UniQuest.UI
{
    /// <summary>
    /// Menu équipe (écran 5) : la liste des persos à gauche, et à droite la fiche
    /// du perso sélectionné (portrait, PV/PM/XP, stats, attaques, arme).
    /// La fiche se met à jour dès qu'on déplace la sélection avec les flèches.
    /// </summary>
    public class TeamMenu : MenuPanel
    {
        [Tooltip("Ce qui implémente IParty. Vide = recherche automatique.")]
        [SerializeField] private MonoBehaviour partySource;

        [Header("Liste")]
        [SerializeField] private Transform listContainer;
        [SerializeField] private PartyMemberButton memberPrefab;

        [Header("Fiche")]
        [Tooltip("Portrait, nom, type, niveau, barres")]
        [SerializeField] private CharacterHUD detailHUD;
        [SerializeField] private TMP_Text attackValue;
        [SerializeField] private TMP_Text defenseValue;
        [SerializeField] private TMP_Text speedValue;
        [SerializeField] private TMP_Text accuracyValue;
        [SerializeField] private Transform attacksContainer;
        [SerializeField] private AttackRow attackRowPrefab;

        [Header("Arme (optionnel)")]
        [SerializeField] private GameObject weaponGroup;
        [SerializeField] private Image weaponIcon;
        [SerializeField] private TMP_Text weaponName;
        [SerializeField] private TMP_Text weaponBonus;

        private IParty party;

        protected override void HandleOpened()
        {
            if (party == null) party = UIServices.Resolve<IParty>(partySource);
            if (party == null) { Debug.LogWarning("[TeamMenu] Aucun IParty trouvé.", this); return; }

            UINavigation.ClearChildren(listContainer);
            var selectables = new List<Selectable>();
            foreach (var m in party.Members)
            {
                var row = Instantiate(memberPrefab, listContainer);
                row.Setup(m, selectable: true);       // les KO restent consultables
                row.OnHighlighted += ShowDetail;
                selectables.Add(row.Button);
            }
            UINavigation.Vertical(selectables);
            firstSelected = selectables.Count > 0 ? selectables[0] : null;
            if (party.Members.Count > 0) ShowDetail(party.Members[0]);
        }

        private void ShowDetail(ICombatant c)
        {
            if (c == null) return;

            detailHUD.Bind(c);
            if (attackValue != null) attackValue.text = c.Attack.ToString();
            if (defenseValue != null) defenseValue.text = c.Defense.ToString();
            if (speedValue != null) speedValue.text = c.Speed.ToString();
            if (accuracyValue != null) accuracyValue.text = $"{c.Accuracy} %";

            UINavigation.ClearChildren(attacksContainer);
            foreach (var a in c.Attacks)
                Instantiate(attackRowPrefab, attacksContainer).Setup(a, c.Level);

            if (weaponGroup != null)
            {
                var w = c.Weapon;
                weaponGroup.SetActive(w != null);
                if (w != null)
                {
                    if (weaponName != null) weaponName.text = w.UpgradeLevel > 0 ? $"{w.Name} +{w.UpgradeLevel}" : w.Name;
                    if (weaponBonus != null) weaponBonus.text = $"Attaque +{w.AttackBonus}";
                    if (weaponIcon != null)
                    {
                        weaponIcon.sprite = w.Icon;
                        weaponIcon.enabled = w.Icon != null;
                        weaponIcon.preserveAspect = true;
                    }
                }
            }
        }
    }
}
