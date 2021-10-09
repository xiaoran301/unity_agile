using UnityEngine.AddressableAssets;
using UnityTemplateProjects.Foundation.Base;

namespace Foundation.SceneManage.ScriptableObjects
{
	public class GameSceneSO : DescriptionBaseSO
	{
		public GameSceneType sceneType;
		public AssetReference sceneReference; //Used at runtime to load the scene from the right AssetBundle

		/// <summary>
		/// Used by the SceneSelector tool to discern what type of scene it needs to load
		/// </summary>
		public enum GameSceneType
		{
			//Playable scenes
			Level, //SceneSelector tool will also load PersistentManagers and Gameplay
			MainMenu, //SceneSelector tool will also load Gameplay

			//Special scenes
			Initialisation,
			PersistentManagers,
			LevelManagers,

			//Work in progress scenes that don't need to be played
			Art,
		}
	}

   
}