using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle.HexCapturing
{
    public static class PrepareHexCaptureSituationLogic
    {
        public static HexCaptureSituation Execute(int2 hexCoord, int playersCount, IReadOnlyDictionary<HexDataKey, PlayerCapturePowers> powersData)
        {
            var situation = new HexCaptureSituation()
            {
                MaxPowerfulPlayerId = -1,
                SecondPowerfulPlayerId = -1,
                MaxPowerfulPlayerPowers = default,
                SecondPlayerPowers = default,
            };

            for (var i = 0; i < playersCount; i++)
            {
                if (!powersData.TryGetValue(new(hexCoord, i), out var playerPowers))
                    continue;


                var isNewMostPowerfulPlayer = (playerPowers.ActiveCapturePower > situation.MaxPowerfulPlayerPowers.ActiveCapturePower)

                    // if all participants are passive, select with most resistive power
                    || (situation.MaxPowerfulPlayerPowers.ActiveCapturePower == 0f && playerPowers.ResistPower != 0);

                if (isNewMostPowerfulPlayer)
                {
                    situation.SecondPlayerPowers = situation.MaxPowerfulPlayerPowers;
                    situation.SecondPowerfulPlayerId = situation.MaxPowerfulPlayerId;

                    situation.MaxPowerfulPlayerId = i;
                    situation.MaxPowerfulPlayerPowers = playerPowers;
                }
                else
                {
                    if (playerPowers.ActiveCapturePower > situation.SecondPlayerPowers.ActiveCapturePower 
                        || (situation.SecondPlayerPowers.ActiveCapturePower == 0f && playerPowers.ResistPower !=0))
                    {
                        situation.SecondPowerfulPlayerId = i;
                        situation.SecondPlayerPowers = playerPowers;
                    }
                }
            }

            return situation;
        }
    }
}
