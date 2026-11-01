using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework.GameObjects
{
    // stores a projectile's movement and damage separately from the unit that fired it
    internal class Shell
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public int Damage;
        public float Speed;
        public bool IsAlive = true;
        public bool IsPlayerShell;

        private readonly Texture2D texture;

        // calculating the direction once makes the shell travel towards the position it was aimed at
        public Shell(Texture2D texture, Vector2 start, Vector2 target, bool isPlayerShell, int damage = 20, float speed = 800f)
        {
            this.texture = texture;
            Position = start;
            Damage = damage;
            Speed = speed;
            IsPlayerShell = isPlayerShell;

            Vector2 direction = target - start;
            // a default direction avoids trying to normalise a zero-length vector
            if (direction == Vector2.Zero)
                direction = new Vector2(isPlayerShell ? 1 : -1, 0);
            // making the direction length one means the speed controls how fast the shell travels
            direction.Normalize();
            Velocity = direction * Speed;
        }

        // moving with elapsed time and removing shells outside the playfield so they do not stay in the list forever
        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += Velocity * dt;

            if (Position.X < -100 || Position.X > 2100 || Position.Y < -100 || Position.Y > 1500)
                IsAlive = false;
        }

        // using a centred origin keeps the shell picture centred on its position
        public void Draw(SpriteBatch spriteBatch)
        {
            if (texture != null)
                spriteBatch.Draw(texture, Position, null, Color.White, 0f, new Vector2(texture.Width / 2f, texture.Height / 2f), 0.1f, SpriteEffects.None, 0f);
        }

        // a small box around the shell is enough for the tower collision check
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
