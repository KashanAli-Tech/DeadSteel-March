using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Coursework.GameObjects;
using Coursework.Managers;

namespace Coursework.States
{
    internal class GameState : BaseState
    {

        // I NEED TO ADD MORE VISUALS AFTER COMPLETING THE PROGRAM
        // FOR NOW,  1 WILL SPAWN SOLDEIR, 2 WILL SPAWN MACHINE GUNNER AND 3 WILL SPANW TANK I WILL CHANGE IT LATER



        // VISUALS
        private Texture2D background;
        private Texture2D gameOverBackground;
        private Texture2D moneyIcon;
        private Texture2D upgradeIcon;
        private Texture2D buttonTexture;

        // Towers
        private Texture2D tower1Texture;
        private Texture2D tower2Texture;

        private Rectangle Tower1HealthRectangle;
        private Texture2D Tower1healthTexture;

        private Rectangle Tower2HealthRectangle;
        private Texture2D Tower2healthTexture;


        // Units
        private Texture2D soldierTexture;
        private Texture2D machineGunnerTexture;
        private Texture2D tankTexture;

        // Projectiles & FX
        private Texture2D shellTexture;
        private Texture2D explosionTexture;

        // Fonts
        private SpriteFont defaultFont;

        // Music
        private Song gameplayMusic;

        private KeyboardState previousKeyboardState;


        // GAME OBJECTS & MANAGERS
        private UnitManager unitManager;
        private ShellManager shellManager;
        private ExplosionManager explosionManager;
        private CollisionManager collisionManager;
        private MoneyManager moneyManager;
        private UpgradeManager upgradeManager;
        private SoundManager soundManager;
        private UIManager uiManager;
        private EnemyAIManager enemyAI;

        // Towers
        public Tower[] Tower1 = new Tower[1];
        public Tower[] Tower2 = new Tower[1];

        public GameState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
            : base(game, graphicsDevice, content)
        {
        }

        public override void LoadContent()
        {
            // LOAD VISUALS
            background = Content.Load<Texture2D>("Background");
            gameOverBackground = Content.Load<Texture2D>("GameOverBackground");

            moneyIcon = Content.Load<Texture2D>("MoneyIcon");
            upgradeIcon = Content.Load<Texture2D>("UpgradeIcon");
            buttonTexture = Content.Load<Texture2D>("Button");

            tower1Texture = Content.Load<Texture2D>("PlayerTower");
            tower2Texture = Content.Load<Texture2D>("EnemyTower");


            soldierTexture = Content.Load<Texture2D>("PlayerSoldier");
            machineGunnerTexture = Content.Load<Texture2D>("PlayerMachineGunner");
            tankTexture = Content.Load<Texture2D>("PlayerTank");

            shellTexture = Content.Load<Texture2D>("ShellPic1");
            explosionTexture = Content.Load<Texture2D>("ExplosionFinal");

            defaultFont = Content.Load<SpriteFont>("DefaultFont");

            Tower1healthTexture = Content.Load<Texture2D>("HealthBar");
            Tower2healthTexture = Content.Load<Texture2D>("HealthBar");


            // Music
            gameplayMusic = Content.Load<Song>("GameBackgroundMusic");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(gameplayMusic);

            
            // CREATE MANAGERS
            
            unitManager = new UnitManager();
            shellManager = new ShellManager();
            explosionManager = new ExplosionManager();
            collisionManager = new CollisionManager(unitManager, shellManager, explosionManager);
            moneyManager = new MoneyManager();
            upgradeManager = new UpgradeManager();
            soundManager = new SoundManager();
            uiManager = new UIManager();

            // Load manager content
            unitManager.LoadContent(Content);
            shellManager.LoadContent(Content);
            explosionManager.LoadContent(Content);
            soundManager.LoadContent(Content);
            uiManager.LoadContent(Content, GraphicsDevice);

            // Wire managers
            unitManager.ShellManager = shellManager;
            unitManager.ExplosionManager = explosionManager;
            unitManager.SoundManager = soundManager;
            unitManager.MoneyManager = moneyManager;

            collisionManager.MoneyManager = moneyManager;
            collisionManager.SoundManager = soundManager;

            // CREATE TOWERS
            Tower1[0] = new Tower(tower1Texture, new Rectangle(20, 250, 300, 400), 20, 250, 300, 400);
            Tower2[0] = new Tower(tower2Texture, new Rectangle(1600, 250, 300, 400), 1600, 250, 300, 400);
            unitManager.SetTowers(Tower1[0], Tower2[0]);

            collisionManager.PlayerTower = Tower1[0];
            collisionManager.EnemyTower = Tower2[0];

            // Set tower centers for unit targeting
            Vector2 leftTowerCenter = new Vector2(
                Tower1[0].Rectangle.X + Tower1[0].Rectangle.Width / 2,
                Tower1[0].Rectangle.Y + Tower1[0].Rectangle.Height / 2
            );

            Vector2 rightTowerCenter = new Vector2(
                Tower2[0].Rectangle.X + Tower2[0].Rectangle.Width / 2,
                Tower2[0].Rectangle.Y + Tower2[0].Rectangle.Height / 2
            );

            unitManager.SetTowerPositions(leftTowerCenter, rightTowerCenter);

            // ENEMY AI
            enemyAI = new EnemyAIManager(unitManager, moneyManager, Tower1[0], Tower2[0]);
        }

        public override void UnloadContent()
        {
        }

        public override void Update(GameTime gameTime)
        {

            var ks = Keyboard.GetState();

            // Player spawn keys
            if (ks.IsKeyDown(Keys.D1) && previousKeyboardState.IsKeyUp(Keys.D1))
                unitManager.SpawnPlayerSoldier();

            if (ks.IsKeyDown(Keys.D2) && previousKeyboardState.IsKeyUp(Keys.D2))
                unitManager.SpawnPlayerMachineGunner();

            if (ks.IsKeyDown(Keys.D3) && previousKeyboardState.IsKeyUp(Keys.D3))
                unitManager.SpawnPlayerTank();

            // Update managers
            unitManager.Update(gameTime);
            shellManager.Update(gameTime);
            explosionManager.Update(gameTime);
            collisionManager.Update(gameTime);
            
            moneyManager.Update();

            // Enemy AI
            enemyAI.Update(gameTime);


            //Player health bar rectangles
            Tower1HealthRectangle = new Rectangle(Tower1[0].TowerX, Tower1[0].TowerY - 20, (Tower1[0].TowerHealth)/ 5, 30);
            Tower2HealthRectangle = new Rectangle(Tower2[0].TowerX, Tower2[0].TowerY - 20, (Tower2[0].TowerHealth)/ 5, 30);

            // Return to menu
            if (ks.IsKeyDown(Keys.M))
            {
                Game.ChangeState(new MenuState(Game, GraphicsDevice, Content));
            }

            previousKeyboardState = ks;

        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Background
            spriteBatch.Draw(background, Vector2.Zero, Color.White);

            // Towers
            Tower1[0].Draw(spriteBatch);
            Tower2[0].Draw(spriteBatch);

            // Units, shells, explosions
            unitManager.Draw(spriteBatch);
            shellManager.Draw(spriteBatch);
            explosionManager.Draw(spriteBatch);

            // Money UI
            spriteBatch.Draw(moneyIcon, new Vector2(20, 20), Color.White);
            spriteBatch.DrawString(defaultFont, moneyManager.Money.ToString(), new Vector2(70, 20), Color.Yellow);

            uiManager.DrawHealthBar(spriteBatch, Tower1HealthRectangle, Tower1[0].TowerHealth, 1000);
            uiManager.DrawHealthBar(spriteBatch, Tower2HealthRectangle, Tower2[0].TowerHealth, 1000);

            // Tower health bars
            uiManager.DrawTowerHealth(spriteBatch, Tower1[0], new Vector2(Tower1[0].TowerX + 43, Tower1[0].TowerY - 20));
            uiManager.DrawTowerHealth(spriteBatch, Tower2[0], new Vector2(Tower2[0].TowerX + 40, Tower2[0].TowerY - 20));

            
        }
    }
}
