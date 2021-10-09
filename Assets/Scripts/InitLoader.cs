using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityTemplateProjects.Foundation.Event;
using UnityTemplateProjects.Foundation.SceneManager;

namespace UnityTemplateProjects
{
    public class InitLoader : MonoBehaviour
    {
        [SerializeField] private GameSceneSO _managersScene = default;

        [SerializeField] private GameSceneSO _menuToLoad = default;

        [Header("Broadcasting on")] [SerializeField]
        private AssetReference _menuLoadChannel = default;

        private void Start()
        {
            //Load the persistent managers scene(包括加载所有chanel)
            _managersScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true).Completed += LoadEventChannel;
        }

        private void LoadEventChannel(AsyncOperationHandle<SceneInstance> obj)
        {
            // persistent 场景加载成功后，开始加载main menu SO Asset
            _menuLoadChannel.LoadAssetAsync<LoadEventChannelSO>().Completed += LoadMainMenu;
        }

        private void LoadMainMenu(AsyncOperationHandle<LoadEventChannelSO> obj)
        {
            obj.Result.RaiseEvent(_menuToLoad, true); // 发送event

            // 卸载initialization scene
            SceneManager.UnloadSceneAsync(0); //Initialization is the only scene in BuildSettings, thus it has index 0
        }
    }
}