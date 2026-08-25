using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework.GameObjects
{
    internal class Explosion
    {
        private const int TotalFrames =30;
        private const float SecondsPerFrame = 0.001f;
        public Vector2 Position;
        public int Damage;
        public float Radius;
        private readonly Texture2D texture;
        private float frameTimer;
        private int currentFrame;

        public Explosion(Texture2D texture, Vector2 position, float lifetime, int damage = 30, float radius = 40f)
        {
            this.texture = texture;
            Position = position;
            Damage = damage;
            Radius = radius;
            // 'lifetime' kept as a parameter for backwards compatibility with existing callers;
            // actual duration is driven by the frame animation (TotalFrames * SecondsPerFrame) below.
        }

        // BUG FIX: previously this compared an internal Timer (which resets to 0 every frame tick)
        // against Lifetime, so it was almost never true and explosions never got removed.
        // Now it's tied to the same frame counter the animation actually uses.
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

        public Rectangle Bounds => new Rectangle((int)(Position.X - Radius), (int)(Position.Y - Radius), (int)(Radius * 2), (int)(Radius * 2));
    }
}