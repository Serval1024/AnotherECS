namespace AnotherECS.Core.Remote
{
    public static class IBehaviorContextExtensions
    {
        public static Player GetNextOtherPlayer(this IBehaviorContext context, Player player)
        {
            var players = context.Players;
            if (players.Length > 1)
            {
                int startIndex = 0;
                for (int i = 0; i < players.Length; ++i)
                {
                    if (players[i].Id == player.Id)
                    {
                        startIndex = i;
                        break;
                    }
                }

                do
                {
                    startIndex = (startIndex + 1) % players.Length;
                }
                while (players[startIndex] != context.LocalPlayer);

                return players[startIndex];
            }
            return player;
        }
    }
}
