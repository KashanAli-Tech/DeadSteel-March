using Coursework.GameObjects;
using Coursework.GameObjects.Units;
using Microsoft.Xna.Framework;
using System;

namespace Coursework.Managers
{
    // these named phases are used by the enemy ai to choose its current settings
    internal enum GamePhase
    {
        EarlyGame,
        MidGame,
        LateGame,
        PanicMode
    }

    // handles damage from the player's clicks, with enemy target selection kept in the ai manager
    internal class UnitKillManager
    {
        // separate click counts and rewards make each enemy type easier to balance
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

        // keeping the unit manager reference lets clicks use the enemies that are actually in the current match
        public UnitKillManager(UnitManager unitManager, Tower panicTrigger)
        {
            this.unitManager = unitManager;
            // this argument is unused here now that the enemy ai handles its own phase checks
            _ = panicTrigger;
        }

        // applying damage to the clicked enemy and only awarding a kill when that hit changes it from alive to dead
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

        // checking only living enemy rectangles prevents clicks from damaging friendly or already dead units
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

        // dividing maximum health by the required clicks keeps click damage consistent for that enemy's starting health
        private int DamagePerClick(BaseUnit unit)
        {
            int clicks = RequiredClicksFor(unit);
            int hp = unit.MaxHealth > 0 ? unit.MaxHealth : Math.Max(1, unit.Health);
            // rounding up and requiring at least one damage avoids clicks having no effect on low-health units
            return Math.Max(1, (int)Math.Ceiling(hp / (float)clicks));
        }

        // clamping each setting to at least one avoids dividing by zero when calculating click damage
        private int RequiredClicksFor(BaseUnit unit)
        {
            if (unit is Tank) return Math.Max(1, TankRequiredClicks);
            if (unit is MachineGunner) return Math.Max(1, MachineGunnerRequiredClicks);
            return Math.Max(1, SoldierRequiredClicks);
        }

        // using the enemy's type selects its configured reward without duplicating the death-handling code
        private int RewardFor(BaseUnit unit)
        {
            if (unit is Tank) return TankKillReward;
            if (unit is MachineGunner) return MachineGunnerKillReward;
            return SoldierKillReward;
        }

        // a zero-damage explosion shows the kill without adding another damaging blast around it
        private void HandleEnemyDeath(BaseUnit unit)
        {
            ExplosionManager?.AddExplosion(unit.Center, 0, 24f);
            SoundManager?.PlayExplosion();
            MoneyManager?.AddMoney(RewardFor(unit));
        }
    }
}
