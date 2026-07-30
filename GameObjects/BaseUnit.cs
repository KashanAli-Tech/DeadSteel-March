using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Coursework.GameObjects
{
    internal class BaseUnit
    {
        protected Texture2D texture;
        public Vector2 Position;
        public float Speed;
        public int Health;
        public int Damage;
        public float Scale;
        public bool JustSpawned = true;

        public float attackTimer = 0f;
        public float attackCooldown = 1f;

        public BaseUnit(Texture2D texture, int health, float speed, int damage, Vector2 position, float scale = 1f)
        {
            this.texture = texture;
            Health = health;
            Speed = speed;
            Damage = damage;
            Position = position;
            Scale = scale;
        }

        public virtual void Move(Vector2 targetPosition, GameTime gameTime)
        {
            // Move horizontally toward target X while preserving Y
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float direction = Math.Sign(targetPosition.X - Position.X);
            Position.X += direction * Speed * 60f * dt; // scaled to feel consistent

            // Snap if very close
            if (Math.Abs(targetPosition.X - Position.X) < Speed * 60f * dt)
                Position.X = targetPosition.X;
        }

        public virtual void Update(Vector2 targetPosition, GameTime gameTime)
        {
            if (!JustSpawned)
                Move(targetPosition, gameTime);

            EdgeCollide();
            JustSpawned = false;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (texture != null)
            {
                spriteBatch.Draw(texture, Position, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
            }
        }

        public Rectangle Rectangle
        {
            get
            {
                if (texture == null) return Rectangle.Empty;
                return new Rectangle((int)Position.X, (int)Position.Y, (int)(texture.Width * Scale), (int)(texture.Height * Scale));
            }
        }

        protected void EdgeCollide()
        {
            // Keep units inside screen bounds (0..2000 x 0..1375)
            // Reminder for myself: If i change my screen dimension i need to change this aswell

            Position.X = MathHelper.Clamp(Position.X, 0, 2000); // clamp function is used for 
            Position.Y = MathHelper.Clamp(Position.Y, 0, 1375);
        }
    }
}
