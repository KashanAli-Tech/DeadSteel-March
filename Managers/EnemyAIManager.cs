using Coursework.GameObjects;
using Microsoft.Xna.Framework;
using System;

namespace Coursework.Managers
{
    internal class EnemyAIManager
    {
        private UnitManager unitManager;
        private MoneyManager moneyManager;

        // AI State Machine
        private enum AIState
        {
            EarlyGame, 
            MidGame,
            LateGame,
            PanicMode // make wise descisons etc.
        }

        private AIState currentState = AIState.EarlyGame;

        // Timers
        private float spawnTimer = 0f;
        private float spawnCooldown = 2.5f; // base cooldown

        // Difficulty progression
        private float gameTimeElapsed = 0f;

        // Tower references
        private Tower playerTower;
        private Tower enemyTower;

        public EnemyAIManager(UnitManager unitManager, MoneyManager moneyManager, Tower playerTower, Tower enemyTower)
        {
            this.unitManager = unitManager;
            this.moneyManager = moneyManager;
            this.playerTower = playerTower;
            this.enemyTower = enemyTower;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            spawnTimer += dt;
            gameTimeElapsed += dt;

            UpdateStateMachine();
            TrySpawnUnit();
        }

      
        // FINITE STATE MACHINE LOGIC
        
        private void UpdateStateMachine()
        {
            if (enemyTower.TowerHealth < 300)
            {
                currentState = AIState.PanicMode;
                return;
            }

            if (gameTimeElapsed < 30)
                currentState = AIState.EarlyGame;
            else if (gameTimeElapsed < 60)
                currentState = AIState.MidGame;
            else
                currentState = AIState.LateGame;
        }

        // 2. UTILITY AI DECISION MAKING

        private void TrySpawnUnit()
        {
            if (spawnTimer < spawnCooldown)
                return;

            spawnTimer = 0f;

            // Utility scores
            float soldierScore = 0f;
            float mgScore = 0f;
            float tankScore = 0f;

            // Base scores by state
            switch (currentState)
            {
                // THESE SCORES ARE THE SAME AS THE TUTORIAL ONES
                // I need to change these utility scores once the game is finished to make it a bit more competitive
                case AIState.EarlyGame:
                    soldierScore += 0.8f;
                    mgScore += 0.4f;
                    tankScore += 0.1f;
                    break;

                case AIState.MidGame:
                    soldierScore += 0.5f;
                    mgScore += 0.7f;
                    tankScore += 0.4f;
                    break;

                case AIState.LateGame:
                    soldierScore += 0.3f;
                    mgScore += 0.6f;
                    tankScore += 0.9f;
                    break;

                case AIState.PanicMode:
                    soldierScore += 0.2f;
                    mgScore += 0.5f;
                    tankScore += 1.2f; // panic = heavy units
                    break;
            }

            // Adjust based on money
            int money = moneyManager.Money;

            if (money < 20)
            {
                // Can't afford anything expensive
                tankScore = 0;
                mgScore = 0.2f;
                soldierScore = 1f;
            }
            else if (money < 50)
            {
                tankScore = 0;
                mgScore += 0.4f;
            }

            // Adjust based on player strength
            int playerUnitCount =
                unitManager.PlayerSoldiers.Count +
                unitManager.PlayerMachineGunners.Count +
                unitManager.PlayerTanks.Count;

            if (playerUnitCount > 10)
            {
                // Player is strong → AI should spawn stronger units
                tankScore += 0.6f;
                mgScore += 0.4f;
            }

            // Pick best option
            float best = Math.Max(soldierScore, Math.Max(mgScore, tankScore));

            if (best == soldierScore)
                SpawnSoldier();
            else if (best == mgScore)
                SpawnMachineGunner();
            else
                SpawnTank();
        }

        // 3. SPAWN FUNCTIONS
        private void SpawnSoldier()
        {
            if (moneyManager.Spend(10))
                unitManager.SpawnEnemySoldier();
        }

        private void SpawnMachineGunner()
        {
            if (moneyManager.Spend(20))
                unitManager.SpawnEnemyMachineGunner();
        }

        private void SpawnTank()
        {
            if (moneyManager.Spend(40))
                unitManager.SpawnEnemyTank();
        }
    }
}
