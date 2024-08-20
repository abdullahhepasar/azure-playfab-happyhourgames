namespace PlayFab.AzureFunctions
{
    using static Constants;
    public class RawEmployeeToEmployeeObjectMapper : IMapper<RawPlayer, Player>
    {
        public Player Map(RawPlayer rawPlayer)
        {
            return new Player(TablePartitionKey, rawPlayer.Id)
            {
                 GameLaunch = rawPlayer.GameLaunch
            };
        }
    }
}
