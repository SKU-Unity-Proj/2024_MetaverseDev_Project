using Fusion;
using UnityEngine;

namespace _01_04_Network_Properties
{
    public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
    {
        public GameObject[] playerPrefabs; // 두 가지 캐릭터 프리팹을 담을 배열
        public GameObject npcPrefabs; // NPC 프리팹 배열

        public bool haveName = false;
        /// <summary>
        /// 플레이어가 세션에 접속했을 때 호출되는 함수
        /// </summary>
        /// <param name="player">https://doc-api.photonengine.com/en/fusion/v2/struct_fusion_1_1_player_ref.html</param>
        public void PlayerJoined(PlayerRef player)
        {
            // 함수를 호출한 플레이어가 로컬 플레이어일 때
            if (player == Runner.LocalPlayer)
            {
                // 랜덤으로 하나의 캐릭터 프리팹을 선택한다
                int randomIndex = Random.Range(0, playerPrefabs.Length);
                GameObject selectedPrefab = playerPrefabs[randomIndex];

                // 선택된 캐릭터 프리팹을 스폰한다
                Runner.Spawn(selectedPrefab, Vector3.zero, Quaternion.identity);

                Runner.Spawn(npcPrefabs, Vector3.zero, Quaternion.identity);
            }
        }
    }
}