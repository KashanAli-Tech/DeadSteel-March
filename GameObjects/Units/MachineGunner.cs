using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework.GameObjects.Units
{
    using Coursework.GameObjects;

    // a quicker firing unit that inherits the common movement and health code
    internal class MachineGunner : BaseUnit
    {
        // the shorter attack interval makes this unit fire more often without needing a different update algorithm
        public MachineGunner(Texture2D texture, Vector2 position)
            : base(texture, 120, 1.8f, 15, position, 1f, 180f, 0.35f)
        {
        }

        // using the same projectile route as the other units keeps tower hit detection consistent
        public override bool FiresShells => true;

        public override void Update(Vector2 targetPosition, GameTime gameTime)
        {
            base.Update(targetPosition, gameTime);
        }
    }
}
