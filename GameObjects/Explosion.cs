using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework.GameObjects
{
    internal class Explosion
    {
        public Vector2 Position;
        public float Lifetime = 1f;
        public float Timer = 0f;
        private const int TotalFrames = 20;
        public int Damage;
        public float Radius;
        private Texture2D texture;
        private int currentFrame = 0;
        private bool isFinished = false;

        public Explosion(Texture2D texture, Vector2 position, float lifetime, int damage = 30, float radius = 40f)
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

            if (Timer >= 0.03f)
            {
                currentFrame++;
                Timer = 0f;
            }

            if (currentFrame >= 20)
            {
                isFinished = true;
                return;
            }
        }

        public bool IsFinished => Timer >= Lifetime;

        public void Draw(SpriteBatch spriteBatch)
        {
            if (texture == null || isFinished)
                return;

            int frameWidth = texture.Width / 20;
            int frameHeight = texture.Height;

            // SAFETY CHECK
            if (currentFrame < 0 || currentFrame >= 20)
                return;

            Rectangle sourceRect = new Rectangle(frame * frameWidth, 0, frameWidth, frameHeight);

                spriteBatch.Draw(texture, Position, sourceRect, Color.White, 0f, new Vector2(frameWidth / 2f, frameHeight / 2f), 1f, SpriteEffects.None, 0f);
            
        }

        public Rectangle Bounds => new Rectangle((int)(Position.X - Radius), (int)(Position.Y - Radius), (int)(Radius * 2), (int)(Radius * 2));
    }
}
