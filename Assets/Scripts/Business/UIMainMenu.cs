using UnityEngine;
using UnityTemplateProjects.Foundation.Event;

namespace Business
{
    public class UIMainMenu : MonoBehaviour
    {
        [Header("Broadcasting on ")]
        [SerializeField] private VoidEventChannelSO _onNewGameButton = default;
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void OnClickNewGameBtn()
        {
            if (_onNewGameButton)
            {
                _onNewGameButton.RaiseEvent();
            }
        }
    }
}
