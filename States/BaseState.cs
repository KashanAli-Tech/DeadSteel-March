using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework.States
{
    // a shared base for screens so each state provides the same loading, updating and drawing methods
    public abstract class BaseState
    {
        protected Game1 Game;
        protected GraphicsDevice GraphicsDevice;
        protected ContentManager Content;

        // passing in the existing game resources so states do not need separate graphics or content systems
        protected BaseState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
        {
            Game = game;
            GraphicsDevice = graphicsDevice;
            Content = content;
        }

        // each state loads the assets it needs before it is drawn
        public abstract void LoadContent();
        // gives states a place for any cleanup when the game switches screens
        public abstract void UnloadContent();
        // input and changing game data belong in update, separate from drawing
        public abstract void Update(GameTime gameTime);
        // states draw through the sprite batch supplied by the main game
        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);

        
    }
}
