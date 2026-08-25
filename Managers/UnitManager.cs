using Coursework.GameObjects;
using Coursework.GameObjects.Units;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Coursework.Managers
{
    /// <summary>
    /// Owns every unit in the game: spawning, movement toward the opposing tower, tower attacks, and cleanup.
    /// Units never fight each other. Player click-damage and AI damage to units live in UnitKillManager.
    /// </summary>
    internal class UnitManager
    {
        // ---- Unit lists ----
        public List<Soldier> PlayerSoldiers = new List<Soldier>();
        public List<Soldier> EnemySoldiers = new List<Soldier>();
        public List<Tank> PlayerTanks = new List<Tank>();
        public List<Tank> EnemyTanks = new List<Tank>();
        public List<MachineGunner> PlayerMachineGunners = new List<MachineGunner>();
        public List<MachineGunner> EnemyMachineGunners = new List<MachineGunner>();

        // ---- Textures ----
        private Texture2D playerSoldierTexture;
        private Texture2D playerTankTexture;
        private Texture2D playerMachineGunTexture;
        private Texture2D enemySoldierTexture;
        private Texture2D enemyTankTexture;
        private Texture2D enemyMachineGunTexture;

        public Texture2D PlayerSoldierTexture => playerSoldierTexture;
        public Texture2D EnemySoldierTexture => enemySoldierTexture;
        public Texture2D PlayerTankTexture => playerTankTexture;
        public Texture2D EnemyTankTexture => enemyTankTexture;
        public Texture2D PlayerMachineGunTexture => playerMachineGunTexture;
        public Texture2D EnemyMachineGunTexture => enemyMachineGunTexture;

        // ---- Difficulty ----
        // EnemyAIManager decides WHEN to spawn/kill units; this controls HOW STRONG those units are
        // once they exist. 1f = same stats as an equivalent player unit. Tune freely.
        public float EnemyDifficultyScale = 0.75f;

        public Vector2 PlayerTowerV;
        public Vector2 EnemyTowerV;
        private readonly Random rand = new Random();
        private const float SpawnClearance = 24f;
        private const int SpawnFootprint = 56;

        // External managers
        public ShellManager ShellManager;
        public SoundManager SoundManager;

        // same gap on both sides, and never further than a soldier can shoot
        private const float TowerStopDistance = 70f;

        private Tower playerTower;
        private Tower enemyTower;

        public UnitManager()
        {
            PlayerTowerV = new Vector2(20, 250);
            EnemyTowerV = new Vector2(1600, 250);
        }

        public void SetTowerPositions(Vector2 left, Vector2 right)
        {
            PlayerTowerV = left;
            EnemyTowerV = right;
        }

        public void SetTowers(Tower player, Tower enemy)
        {
            playerTower = player;
            enemyTower = enemy;
        }

        public void LoadContent(ContentManager content)
        {
            playerSoldierTexture = content.Load<Texture2D>("PlayerSoldier");
            playerTankTexture = content.Load<Texture2D>("PlayerTank");
            playerMachineGunTexture = content.Load<Texture2D>("PlayerMachineGunner");

            enemySoldierTexture = content.Load<Texture2D>("EnemySoldier");
            enemyTankTexture = content.Load<Texture2D>("EnemyTank");
            enemyMachineGunTexture = content.Load<Texture2D>("EnemyMachineGunner");
        }

        // ---------------- Spawning ----------------

        public void SpawnPlayerSoldier()
        {
            var unit = new Soldier(playerSoldierTexture, PlayerSpawnPosition());
            unit.IsPlayer = true;
            PlayerSoldiers.Add(unit);
        }

        public void SpawnPlayerTank()
        {
            var unit = new Tank(playerTankTexture, PlayerSpawnPosition());
            unit.IsPlayer = true;
            PlayerTanks.Add(unit);
        }

        public void SpawnPlayerMachineGunner()
        {
            var unit = new MachineGunner(playerMachineGunTexture, PlayerSpawnPosition());
            unit.IsPlayer = true;
            PlayerMachineGunners.Add(unit);
        }

        // Call these from EnemyAIManager - it decides *when* to spawn, this just creates the unit
        // (at the correct difficulty) and adds it to the right list.
        public void SpawnEnemySoldier()
        {
            var unit = new Soldier(enemySoldierTexture, EnemySpawnPosition());
            unit.IsPlayer = false;
            ApplyDifficultyScale(unit);
            EnemySoldiers.Add(unit);
        }

        public void SpawnEnemyTank()
        {
            var unit = new Tank(enemyTankTexture, EnemySpawnPosition());
            unit.IsPlayer = false;
            ApplyDifficultyScale(unit);
            EnemyTanks.Add(unit);
        }

        public void SpawnEnemyMachineGunner()
        {
            var unit = new MachineGunner(enemyMachineGunTexture, EnemySpawnPosition());
            unit.IsPlayer = false;
            ApplyDifficultyScale(unit);
            EnemyMachineGunners.Add(unit);
        }

        private Vector2 PlayerSpawnPosition()
        {
            float towerWidth = playerTower != null ? playerTower.TowerWidth : 300f;
            float towerHeight = playerTower != null ? playerTower.TowerHeight : 400f;
            float x = PlayerTowerV.X + towerWidth + SpawnClearance;
            return FindFreeSpawnPosition(x, PlayerTowerV.Y, towerHeight);
        }

        private Vector2 EnemySpawnPosition()
        {
            float towerHeight = enemyTower != null ? enemyTower.TowerHeight : 400f;
            float x = EnemyTowerV.X - SpawnClearance - SpawnFootprint;
            return FindFreeSpawnPosition(x, EnemyTowerV.Y, towerHeight);
        }

        private Vector2 FindFreeSpawnPosition(float x, float towerY, float towerHeight)
        {
            int minY = (int)(towerY + 16f);
            int maxY = (int)(towerY + Math.Max(towerHeight - SpawnFootprint - 16f, 32f));
            if (maxY <= minY)
                maxY = minY + 1;

            for (int attempt = 0; attempt < 16; attempt++)
            {
                int y = rand.Next(minY, maxY);
                var pos = new Vector2(x, y);
                if (!SpawnWouldOverlap(pos))
                    return pos;
            }

            int stackedY = minY + ((CountAliveUnits() * 40) % Math.Max(1, maxY - minY));
            return new Vector2(x, stackedY);
        }

        private bool SpawnWouldOverlap(Vector2 pos)
        {
            var footprint = new Rectangle((int)pos.X, (int)pos.Y, SpawnFootprint, SpawnFootprint);

            if (playerTower != null && footprint.Intersects(playerTower.Rectangle))
                return true;
            if (enemyTower != null && footprint.Intersects(enemyTower.Rectangle))
                return true;

            foreach (var unit in AllUnits())
            {
                if (unit.IsAlive && footprint.Intersects(unit.Rectangle))
                    return true;
            }

            return false;
        }

        private int CountAliveUnits()
        {
            int count = 0;
            foreach (var unit in AllUnits())
            {
                if (unit.IsAlive)
                    count++;
            }
            return count;
        }

        public List<BaseUnit> GetAlivePlayerUnits()
        {
            var list = new List<BaseUnit>();
            AddAlive(PlayerSoldiers, list);
            AddAlive(PlayerTanks, list);
            AddAlive(PlayerMachineGunners, list);
            return list;
        }

        public List<BaseUnit> GetAliveEnemyUnits()
        {
            var list = new List<BaseUnit>();
            AddAlive(EnemySoldiers, list);
            AddAlive(EnemyTanks, list);
            AddAlive(EnemyMachineGunners, list);
            return list;
        }

        private IEnumerable<BaseUnit> AllUnits()
        {
            foreach (var u in PlayerSoldiers) yield return u;
            foreach (var u in PlayerTanks) yield return u;
            foreach (var u in PlayerMachineGunners) yield return u;
            foreach (var u in EnemySoldiers) yield return u;
            foreach (var u in EnemyTanks) yield return u;
            foreach (var u in EnemyMachineGunners) yield return u;
        }

        private static void AddAlive<T>(List<T> source, List<BaseUnit> dest) where T : BaseUnit
        {
            foreach (var unit in source)
            {
                if (unit != null && unit.IsAlive)
                    dest.Add(unit);
            }
        }

        private void ApplyDifficultyScale(BaseUnit unit)
        {
            unit.Health = Math.Max(1, (int)(unit.Health * EnemyDifficultyScale));
            unit.MaxHealth = unit.Health;
            unit.Damage = Math.Max(1, (int)(unit.Damage * EnemyDifficultyScale));
        }

        // ---------------- Update ----------------

        public void Update(GameTime gameTime)
        {
            MoveUnits(gameTime);
            PreventWalkingThroughTowers();

            if (playerTower == null || enemyTower == null || playerTower.IsDead || enemyTower.IsDead)
            {
                CleanupDeadUnits();
                return;
            }

            HandleTowerEngagements();
            CleanupDeadUnits();
        }

        private void MoveUnits(GameTime gameTime)
        {
            Vector2 enemyApproach = GetEnemyApproachPoint();
            Vector2 playerApproach = GetPlayerApproachPoint();

            foreach (var s in PlayerSoldiers) if (s != null && s.IsAlive) s.Update(new Vector2(enemyApproach.X, s.Position.Y), gameTime);
            foreach (var t in PlayerTanks) if (t != null && t.IsAlive) t.Update(new Vector2(enemyApproach.X, t.Position.Y), gameTime);
            foreach (var m in PlayerMachineGunners) if (m != null && m.IsAlive) m.Update(new Vector2(enemyApproach.X, m.Position.Y), gameTime);

            foreach (var s in EnemySoldiers) if (s != null && s.IsAlive) s.Update(new Vector2(playerApproach.X, s.Position.Y), gameTime);
            foreach (var t in EnemyTanks) if (t != null && t.IsAlive) t.Update(new Vector2(playerApproach.X, t.Position.Y), gameTime);
            foreach (var m in EnemyMachineGunners) if (m != null && m.IsAlive) m.Update(new Vector2(playerApproach.X, m.Position.Y), gameTime);
        }

        private Vector2 GetEnemyApproachPoint()
        {
            if (enemyTower != null)
                return new Vector2(enemyTower.TowerX, enemyTower.Center.Y);
            return EnemyTowerV;
        }

        private Vector2 GetPlayerApproachPoint()
        {
            if (playerTower != null)
                return new Vector2(playerTower.TowerX + playerTower.TowerWidth, playerTower.Center.Y);
            return PlayerTowerV;
        }

        private void PreventWalkingThroughTowers()
        {
            if (enemyTower != null)
            {
                foreach (var unit in GetAlivePlayerUnits())
                {
                    float stop = Math.Min(TowerStopDistance, unit.AttackRange);
                    float stopX = enemyTower.TowerX - stop - 20;
                    if (unit.Position.X > stopX)
                        unit.Position.X = stopX;
                }
            }

            if (playerTower != null)
            {
                foreach (var unit in GetAliveEnemyUnits())
                {
                    float stop = Math.Min(TowerStopDistance, unit.AttackRange);
                    float stopX = playerTower.TowerX + playerTower.TowerWidth + stop;
                    if (unit.Position.X < stopX)
                        unit.Position.X = stopX;
                }
            }
        }

        private void HandleTowerEngagements()
        {
            Vector2 enemyApproach = GetEnemyApproachPoint();
            Vector2 playerApproach = GetPlayerApproachPoint();
            Vector2 enemyAim = enemyTower != null ? enemyTower.Center : EnemyTowerV;
            Vector2 playerAim = playerTower != null ? playerTower.Center : PlayerTowerV;

            EngageTower(PlayerSoldiers, enemyApproach, enemyAim, enemyTower, isPlayer: true);
            EngageTower(PlayerTanks, enemyApproach, enemyAim, enemyTower, isPlayer: true);
            EngageTower(PlayerMachineGunners, enemyApproach, enemyAim, enemyTower, isPlayer: true);

            EngageTower(EnemySoldiers, playerApproach, playerAim, playerTower, isPlayer: false);
            EngageTower(EnemyTanks, playerApproach, playerAim, playerTower, isPlayer: false);
            EngageTower(EnemyMachineGunners, playerApproach, playerAim, playerTower, isPlayer: false);
        }

        private void EngageTower<T>(List<T> units, Vector2 approachPoint, Vector2 aimPoint, Tower tower, bool isPlayer) where T : BaseUnit
        {
            if (tower == null || tower.IsDead)
                return;

            foreach (var unit in units)
            {
                if (unit == null || !unit.IsAlive)
                    continue;

                bool atTower = unit.IsInAttackRange(new Vector2(approachPoint.X, unit.Position.Y))
                    || unit.Rectangle.Intersects(tower.Rectangle);
                if (!atTower)
                    continue;

                if (unit.attackTimer > 0f)
                    continue;

                if (unit.FiresShells)
                    FireShellAtTower(unit, aimPoint, isPlayer);
                else
                    tower.TakeDamage(unit.Damage);

                unit.attackTimer = unit.AttackSpeed;
            }
        }

        private void FireShellAtTower(BaseUnit unit, Vector2 towerTarget, bool isPlayer)
        {
            if (ShellManager == null || ShellManager.ShellTexture == null || unit == null)
                return;

            var shell = new Shell(ShellManager.ShellTexture, unit.Center, towerTarget, isPlayer, unit.Damage, 900f);
            ShellManager.AddShell(shell);
            SoundManager?.PlayShoot();
        }

        public void CleanupDeadUnits()
        {
            PlayerSoldiers.RemoveAll(u => u == null || u.Health <= 0);
            EnemySoldiers.RemoveAll(u => u == null || u.Health <= 0);
            PlayerTanks.RemoveAll(u => u == null || u.Health <= 0);
            EnemyTanks.RemoveAll(u => u == null || u.Health <= 0);
            PlayerMachineGunners.RemoveAll(u => u == null || u.Health <= 0);
            EnemyMachineGunners.RemoveAll(u => u == null || u.Health <= 0);
        }

        // ---------------- Draw ----------------

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var s in PlayerSoldiers) s.Draw(spriteBatch);
            foreach (var t in PlayerTanks) t.Draw(spriteBatch);
            foreach (var m in PlayerMachineGunners) m.Draw(spriteBatch);

            foreach (var s in EnemySoldiers) s.Draw(spriteBatch);
            foreach (var t in EnemyTanks) t.Draw(spriteBatch);
            foreach (var m in EnemyMachineGunners) m.Draw(spriteBatch);
        }
    }
}