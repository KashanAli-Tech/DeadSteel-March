using Coursework.GameObjects;
using Coursework.GameObjects.Units;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Coursework.Managers
{
     // keeps spawning, movement and tower attacks together, while player click damage and enemy ai targeting stay in their own managers
    internal class UnitManager
    {
        public int PlayerUnitsDeployed { get; private set; }
        public int EnemyUnitsEliminated { get; private set; }
        // this copy belongs to the match, so editing preferences cannot alter later spawns halfway through
        public MatchConfig MatchConfig;
        // lists can grow and shrink during a match as units spawn or die, with separate lists for each side and type
        public List<Soldier> PlayerSoldiers = new List<Soldier>();
        public List<Soldier> EnemySoldiers = new List<Soldier>();
        public List<Tank> PlayerTanks = new List<Tank>();
        public List<Tank> EnemyTanks = new List<Tank>();
        public List<MachineGunner> PlayerMachineGunners = new List<MachineGunner>();
        public List<MachineGunner> EnemyMachineGunners = new List<MachineGunner>();

        private Texture2D playerSoldierTexture;
        private Texture2D playerTankTexture;
        private Texture2D playerMachineGunTexture;
        private Texture2D enemySoldierTexture;
        private Texture2D enemyTankTexture;
        private Texture2D enemyMachineGunTexture;

        // the ai controls when enemies spawn, while this multiplier changes their starting health and damage
        public float EnemyDifficultyScale = 0.75f;

        public Vector2 PlayerTowerV;
        public Vector2 EnemyTowerV;
        private readonly Random rand = new Random();
        private const float SpawnClearance = 24f;

        public ShellManager ShellManager;
        public SoundManager SoundManager;
        public MoneyManager MoneyManager;

        public int PlayerSoldierCost = 10;
        public int PlayerMachineGunnerCost = 25;
        public int PlayerTankCost = 50;

        // limiting the stop gap by attack range stops units being held too far away to attack
        private const float TowerStopDistance = 70f;

        private Tower playerTower;
        private Tower enemyTower;

        public UnitManager()
        {
            PlayerTowerV = new Vector2(20, 250);
            EnemyTowerV = new Vector2(1600, 250);
        }

        // storing the tower positions also provides fallback targets if tower objects are not available
        public void SetTowerPositions(Vector2 left, Vector2 right)
        {
            PlayerTowerV = left;
            EnemyTowerV = right;
        }

        // keeping references to the real towers lets movement and attacks use their current bounds and health
        public void SetTowers(Tower player, Tower enemy)
        {
            playerTower = player;
            enemyTower = enemy;
        }

        // loading one texture per side and unit type so spawned units can share the same image
        public void LoadContent(ContentManager content)
        {
            playerSoldierTexture = content.Load<Texture2D>("PlayerSoldier");
            playerTankTexture = content.Load<Texture2D>("PlayerTank");
            playerMachineGunTexture = content.Load<Texture2D>("PlayerMachineGunner");

            enemySoldierTexture = content.Load<Texture2D>("EnemySoldier");
            enemyTankTexture = content.Load<Texture2D>("EnemyTank");
            enemyMachineGunTexture = content.Load<Texture2D>("EnemyMachineGunner");
        }



        // checking placement before payment means a blocked spawn cannot spend the player's money
        public void SpawnPlayerSoldier()
        {
            if (MoneyManager != null && (PlayerSoldierCost < 0 || MoneyManager.Money < PlayerSoldierCost))
                return;

            var unit = new Soldier(playerSoldierTexture, Vector2.Zero);
            if (!TryPlayerSpawnPosition(unit, out Vector2 position))
                return;
            unit.Position = position;
            MatchConfig?.ApplyTo(unit);
            unit.IsPlayer = true;
            if (MoneyManager != null && !MoneyManager.Spend(PlayerSoldierCost))
                return;
            PlayerSoldiers.Add(unit);
            PlayerUnitsDeployed++;
        }

        // a successful payment is needed before the tank is added to the player's list
        public void SpawnPlayerTank()
        {
            if (MoneyManager != null && (PlayerTankCost < 0 || MoneyManager.Money < PlayerTankCost))
                return;

            var unit = new Tank(playerTankTexture, Vector2.Zero);
            if (!TryPlayerSpawnPosition(unit, out Vector2 position))
                return;
            unit.Position = position;
            MatchConfig?.ApplyTo(unit);
            unit.IsPlayer = true;
            if (MoneyManager != null && !MoneyManager.Spend(PlayerTankCost))
                return;
            PlayerTanks.Add(unit);
            PlayerUnitsDeployed++;
        }

        // keeping the machine gunner's cost separate makes unit prices easier to balance
        public void SpawnPlayerMachineGunner()
        {
            if (MoneyManager != null && (PlayerMachineGunnerCost < 0 || MoneyManager.Money < PlayerMachineGunnerCost))
                return;

            var unit = new MachineGunner(playerMachineGunTexture, Vector2.Zero);
            if (!TryPlayerSpawnPosition(unit, out Vector2 position))
                return;
            unit.Position = position;
            MatchConfig?.ApplyTo(unit);
            unit.IsPlayer = true;
            if (MoneyManager != null && !MoneyManager.Spend(PlayerMachineGunnerCost))
                return;
            PlayerMachineGunners.Add(unit);
            PlayerUnitsDeployed++;
        }


        // the ai decides when to call this, then the new enemy gets the current difficulty scale
        public void SpawnEnemySoldier()
        {
            var unit = new Soldier(enemySoldierTexture, Vector2.Zero);
            if (!TryEnemySpawnPosition(unit, out Vector2 position))
                return;
            unit.Position = position;
            MatchConfig?.ApplyTo(unit);
            unit.IsPlayer = false;
            ApplyDifficultyScale(unit);
            EnemySoldiers.Add(unit);
        }

        // creating enemies on their own side keeps player costs and upgrade effects separate
        public void SpawnEnemyTank()
        {
            var unit = new Tank(enemyTankTexture, Vector2.Zero);
            if (!TryEnemySpawnPosition(unit, out Vector2 position))
                return;
            unit.Position = position;
            MatchConfig?.ApplyTo(unit);
            unit.IsPlayer = false;
            ApplyDifficultyScale(unit);
            EnemyTanks.Add(unit);
        }

        // marking the unit as an enemy before adding it keeps its targeting and rendering on the correct side
        public void SpawnEnemyMachineGunner()
        {
            var unit = new MachineGunner(enemyMachineGunTexture, Vector2.Zero);
            if (!TryEnemySpawnPosition(unit, out Vector2 position))
                return;
            unit.Position = position;
            MatchConfig?.ApplyTo(unit);
            unit.IsPlayer = false;
            ApplyDifficultyScale(unit);
            EnemyMachineGunners.Add(unit);
        }

        // placing player units past the right edge of their tower leaves space for them to move out
        private bool TryPlayerSpawnPosition(BaseUnit unit, out Vector2 position)
        {
            float towerWidth = playerTower != null ? playerTower.TowerWidth : 300f;
            float towerHeight = playerTower != null ? playerTower.TowerHeight : 400f;
            float x = PlayerTowerV.X + towerWidth + SpawnClearance;
            return TryFindFreeSpawnPosition(unit, x, PlayerTowerV.Y, towerHeight, out position);
        }

        // allowing for the full unit width gives both teams the same clearance from their own tower
        private bool TryEnemySpawnPosition(BaseUnit unit, out Vector2 position)
        {
            float towerHeight = enemyTower != null ? enemyTower.TowerHeight : 400f;
            float x = EnemyTowerV.X - SpawnClearance - unit.Rectangle.Width;
            return TryFindFreeSpawnPosition(unit, x, EnemyTowerV.Y, towerHeight, out position);
        }

        // trying a limited number of random lanes avoids an endless search when the spawn area is busy
        private bool TryFindFreeSpawnPosition(BaseUnit unit, float x, float towerY, float towerHeight, out Vector2 position)
        {
            position = Vector2.Zero;
            Rectangle bounds = unit.Rectangle;
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return false;

            int minY = (int)(towerY + 16f);
            int maxY = (int)(towerY + towerHeight - bounds.Height - 16f);
            if (maxY < minY)
                return false;

            for (int attempt = 0; attempt < 16; attempt++)
            {
                int y = rand.Next(minY, maxY + 1);
                var pos = new Vector2(x, y);
                var footprint = new Rectangle((int)x, y, bounds.Width, bounds.Height);
                if (!SpawnWouldOverlap(footprint))
                {
                    position = pos;
                    return true;
                }
            }

            // checking the remaining lanes avoids rejecting a narrow gap just because the random attempts missed it
            for (int y = minY; y <= maxY; y++)
            {
                var footprint = new Rectangle((int)x, y, bounds.Width, bounds.Height);
                if (!SpawnWouldOverlap(footprint))
                {
                    position = new Vector2(x, y);
                    return true;
                }
            }
            return false;
        }

        // using the same bounds as BaseUnit.Rectangle keeps placement consistent with hit detection
        private bool SpawnWouldOverlap(Rectangle footprint)
        {
            if (playerTower != null && footprint.Intersects(playerTower.Rectangle))
                return true;
            if (enemyTower != null && footprint.Intersects(enemyTower.Rectangle))
                return true;

            foreach (var unit in AllUnits())
            {
                if (unit != null && unit.IsAlive && footprint.Intersects(unit.Rectangle))
                    return true;
            }

            return false;
        }

        // returning a shared base-unit list lets upgrades and targeting handle different player unit types together
        public List<BaseUnit> GetAlivePlayerUnits()
        {
            var list = new List<BaseUnit>();
            AddAlive(PlayerSoldiers, list);
            AddAlive(PlayerTanks, list);
            AddAlive(PlayerMachineGunners, list);
            return list;
        }

        // collecting living enemies gives other managers current targets without including dead units
        public List<BaseUnit> GetAliveEnemyUnits()
        {
            var list = new List<BaseUnit>();
            AddAlive(EnemySoldiers, list);
            AddAlive(EnemyTanks, list);
            AddAlive(EnemyMachineGunners, list);
            return list;
        }

        // yielding from each list lets shared checks visit every unit without keeping another permanent list
        private IEnumerable<BaseUnit> AllUnits()
        {
            foreach (var u in PlayerSoldiers) yield return u;
            foreach (var u in PlayerTanks) yield return u;
            foreach (var u in PlayerMachineGunners) yield return u;
            foreach (var u in EnemySoldiers) yield return u;
            foreach (var u in EnemyTanks) yield return u;
            foreach (var u in EnemyMachineGunners) yield return u;
        }

        // the base-unit constraint lets the same filtering code work with any of the unit lists
        private static void AddAlive<T>(List<T> source, List<BaseUnit> dest) where T : BaseUnit
        {
            foreach (var unit in source)
            {
                if (unit != null && unit.IsAlive)
                    dest.Add(unit);
            }
        }

        // applying difficulty once at spawn avoids increasing an enemy's stats again every frame
        private void ApplyDifficultyScale(BaseUnit unit)
        {
            unit.Health = Math.Max(1, (int)(unit.Health * EnemyDifficultyScale));
            unit.MaxHealth = unit.Health;
            unit.Damage = Math.Max(1, (int)(unit.Damage * EnemyDifficultyScale));
        }


        // moving and checking tower boundaries first, then attacking and removing dead units
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

        // using each unit's own y position keeps its lane while it approaches the opposing tower
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

        // player units approach the enemy tower's near edge instead of walking towards its centre
        private Vector2 GetEnemyApproachPoint()
        {
            if (enemyTower != null)
                return new Vector2(enemyTower.TowerX, enemyTower.Center.Y);
            return EnemyTowerV;
        }

        // enemy units approach the player's right edge so they stop on the correct side of the tower
        private Vector2 GetPlayerApproachPoint()
        {
            if (playerTower != null)
                return new Vector2(playerTower.TowerX + playerTower.TowerWidth, playerTower.Center.Y);
            return PlayerTowerV;
        }

        // clamping units before a tower stops them walking through it while still leaving them in attack range
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

        // separating the approach point from the aim point lets units stop near the wall but shoot towards the tower centre
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

        // sharing the attack checks across unit types keeps range, cooldown and tower-death rules consistent
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

                // only allowing an attack when the timer is ready prevents damage being dealt on every update
                if (unit.attackTimer > 0f)
                    continue;

                if (unit.FiresShells)
                    FireShellAtTower(unit, aimPoint, isPlayer);
                else
                    tower.TakeDamage(unit.Damage);

                unit.attackTimer = unit.AttackSpeed;
            }
        }

        // copying the unit's current damage into the shell keeps purchased damage bonuses in the normal attack path
        private void FireShellAtTower(BaseUnit unit, Vector2 towerTarget, bool isPlayer)
        {
            if (ShellManager == null || ShellManager.ShellTexture == null || unit == null)
                return;

            var shell = new Shell(ShellManager.ShellTexture, unit.Center, towerTarget, isPlayer, unit.Damage, 900f);
            ShellManager.AddShell(shell);
            SoundManager?.PlayShoot(unit);
        }

        // removing dead entries from each list keeps later movement and drawing focused on units still in the match
        public void CleanupDeadUnits()
        {
            // counting dead enemies before removing them includes clicks and blasts, with each death counted once
            foreach (var unit in AllUnits())
                if (unit != null && !unit.IsPlayer && !unit.IsAlive)
                    EnemyUnitsEliminated++;
            PlayerSoldiers.RemoveAll(u => u == null || u.Health <= 0);
            EnemySoldiers.RemoveAll(u => u == null || u.Health <= 0);
            PlayerTanks.RemoveAll(u => u == null || u.Health <= 0);
            EnemyTanks.RemoveAll(u => u == null || u.Health <= 0);
            PlayerMachineGunners.RemoveAll(u => u == null || u.Health <= 0);
            EnemyMachineGunners.RemoveAll(u => u == null || u.Health <= 0);
        }


        // drawing the stored units here keeps the main game state from handling each unit type's list
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
