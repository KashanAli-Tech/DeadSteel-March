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
        // 1 = Soldier, 2 = Machine Gunner, 3 = Tank (placeholder keybinds until proper UI buttons exist)

        private const int HealthBarWidth = 200;

        // VISUALS
        private Texture2D background;
        private Texture2D gameOverBackground;
        private Texture2D moneyIcon;

        // Towers
        private Texture2D tower1Texture;
        private Texture2D tower2Texture;

        private Rectangle Tower1HealthRectangle;
        private Rectangle Tower2HealthRectangle;

        // Fonts
        private SpriteFont defaultFont;

        // Music
        private Song gameplayMusic;

        private KeyboardState previousKeyboardState;
        private MouseState previousMouseState;

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
        private UnitKillManager unitKillManager;

        // Towers
        public Tower[] Tower1 = new Tower[1];
        public Tower[] Tower2 = new Tower[1];
        public Vector2 PlayerTowerV;
        public Vector2 EnemyTowerV;

        private bool IsGameOver => Tower1[0].IsDead || Tower2[0].IsDead;

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

            tower1Texture = Content.Load<Texture2D>("PlayerTower");
            tower2Texture = Content.Load<Texture2D>("EnemyTower");

            defaultFont = Content.Load<SpriteFont>("DefaultFont");

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

            // Load manager content (each manager owns and loads its own textures)
            unitManager.LoadContent(Content);
            shellManager.LoadContent(Content);
            explosionManager.LoadContent(Content);
            soundManager.LoadContent(Content);
            uiManager.LoadContent(Content, GraphicsDevice);

            // Wire managers
            unitManager.ShellManager = shellManager;
            unitManager.SoundManager = soundManager;
            collisionManager.SoundManager = soundManager;

            // CREATE TOWERS
            Tower1[0] = new Tower(tower1Texture, new Rectangle(20, 250, 300, 400), 20, 250, 300, 400);
            Tower2[0] = new Tower(tower2Texture, new Rectangle(1580, 250, 300, 400), 1580, 250, 300, 400);

            PlayerTowerV = new Vector2(Tower1[0].TowerX, Tower1[0].TowerY);
            EnemyTowerV = new Vector2(Tower2[0].TowerX, Tower2[0].TowerY);

            unitManager.SetTowers(Tower1[0], Tower2[0]);
            unitManager.SetTowerPositions(PlayerTowerV, EnemyTowerV);

            collisionManager.PlayerTower = Tower1[0];
            collisionManager.EnemyTower = Tower2[0];

            enemyAI = new EnemyAIManager(unitManager, moneyManager, Tower1[0], Tower2[0]);
            enemyAI.ExplosionManager = explosionManager;
            enemyAI.SoundManager = soundManager;

            // click-to-kill only — AI targeting is enemyAI
            unitKillManager = new UnitKillManager(unitManager, Tower2[0]);
            unitKillManager.ExplosionManager = explosionManager;
            unitKillManager.SoundManager = soundManager;
            unitKillManager.MoneyManager = moneyManager;
        }

        public override void UnloadContent()
        {
        }

        public override void Update(GameTime gameTime)
        {
            var ks = Keyboard.GetState();

            if (!IsGameOver)
            {
                // Player spawn keys
                if (ks.IsKeyDown(Keys.D1) && previousKeyboardState.IsKeyUp(Keys.D1))
                    unitManager.SpawnPlayerSoldier();

                if (ks.IsKeyDown(Keys.D2) && previousKeyboardState.IsKeyUp(Keys.D2))
                    unitManager.SpawnPlayerMachineGunner();

                if (ks.IsKeyDown(Keys.D3) && previousKeyboardState.IsKeyUp(Keys.D3))
                    unitManager.SpawnPlayerTank();

                // Click-to-kill: only reacts to the click transitioning down this frame,
                // same edge-trigger pattern as the keyboard spawn keys above.
                var ms = Mouse.GetState();
                if (ms.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                    unitKillManager.RegisterClick(ms.Position);
                previousMouseState = ms;

                unitManager.Update(gameTime);
                shellManager.Update(gameTime);
                collisionManager.Update(gameTime);
                explosionManager.Update(gameTime);

                moneyManager.Update();

                enemyAI.Update(gameTime);
            }

            // Health bar rectangles - width scales off the tower's own max health,
            // so this stays correct even if MaxTowerHealth ever changes.
            Tower1HealthRectangle = new Rectangle(Tower1[0].TowerX, Tower1[0].TowerY - 20, (int)(Tower1[0].HealthPercentage * HealthBarWidth), 30);
            Tower2HealthRectangle = new Rectangle(Tower2[0].TowerX, Tower2[0].TowerY - 20, (int)(Tower2[0].HealthPercentage * HealthBarWidth), 30);

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

            // towers first so they sit behind stuff at the wall
            Tower1[0].Draw(spriteBatch);
            Tower2[0].Draw(spriteBatch);

            unitManager.Draw(spriteBatch);
            shellManager.Draw(spriteBatch); // shells last so they aren't painted over by the tower
            explosionManager.Draw(spriteBatch);

            // Money UI
            spriteBatch.Draw(moneyIcon, new Vector2(20, 20), Color.White);
            spriteBatch.DrawString(defaultFont, moneyManager.Money.ToString(), new Vector2(70, 20), Color.Yellow);

            uiManager.DrawHealthBar(spriteBatch, Tower1HealthRectangle, Tower1[0].TowerHealth, Tower1[0].MaxTowerHealth);
            uiManager.DrawHealthBar(spriteBatch, Tower2HealthRectangle, Tower2[0].TowerHealth, Tower2[0].MaxTowerHealth);

            // Tower health bars
            uiManager.DrawTowerHealth(spriteBatch, Tower1[0], new Vector2(Tower1[0].TowerX + 43, Tower1[0].TowerY - 20));
            uiManager.DrawTowerHealth(spriteBatch, Tower2[0], new Vector2(Tower2[0].TowerX + 40, Tower2[0].TowerY - 20));

            // Game over overlay - previously gameOverBackground was loaded but never drawn.
            if (IsGameOver)
            {
                spriteBatch.Draw(gameOverBackground, Vector2.Zero, Color.White);

                string message = Tower1[0].IsDead ? "Defeat! Press M for menu" : "Victory! Press M for menu";
                Vector2 textSize = defaultFont.MeasureString(message);
                Vector2 textPosition = new Vector2(
                    (GraphicsDevice.Viewport.Width - textSize.X) / 2f,
                    (GraphicsDevice.Viewport.Height - textSize.Y) / 2f
                );

                spriteBatch.DrawString(defaultFont, message, textPosition, Color.White);
            }
        }
    }
}