using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Coursework.UI;

namespace Coursework.States
{
    // keeping the menu separate means its buttons and music do not need to run during gameplay
    internal class MenuState : BaseState
    {
        private Texture2D menuBackground;
        private Texture2D buttonTexture;

        private SpriteFont menuFont;

        private Song menuMusic;

        private Rectangle playButtonRect;
        private Rectangle quitButtonRect;
        private MenuScreens menus;
        private SettingsMenu settingsMenu;
        private bool settingsOpen;
        private MouseState previousMouse;
        private KeyboardState previousKeyboard;
        public bool IsSubmenuOpen => settingsOpen;

        public MenuState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
            : base(game, graphicsDevice, content)
        {
        }

        // loading menu assets once on entry, then setting up the clickable button areas
        public override void LoadContent()
        {
            menuBackground = Content.Load<Texture2D>("MenuBackground");
            buttonTexture = Content.Load<Texture2D>("Button");

            menuFont = Content.Load<SpriteFont>("DefaultFont");
            menus = new MenuScreens(GraphicsDevice, Content, menuFont);
            settingsMenu = new SettingsMenu(menus);
            previousMouse = Mouse.GetState();
            previousKeyboard = Keyboard.GetState();

            menuMusic = Content.Load<Song>("MenuBackgroundMusic");
            // looping the menu track so it does not go quiet while the player is choosing an option
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(menuMusic);

            playButtonRect = new Rectangle(700, 450, 500, 120);
            quitButtonRect = new Rectangle(700, 600, 500, 120);
        }

        public override void UnloadContent()
        {
            menus.Dispose();
            MediaPlayer.Stop();
        }

        // using the mouse position and button areas to decide which menu option was selected
        public override void Update(GameTime gameTime)
        {
            var mouse = Mouse.GetState();
            var keyboard = Keyboard.GetState();
            if (settingsOpen)
            {
                if (settingsMenu.Update(mouse, previousMouse) || (keyboard.IsKeyDown(Keys.Escape) && previousKeyboard.IsKeyUp(Keys.Escape)))
                    settingsOpen = false;
                previousMouse = mouse;
                previousKeyboard = keyboard;
                return;
            }
            bool clicked = mouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released;

            // a press inside the play rectangle requests gameplay through the existing state-switch method
            if (clicked &&
                playButtonRect.Contains(mouse.Position)) 
            {
                Game.ChangeState(new GameState(Game, GraphicsDevice, Content));
            }

            if (clicked &&
                quitButtonRect.Contains(mouse.Position))
            {
                Game.Exit();
            }
            if (menus.Clicked(new Rectangle(700, 750, 500, 120), mouse, previousMouse)) settingsOpen = true;
            previousMouse = mouse;
            previousKeyboard = keyboard;
        }

        // drawing the background first so the title and buttons stay visible on top
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(menuBackground, Vector2.Zero, Color.White);

            spriteBatch.DrawString(menuFont, "DEADSTEEL MARCH",
                new Vector2(800, 200), Color.Red);

            spriteBatch.Draw(buttonTexture, playButtonRect, Color.White);
            // positioning each label from its button rectangle so the text moves with the button
            spriteBatch.DrawString(menuFont, "PLAY",
                new Vector2(playButtonRect.X + 200, playButtonRect.Y + 35),
                Color.White);

            spriteBatch.Draw(buttonTexture, quitButtonRect, Color.White);
            spriteBatch.DrawString(menuFont, "QUIT",
                new Vector2(quitButtonRect.X + 200, quitButtonRect.Y + 35),
                Color.White);
            if (settingsOpen) settingsMenu.Draw(spriteBatch, Mouse.GetState());
            else menus.Button(spriteBatch, new Rectangle(700, 750, 500, 120), "SETTINGS", Mouse.GetState());
        }
    }
}
