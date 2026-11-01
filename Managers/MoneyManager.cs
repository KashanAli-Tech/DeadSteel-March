namespace Coursework.Managers
{
    // keeps the player's balance in one place so unit costs, upgrades and rewards use the same money
    internal class MoneyManager
    {
        // other classes can read the balance, but spending and rewards go through these methods
        public int Money { get; private set; } = 500;

        // choosing the starting balance here only affects a fresh match
        public MoneyManager(int startingMoney = 500)
        {
            Money = System.Math.Max(0, startingMoney);
        }

        // adding rewards through one method avoids separate balances in the managers that award them
        public void AddMoney(int amount)
        {
            Money += amount;
        }

        // checking funds and deducting together lets callers create a unit or apply an upgrade only after payment succeeds
        public bool Spend(int amount)
        {
            if (amount >= 0 && Money >= amount)
            {
                Money -= amount;
                return true;
            }
            return false;
        }

        // there is no automatic income here yet; money currently changes through rewards and spending
        public void Update() { /* placeholder for interest/over-time income */ }
    }
}
