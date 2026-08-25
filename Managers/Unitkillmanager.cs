using Coursework.GameObjects;
using Coursework.GameObjects.Units;
using Microsoft.Xna.Framework;
using System;

namespace Coursework.Managers
{
    internal enum GamePhase
    {
        EarlyGame,
        MidGame,
        LateGame,
        PanicMode
    }

    // player click-damage only. AI targeting is in EnemyAIManager.
    internal class UnitKillManager
    {
        // clicks needed to kill a full-health unit of that type
        public int SoldierRequiredClicks = 2;
        public int MachineGunnerRequiredClicks = 4;
        public int TankRequiredClicks = 6;

        public int SoldierKillReward = 15;
        public int MachineGunnerKillReward = 25;
        public int TankKillReward = 40;

        public ExplosionManager ExplosionManager;
        public SoundManager SoundManager;
        public MoneyManager MoneyManager;

        private readonly UnitManager unitManager;

        public UnitKillManager(UnitManager unitManager, Tower panicTrigger)
        {
            this.unitManager = unitManager;
            _ = panicTrigger; // leftover from when this class also ran AI panic
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

        private void HandleEnemyDeath(BaseUnit unit)
        {
            ExplosionManager?.AddExplosion(unit.Center, 0, 24f);
            SoundManager?.PlayExplosion();
            MoneyManager?.AddMoney(RewardFor(unit));
        }
    }
}
