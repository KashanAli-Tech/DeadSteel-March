using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework.States
{
    public abstract class BaseState
    {
        protected Game1 Game;
        protected GraphicsDevice GraphicsDevice;
        protected ContentManager Content;

        protected BaseState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
        {
            Game = game;
            GraphicsDevice = graphicsDevice;
            Content = content;
        }

        public abstract void LoadContent();
        public abstract void UnloadContent();
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);

        
    }
}
