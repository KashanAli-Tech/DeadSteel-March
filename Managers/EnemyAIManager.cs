using Coursework.GameObjects;
using Coursework.GameObjects.Units;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Coursework.Managers
{
    // named difficulty choices make presets easier to select than using unexplained numbers
    internal enum AiDifficulty
    {
        Easy,
        Normal,
        Hard
    }

    // keeps enemy spawning, target choices and phase changes separate from player input
    internal class EnemyAIManager
    {
        // presets and phase settings are kept here so the ai can be balanced without changing the tree structure
        public int AiDamage = 200;
        public float EarlyKillInterval = 1f;
        public float MidKillInterval = 0.8f;
        public float LateKillInterval = 0.6f;
        public float PanicKillInterval = 0.4f;
        public float CloseGroupPixels = 180f;

        // separate intervals allow spawn frequency to change between game phases
        public float EarlySpawnInterval = 0.1f;
        public float MidSpawnInterval = 0.3f;
        public float LateSpawnInterval = 0.1f;
        public float PanicSpawnInterval = 0.01f;

        // weights change the chance of each unit type instead of forcing the same spawn order every match
        public float EarlySoldierWeight = 0.4f;
        public float EarlyMgWeight = 0.35f;
        public float EarlyTankWeight = 0.25f;

        public float MidSoldierWeight = 0.1f;
        public float MidMgWeight = 0.3f;
        public float MidTankWeight = 0.6f;

        public float LateSoldierWeight = 0.05f;
        public float LateMgWeight = 0.15f;
        public float LateTankWeight = 0.8f;

        public float PanicSoldierWeight = 0.02f;
        public float PanicMgWeight = 0.08f;
        public float PanicTankWeight = 0.9f;

        public float EarlyDifficultyScale = 3f;
        public float MidDifficultyScale = 5f;
        public float LateDifficultyScale = 7f;
        public float PanicDifficultyScale = 15f;
        public float DifficultyUpdateInterval = 3f;

        public float MidGameStartSeconds = 30f;
        public float LateGameStartSeconds = 60f;
        public float PanicHealthPercent = 0.65f;
        public GamePhase CurrentPhase { get; private set; } = GamePhase.EarlyGame;

        public ExplosionManager ExplosionManager;
        public SoundManager SoundManager;

        private readonly UnitManager unitManager;
        private readonly Tower enemyTower;
        private readonly Random rand = new Random();

        // separate timers let targeting, spawning and difficulty changes happen at their own intervals
        private float elapsedSeconds;
        private float targetingTimer;
        private float spawnTimer;
        private float difficultyTimer;
        private float dt;

        private readonly IBtNode root;

        // applying the starting preset before building the tree makes its first timers use the selected settings
        public EnemyAIManager(UnitManager unitManager, MoneyManager moneyManager, Tower playerTower, Tower enemyTower, AiDifficulty difficulty = AiDifficulty.Normal)
        {
            this.unitManager = unitManager;
            this.enemyTower = enemyTower;

            // the menu difficulties share normal phase weights and target intervals, with strength and spawning scaled below
            ApplyPreset(AiDifficulty.Normal);
            // small multipliers keep the existing normal timings, weights and target choices intact
            float strength = difficulty == AiDifficulty.Easy ? .85f : difficulty == AiDifficulty.Hard ? 1.15f : 1f;
            float interval = difficulty == AiDifficulty.Easy ? 1.2f : difficulty == AiDifficulty.Hard ? .85f : 1f;
            AiDamage = (int)Math.Round(AiDamage * strength);
            EarlyDifficultyScale *= strength; MidDifficultyScale *= strength;
            LateDifficultyScale *= strength; PanicDifficultyScale *= strength;
            EarlySpawnInterval *= interval; MidSpawnInterval *= interval;
            LateSpawnInterval *= interval; PanicSpawnInterval *= interval;
            targetingTimer = EarlyKillInterval;
            spawnTimer = EarlySpawnInterval;
            difficultyTimer = 0f;
            unitManager.EnemyDifficultyScale = EarlyDifficultyScale;

            // building the tree once avoids recreating the same checks each frame
            // each sequence checks its own timer before running its action
            root = new ParallelNode(
                new SequenceNode(
                    new ConditionNode(() => TickTimer(ref targetingTimer)),
                    new ActionNode(DoTargeting)
                ),
                new SequenceNode(
                    new ConditionNode(() => TickTimer(ref spawnTimer)),
                    new ActionNode(DoSpawning)
                ),
                new SequenceNode(
                    new ConditionNode(() => TickTimer(ref difficultyTimer)),
                    new ActionNode(DoDifficultyUpdate)
                )
            );
        }

        // keeping each preset's values together makes spawn rates, attack timings and starting strength easier to adjust
        public void ApplyPreset(AiDifficulty preset)
        {
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
                AiDamage = 100;

                EarlyKillInterval = 5f;
                MidKillInterval = 3.5f;
                LateKillInterval = 2.5f;
                PanicKillInterval = 1.5f;

                EarlySpawnInterval = 1f;
                MidSpawnInterval = 1;
                LateSpawnInterval = 1.5f;
                PanicSpawnInterval = 1f;

                EarlySoldierWeight = 0.85f;
                EarlyMgWeight = 0.15f;
                EarlyTankWeight = 0f;

                MidSoldierWeight = 0.4f;
                MidMgWeight = 0.45f;
                MidTankWeight = 0.15f;

                LateSoldierWeight = 0.2f;
                LateMgWeight = 0.3f;
                LateTankWeight = 0.5f;

                PanicSoldierWeight = 0.1f;
                PanicMgWeight = 0.25f;
                PanicTankWeight = 0.65f;

                EarlyDifficultyScale = 0.6f;
                MidDifficultyScale = 0.75f;
                LateDifficultyScale = 0.9f;
                PanicDifficultyScale = 1.1f;
            }
        }

        // using elapsed seconds keeps the ai timers tied to game time rather than how many frames have passed
        public void Update(GameTime gameTime)
        {
            dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            elapsedSeconds += dt;
            CurrentPhase = DeterminePhase();

            // ticking the root visits the targeting, spawning and difficulty branches in one place
            root.Tick();
        }

        // passing the timer by reference updates its stored value; the matching action resets it when it becomes due
        private bool TickTimer(ref float timer)
        {
            timer -= dt;
            if (timer > 0f)
                return false;
            return true;
        }

        // checking low enemy tower health first lets panic mode take priority over the normal time-based phases
        private GamePhase DeterminePhase()
        {
            if (enemyTower != null && enemyTower.HealthPercentage <= PanicHealthPercent)
                return GamePhase.PanicMode;

            if (elapsedSeconds >= LateGameStartSeconds) return GamePhase.LateGame;
            if (elapsedSeconds >= MidGameStartSeconds) return GamePhase.MidGame;
            return GamePhase.EarlyGame;
        }


        // resetting the targeting timer before picking a unit means an empty battlefield still waits until the next check
        private void DoTargeting()
        {
            targetingTimer = KillIntervalFor(CurrentPhase);

            BaseUnit target = PickPlayerUnitToDamage();
            if (target == null)
                return;

            // checking the change from alive to dead means the death effect only happens for a killing hit
            bool wasAlive = target.IsAlive;
            target.TakeDamage(AiDamage);

            if (wasAlive && !target.IsAlive)
            {
                ExplosionManager?.AddExplosion(target.Center, 0, 24f);
                SoundManager?.PlayExplosion();
            }
        }

        // selecting an interval by phase keeps targeting frequency separate from the target-picking algorithm
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

        // choosing threats within a distance band keeps distant heavy units from taking priority over nearby attackers
        private BaseUnit PickPlayerUnitToDamage()
        {
            var units = new List<BaseUnit>();
            foreach (var u in unitManager.PlayerSoldiers) if (u != null && u.IsAlive) units.Add(u);
            foreach (var u in unitManager.PlayerMachineGunners) if (u != null && u.IsAlive) units.Add(u);
            foreach (var u in unitManager.PlayerTanks) if (u != null && u.IsAlive) units.Add(u);
            if (units.Count == 0)
                return null;

            Vector2 towerPos = enemyTower != null ? enemyTower.Center : unitManager.EnemyTowerV;

            // the first pass finds how close the nearest player unit is to the enemy tower
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
                // this compares distances from the tower, rather than distances between the units themselves
                if (d > closest + CloseGroupPixels)
                    continue;

                int threat = ThreatOf(u);
                // higher threat wins, with distance breaking ties between units of the same threat
                if (threat > bestThreat || (threat == bestThreat && d < bestDist))
                {
                    best = u;
                    bestThreat = threat;
                    bestDist = d;
                }
            }

            return best;
        }

        // ranking tanks above machine gunners and soldiers gives the ai a simple target priority
        private static int ThreatOf(BaseUnit unit)
        {
            if (unit is Tank) return 3;
            if (unit is MachineGunner) return 2;
            return 1;
        }


        // resetting the spawn timer here gives the next spawn the interval for the current phase
        private void DoSpawning()
        {
            spawnTimer = SpawnIntervalFor(CurrentPhase);
            TrySpawnForPhase(CurrentPhase);
        }

        // choosing the spawn interval by phase allows the ai to change its pace during the match
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

        // using one random roll across the combined weights gives each unit type its configured chance
        private void TrySpawnForPhase(GamePhase phase)
        {
            GetWeights(phase, out float soldierW, out float mgW, out float tankW);

            float total = soldierW + mgW + tankW;
            // falling back to a soldier means an all-zero set of weights still produces a valid spawn
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

        // returning all three weights together keeps them matched to the same phase
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


        // asking the unit manager to create enemies keeps spawn positions and unit setup out of the ai
        private void SpawnSoldier()
        {
            unitManager.SpawnEnemySoldier();
        }

        private void SpawnMachineGunner()
        {
            unitManager.SpawnEnemyMachineGunner();
        }

        private void SpawnTank()
        {
            unitManager.SpawnEnemyTank();
        }



        // changing the unit manager's scale affects enemies spawned afterwards rather than rewriting existing units
        private void DoDifficultyUpdate()
        {
            difficultyTimer = DifficultyUpdateInterval;
            unitManager.EnemyDifficultyScale = ScaleFor(CurrentPhase);
        }

        // keeping strength multipliers separate from spawn intervals allows the two to be balanced independently
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
