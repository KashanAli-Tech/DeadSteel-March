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
            for (int i = ShellManager.Shells.Count - 1; i >= 0; i--)
            {
                var shell = ShellManager.Shells[i];

                bool hit = false;

                // PLAYER SHELLS
                if (shell.IsPlayerShell)
                {
                    foreach (var u in UnitManager.EnemySoldiers)
                    {
                        if (shell.Rectangle.Intersects(u.Rectangle))
                        {
                            u.Health -= shell.Damage;
                            hit = true;
                            break;
                        }
                    }

                    foreach (var u in UnitManager.EnemyTanks)
                    {
                        if (!hit && shell.Rectangle.Intersects(u.Rectangle))
                        {
                            u.Health -= shell.Damage;
                            hit = true;
                            break;
                        }
                    }

                    foreach (var u in UnitManager.EnemyMachineGunners)
                    {
                        if (!hit && shell.Rectangle.Intersects(u.Rectangle))
                        {
                            u.Health -= shell.Damage;
                            hit = true;
                            break;
                        }
                    }

                    if (!hit && EnemyTower != null && shell.Rectangle.Intersects(EnemyTower.Rectangle))
                    {
                        EnemyTower.TakeDamage(shell.Damage);
                        hit = true;
                    }
                }


                // ENEMY SHELLS
                else
                {
                    foreach (var u in UnitManager.PlayerSoldiers)
                    {
                        if (shell.Rectangle.Intersects(u.Rectangle))
                        {
                            u.Health -= shell.Damage;
                            hit = true;
                            break;
                        }
                    }

                    foreach (var u in UnitManager.PlayerTanks)
                    {
                        if (!hit && shell.Rectangle.Intersects(u.Rectangle))
                        {
                            u.Health -= shell.Damage;
                            hit = true;
                            break;
                        }
                    }

                    foreach (var u in UnitManager.PlayerMachineGunners)
                    {
                        if (!hit && shell.Rectangle.Intersects(u.Rectangle))
                        {
                            u.Health -= shell.Damage;
                            hit = true;
                            break;
                        }
                    }

                    if (!hit && PlayerTower != null && shell.Rectangle.Intersects(PlayerTower.Rectangle))
                    {
                        PlayerTower.TakeDamage(shell.Damage);
                        hit = true;
                    }
                }


                if (hit)
                {
                    ExplosionManager.AddExplosion(shell.Position, shell.Damage, 30f);
                    SoundManager?.PlayExplosion();

                    if (shell.IsPlayerShell)
                        MoneyManager?.AddMoney(10);

                    ShellManager.Shells.RemoveAt(i);
                }
            }


            // Explosion damage
            for (int e = ExplosionManager.Explosions.Count - 1; e >= 0; e--)
            {
                var ex = ExplosionManager.Explosions[e];


                foreach (var u in UnitManager.EnemySoldiers)
                {
                    if (ex.Bounds.Intersects(u.Rectangle))
                        u.Health -= ex.Damage;
                }

                foreach (var u in UnitManager.EnemyTanks)
                {
                    if (ex.Bounds.Intersects(u.Rectangle))
                        u.Health -= ex.Damage;
                }

                foreach (var u in UnitManager.EnemyMachineGunners)
                {
                    if (ex.Bounds.Intersects(u.Rectangle))
                        u.Health -= ex.Damage;
                }


                foreach (var u in UnitManager.PlayerSoldiers)
                {
                    if (ex.Bounds.Intersects(u.Rectangle))
                        u.Health -= ex.Damage;
                }

                foreach (var u in UnitManager.PlayerTanks)
                {
                    if (ex.Bounds.Intersects(u.Rectangle))
                        u.Health -= ex.Damage;
                }

                foreach (var u in UnitManager.PlayerMachineGunners)
                {
                    if (ex.Bounds.Intersects(u.Rectangle))
                        u.Health -= ex.Damage;
                }
            }
        }
    }
}