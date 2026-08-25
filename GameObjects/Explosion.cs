using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework.GameObjects
{
    internal class Explosion
    {
        private const int TotalFrames = 30;
        private const float SecondsPerFrame = 0.001f;
        public Vector2 Position;
        public int Damage;
        public float Radius;
        public bool DamageApplied;
        private readonly Texture2D texture;
        private float frameTimer;
        private int currentFrame;

        public Explosion(Texture2D texture, Vector2 position, float lifetime, int damage = 30, float radius = 20f)
        {
            this.texture = texture;
            Position = position;
            Damage = damage;
            Radius = radius;
            _ = lifetime; // animation length is just TotalFrames now
        }

        public bool IsFinished => currentFrame >= TotalFrames;

        public void Update(GameTime gameTime)
        {
            if (IsFinished)
                return;

            frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (frameTimer >= SecondsPerFrame)
            {
                currentFrame++;
                frameTimer = 0f;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (texture == null || IsFinished)
                return;

            int frameWidth = texture.Width / TotalFrames;
            int frameHeight = texture.Height;

            Rectangle sourceRect = new Rectangle(currentFrame * frameWidth, 0, frameWidth, frameHeight);
            spriteBatch.Draw(texture, Position, sourceRect, Color.White, 0f, new Vector2(frameWidth / 2f, frameHeight / 2f), 1f, SpriteEffects.None, 0f);
        }

        public bool Hits(Vector2 point)
        {
            float r = Radius;
            return Vector2.DistanceSquared(Position, point) <= r * r;
        }
    }
}