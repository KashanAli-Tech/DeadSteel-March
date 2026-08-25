using Coursework.GameObjects;
using Coursework.GameObjects.Units;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Coursework.Managers
{
    internal enum AiDifficulty
    {
        Easy,
        Normal,
        Hard
    }

    // spawning + targeting + difficulty. click-to-kill lives in UnitKillManager, not here.
    internal class EnemyAIManager
    {
        // swap this (or call ApplyPreset) to retune the whole AI
        public AiDifficulty DifficultyPreset = AiDifficulty.Normal;

        // ---- kill / targeting ----
        public int AiDamage = 100;
        public float EarlyKillInterval = 1f;
        public float MidKillInterval = 5f;
        public float LateKillInterval = 2.5f;
        public float PanicKillInterval = 1f;
        public float CloseGroupPixels = 80f; // "several are close" window

        // ---- spawn ----
        public float EarlySpawnInterval = 3f;
        public float MidSpawnInterval = 2.5f;
        public float LateSpawnInterval = 2f;
        public float PanicSpawnInterval = 1.4f;

        // weights don't need to add to 1, we just roll against the total
        public float EarlySoldierWeight = 0.85f;
        public float EarlyMgWeight = 0.15f;
        public float EarlyTankWeight = 0.0f;

        public float MidSoldierWeight = 0.4f;
        public float MidMgWeight = 0.45f;
        public float MidTankWeight = 0.15f;

        public float LateSoldierWeight = 0.2f;
        public float LateMgWeight = 0.3f;
        public float LateTankWeight = 0.5f;

        public float PanicSoldierWeight = 0.1f;
        public float PanicMgWeight = 0.25f;
        public float PanicTankWeight = 0.65f;

        public int SoldierCost = 10;
        public int MachineGunnerCost = 20;
        public int TankCost = 40;

        // ---- difficulty scale (applied onto UnitManager for new enemy units) ----
        public float EarlyDifficultyScale = 0.6f;
        public float MidDifficultyScale = 0.75f;
        public float LateDifficultyScale = 0.9f;
        public float PanicDifficultyScale = 1.1f;
        public float DifficultyUpdateInterval = 5f;

        // ---- phase timing ----
        public float MidGameStartSeconds = 60f;
        public float LateGameStartSeconds = 150f;
        public float PanicHealthPercent = 0.3f;

        public GamePhase CurrentPhase { get; private set; } = GamePhase.EarlyGame;

        public ExplosionManager ExplosionManager;
        public SoundManager SoundManager;

        private readonly UnitManager unitManager;
        private readonly MoneyManager moneyManager;
        private readonly Tower playerTower;
        private readonly Tower enemyTower;
        private readonly Random rand = new Random();

        // three separate clocks so you can tweak them without stepping on each other
        private float elapsedSeconds;
        private float targetingTimer;
        private float spawnTimer;
        private float difficultyTimer;

        public EnemyAIManager(UnitManager unitManager, MoneyManager moneyManager, Tower playerTower, Tower enemyTower)
        {
            this.unitManager = unitManager;
            this.moneyManager = moneyManager;
            this.playerTower = playerTower;
            this.enemyTower = enemyTower;

            ApplyPreset(DifficultyPreset);
            targetingTimer = EarlyKillInterval;
            spawnTimer = EarlySpawnInterval;
            difficultyTimer = 0f;
            unitManager.EnemyDifficultyScale = EarlyDifficultyScale;
        }

        public void ApplyPreset(AiDifficulty preset)
        {
            DifficultyPreset = preset;

            if (preset == AiDifficulty.Easy)
            {
                AiDamage = 100;
                EarlyKillInterval = 1f;
                MidKillInterval = 2f;
                LateKillInterval = 4.5f;
                PanicKillInterval = 2.5f;

                EarlySpawnInterval = 3.6f;
                MidSpawnInterval = 3.1f;
                LateSpawnInterval = 2.6f;
                PanicSpawnInterval = 2f;

                EarlySoldierWeight = 0.9f; EarlyMgWeight = 0.1f; EarlyTankWeight = 0f;
                MidSoldierWeight = 0.55f; MidMgWeight = 0.35f; MidTankWeight = 0.1f;
                LateSoldierWeight = 0.35f; LateMgWeight = 0.4f; LateTankWeight = 0.25f;
                PanicSoldierWeight = 0.2f; PanicMgWeight = 0.4f; PanicTankWeight = 0.4f;

                EarlyDifficultyScale = 0.5f;
                MidDifficultyScale = 0.6f;
                LateDifficultyScale = 0.7f;
                PanicDifficultyScale = 0.85f;
            }
            else if (preset == AiDifficulty.Hard)
            {
                AiDamage = 50;
                EarlyKillInterval = 5.5f;
                MidKillInterval = 3.5f;
                LateKillInterval = 2f;
                PanicKillInterval = 0.8f;

                EarlySpawnInterval = 2.4f;
                MidSpawnInterval = 1.9f;
                LateSpawnInterval = 1.4f;
                PanicSpawnInterval = 1f;

                EarlySoldierWeight = 0.7f; EarlyMgWeight = 0.25f; EarlyTankWeight = 0.05f;
                MidSoldierWeight = 0.25f; MidMgWeight = 0.4f; MidTankWeight = 0.35f;
                LateSoldierWeight = 0.1f; LateMgWeight = 0.25f; LateTankWeight = 0.65f;
                PanicSoldierWeight = 0.05f; PanicMgWeight = 0.2f; PanicTankWeight = 0.75f;

                EarlyDifficultyScale = 0.85f;
                MidDifficultyScale = 1f;
                LateDifficultyScale = 1.15f;
                PanicDifficultyScale = 1.3f;
            }
            else
            {
                // Normal - baseline
                AiDamage = 35;
                EarlyKillInterval = 8f;
                MidKillInterval = 5f;
                LateKillInterval = 2.5f;
                PanicKillInterval = 1f;

                EarlySpawnInterval = 3f;
                MidSpawnInterval = 2.5f;
                LateSpawnInterval = 2f;
                PanicSpawnInterval = 1.4f;

                EarlySoldierWeight = 0.85f; EarlyMgWeight = 0.15f; EarlyTankWeight = 0f;
                MidSoldierWeight = 0.4f; MidMgWeight = 0.45f; MidTankWeight = 0.15f;
                LateSoldierWeight = 0.2f; LateMgWeight = 0.3f; LateTankWeight = 0.5f;
                PanicSoldierWeight = 0.1f; PanicMgWeight = 0.25f; PanicTankWeight = 0.65f;

                EarlyDifficultyScale = 0.6f;
                MidDifficultyScale = 0.75f;
                LateDifficultyScale = 0.9f;
                PanicDifficultyScale = 1.1f;
            }
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            elapsedSeconds += dt;
            CurrentPhase = DeterminePhase();

            UpdateTargeting(dt);
            UpdateSpawning(dt);
            UpdateDifficulty(dt);
        }

        private GamePhase DeterminePhase()
        {
            if (enemyTower != null && enemyTower.HealthPercentage <= PanicHealthPercent)
                return GamePhase.PanicMode;

            if (elapsedSeconds >= LateGameStartSeconds) return GamePhase.LateGame;
            if (elapsedSeconds >= MidGameStartSeconds) return GamePhase.MidGame;
            return GamePhase.EarlyGame;
        }

        // ---- targeting (own timer) ----

        private void UpdateTargeting(float dt)
        {
            targetingTimer -= dt;
            if (targetingTimer > 0f)
                return;

            targetingTimer = KillIntervalFor(CurrentPhase);

            BaseUnit target = PickPlayerUnitToDamage();
            if (target == null)
                return;

            bool wasAlive = target.IsAlive;
            target.TakeDamage(AiDamage);

            if (wasAlive && !target.IsAlive)
            {
                ExplosionManager?.AddExplosion(target.Center, 0, 24f);
                SoundManager?.PlayExplosion();
            }
        }

        private float KillIntervalFor(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.MidGame: return MidKillInterval;
                case GamePhase.LateGame: return LateKillInterval;
                case GamePhase.PanicMode: return PanicKillInterval;
                default: return EarlyKillInterval;
            }
        }

        // closest to the enemy tower wins; if a few are bunched up, tanks first
        private BaseUnit PickPlayerUnitToDamage()
        {
            var units = new List<BaseUnit>();
            foreach (var u in unitManager.PlayerSoldiers) if (u != null && u.IsAlive) units.Add(u);
            foreach (var u in unitManager.PlayerMachineGunners) if (u != null && u.IsAlive) units.Add(u);
            foreach (var u in unitManager.PlayerTanks) if (u != null && u.IsAlive) units.Add(u);
            if (units.Count == 0)
                return null;

            Vector2 towerPos = enemyTower != null ? enemyTower.Center : unitManager.EnemyTowerV;

            float closest = float.MaxValue;
            foreach (var u in units)
            {
                float d = Vector2.Distance(u.Center, towerPos);
                if (d < closest) closest = d;
            }

            BaseUnit best = null;
            int bestThreat = -1;
            float bestDist = float.MaxValue;

            foreach (var u in units)
            {
                float d = Vector2.Distance(u.Center, towerPos);
                if (d > closest + CloseGroupPixels)
                    continue;

                int threat = ThreatOf(u);
                if (threat > bestThreat || (threat == bestThreat && d < bestDist))
                {
                    best = u;
                    bestThreat = threat;
                    bestDist = d;
                }
            }

            return best;
        }

        private static int ThreatOf(BaseUnit unit)
        {
            if (unit is Tank) return 3;
            if (unit is MachineGunner) return 2;
            return 1;
        }

        // ---- spawning (own timer) ----

        private void UpdateSpawning(float dt)
        {
            spawnTimer -= dt;
            if (spawnTimer > 0f)
                return;

            spawnTimer = SpawnIntervalFor(CurrentPhase);
            TrySpawnForPhase(CurrentPhase);
        }

        private float SpawnIntervalFor(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.MidGame: return MidSpawnInterval;
                case GamePhase.LateGame: return LateSpawnInterval;
                case GamePhase.PanicMode: return PanicSpawnInterval;
                default: return EarlySpawnInterval;
            }
        }

        private void TrySpawnForPhase(GamePhase phase)
        {
            GetWeights(phase, out float soldierW, out float mgW, out float tankW);

            float total = soldierW + mgW + tankW;
            if (total <= 0f)
            {
                SpawnSoldier();
                return;
            }

            float roll = (float)rand.NextDouble() * total;
            if (roll < soldierW)
                SpawnSoldier();
            else if (roll < soldierW + mgW)
                SpawnMachineGunner();
            else
                SpawnTank();
        }

        private void GetWeights(GamePhase phase, out float soldier, out float mg, out float tank)
        {
            switch (phase)
            {
                case GamePhase.MidGame:
                    soldier = MidSoldierWeight; mg = MidMgWeight; tank = MidTankWeight; break;
                case GamePhase.LateGame:
                    soldier = LateSoldierWeight; mg = LateMgWeight; tank = LateTankWeight; break;
                case GamePhase.PanicMode:
                    soldier = PanicSoldierWeight; mg = PanicMgWeight; tank = PanicTankWeight; break;
                default:
                    soldier = EarlySoldierWeight; mg = EarlyMgWeight; tank = EarlyTankWeight; break;
            }
        }

        private void SpawnSoldier()
        {
            if (moneyManager != null && !moneyManager.Spend(SoldierCost))
                return;
            unitManager.SpawnEnemySoldier();
        }

        private void SpawnMachineGunner()
        {
            if (moneyManager != null && !moneyManager.Spend(MachineGunnerCost))
            {
                SpawnSoldier(); // couldn't afford mg, drop down
                return;
            }
            unitManager.SpawnEnemyMachineGunner();
        }

        private void SpawnTank()
        {
            if (moneyManager != null && !moneyManager.Spend(TankCost))
            {
                SpawnMachineGunner();
                return;
            }
            unitManager.SpawnEnemyTank();
        }

        // ---- difficulty scale (own timer) ----

        private void UpdateDifficulty(float dt)
        {
            difficultyTimer -= dt;
            if (difficultyTimer > 0f)
                return;

            difficultyTimer = DifficultyUpdateInterval;
            unitManager.EnemyDifficultyScale = ScaleFor(CurrentPhase);
        }

        private float ScaleFor(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.MidGame: return MidDifficultyScale;
                case GamePhase.LateGame: return LateDifficultyScale;
                case GamePhase.PanicMode: return PanicDifficultyScale;
                default: return EarlyDifficultyScale;
            }
        }
    }
}
