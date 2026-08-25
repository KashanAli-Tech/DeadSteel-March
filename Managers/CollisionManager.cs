using Coursework.GameObjects;
using Microsoft.Xna.Framework;

namespace Coursework.Managers
{
    internal class CollisionManager
    {
        public UnitManager UnitManager;
        public ShellManager ShellManager;
        public ExplosionManager ExplosionManager;
        public SoundManager SoundManager;

        public Tower PlayerTower;
        public Tower EnemyTower;

        public CollisionManager(UnitManager unitManager, ShellManager shellManager, ExplosionManager explosionManager)
        {
            UnitManager = unitManager;
            ShellManager = shellManager;
            ExplosionManager = explosionManager;
        }

        public void Update(GameTime gameTime)
        {
            ResolveShellHits();
            ResolveExplosionHits();
            UnitManager?.CleanupDeadUnits();
        }

        private void ResolveShellHits()
        {
            if (ShellManager == null)
                return;

            var shells = ShellManager.Shells;
            for (int i = shells.Count - 1; i >= 0; i--)
            {
                var shell = shells[i];
                if (shell == null || !shell.IsAlive)
                    continue;

                if (TryHitTower(shell))
                    shells.RemoveAt(i);
            }
        }

        private bool TryHitTower(Shell shell)
        {
            // Player shells only hit the enemy tower; enemy shells only hit the player tower.
            var targetTower = shell.IsPlayerShell ? EnemyTower : PlayerTower;
            if (targetTower == null || targetTower.IsDead)
                return false;

            if (!shell.Rectangle.Intersects(targetTower.Rectangle))
                return false;

            targetTower.TakeDamage(shell.Damage);
            ExplosionManager?.AddExplosion(targetTower.Center, shell.Damage, 20f);
            SoundManager?.PlayExplosion();
            shell.IsAlive = false;
            return true;
        }

        // circle check - old Bounds rect wasn't used anywhere else
        private void ResolveExplosionHits()
        {
            if (ExplosionManager == null || UnitManager == null)
                return;

            foreach (var explosion in ExplosionManager.Explosions)
            {
                if (explosion == null || explosion.DamageApplied || explosion.Damage <= 0)
                    continue;

                explosion.DamageApplied = true;

                foreach (var unit in UnitManager.GetAlivePlayerUnits())
                {
                    if (unit != null && explosion.Hits(unit.Center))
                        unit.TakeDamage(explosion.Damage);
                }

                foreach (var unit in UnitManager.GetAliveEnemyUnits())
                {
                    if (unit != null && explosion.Hits(unit.Center))
                        unit.TakeDamage(explosion.Damage);
                }
            }
        }
    }
}
