using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DnDAR.Combat
{
    /// <summary>
    /// Parsea y tira notación de dados estándar de D&D como "1d6+2", "2d8-1", "1d20".
    /// </summary>
    public static class DiceRoller
    {
        private static readonly Regex NotationPattern =
            new Regex(@"^\s*(\d+)d(\d+)\s*([+-]\s*\d+)?\s*$", RegexOptions.IgnoreCase);

        public struct DiceExpression
        {
            public int Count;
            public int Sides;
            public int Modifier;

            public int Min => Count * 1 + Modifier;
            public int Max => Count * Sides + Modifier;
        }

        public static DiceExpression Parse(string notation)
        {
            var match = NotationPattern.Match(notation);
            if (!match.Success)
                throw new ArgumentException($"Notación de dados inválida: '{notation}'");

            int count = int.Parse(match.Groups[1].Value);
            int sides = int.Parse(match.Groups[2].Value);
            int modifier = 0;
            if (match.Groups[3].Success)
                modifier = int.Parse(match.Groups[3].Value.Replace(" ", ""));

            return new DiceExpression { Count = count, Sides = sides, Modifier = modifier };
        }

        public static int Roll(DiceExpression dice)
        {
            int total = dice.Modifier;
            for (int i = 0; i < dice.Count; i++)
                total += UnityEngine.Random.Range(1, dice.Sides + 1);
            return total;
        }

        public static int Roll(string notation) => Roll(Parse(notation));

        public static int RollD20(bool advantage = false, bool disadvantage = false)
        {
            int first = UnityEngine.Random.Range(1, 21);
            if (!advantage && !disadvantage) return first;

            int second = UnityEngine.Random.Range(1, 21);
            return advantage ? Mathf.Max(first, second) : Mathf.Min(first, second);
        }
    }

    /// <summary>
    /// Calcula el porcentaje de acierto y el rango de daño ANTES de tirar,
    /// para mostrarlo en el preview cuando se toca "Atacar".
    /// </summary>
    public static class AttackCalculator
    {
        public struct AttackPreview
        {
            public float HitChancePercent;
            public int MinDamage;
            public int MaxDamage;
            public int NeededRoll; // lo que hay que sacar en el d20 (sin contar el bono)
        }

        public static AttackPreview GetPreview(int attackBonus, int targetAC, string damageNotation,
            bool advantage = false, bool disadvantage = false)
        {
            int neededRoll = Mathf.Clamp(targetAC - attackBonus, 1, 20);

            // Probabilidad base sobre 20 caras; nat 1 falla siempre, nat 20 acierta siempre
            float baseChance = Mathf.Clamp((21 - neededRoll) / 20f, 0.05f, 0.95f);

            float chance;
            if (advantage)
                chance = 1f - (1f - baseChance) * (1f - baseChance);
            else if (disadvantage)
                chance = baseChance * baseChance;
            else
                chance = baseChance;

            var dmg = DiceRoller.Parse(damageNotation);

            return new AttackPreview
            {
                HitChancePercent = chance * 100f,
                MinDamage = dmg.Min,
                MaxDamage = dmg.Max,
                NeededRoll = neededRoll
            };
        }
    }

    /// <summary>
    /// Resultado de resolver un ataque completo, listo para aplicar al HP del objetivo.
    /// </summary>
    public struct AttackResult
    {
        public int AttackRoll;
        public bool Hit;
        public bool CriticalHit;
        public int DamageDealt;
    }

    /// <summary>
    /// Conecta el botón de "Atacar" con la UI. Ya no guarda sus propias stats:
    /// se las pide a CombatManager, que sabe quién está peleando con quién
    /// (jugador y enemigo actualmente en rango).
    /// </summary>
    public class AttackController : MonoBehaviour
    {
        [Header("UI")]
        public Button attackButton;
        public Button confirmButton;
        public GameObject previewPanel;
        public TMP_Text hitChanceLabel;
        public TMP_Text damageRangeLabel;
        public TMP_Text resultLabel;

        private void Awake()
        {
            attackButton.onClick.AddListener(ShowPreview);
            confirmButton.onClick.AddListener(ResolveAttack);
            previewPanel.SetActive(false);
        }

        private void ShowPreview()
        {
            if (CombatManager.Instance == null || !CombatManager.Instance.TryGetPreview(out var preview))
            {
                Debug.LogWarning("No hay un enemigo en rango para atacar");
                return;
            }

            hitChanceLabel.text = $"{preview.HitChancePercent:0}% de acierto";
            damageRangeLabel.text = $"Daño: {preview.MinDamage}-{preview.MaxDamage}";

            previewPanel.SetActive(true);
        }

        private void ResolveAttack()
        {
            if (CombatManager.Instance == null || !CombatManager.Instance.TryResolveAttack(out var result))
            {
                Debug.LogWarning("No hay un enemigo en rango para atacar");
                previewPanel.SetActive(false);
                return;
            }

            resultLabel.text = result.CriticalHit
                ? $"¡Crítico! {result.DamageDealt} de daño"
                : result.Hit
                    ? $"Impacto ({result.AttackRoll}). {result.DamageDealt} de daño"
                    : $"Fallo ({result.AttackRoll})";

            previewPanel.SetActive(false);
        }

        public static AttackResult ResolveAttackRoll(int attackerBonus, int targetAC, string damageNotation,
            bool advantage = false, bool disadvantage = false)
        {
            int roll = DiceRoller.RollD20(advantage, disadvantage);
            bool naturalCrit = roll == 20;
            bool naturalMiss = roll == 1;

            int total = roll + attackerBonus;
            bool hit = naturalCrit || (!naturalMiss && total >= targetAC);

            int damage = 0;
            if (hit)
            {
                var dice = DiceRoller.Parse(damageNotation);
                damage = DiceRoller.Roll(dice);
                if (naturalCrit)
                {
                    // Regla estándar 5e: en crítico se duplican los dados de daño (no el modificador)
                    damage += DiceRoller.Roll(new DiceRoller.DiceExpression
                    {
                        Count = dice.Count,
                        Sides = dice.Sides,
                        Modifier = 0
                    });
                }
            }

            return new AttackResult
            {
                AttackRoll = roll,
                Hit = hit,
                CriticalHit = naturalCrit,
                DamageDealt = damage
            };
        }
    }
}