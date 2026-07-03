namespace Coursework.Managers
{
    internal class MoneyManager
    {
        public int Money { get; private set; } = 500; // STARTS WITH 500, I WILL CHANGE THIS TO MAKE THE GAME A BIT MORE COMPETITIVE DEPENDING ON FINAL RESULT

        public void AddMoney(int amount)
        {
            Money += amount;
        }

        public bool Spend(int amount)
        {
            if (Money >= amount)
            {
                Money -= amount;
                return true;
            }
            return false;
        }

        public void Update() { /* placeholder for interest/over-time income */ }
    }
}
