using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Coursework.States;

namespace Coursework
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // State management
        private BaseState _currentState;
        private BaseState _nextState;

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

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Start with MenuState
            _currentState = new MenuState(this, GraphicsDevice, Content);
            _currentState.LoadContent();
        }

        protected override void UnloadContent()
        {   //call the method only if it is not null otherwise ignore.
            _currentState?.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        { // if the user presses back or escape, then exit the game
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                
                Exit();

            // Allow states to request a state change by setting NextState property
            _currentState.Update(gameTime);

            if (_nextState != null)
            {
                _currentState.UnloadContent();
                _currentState = _nextState;
                _nextState = null;
                _currentState.LoadContent();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            

            _spriteBatch.Begin();
            _currentState.Draw(gameTime, _spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        // Called by states to request a transition
        public void ChangeState(BaseState newState)
        {
            _nextState = newState;
        }
    }
}
