using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UniQuest.Contracts;

namespace UniQuest.UI
{
    /// <summary>
    /// Une ligne de la liste d'attaques : "Boule de feu      Feu · 6 PM".
    /// Prefab : Button + MenuButton + ce script, avec deux textes.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class AttackButton : MonoBehaviour, ISelectHandler
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text infoText;

        private Button button;
        private bool initialized;

        public IAttackInfo Attack { get; private set; }
        public Button Button => button;

        public event Action<IAttackInfo> OnChosen;
        public event Action<IAttackInfo> OnHighlighted;

        private void Init()
        {
            if (initialized) return;
            initialized = true;
            button = GetComponent<Button>();
            button.onClick.AddListener(() => OnChosen?.Invoke(Attack));
        }

        private void Awake() => Init();

        public void Setup(IAttackInfo attack, int currentMP)
        {
            Init();
            Attack = attack;
            bool affordable = attack.ManaCost <= currentMP;

            nameText.text = attack.Name;
            if (attack.ManaCost <= 0) infoText.text = attack.TypeName;
            else if (affordable) infoText.text = $"{attack.TypeName} · {attack.ManaCost} PM";
            else infoText.text = $"PM insuffisants · {attack.ManaCost} PM";

            var mb = GetComponent<MenuButton>();
            if (mb != null) mb.SetInteractable(affordable);
            else button.interactable = affordable;
        }

        public void OnSelect(BaseEventData eventData) => OnHighlighted?.Invoke(Attack);
    }
}
