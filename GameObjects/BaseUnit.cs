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
        public int MaxHealth;
        public int Damage;
        public float Scale;

        public bool IsPlayer;

        public float attackTimer = 0f;
        public float attackCooldown = 1f;
        public float AttackSpeed = 1f;

        // Distance (in pixels) at which this unit stops walking and starts attacking the opposing tower.
        public float AttackRange { get; protected set; }

        public virtual bool FiresShells => false;

        public bool IsAlive => Health > 0;

        // The unit's true visual center, based on its actual texture size and scale.
        // Used (among other things) as the shell muzzle position so shells spawn from
        // wherever this unit actually is, regardless of that unit type's sprite size.
        public Vector2 Center => texture == null
            ? Position
            : new Vector2(Position.X + (texture.Width * Scale) / 2f, Position.Y + (texture.Height * Scale) / 2f);

        public BaseUnit(Texture2D texture, int health, float speed, int damage, Vector2 position, float scale = 1f, float attackRange = 150f, float attackSpeed = 1f)
        {
            this.texture = texture;
            Health = health;
            MaxHealth = health;
            Speed = speed;
            Damage = damage;
            Position = position;
            Scale = scale;
            AttackRange = attackRange;
            AttackSpeed = attackSpeed;
            attackCooldown = attackSpeed;
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive || amount <= 0)
                return;

            Health -= amount;
            if (Health < 0)
                Health = 0;
        }

        /// <summary>
        /// Moves the unit horizontally toward the target's X position, keeping its own Y (lane-based movement).
        /// </summary>
        public virtual void Move(Vector2 targetPosition, GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float step = Speed * 60f * dt; // scaled to feel consistent regardless of frame rate

            float remaining = targetPosition.X - Position.X;

            if (Math.Abs(remaining) <= step)
                Position.X = targetPosition.X;
            else
                Position.X += Math.Sign(remaining) * step;
        }

        /// <summary>
        /// Standard per-frame update: walk toward the target until in attack range, then hold position;
        /// always ticks down the attack timer.
        /// </summary>
        public virtual void Update(Vector2 targetPosition, GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!IsInAttackRange(targetPosition))
                Move(targetPosition, gameTime);

            EdgeCollide();

            if (attackTimer > 0f)
                attackTimer -= dt;
        }

        public float DistanceToTargetX(Vector2 targetPosition) => Math.Abs(targetPosition.X - Position.X);

        public bool IsInAttackRange(Vector2 targetPosition) => DistanceToTargetX(targetPosition) <= AttackRange;

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (texture != null)
                spriteBatch.Draw(texture, Position, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
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
            // Keep units inside the playfield bounds.
            // NOTE: if the screen dimensions change, update these bounds too.
            Position.X = MathHelper.Clamp(Position.X, 0, 2000);
            Position.Y = MathHelper.Clamp(Position.Y, 0, 1375);
        }
    }
}