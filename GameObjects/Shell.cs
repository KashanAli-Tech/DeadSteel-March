using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework.GameObjects
{
    internal class Shell
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public int Damage;
        public float Speed;
        public bool IsAlive = true;
        public bool IsPlayerShell;

        private readonly Texture2D texture;

        public Shell(Texture2D texture, Vector2 start, Vector2 target, bool isPlayerShell, int damage = 20, float speed = 800f)
        {
            this.texture = texture;
            Position = start;
            Damage = damage;
            Speed = speed;
            IsPlayerShell = isPlayerShell;

            Vector2 direction = target - start;
            if (direction == Vector2.Zero)
                direction = new Vector2(isPlayerShell ? 1 : -1, 0);
            direction.Normalize();
            Velocity = direction * Speed;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += Velocity * dt;

            if (Position.X < -100 || Position.X > 2100 || Position.Y < -100 || Position.Y > 1500)
                IsAlive = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (texture != null)
                // 0.1 was basically invisible on the tower; 0.35 still small but you can see it
                spriteBatch.Draw(texture, Position, null, Color.White, 0f, new Vector2(texture.Width / 2f, texture.Height / 2f), 0.1f, SpriteEffects.None, 0f);
        }

        public Rectangle Rectangle
        {
            get
            {
                if (texture == null) return Rectangle.Empty;
                return new Rectangle((int)Position.X - 5, (int)Position.Y - 5, 10, 10);
            }
        }
    }
}
