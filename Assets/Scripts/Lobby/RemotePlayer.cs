using TMPro;
using UnityEngine;

namespace HelloServer
{
    public class RemotePlayer : MonoBehaviour
    {
        [Header("모양")]
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private TMP_Text label;

        [Header("따라가기")]
        // 받은 자리로 얼마나 빨리 다가갈지. 0 이면 그대로 놓는다.
        [SerializeField] private float smooth = 10f;

        public User User { get; private set; }

        private Vector2 target;

        public void Setup(User user)
        {
            User = user;
            label.text = $"{user.NickName} ({user.Id})";

            // 처음 나타날 때 원점에서 날아오지 않게 지금 자리를 목표로 둔다.
            target = transform.position;
        }
        
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
            target = position;
        }

        private void Update()
        {
            if (smooth <= 0f)
            {
                Place(target);
                return;
            }

            // Lerp 에 Time.deltaTime 을 그냥 넣으면 프레임이 빠른 기기에서 더 빨리 붙는다.
            // 이 꼴로 적으면 프레임과 상관없이 같은 속도가 된다.
            float t = 1f - Mathf.Exp(-smooth * Time.deltaTime);
            Place(Vector2.Lerp(transform.position, target, t));
        }

        // 오간 것은 x와 y. z는 건드리지 않음.
        // Vector2 를 position 에 그대로 넣으면 z가 말없이 0이 됨 조심.
        // 스프라이트를 z 로 겹쳐 놓았다면 그 순간 앞뒤가 뒤집히니 조심.
        private void Place(Vector2 next)
        {
            transform.position = new Vector3(next.x, next.y, transform.position.z);
        }
        
        public void SetSprite(Sprite newSprite)
        {
            if (body != null && newSprite != null)
            {
                body.sprite = newSprite;
            }
        }
        
    }
}
