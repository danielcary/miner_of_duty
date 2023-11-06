using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Miner_Of_Duty.LobbyCode
{
    public interface ILobby : IGameScreen
    {
        void LeaveLobby(int selected);
    }
}
