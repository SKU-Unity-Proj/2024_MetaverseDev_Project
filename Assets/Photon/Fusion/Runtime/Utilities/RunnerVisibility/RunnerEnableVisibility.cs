namespace Fusion
{
    using System;
    using System.Collections.Generic;
    using Sockets;
    using UnityEngine;

    /// <summary>
    ///   When running in Multi-Peer mode, this component automatically will register the associated
    ///   <see cref="NetworkRunner" /> with <see cref="NetworkRunnerVisibilityExtensions" />,
    ///   and will automatically attach loaded scene objects and spawned objects with the peers visibility handling.
    /// </summary>
    [ScriptHelp(BackColor = ScriptHeaderBackColor.Sand)] // 이 스크립트의 배경색을 설정합니다.
    [DisallowMultipleComponent] // 이 컴포넌트가 중복해서 추가되지 않도록 합니다.
    public class RunnerEnableVisibility : Behaviour, INetworkRunnerCallbacks
    {
        private NetworkRunner runner;
        public GameObject nickNameCanvas;

        private void Awake()
        {
            runner = GetComponentInParent<NetworkRunner>();
            if (runner)
            {
                // Optimistically register this as if we are running multi-peer (can't know yet)
                // 멀티 피어 모드로 실행 중인 것처럼 낙관적으로 등록합니다 (아직 알 수 없음)
                runner.EnableVisibilityExtension();

                // Just to be safe against double registration.
                // 중복 등록을 방지하기 위해 안전 조치합니다.
                runner.ObjectAcquired -= RunnerOnObjectAcquired;
                runner.ObjectAcquired += RunnerOnObjectAcquired;
            }
        }

        private void OnDestroy()
        {
            if (TryGetComponent<NetworkRunner>(out var runner))
            {
                // 시각화 확장을 비활성화합니다.
                runner.DisableVisibilityExtension();
                // 콜백을 제거합니다.
                runner.RemoveCallbacks(this);
                // 오브젝트 획득 이벤트를 제거합니다.
                runner.ObjectAcquired -= RunnerOnObjectAcquired;
            }
        }

        private void RunnerOnObjectAcquired(NetworkRunner runner, NetworkObject obj)
        {
            if (runner.IsRunning == false) return;
            if (runner.Config.PeerMode == NetworkProjectConfig.PeerModes.Single)
            {
                // 싱글 피어 모드이면 이 컴포넌트를 파괴합니다.
                Destroy(this);
                return;
            }

            // 오브젝트에 시각화 노드를 추가합니다.
            runner.AddVisibilityNodes(obj.gameObject);
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
            // 구현 내용 없음
        }

        void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
        {
            if (runner.IsRunning == false) return;
            if (runner.Config.PeerMode == NetworkProjectConfig.PeerModes.Single)
            {
                // 싱글 피어 모드이면 이 컴포넌트를 파괴합니다.
                Destroy(this);
                return;
            }

            var scene = runner.SimulationUnityScene;

            if (scene.IsValid())
                // 씬의 루트 게임 오브젝트에 시각화 노드를 추가합니다.
                foreach (var obj in scene.GetRootGameObjects())
                    runner.AddVisibilityNodes(obj);
        }
        /*
        public void SetPlayerNicknameAndSpawn(string nickname)
        {
            playerNickname = nickname;

            // 로컬 플레이어일 때만
            if (runner.LocalPlayer != PlayerRef.None)
            {
                // 플레이어 오브젝트에 닉네임 설정
            }
        }
        */
        #region Unused
        // 사용되지 않는 콜백 메서드들
        void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
        {
            nickNameCanvas.SetActive(true);
        }
        void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input) { }
        void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }
        void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) { }
        #endregion
    }
}
