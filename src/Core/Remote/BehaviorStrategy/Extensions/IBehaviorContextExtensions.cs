namespace AnotherECS.Core.Remote
{
    public static class IBehaviorContextExtensions
    {
        public static bool TryGetMostPerformanceOtherPlayer(this IBehaviorContext context, out Player result)
        {
            var players = context.Players;
            long current = long.MaxValue;
            int index = -1;
            for (int i = 0; i < players.Length; ++i)
            {
                var player = players[i];
                if (!player.IsLocal && current > player.PerformanceTiming)
                {
                    current = player.PerformanceTiming;
                    index = i;
                }
            }

            if (index != -1)
            {
                result = players[index];
                return true;
            }

            result = default;
            return false;
        }

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
