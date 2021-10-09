using UnityEngine;

namespace Foundation.EditorTools
{
    public class EditorColdStartup : MonoBehaviour
    {
        #if UNITY_EDITOR

	    [SerializeField] private GameSceneSO _thisSceneSO = default;
	    [SerializeField] private GameSceneSO _persistentManagersSO = default;
	    [SerializeField] private AssetReference _notifyColdStartupChannel = default;
	    [SerializeField] private VoidEventChannelSO _onSceneReadyChannel = default;
        #endif
    }
}