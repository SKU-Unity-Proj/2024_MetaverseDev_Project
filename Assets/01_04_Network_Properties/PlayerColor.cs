
using Fusion;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _01_04_Network_Properties
{
    
    public class PlayerColor : NetworkBehaviour
    {
        public MeshRenderer MeshRenderer;

        /// <summary>
        /// 서버에 변수값을 동기화하기 위해 [Networked] 프로퍼티를 사용한다.
        /// StateAuthority가 true인 클라이언트의 변수를 StateAuthority가 false인 클라이언트에 동기화한다.
        /// [OnChangeRender] 프로퍼티로 Networked변수값이 바뀔 시 지정한 함수를 호출하게 한다.
        /// </summary>
        [Networked, OnChangedRender(nameof(OnColorChanged))]
        public Color NetworkedColor { get; set; }


        private void Awake()
        {
            MeshRenderer = GetComponent<MeshRenderer>();
        }

        void Update()
        {
            // 버튼을 누른 클라이언트에서만 실행되며 버튼을 누른 클라이언트에서 NetworkColor값을 변경한다.
            if (HasStateAuthority && Input.GetKeyDown(KeyCode.E))
            {
                NetworkedColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
            }
        }
        
        // NetworkColor값을 실제 메시 색으로 변경하는 함수
        void OnColorChanged()
        {
            MeshRenderer.material.color = NetworkedColor;
        }
    }
}
