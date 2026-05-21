using UnityEngine;
using Photon.Pun;

namespace Photon.VR.Player
{
    public class PlayerSpawner : MonoBehaviourPunCallbacks
    {
        [Tooltip("The location of the player prefab")]
        public string PrefabLocation = "PhotonVR/Player";
        
        private GameObject playerTemp;

        // --- PHẦN SỬA ĐỔI QUAN TRỌNG ---

        // 1. XÓA BỎ dòng DontDestroyOnLoad ở Awake
        // Spawner phải chết khi chuyển scene để Spawner mới làm việc
        private void Awake() 
        {
            // Để trống hoặc xóa hàm này đi cũng được
        }

        // 2. Thêm hàm Start
        // Lý do: Khi sang Scene Classroom, bạn "ĐÃ" ở trong phòng rồi (OnJoinedRoom không chạy nữa).
        // Hàm này kiểm tra: "Nếu tao đang ở trong phòng rồi thì đẻ nhân vật luôn đi".
        private void Start()
        {
            if (PhotonNetwork.InRoom)
            {
                SpawnPlayer();
            }
        }

        // 3. Giữ nguyên hàm này cho Scene Lobby (khi mới vào chưa có phòng)
        public override void OnJoinedRoom()
        {
            SpawnPlayer();
        }

        // Hàm sinh nhân vật (Tách ra để dùng chung)
        void SpawnPlayer()
        {
            // Kiểm tra kỹ để không sinh 2 lần
            if (playerTemp == null)
            {
                // Sinh ra nhân vật tại vị trí của Spawner này (transform.position)
                // Điều này giúp bạn đặt vị trí xuất hiện khác nhau ở Lobby và Classroom dễ dàng
                playerTemp = PhotonNetwork.Instantiate(PrefabLocation, transform.position, transform.rotation);
            }
        }

        // --------------------------------

        public override void OnLeftRoom()
        {
            // Hủy nhân vật khi thoát phòng (về menu chính hoặc thoát game)
            if (playerTemp != null)
            {
                PhotonNetwork.Destroy(playerTemp);
                playerTemp = null;
            }
        }
    }
}