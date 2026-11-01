using Coursework.GameObjects;
using Coursework.GameObjects.Units;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace Coursework.Managers
{
    // loading shared sound effects here avoids each unit needing its own copy or content-loading code
    internal class SoundManager
    {




        private SoundEffect shoot;
        private SoundEffect soldierShoot;
        private SoundEffect machineGunnerShoot;
        private SoundEffect explosion;

        // keeping the sound asset names in one place makes the effects easier to change
        public void LoadContent(ContentManager content)
        {
            shoot = content.Load<SoundEffect>("Shoot");
            soldierShoot = content.Load<SoundEffect>("SoldierShoot");
            machineGunnerShoot = content.Load<SoundEffect>("MachineGunnerShoot");
            explosion = content.Load<SoundEffect>("explosion");
        }

        // both teams select the same firing sound by unit type and use the shared effects volume
        public void PlayShoot(BaseUnit unit)
        {
            SoundEffect sound = unit is Soldier ? soldierShoot
                : unit is MachineGunner ? machineGunnerShoot
                : shoot;
            sound?.Play();
        }
        public void PlayExplosion() { explosion?.Play(); }
    }
}
