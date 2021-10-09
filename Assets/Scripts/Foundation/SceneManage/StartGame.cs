using Foundation.SceneManage.ScriptableObjects;
using UnityEngine;
using UnityTemplateProjects.Foundation.Event;

namespace Foundation.SceneManage
{
    public class StartGame : MonoBehaviour
    {
        [SerializeField] private GameSceneSO _loadLevelScene; // 开始游戏要加载的关卡

        [Header("Broadcasting on")] [SerializeField]
        private LoadEventChannelSO _loadLevelChannel; // 发送event

        
        [Header("Listening to")]
        [SerializeField] private VoidEventChannelSO _onNewGameButton = default;
        private void Start()
        {
            _onNewGameButton.OnEventRaised += StartNewGame;
        }

        private void OnDestroy()
        {
            _onNewGameButton.OnEventRaised -= StartNewGame;
        }

        void StartNewGame()
        {
            if (_loadLevelChannel && _loadLevelScene)
            {
                _loadLevelChannel.RaiseEvent(_loadLevelScene);
            }
            else
            {
                Debug.LogWarningFormat("未绑定要启动的level，不能开始游戏");
            }
        }
    }
}