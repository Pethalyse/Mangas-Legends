using Fusion;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;

namespace MangaLegends.Simulation.Match
{
    public sealed class PlayerSlot : NetworkBehaviour
    {
        [Networked] public PlayerRef Player { get; set; }
        [Networked] public NetworkString<_32> PlayerName { get; set; }
        [Networked] public Team Team { get; set; }
        [Networked] public short Champion { get; set; }
        [Networked] public short Summoner1 { get; set; }
        [Networked] public short Summoner2 { get; set; }
        [Networked] public NetworkBool Ready { get; set; }
        [Networked] public NetworkId ChampionUnit { get; set; }

        public bool IsLocal => Object != null && Object.HasInputAuthority;

        public override void Spawned() => MatchServices.Of(Runner)?.Match?.Register(this);

        public override void Despawned(NetworkRunner runner, bool hasState) => MatchServices.Of(runner)?.Match?.Unregister(this);

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RpcChoose(short champion, short summoner1, short summoner2, Team team, NetworkString<_32> playerName)
        {
            MatchController match = MatchServices.Of(Runner)?.Match;
            if (match == null || match.Phase != MatchPhase.Lobby) return;
            var content = MatchServices.Of(Runner).Content;
            if (champion >= 0 && champion < content.Champions.Count) Champion = champion;
            if (summoner1 >= 0 && summoner1 < content.SummonerSpells.Count) Summoner1 = summoner1;
            if (summoner2 >= 0 && summoner2 < content.SummonerSpells.Count && summoner2 != Summoner1) Summoner2 = summoner2;
            if ((team == Team.Blue || team == Team.Red) && match.HasRoom(team, this)) Team = team;
            if (playerName.Length > 0) PlayerName = playerName;
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RpcSetReady(NetworkBool ready) => Ready = ready;
    }
}
