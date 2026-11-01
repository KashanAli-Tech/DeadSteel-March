using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Coursework.GameObjects;
using Coursework.Managers;
using Coursework.UI;

namespace Coursework.States
{
    // brings the gameplay managers together while leaving their individual rules in their own classes
    internal class GameState : BaseState
    {

        private const int HealthBarWidth = 240;

        private Texture2D background;
        private Texture2D gameOverBackground;

        private Texture2D tower1Texture;
        private Texture2D tower2Texture;

        private Rectangle Tower1HealthRectangle;
        private Rectangle Tower2HealthRectangle;

        private SpriteFont defaultFont;

        private Song gameplayMusic;

        // keeping the previous input states so spawning, upgrades and click damage can detect a new press
        private KeyboardState previousKeyboardState;
        private MouseState previousMouseState;
        // only used for the hud clock, so this cannot change any spawn or upgrade timers
        private double gameplayElapsedSeconds;

        // separate managers keep combat, money, effects and ui from becoming one large update method
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
        private MenuScreens menus;
        private SettingsMenu settingsMenu;
        private MenuPage menuPage;
        public bool IsPaused => menuPage != MenuPage.None;

        public Tower[] Tower1 = new Tower[1];
        public Tower[] Tower2 = new Tower[1];
        public Vector2 PlayerTowerV;
        public Vector2 EnemyTowerV;

        // either tower being destroyed ends the active gameplay
        private bool IsGameOver => Tower1[0].IsDead || Tower2[0].IsDead;

        public GameState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
            : base(game, graphicsDevice, content)
        {
        }

        // loading the match assets and creating fresh managers so each game starts with its own data
        public override void LoadContent()
        {
            MatchConfig match = GameSettings.Current.CreateMatchConfig();
            background = Content.Load<Texture2D>("Background");
            gameOverBackground = Content.Load<Texture2D>("GameOverBackground");
            // resetting the display clock when gameplay loads rather than using time spent in the menu
            gameplayElapsedSeconds = 0d;
            menuPage = MenuPage.None;

            tower1Texture = Content.Load<Texture2D>("PlayerTower");
            tower2Texture = Content.Load<Texture2D>("EnemyTower");

            defaultFont = Content.Load<SpriteFont>("DefaultFont");
            menus = new MenuScreens(GraphicsDevice, Content, defaultFont);
            settingsMenu = new SettingsMenu(menus);

            gameplayMusic = Content.Load<Song>("GameBackgroundMusic");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(gameplayMusic);

            unitManager = new UnitManager
            {
                MatchConfig = match,
                PlayerSoldierCost = match.SoldierCost,
                PlayerMachineGunnerCost = match.MachineGunnerCost,
                PlayerTankCost = match.TankCost
            };
            shellManager = new ShellManager();
            explosionManager = new ExplosionManager();
            collisionManager = new CollisionManager(unitManager, shellManager, explosionManager);
            moneyManager = new MoneyManager(match.StartingMoney);
            soundManager = new SoundManager();
            uiManager = new UIManager();

            // each manager loads its own assets through the shared content manager
            unitManager.LoadContent(Content);
            shellManager.LoadContent(Content);
            explosionManager.LoadContent(Content);
            soundManager.LoadContent(Content);
            uiManager.LoadContent(Content, GraphicsDevice);

            // passing managers the existing objects they need, e.g. the same money manager handles spawning and upgrades
            unitManager.ShellManager = shellManager;
            unitManager.SoundManager = soundManager;
            collisionManager.SoundManager = soundManager;
            unitManager.MoneyManager = moneyManager;
            Tower1[0] = new Tower(tower1Texture, new Rectangle(0, 250, 200, 300), -25, 300, 200, 300, match.PlayerTowerHealth);
            Tower2[0] = new Tower(tower2Texture, new Rectangle(1600, 250, 200, 300), 1750, 300, 200, 300, match.EnemyTowerHealth);

            upgradeManager = new UpgradeManager(Tower1[0], unitManager, moneyManager) { UpgradeRefreshSeconds = match.UpgradeRefreshSeconds };
            upgradeManager.LoadContent(Content, defaultFont);

            PlayerTowerV = new Vector2(Tower1[0].TowerX, Tower1[0].TowerY);
            EnemyTowerV = new Vector2(Tower2[0].TowerX, Tower2[0].TowerY);

            // sharing the real tower objects so attacks and collision checks use their current health and positions
            unitManager.SetTowers(Tower1[0], Tower2[0]);
            unitManager.SetTowerPositions(PlayerTowerV, EnemyTowerV);

            collisionManager.PlayerTower = Tower1[0];
            collisionManager.EnemyTower = Tower2[0];

            enemyAI = new EnemyAIManager(unitManager, moneyManager, Tower1[0], Tower2[0], GameSettings.Current.Difficulty);
            enemyAI.ExplosionManager = explosionManager;
            enemyAI.SoundManager = soundManager;

            // player clicks use their own manager, while the enemy ai chooses targets separately
            unitKillManager = new UnitKillManager(unitManager, Tower2[0]);
            unitKillManager.ExplosionManager = explosionManager;
            unitKillManager.SoundManager = soundManager;
            unitKillManager.MoneyManager = moneyManager;
        }

        public override void UnloadContent()
        {
            uiManager?.Dispose();
            menus.Dispose();
            MediaPlayer.Stop();
        }

        // processing input and updating the match only while both towers are still standing
        public override void Update(GameTime gameTime)
        {
            var ks = Keyboard.GetState();
            var menuMouse = Mouse.GetState();
            if (UpdateMenus(ks, menuMouse))
            {
                // remembering inputs while paused stops held gameplay keys firing when the match resumes
                previousMouseState = menuMouse;
                previousKeyboardState = ks;
                return;
            }

            if (!IsGameOver)
            {
                // adding elapsed seconds keeps the display clock tied to gameplay time rather than the frame rate
                gameplayElapsedSeconds += gameTime.ElapsedGameTime.TotalSeconds;

                // comparing old and new key states so holding 1, 2 or 3 does not repeatedly spawn units
                if (ks.IsKeyDown(Keys.D1) && previousKeyboardState.IsKeyUp(Keys.D1))
                    unitManager.SpawnPlayerSoldier();

                if (ks.IsKeyDown(Keys.D2) && previousKeyboardState.IsKeyUp(Keys.D2))
                    unitManager.SpawnPlayerMachineGunner();

                if (ks.IsKeyDown(Keys.D3) && previousKeyboardState.IsKeyUp(Keys.D3))
                    unitManager.SpawnPlayerTank();

                // updating upgrades after player spawning so a new unit gets any bought bonuses before it moves or fires
                upgradeManager.Update(gameTime, ks, previousKeyboardState);

                // a click only counts when the left button changes from released to pressed
                var ms = Mouse.GetState();
                if (ms.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                    unitKillManager.RegisterClick(ms.Position);
                previousMouseState = ms;

                // moving and firing first, then checking shell hits before clearing up finished effects
                unitManager.Update(gameTime);
                shellManager.Update(gameTime);
                collisionManager.Update(gameTime);
                explosionManager.Update(gameTime);

                if (!IsGameOver)
                {
                    moneyManager.Update();
                    enemyAI.Update(gameTime);
                }
            }

            // keeping each health bar frame full width cuz the ui manager scales just its coloured fill
            Tower1HealthRectangle = new Rectangle((int)Tower1[0].Center.X - HealthBarWidth / 2 + 45, Tower1[0].TowerY - 40, HealthBarWidth, 32);
            Tower2HealthRectangle = new Rectangle((int)Tower2[0].Center.X - HealthBarWidth / 2 - 50, Tower2[0].TowerY - 40, HealthBarWidth, 32);

            if (ks.IsKeyDown(Keys.M))
            {
                Game.ChangeState(new MenuState(Game, GraphicsDevice, Content));
            }

            // saving this frame's keys after the input checks, ready for the next update
            previousKeyboardState = ks;
        }

        // drawing the world first and the hud afterwards so the information is not covered by units
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(background, Vector2.Zero, Color.White);

            // towers go behind the units, shells and explosions
            Tower1[0].Draw(spriteBatch);
            Tower2[0].Draw(spriteBatch);

            unitManager.Draw(spriteBatch);
            shellManager.Draw(spriteBatch);
            explosionManager.Draw(spriteBatch);

            // reading existing match values for the hud without changing money, health or upgrade data
            uiManager.DrawMoney(spriteBatch, moneyManager.Money, GraphicsDevice.Viewport);
            uiManager.DrawGameTimer(spriteBatch, gameplayElapsedSeconds, GraphicsDevice.Viewport);
            uiManager.DrawTowerHealth(spriteBatch, Tower1[0], Tower1HealthRectangle, new Color(83, 165, 119));
            uiManager.DrawTowerHealth(spriteBatch, Tower2[0], Tower2HealthRectangle, new Color(210, 99, 112));

            upgradeManager.Draw(spriteBatch, GraphicsDevice.Viewport);

            // covering the match with the result screen once a tower has been destroyed
            if (IsGameOver)
            {
                menus.DrawResults(spriteBatch, gameOverBackground, !Tower1[0].IsDead, gameplayElapsedSeconds,
                    unitManager.EnemyUnitsEliminated, unitManager.PlayerUnitsDeployed, upgradeManager.TotalPurchases, Mouse.GetState());
            }
            else if (menuPage == MenuPage.Instructions)
                menus.DrawInstructions(spriteBatch, Mouse.GetState(), unitManager.PlayerSoldierCost, unitManager.PlayerMachineGunnerCost, unitManager.PlayerTankCost);
            else if (menuPage == MenuPage.Settings)
                settingsMenu.Draw(spriteBatch, Mouse.GetState());
            else if (IsPaused)
                menus.DrawPause(spriteBatch, Mouse.GetState());
        }

        // returning before the match update freezes every gameplay timer but still lets menus receive input
        private bool UpdateMenus(KeyboardState keyboard, MouseState mouse)
        {
            if (IsGameOver)
            {
                HandleMenuAction(menus.UpdateResults(mouse, previousMouseState));
                if (keyboard.IsKeyDown(Keys.M)) HandleMenuAction(MenuAction.MainMenu);
                return true;
            }
            if (keyboard.IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape))
            {
                menuPage = menuPage == MenuPage.Pause ? MenuPage.None : MenuPage.Pause;
                return true;
            }
            if (!IsPaused) return false;
            if (menuPage == MenuPage.Pause) HandleMenuAction(menus.UpdatePause(mouse, previousMouseState));
            else if (menuPage == MenuPage.Settings && settingsMenu.Update(mouse, previousMouseState)) HandleMenuAction(MenuAction.Back);
            else if (menuPage == MenuPage.Instructions && menus.Clicked(new Rectangle(760, 940, 400, 100), mouse, previousMouseState))
                HandleMenuAction(MenuAction.Back);
            return true;
        }

        // using the existing state switch creates fresh managers for retry and leaves preferences separate from match data
        private void HandleMenuAction(MenuAction action)
        {
            switch (action)
            {
                case MenuAction.Retry: Game.ChangeState(new GameState(Game, GraphicsDevice, Content)); break;
                case MenuAction.MainMenu: Game.ChangeState(new MenuState(Game, GraphicsDevice, Content)); break;
                case MenuAction.Exit: Game.Exit(); break;
                case MenuAction.Resume: menuPage = MenuPage.None; break;
                case MenuAction.Settings: menuPage = MenuPage.Settings; break;
                case MenuAction.Instructions: menuPage = MenuPage.Instructions; break;
                case MenuAction.Back: menuPage = MenuPage.Pause; break;
            }
        }
    }
}
