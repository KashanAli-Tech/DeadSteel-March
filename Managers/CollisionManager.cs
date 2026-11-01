using Coursework.GameObjects;
using Microsoft.Xna.Framework;

namespace Coursework.Managers
{
    // keeps hit detection separate from moving and drawing units, shells and explosions
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

        // resolving hits before cleaning up units removes enemies or allies killed by those hits
        public void Update(GameTime gameTime)
        {
            ResolveShellHits();
            ResolveExplosionHits();
            UnitManager?.CleanupDeadUnits();
        }

        // looping backwards lets a shell be removed after a hit without skipping the next list entry
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

        // using the shell's side selects the opposing tower, so it cannot directly hit its own tower
        private bool TryHitTower(Shell shell)
        {
            var targetTower = shell.IsPlayerShell ? EnemyTower : PlayerTower;
            if (targetTower == null || targetTower.IsDead)
                return false;

            // a rectangle overlap is a simple enough hit check for shells reaching the large tower target
            if (!shell.Rectangle.Intersects(targetTower.Rectangle))
                return false;

            targetTower.TakeDamage(shell.Damage);
            ExplosionManager?.AddExplosion(targetTower.Center, shell.Damage, 20f);
            SoundManager?.PlayExplosion();
            shell.IsAlive = false;
            return true;
        }

        // checking both sides means any living unit inside the blast radius can take explosion damage
        private void ResolveExplosionHits()
        {
            if (ExplosionManager == null || UnitManager == null)
                return;

            foreach (var explosion in ExplosionManager.Explosions)
            {
                if (explosion == null || explosion.DamageApplied || explosion.Damage <= 0)
                    continue;

                // marking the blast before checking units stops its damage being repeated on later frames
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
