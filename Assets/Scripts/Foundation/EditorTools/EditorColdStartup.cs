using Foundation.SceneManage.ScriptableObjects;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityTemplateProjects.Foundation.Event;

namespace Foundation.EditorTools
{
    public class EditorColdStartup : MonoBehaviour
    {
#if UNITY_EDITOR

        [SerializeField] private GameSceneSO _thisSceneSO = default;
        [SerializeField] private GameSceneSO _persistentManagersSO = default;
        [SerializeField] private AssetReference _notifyColdStartupChannel = default;
        [SerializeField] private VoidEventChannelSO _onSceneReadyChannel = default;

        private bool isColdStart = false;

        private void Awake()
        {
            // persistmanager 没加载，则加载
            if (!SceneManager.GetSceneByName(_persistentManagersSO.sceneReference.editorAsset.name).isLoaded)
            {
                isColdStart = true;
            }
        }

        private void Start()
        {
            if (isColdStart)
            {
                _persistentManagersSO.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true).Completed +=
                    LoadEventChannel;
            }
        }

        private void LoadEventChannel(AsyncOperationHandle<SceneInstance> obj)
        {
            // persistmanger scene 加载完成后，加载 coldStartup channel so
            _notifyColdStartupChannel.LoadAssetAsync<LoadEventChannelSO>().Completed += OnNotifyChannelLoaded;
        }

        private void OnNotifyChannelLoaded(AsyncOperationHandle<LoadEventChannelSO> obj)
        {
            // coldStartup channel加载成功后，raise Event，方便SceneLoader尝试加载level1
            if (_thisSceneSO != null)
            {
                obj.Result.RaiseEvent(_thisSceneSO);
            }
            else
            {
                //Raise a fake scene ready event, so the player is spawned
                // _onSceneReadyChannel.RaiseEvent();
                //When this happens, the player won't be able to move between scenes because the SceneLoader has no conception of which scene we are in
                Debug.LogWarning("_thisSceneSo is not specified ");
            }
        }
#endif
    }
}