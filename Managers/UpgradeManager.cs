namespace Coursework.Managers
{
    // NOT FINISHED - NEED TO ADD MORE THINGS AND FIX SOME THINGS

    internal class UpgradeManager
    {
        public int TowerHealthUpgradeLevel { get; private set; } = 0;
        public int TowerDamageUpgradeLevel { get; private set; } = 0;

        public void UpgradeTowerHealth()
        {
            TowerHealthUpgradeLevel++;
        }

        public void UpgradeTowerDamage()
        {
            TowerDamageUpgradeLevel++;
        }
    }
}
