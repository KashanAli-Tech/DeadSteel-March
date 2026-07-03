using Coursework.GameObjects;
using Coursework.GameObjects.Units;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Coursework.Managers
{
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
            // Shell collisions
            for (int i = ShellManager.Shells.Count - 1; i >= 0; i--)
            {
                var shell = ShellManager.Shells[i];

                // Check against enemy units and towers
                bool hit = false;

                // Player shells hitting enemy units
                foreach (var u in UnitManager.EnemySoldiers)
                {
                    if (shell.Rectangle.Intersects(u.Rectangle))
                    {
                        u.Health -= shell.Damage;
                        ExplosionManager.AddExplosion(shell.Position, shell.Damage, 30f);
                        shell.IsAlive = false;
                        MoneyManager?.AddMoney(10); // reward attacker? adjust logic as needed
                        SoundManager?.PlayExplosion();
                        hit = true;
                        break;
                    }
                }
                if (hit) { ShellManager.Shells.RemoveAt(i); continue; }

                // Player shells hitting enemy tower
                if (EnemyTower != null && shell.Rectangle.Intersects(EnemyTower.Rectangle))
                {
                    EnemyTower.TakeDamage(shell.Damage);
                    ExplosionManager.AddExplosion(shell.Position, shell.Damage, 40f);
                    shell.IsAlive = false;
                    SoundManager?.PlayExplosion();
                    ShellManager.Shells.RemoveAt(i);
                    continue;
                }

                // Enemy shells hitting player units
                foreach (var u in UnitManager.PlayerSoldiers)
                {
                    if (shell.Rectangle.Intersects(u.Rectangle))
                    {
                        u.Health -= shell.Damage;
                        ExplosionManager.AddExplosion(shell.Position, shell.Damage, 30f);
                        shell.IsAlive = false;
                        SoundManager?.PlayExplosion();
                        hit = true;
                        break;
                    }
                }
                if (hit) { ShellManager.Shells.RemoveAt(i); continue; }

                // Enemy shells hitting player tower
                if (PlayerTower != null && shell.Rectangle.Intersects(PlayerTower.Rectangle))
                {
                    PlayerTower.TakeDamage(shell.Damage);
                    ExplosionManager.AddExplosion(shell.Position, shell.Damage, 40f);
                    shell.IsAlive = false;
                    SoundManager?.PlayExplosion();
                    ShellManager.Shells.RemoveAt(i);
                    continue;
                }
            }

            // Explosion area damage (simple)
            for (int e = ExplosionManager.Explosions.Count - 1; e >= 0; e--)
            {
                var ex = ExplosionManager.Explosions[e];
                // damage enemy units in radius
                foreach (var u in UnitManager.EnemySoldiers)
                {
                    if (ex.Bounds.Intersects(u.Rectangle))
                    {
                        u.Health -= ex.Damage;
                    }
                }
                foreach (var u in UnitManager.PlayerSoldiers)
                {
                    if (ex.Bounds.Intersects(u.Rectangle))
                    {
                        u.Health -= ex.Damage;
                    }
                }
            }
        }
    }
}
