
using Fusion;
using UnityEngine;

namespace _01_04_Network_Properties
{
    public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
    {
        public GameObject playerPrefab;
        
        /// <summary>
        /// 플레이어가 세션에 접속했을 때 호출되는 함수
        /// </summary>
        /// <param name="player">https://doc-api.photonengine.com/en/fusion/v2/struct_fusion_1_1_player_ref.html</param>
        public void PlayerJoined(PlayerRef player)
        {
            // 함수를 호출한 플레이어가 로컬 플레이어일 때
            if (player == Runner.LocalPlayer)
            {
                // 플레이어 게임오브젝트를 스폰한다
                Runner.Spawn(playerPrefab, new Vector3(0, 1, 0), Quaternion.identity);
            }
        }
    }
}


