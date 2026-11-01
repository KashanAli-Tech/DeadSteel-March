using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework.GameObjects
{
    // combines a sprite sheet animation with the position and radius used for explosion damage
    internal class Explosion
    {
        private const int TotalFrames = 30;
        public Vector2 Position;
        public int Damage;
        public float Radius;
        // recording whether damage has happened stops the same explosion hurting units every frame
        public bool DamageApplied;
        private readonly Texture2D texture;
        private readonly double lifetime;
        private double elapsedSeconds;
        private int currentFrame;

        // half a second preserves the existing effect duration if an invalid lifetime is supplied
        public Explosion(Texture2D texture, Vector2 position, float lifetime, int damage = 30, float radius = 20f)
        {
            this.texture = texture;
            Position = position;
            Damage = damage;
            Radius = radius;
            this.lifetime = float.IsFinite(lifetime) && lifetime > 0f ? lifetime : 0.5f;
        }

        public bool IsFinished => currentFrame >= TotalFrames;

        // total elapsed time keeps the duration consistent even when one update spans several animation frames
        public void Update(GameTime gameTime)
        {
            if (IsFinished)
                return;

            elapsedSeconds += System.Math.Max(0d, gameTime.ElapsedGameTime.TotalSeconds);
            currentFrame = (int)(System.Math.Min(1d, elapsedSeconds / lifetime) * TotalFrames);
        }

        // selecting one frame from the horizontal sprite sheet instead of loading a separate image for each frame
        public void Draw(SpriteBatch spriteBatch)
        {
            if (texture == null || IsFinished)
                return;

            int frameWidth = texture.Width / TotalFrames;
            int frameHeight = texture.Height;

            Rectangle sourceRect = new Rectangle(currentFrame * frameWidth, 0, frameWidth, frameHeight);
            spriteBatch.Draw(texture, Position, sourceRect, Color.White, 0f, new Vector2(frameWidth / 2f, frameHeight / 2f), 1f, SpriteEffects.None, 0f);
        }

        // squared distance checks whether a point is inside the blast radius without needing a square root
        public bool Hits(Vector2 point)
        {
            float r = Radius;
            return Vector2.DistanceSquared(Position, point) <= r * r;
        }
    }
}