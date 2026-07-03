using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework.GameObjects
{
    internal class Explosion
    {
        public Vector2 Position;
        public float Lifetime; // seconds
        public float Timer;
        public int Damage;
        public float Radius;
        private Texture2D texture;

        public Explosion(Texture2D texture, Vector2 position, float lifetime = 0.6f, int damage = 30, float radius = 40f)
        {
            this.texture = texture;
            Position = position;
            Lifetime = lifetime;
            Damage = damage;
            Radius = radius;
            Timer = 0f;
        }

        public void Update(GameTime gameTime)
        {
            Timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public bool IsFinished => Timer >= Lifetime;

        public void Draw(SpriteBatch spriteBatch)
        {
            if (texture != null)
            {
                float progress = Timer / Lifetime;
                float scale = 0.5f + progress * 1.5f;
                spriteBatch.Draw(texture, Position, null, Color.White * (1f - progress), 0f, new Vector2(texture.Width / 2f, texture.Height / 2f), scale, SpriteEffects.None, 0f);
            }
        }

        public Rectangle Bounds => new Rectangle((int)(Position.X - Radius), (int)(Position.Y - Radius), (int)(Radius * 2), (int)(Radius * 2));
    }
}
