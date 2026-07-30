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
        private Texture2D texture;
        public bool IsAlive = true;
        public bool IsPlayerShell;

        public Shell(Texture2D texture, Vector2 start, Vector2 target, bool isPlayerShell, int damage = 20, float speed = 800f)
        {
            this.texture = texture;
            Position = start;
            Damage = damage;
            Speed = speed;
            IsPlayerShell = isPlayerShell;

            Vector2 dir = target - start;
            if (dir == Vector2.Zero) dir = new Vector2(1, 0);
            dir.Normalize();
            Velocity = dir * Speed;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += Velocity * dt;

            // Off-screen kill
            if (Position.X < -100 || Position.X > 2100 || Position.Y < -100 || Position.Y > 1500)
                IsAlive = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (texture != null)
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
