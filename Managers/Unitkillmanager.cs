using Coursework.GameObjects;
using Coursework.GameObjects.Units;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Coursework.Managers
{
    internal enum GamePhase
    {
        EarlyGame,
        MidGame,
        LateGame,
        PanicMode
    }

    /// <summary>
    /// Player clicks damage enemy units. Enemy AI periodically damages a random player unit.
    /// Units are removed by UnitManager when Health reaches 0.
    /// </summary>
    internal class UnitKillManager
    {
        // Clicks needed to kill a full-health unit of that type. Damage per click is Health/clicks.
        public int SoldierRequiredClicks = 2;
        public int MachineGunnerRequiredClicks = 4;
        public int TankRequiredClicks = 6;

        public int SoldierKillReward = 15;
        public int MachineGunnerKillReward = 25;
        public int TankKillReward = 40;

        // Enemy AI applies this much Health damage to one random living player unit per cooldown.
        public int AiDamageToPlayerUnits = 35;
        public float EarlyGameKillInterval = 8f;
        public float MidGameKillInterval = 5f;
        public float LateGameKillInterval = 2.5f;
        public float PanicModeKillInterval = 1f;

        public float MidGamePhaseStartSeconds = 60f;
        public float LateGamePhaseStartSeconds = 150f;
        public float PanicModeTowerHealthThreshold = 0.3f;

        public ExplosionManager ExplosionManager;
        public SoundManager SoundManager;
        public MoneyManager MoneyManager;

        public GamePhase CurrentPhase { get; private set; } = GamePhase.EarlyGame;

        private readonly UnitManager unitManager;
        private readonly Tower panicTrigger;
        private readonly Random rand = new Random();

        private float elapsedSeconds;
        private float aiAttackTimer;

        public UnitKillManager(UnitManager unitManager, Tower panicTrigger)
        {
            this.unitManager = unitManager;
            this.panicTrigger = panicTrigger;
            aiAttackTimer = EarlyGameKillInterval;
        }

        public void RegisterClick(Point clickPosition)
        {
            BaseUnit clicked = FindClickedEnemyUnit(clickPosition);
            if (clicked == null)
                return;

            int damage = DamagePerClick(clicked);
            bool wasAlive = clicked.IsAlive;
            clicked.TakeDamage(damage);

            if (wasAlive && !clicked.IsAlive)
                HandleEnemyDeath(clicked);
        }

        private BaseUnit FindClickedEnemyUnit(Point clickPosition)
        {
            foreach (var u in unitManager.EnemySoldiers)
                if (u != null && u.IsAlive && u.Rectangle.Contains(clickPosition)) return u;

            foreach (var u in unitManager.EnemyMachineGunners)
                if (u != null && u.IsAlive && u.Rectangle.Contains(clickPosition)) return u;

            foreach (var u in unitManager.EnemyTanks)
                if (u != null && u.IsAlive && u.Rectangle.Contains(clickPosition)) return u;

            return null;
        }

        private int DamagePerClick(BaseUnit unit)
        {
            int clicks = RequiredClicksFor(unit);
            int hp = unit.MaxHealth > 0 ? unit.MaxHealth : Math.Max(1, unit.Health);
            return Math.Max(1, (int)Math.Ceiling(hp / (float)clicks));
        }

        private int RequiredClicksFor(BaseUnit unit)
        {
            if (unit is Tank) return Math.Max(1, TankRequiredClicks);
            if (unit is MachineGunner) return Math.Max(1, MachineGunnerRequiredClicks);
            return Math.Max(1, SoldierRequiredClicks);
        }

        private int RewardFor(BaseUnit unit)
        {
            if (unit is Tank) return TankKillReward;
            if (unit is MachineGunner) return MachineGunnerKillReward;
            return SoldierKillReward;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            elapsedSeconds += dt;
            CurrentPhase = DeterminePhase();

            aiAttackTimer -= dt;
            if (aiAttackTimer > 0f)
                return;

            aiAttackTimer = IntervalFor(CurrentPhase);

            BaseUnit target = ChooseRandomPlayerUnit();
            if (target == null)
                return;

            bool wasAlive = target.IsAlive;
            target.TakeDamage(AiDamageToPlayerUnits);

            if (wasAlive && !target.IsAlive)
                HandlePlayerUnitDeath(target);
        }

        private GamePhase DeterminePhase()
        {
            if (panicTrigger != null && panicTrigger.HealthPercentage <= PanicModeTowerHealthThreshold)
                return GamePhase.PanicMode;

            if (elapsedSeconds >= LateGamePhaseStartSeconds) return GamePhase.LateGame;
            if (elapsedSeconds >= MidGamePhaseStartSeconds) return GamePhase.MidGame;
            return GamePhase.EarlyGame;
        }

        private float IntervalFor(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.MidGame: return MidGameKillInterval;
                case GamePhase.LateGame: return LateGameKillInterval;
                case GamePhase.PanicMode: return PanicModeKillInterval;
                default: return EarlyGameKillInterval;
            }
        }

        private BaseUnit ChooseRandomPlayerUnit()
        {
            var pool = new List<BaseUnit>();
            foreach (var u in unitManager.PlayerSoldiers) if (u != null && u.IsAlive) pool.Add(u);
            foreach (var u in unitManager.PlayerTanks) if (u != null && u.IsAlive) pool.Add(u);
            foreach (var u in unitManager.PlayerMachineGunners) if (u != null && u.IsAlive) pool.Add(u);
            return pool.Count == 0 ? null : pool[rand.Next(pool.Count)];
        }

        private void HandleEnemyDeath(BaseUnit unit)
        {
            ExplosionManager?.AddExplosion(unit.Center, 0, 24f);
            SoundManager?.PlayExplosion();
            MoneyManager?.AddMoney(RewardFor(unit));
        }

        private void HandlePlayerUnitDeath(BaseUnit unit)
        {
            ExplosionManager?.AddExplosion(unit.Center, 0, 24f);
            SoundManager?.PlayExplosion();
        }
    }
}
