using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Coursework.GameObjects
{
    // shared unit data and movement live here so the different unit types can inherit the same basic behaviour
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
        // this is the interval between shots in seconds, so smaller values fire more often
        public float AttackSpeed = 1f;

        // the range controls when a unit stops moving towards the tower and can begin attacking
        public float AttackRange { get; protected set; }

        public virtual bool FiresShells => false;

        public bool IsAlive => Health > 0;

        // including sprite size and scale gives shells and explosion checks the unit's visual centre
        public Vector2 Center => texture == null
            ? Position
            : new Vector2(Position.X + (texture.Width * Scale) / 2f, Position.Y + (texture.Height * Scale) / 2f);

        // each unit type passes in its own stats, keeping the setup in one shared constructor
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
        }

        // ignoring invalid hits and already dead units, then keeping health from going below zero
        public void TakeDamage(int amount)
        {
            if (!IsAlive || amount <= 0)
                return;

            Health -= amount;
            if (Health < 0)
                Health = 0;
        }

        /// <summary>
        /// moving only on the x axis keeps each unit in its lane while it heads towards the opposing tower
        /// </summary>
        public virtual void Move(Vector2 targetPosition, GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // scaling movement by elapsed seconds keeps the speed consistent if the fps changes
            float step = Speed * 60f * dt;

            float remaining = targetPosition.X - Position.X;

            // snapping to the target when close enough avoids moving past it and turning back each frame
            if (Math.Abs(remaining) <= step)
                Position.X = targetPosition.X;
            else
                Position.X += Math.Sign(remaining) * step;
        }

        /// <summary>
        /// walking until the target is in range, while counting down the attack timer even when the unit is stationary
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

        // horizontal distance matches the lane movement, so height does not stop a unit attacking the tower
        public float DistanceToTargetX(Vector2 targetPosition) => Math.Abs(targetPosition.X - Position.X);

        public bool IsInAttackRange(Vector2 targetPosition) => DistanceToTargetX(targetPosition) <= AttackRange;

        // drawing at the unit's scale so the picture matches the size used by its rectangle
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (texture != null)
                spriteBatch.Draw(texture, Position, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
        }

        // using the scaled sprite size for overlap checks rather than one fixed box for every unit type
        public Rectangle Rectangle
        {
            get
            {
                if (texture == null) return Rectangle.Empty;
                return new Rectangle((int)Position.X, (int)Position.Y, (int)(texture.Width * Scale), (int)(texture.Height * Scale));
            }
        }

        // clamping positions keeps units inside the fixed playfield bounds
        // these limits need updating if the playfield size changes
        protected void EdgeCollide()
        {
            Position.X = MathHelper.Clamp(Position.X, 0, 2000);
            Position.Y = MathHelper.Clamp(Position.Y, 0, 1375);
        }
    }
}