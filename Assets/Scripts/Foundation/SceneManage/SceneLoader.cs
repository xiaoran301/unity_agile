using System.Collections;
using Foundation.SceneManage.ScriptableObjects;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityTemplateProjects.Foundation.Event;

namespace Foundation.SceneManage
{
    public class SceneLoader : MonoBehaviour
    {
	    [FormerlySerializedAs("_gameplayScene")] [SerializeField] private GameSceneSO _levelManagersScene = default;

	    
	    [Header("Listening to")] 
	    [FormerlySerializedAs("_loadLocation")]
	    [SerializeField] private LoadEventChannelSO _loadLevel = default;

	    [FormerlySerializedAs("_loadMenu")] [SerializeField] private LoadEventChannelSO _loadMainMenu = default;
	    [FormerlySerializedAs("_coldStartupLocation")] 
	    [SerializeField] private LoadEventChannelSO _coldStartupLevel = default; // 只在编辑器下使用，为了加快迭代开发测试，省去从init场景启动

	    [Header("Broadcasting on")] 
		[SerializeField] private VoidEventChannelSO _onSceneReady = default; //picked up by the SpawnSystem

	    private AsyncOperationHandle<SceneInstance> _loadingOperationHandle;
	    private AsyncOperationHandle<SceneInstance> _levelManagerLoadingOpHandle;

	    //Parameters coming from scene loading requests
	    private GameSceneSO _sceneToLoad;
	    private GameSceneSO _currentlyLoadedScene;
	    private bool _showLoadingScreen;

	    private SceneInstance _levelManagerSceneInstance = new SceneInstance();
	    private float _fadeDuration = .5f;
	    private bool _isLoading = false; //To prevent a new loading request while already loading a new scene

	    private void OnEnable()
	    {
		    _loadLevel.OnLoadingRequested += LoadLevel; // 提前注册event的回调
		    _loadMainMenu.OnLoadingRequested += LoadMainMenu;
#if UNITY_EDITOR
		    _coldStartupLevel.OnLoadingRequested += LocationColdStartup;
#endif
	    }

	    private void OnDisable()
	    {
		    _loadLevel.OnLoadingRequested -= LoadLevel;
		    _loadMainMenu.OnLoadingRequested -= LoadMainMenu;
#if UNITY_EDITOR
		    _coldStartupLevel.OnLoadingRequested -= LocationColdStartup;
#endif
	    }

#if UNITY_EDITOR
	    /// <summary>
	    /// This special loading function is only used in the editor, when the developer presses Play in a Location scene, without passing by Initialisation.
	    /// </summary>
	    private void LocationColdStartup(GameSceneSO currentlyOpenedLocation, bool showLoadingScreen, bool fadeScreen)
	    {
		    _currentlyLoadedScene = currentlyOpenedLocation;

		    if (_currentlyLoadedScene.sceneType == GameSceneSO.GameSceneType.Level)
		    {
			    //Gameplay managers is loaded synchronously
			    _levelManagerLoadingOpHandle =
				    _levelManagersScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true);
			    _levelManagerLoadingOpHandle.WaitForCompletion();
			    _levelManagerSceneInstance = _levelManagerLoadingOpHandle.Result;

			    StartGameplay();
		    }
	    }
#endif

	    private void LoadLevel(GameSceneSO locationToLoad, bool showLoadingScreen, bool fadeScreen)
	    {
		    //Prevent a double-loading, for situations where the player falls in two Exit colliders in one frame
		    if (_isLoading)
		    {
			   Debug.LogWarning("SceneLoader is busying，when load Main Menu"); 
			    return;
		    }


		    _sceneToLoad = locationToLoad;
		    _showLoadingScreen = showLoadingScreen;
		    _isLoading = true;

		    //In case we are coming from the main menu, we need to load the LevelManager scene first
		    if (_levelManagerSceneInstance.Scene == null
		        || !_levelManagerSceneInstance.Scene.isLoaded)
		    {
			    _levelManagerLoadingOpHandle =
				    _levelManagersScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true);
			    _levelManagerLoadingOpHandle.Completed += OnLevelManagersLoaded;
		    }
		    else
		    {
			    StartCoroutine(LoadNewLevelScene());
		    }
	    }

	    private void OnLevelManagersLoaded(AsyncOperationHandle<SceneInstance> obj)
	    {
		    _levelManagerSceneInstance = _levelManagerLoadingOpHandle.Result;

		    StartCoroutine(LoadNewLevelScene());
	    }

	    /// <summary>
	    /// Prepares to load the main menu scene, first removing the Gameplay scene in case the game is coming back from gameplay to menus.
	    /// </summary>
	    private void LoadMainMenu(GameSceneSO menuToLoad, bool showLoadingScreen, bool fadeScreen)
	    {
		    //Prevent a double-loading, for situations where the player falls in two Exit colliders in one frame
		    if (_isLoading)
		    {
			   Debug.LogWarning("SceneLoader is busying，when load Main Menu"); 
			    return;
		    }

		    _sceneToLoad = menuToLoad;
		    _showLoadingScreen = showLoadingScreen;
		    _isLoading = true;

		    //In case we are coming from a Location back to the main menu, we need to get rid of the persistent Gameplay manager scene
		    if (_levelManagerSceneInstance.Scene != null
		        && _levelManagerSceneInstance.Scene.isLoaded)
			    Addressables.UnloadSceneAsync(_levelManagerLoadingOpHandle, true);

		    StartCoroutine(LoadNewLevelScene());
	    }

	    /// <summary>
	    /// In both Location and Menu loading, this function takes care of removing previously loaded scenes.
	    /// </summary>
	    private IEnumerator LoadNewLevelScene()
	    {
			// 淡出
		    yield return new WaitForSeconds(_fadeDuration);

		    // 先卸载掉旧的level
		    if (_currentlyLoadedScene != null) //would be null if the game was started in Initialisation
		    {
			    if (_currentlyLoadedScene.sceneReference.OperationHandle.IsValid())
			    {
				    //Unload the scene through its AssetReference, i.e. through the Addressable system
				    _currentlyLoadedScene.sceneReference.UnLoadScene();
			    }
#if UNITY_EDITOR
			    else
			    {
				    //Only used when, after a "cold start", the player moves to a new scene
				    //Since the AsyncOperationHandle has not been used (the scene was already open in the editor),
				    //the scene needs to be unloaded using regular SceneManager instead of as an Addressable
				    UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(_currentlyLoadedScene.sceneReference.editorAsset.name);
			    }
#endif
		    }

		    // 加载新的level
		    LoadNewScene();
	    }

	    /// <summary>
	    /// Kicks off the asynchronous loading of a scene, either menu or Location.
	    /// </summary>
	    private void LoadNewScene()
	    {


		    _loadingOperationHandle = _sceneToLoad.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true, 0);
		    _loadingOperationHandle.Completed += OnNewSceneLoaded;
	    }

	    private void OnNewSceneLoaded(AsyncOperationHandle<SceneInstance> obj)
	    {
		    //Save loaded scenes (to be unloaded at next load request)
		    _currentlyLoadedScene = _sceneToLoad;

		    Scene s = obj.Result.Scene;
		    UnityEngine.SceneManagement.SceneManager.SetActiveScene(s);
		    LightProbes.TetrahedralizeAsync();

		    _isLoading = false;



		    StartGameplay();
	    }

	    private void StartGameplay()
	    {
		    _onSceneReady.RaiseEvent(); //Spawn system will spawn the PigChef in a gameplay scene
	    }

	    private void ExitGame()
	    {
		    Application.Quit();
		    Debug.Log("Exit!");
	    }
    }
}