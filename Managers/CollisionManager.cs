using Coursework.GameObjects;
using Microsoft.Xna.Framework;

namespace Coursework.Managers
{
    /// <summary>
    /// Shell vs tower hits only. Units never damage other units here.
    /// </summary>
    internal class CollisionManager
    {
        public UnitManager UnitManager;
        public ShellManager ShellManager;
        public ExplosionManager ExplosionManager;
        public MoneyManager MoneyManager;
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
            ExplosionManager?.AddExplosion(shell.Position, shell.Damage, 30f);
            SoundManager?.PlayExplosion();
            shell.IsAlive = false;
            return true;
        }
    }
}
