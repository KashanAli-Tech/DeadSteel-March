using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Coursework.States;
using Coursework.Managers;

namespace Coursework
{
    // monogame runs the main loop here, while separate states keep the menu and gameplay code apart
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // storing states through the base type so the main loop can use either screen in the same way
        private BaseState _currentState;
        private BaseState _nextState;
        private KeyboardState previousKeyboard;

        // setting up the graphics and content folder before the game starts loading its assets
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

          
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        // creating one sprite batch to share between screens, then loading the menu first
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            GameSettings.Current.ApplyAudio();

            _currentState = new MenuState(this, GraphicsDevice, Content);
            _currentState.LoadContent();
        }

        protected override void UnloadContent()
        {
            _currentState?.UnloadContent();
        }

        // updating the current screen and applying any requested state change afterwards
        protected override void Update(GameTime gameTime)
        {
            var keyboard = Keyboard.GetState();
            // gameplay handles escape for pause, while the main menu keeps its existing exit shortcut
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                (_currentState is MenuState menu && !menu.IsSubmenuOpen && keyboard.IsKeyDown(Keys.Escape) && previousKeyboard.IsKeyUp(Keys.Escape)))
                
                Exit();

            _currentState.Update(gameTime);
            // keeping the press edge stops escape used for back from also exiting on the next frame
            previousKeyboard = keyboard;

            // waiting until the current update finishes so a state is not replaced halfway through its own logic
            if (_nextState != null)
            {
                _currentState.UnloadContent();
                _currentState = _nextState;
                _nextState = null;
                _currentState.LoadContent();
            }

            base.Update(gameTime);
        }

        // keeping begin and end here so each state can draw through the same sprite batch
        protected override void Draw(GameTime gameTime)
        {
            

            _spriteBatch.Begin();
            _currentState.Draw(gameTime, _spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        // saving the requested state for the main update to switch to safely
        public void ChangeState(BaseState newState)
        {
            _nextState = newState;
        }
    }
}
