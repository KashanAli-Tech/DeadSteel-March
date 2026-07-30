using Coursework.GameObjects;
using Coursework.GameObjects.Units;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;


namespace Coursework.Managers
{
    internal class UnitManager
    {

        // MAKE THE CODE NEAT AND TIDY WHEN FREE 
        // ORGANISE ATTRIBUTES AND FUNCTIONS ETC.
        
        // declaring lists
        public List<Soldier> PlayerSoldiers = new List<Soldier>();
        public List<Soldier> EnemySoldiers = new List<Soldier>();
        public List<Tank> PlayerTanks = new List<Tank>();
        public List<Tank> EnemyTanks = new List<Tank>();
        public List<MachineGunner> PlayerMachineGunners = new List<MachineGunner>();
        public List<MachineGunner> EnemyMachineGunners = new List<MachineGunner>();

        // declaring textures
        private Texture2D PlayersoldierTexture;
        private Texture2D PlayertankTexture;
        private Texture2D PlayermachineGunTexture;
        private Texture2D EnemysoldierTexture;
        private Texture2D EnemytankTexture;
        private Texture2D EnemymachineGunTexture;

        // idk what this is but it was in tutorial
        public Texture2D PlayerSoldierTexture => PlayersoldierTexture;
        public Texture2D EnemySoldierTexture => EnemysoldierTexture;

        public Texture2D PlayerTankTexture => PlayertankTexture;
        public Texture2D EnemyTankTexture => EnemytankTexture;
        public Texture2D PlayerMachineGunTexture => PlayermachineGunTexture;
        public Texture2D EnemyMachineGunTexture => EnemymachineGunTexture;

        private Vector2 PlayerTowerV, EnemyTowerV;
        private Random rand = new Random();

        // External managers
        public ShellManager ShellManager;
        public ExplosionManager ExplosionManager;
        public SoundManager SoundManager;
        public MoneyManager MoneyManager;

        private Tower playerTower;
        private Tower enemyTower;

        public UnitManager()
        {
            PlayerTowerV = new Vector2(25, 450);
            EnemyTowerV = new Vector2(1675, 450);
        }

        public void SetTowerPositions(Vector2 left, Vector2 right)
        {
            PlayerTowerV = left;
            EnemyTowerV = right;
        }

        public void LoadContent(ContentManager content)
        {
            PlayersoldierTexture = content.Load<Texture2D>("PlayerSoldier");
            PlayertankTexture = content.Load<Texture2D>("PlayerTank");
            PlayermachineGunTexture = content.Load<Texture2D>("PlayerMachineGunner");

            EnemysoldierTexture = content.Load<Texture2D>("EnemySoldier");
            EnemytankTexture = content.Load<Texture2D>("EnemyTank");
            EnemymachineGunTexture = content.Load<Texture2D>("EnemyMachineGunner");

        }

        private void SpawnEnemyWave()
        {
            int chance = rand.Next(0, 100);
            int x = rand.Next(600, 2000);
            int y = rand.Next(50, 1325);
            Vector2 pos = new Vector2(x, y);

            if (chance < 2) EnemySoldiers.Add(new Soldier(EnemysoldierTexture, pos));
            else if (chance < 4) EnemyMachineGunners.Add(new MachineGunner(EnemymachineGunTexture, pos));
            else if (chance < 6) EnemyTanks.Add(new Tank(EnemytankTexture, pos));
        }

        public void SpawnPlayerSoldier()
        {
            int y = rand.Next(50, 1325);
            PlayerSoldiers.Add(new Soldier(PlayersoldierTexture, new Vector2(120, y)));
        }

        public void SpawnPlayerTank()
        {
            int y = rand.Next(50, 1325);
            PlayerTanks.Add(new Tank(PlayertankTexture, new Vector2(120, y)));
        }

        public void SpawnPlayerMachineGunner()
        {
            int y = rand.Next(50, 1325);
            PlayerMachineGunners.Add(new MachineGunner(PlayermachineGunTexture, new Vector2(120, y)));
        }

        public void SetTowers(Tower player, Tower enemy)
        {
            playerTower = player;
            enemyTower = enemy;
        }

        public void Update(GameTime gameTime)
        {
            // spawn logic
            if (rand.NextDouble() < 0.02) SpawnEnemyWave();

            // Player units move horizontally toward enemy tower, keep their Y
            foreach (var s in PlayerSoldiers) s.Update(new Vector2(EnemyTowerV.X, s.Position.Y), gameTime);
            foreach (var t in PlayerTanks) t.Update(new Vector2(EnemyTowerV.X, t.Position.Y), gameTime);
            foreach (var m in PlayerMachineGunners) m.Update(new Vector2(EnemyTowerV.X, m.Position.Y), gameTime);

            // Enemy units move horizontally toward player tower, keep their Y
            foreach (var s in EnemySoldiers) s.Update(new Vector2(PlayerTowerV.X, s.Position.Y), gameTime);
            foreach (var t in EnemyTanks) t.Update(new Vector2(PlayerTowerV.X, t.Position.Y), gameTime);
            foreach (var m in EnemyMachineGunners) m.Update(new Vector2(PlayerTowerV.X, m.Position.Y), gameTime);

            if (playerTower.IsDead || enemyTower.IsDead)
                return;

            // When units reach tower X, they should shoot (spawn shells) and/or attack
            HandleTowerEngagements(gameTime);



            // Cleanup dead units
            PlayerSoldiers.RemoveAll(u => u.Health <= 0);
            EnemySoldiers.RemoveAll(u => u.Health <= 0);
            PlayerTanks.RemoveAll(u => u.Health <= 0);
            EnemyTanks.RemoveAll(u => u.Health <= 0);
            PlayerMachineGunners.RemoveAll(u => u.Health <= 0);
            EnemyMachineGunners.RemoveAll(u => u.Health <= 0);
        }

        private void HandleTowerEngagements(GameTime gameTime)
        {
            // Player units at enemy tower X
            foreach (var s in PlayerSoldiers)
            {
                if (Math.Abs(s.Position.X - EnemyTowerV.X) < 150f)
                    if (s.attackTimer <= 0)
                    {
                        FireAtTower(s, EnemyTowerV);
                        s.attackTimer = s.attackCooldown;
                    } 
            }
            foreach (var t in PlayerTanks)
            {
                if (Math.Abs(t.Position.X - EnemyTowerV.X) < 150f)
                    if (t.attackTimer <= 0)
                    {
                        FireAtTower(t, EnemyTowerV);
                        t.attackTimer = t.attackCooldown;
                    }
            }
            foreach (var m in PlayerMachineGunners)
            {
                if (Math.Abs(m.Position.X - EnemyTowerV.X) < 150f)
                    if (m.attackTimer <= 0)
                    {
                        FireAtTower(m, EnemyTowerV);
                        m.attackTimer = m.attackCooldown;
                    }   
            }

            // Enemy units at player tower X
            foreach (var s in EnemySoldiers)
            {
                if (Math.Abs(s.Position.X - PlayerTowerV.X) < 150f)
                    if (s.attackTimer <= 0)
                    {
                        FireAtTower(s, PlayerTowerV);
                        s.attackTimer = s.attackCooldown;
                    }
            }
            foreach (var t in EnemyTanks)
            {
                if (Math.Abs(t.Position.X - PlayerTowerV.X) < 150f)
                    if (t.attackTimer <= 0)
                    {
                        FireAtTower(t, PlayerTowerV);
                        t.attackTimer = t.attackCooldown;
                    }
            }
            foreach (var m in EnemyMachineGunners)
            {
                if (Math.Abs(m.Position.X - PlayerTowerV.X) < 150f)
                    if (m.attackTimer <= 0)
                    {
                        FireAtTower(m, PlayerTowerV);
                        m.attackTimer = m.attackCooldown;
                    }
            }
        }

        private void FireAtTower(BaseUnit unit, Vector2 towerCenter)
        {
            if (ShellManager == null)
                return;

            if (unit.Health <= 0)
                return;

            if (towerCenter == EnemyTowerV && enemyTower.IsDead)
                return;

            if (towerCenter == PlayerTowerV && playerTower.IsDead)
                return;

            // spawn a shell from unit toward tower center
            
                var shellTex = ShellManager.ShellTexture;
                bool isPlayer = unit is Soldier && PlayerSoldiers.Contains((Soldier)unit) || unit is Tank && PlayerTanks.Contains((Tank)unit) || unit is MachineGunner && PlayerMachineGunners.Contains((MachineGunner)unit);
                var shell = new Shell(shellTex, unit.Position + new Vector2(16, 16), towerCenter, isPlayer, unit.Damage, 900f);
                ShellManager.AddShell(shell);
                SoundManager?.PlayShoot();
            
        }

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
