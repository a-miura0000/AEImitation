/** 
 *	@file	AEImitationWindow.cs
 *	@brief	エディタウィンドウ制御
 *
 *	@author miura
 */
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using EventData;

namespace AEImitation
{
	/*----------------------------------------------------------------------------------------------------------*/
	/**
	 *	@class	Window
	 *  @brief  ウィンドウ表示クラス
	 */
	public partial class Window : EditorWindow
	{
		/*----------------------------------------------------------------------------------------------------------*/
		private struct Property
		{
			public int								headerPainHeight;
			public int								hierarchyPainWidth;
		}	// Property
		/*----------------------------------------------------------------------------------------------------------*/
		private interface IDataManager
		{
			int 									TotalFrames { get; }
			int										CurrentFrame { get; }
			EventFlags								Flags { get; set; }
			IReadOnlyList<Element>					ElementList { get; }
			IReadOnlyList<DataManager.Comment>		CommentList { get; }
		}	// IDataManager
		/*----------------------------------------------------------------------------------------------------------*/
		private abstract class PainBase
		{
			/*----------------------------------------------------------------------------------------------------------*/
			public delegate object MessageReceivedEventHandler(Message message, params object[] objs);
			/*----------------------------------------------------------------------------------------------------------*/
			private MessageReceivedEventHandler		m_messageReceiver;
			private VisualElement					m_rootElement;
			private System.Action					m_repaintAction;
			/*----------------------------------------------------------------------------------------------------------*/
			public VisualElement					RootElement { get => m_rootElement; }
			/*----------------------------------------------------------------------------------------------------------*/
			public PainBase()
			{
				m_repaintAction = null;
				EditorApplication.update += update;
			}
			/*----------------------------------------------------------------------------------------------------------*/
			~PainBase()
			{
				EditorApplication.update -= update;
			}
			/*----------------------------------------------------------------------------------------------------------*/
			public void SetMessageReceiver(MessageReceivedEventHandler messageReceiver)
			{
				m_messageReceiver = messageReceiver;
			}
			/*----------------------------------------------------------------------------------------------------------*/
			public void CreateVisualElement(IDataManager dataManager)
			{
				m_rootElement = createRootElement();
				createVisualElement(dataManager);
			}
			/*----------------------------------------------------------------------------------------------------------*/		
			public void RePaint(IDataManager dataManager)
			{
				m_repaintAction = () =>
					{
						repaint(dataManager);
					};
			}
			/*----------------------------------------------------------------------------------------------------------*/		
			public abstract void Play(IDataManager dataManager);
			/*----------------------------------------------------------------------------------------------------------*/
			protected object sendMessage(Message message, params object[] objs)
			{
				if(m_messageReceiver == null) return null;
				return m_messageReceiver(message, objs);
			}
			/*----------------------------------------------------------------------------------------------------------*/
			protected abstract VisualElement createRootElement();
			protected abstract void createVisualElement(IDataManager dataManager);
			protected abstract void repaint(IDataManager dataManager);
			/*----------------------------------------------------------------------------------------------------------*/
			private void update()
			{
				if(m_repaintAction != null)
				{
					m_repaintAction();
					m_repaintAction = null;
				}
			}
		}	// PainBase
		/*----------------------------------------------------------------------------------------------------------*/
		private abstract class DataPainBase : PainBase
		{
			/*----------------------------------------------------------------------------------------------------------*/
			private VisualElement			m_headerElement;
			private VisualElement			m_bodyElement;
			/*----------------------------------------------------------------------------------------------------------*/
			protected VisualElement			headerElement { get => m_headerElement; }
			protected VisualElement			bodyElement { get => m_bodyElement; }
			/*----------------------------------------------------------------------------------------------------------*/
			public abstract void AddElement(Element element);
			public abstract void SyncScroll(float scrollOffset);
			/*----------------------------------------------------------------------------------------------------------*/
			protected sealed override VisualElement createRootElement()
			{
				var element = new VisualElement();
				element.AddToClassList("data-pain");
				m_headerElement = createHeaderElement();
				m_headerElement.AddToClassList("data-pain-header");
				element.Add(m_headerElement);
				var body = new VisualElement();
				body.AddToClassList("data-pain-body");
				element.Add(body);
				
				m_bodyElement = createBodyElement();
				body.Add(m_bodyElement);
				
				return element;
			}
			/*----------------------------------------------------------------------------------------------------------*/
			protected sealed override void createVisualElement(IDataManager dataManager)
			{
				createHeaderVisualElement(dataManager);
				createBodyVisualElement(dataManager);
			}
			/*----------------------------------------------------------------------------------------------------------*/
			protected sealed override void repaint(IDataManager dataManager)
			{
				var scrollOffset = getScrollOffset();
				createHeaderVisualElement(dataManager);
				createBodyVisualElement(dataManager);
				setScrollOffset(scrollOffset);
			}
			/*----------------------------------------------------------------------------------------------------------*/
			protected virtual VisualElement createHeaderElement() { return new VisualElement(); }
			protected virtual void createHeaderVisualElement(IDataManager dataManager) { headerElement.Clear(); }
			/*----------------------------------------------------------------------------------------------------------*/
			protected abstract VisualElement createBodyElement();
			protected abstract void createBodyVisualElement(IDataManager dataManager);
			protected abstract Vector2 getScrollOffset();
			protected abstract void setScrollOffset(Vector2 scrollOffset);
		}	// DataPainBase
		/*----------------------------------------------------------------------------------------------------------*/
		private Property				m_property;
		private HeaderPain				m_headerPain;
		private HierarchyPain			m_hierarchyPain;
		private KeyFramePain			m_keyFramePain;
		private DataManager				m_dataManager;
		private StyleSheet				m_uss;
		private int						m_tmpFps;
		private float					m_deltaTime;
		private const string			cm_windowName = "AEImitation";
		private const string			cm_scenePath = "Assets/Scenes/AEImitation.unity"; 
		private const string			cm_playerName = "Player";
		private const string			cm_windowObjectName = "WindowPrefab";
		private const string			cm_choicesObjectName = "ChoicesPrefab";
		private const string			cm_ussPath = "Assets/Resources/Editor/AEImitation/AEImitationStyles.uss";
		private const string			cm_tmpDataPath = "Assets/tmpData.xml";
		private const string			cm_filePathSaveKey = "AEImitationFilePath";
		private const int				cm_fps = 60;
		/*----------------------------------------------------------------------------------------------------------*/
		[MenuItem("Tools/" + cm_windowName)]
		public static void Initialize()
		{
			var window = GetWindow<Window>();
			window.titleContent = new GUIContent(cm_windowName);
			window.position = new Rect(window.position.x, window.position.y, 1000, 500);	// todo
			
			if(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path != cm_scenePath)
			{
				UnityEditor.SceneManagement.EditorSceneManager.OpenScene(cm_scenePath);
			}
			
			EditorPrefs.DeleteKey(cm_filePathSaveKey);
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public Window()
		{
			// todo
			m_property.headerPainHeight = 100;
			m_property.hierarchyPainWidth = 730;
			
			m_headerPain = new HeaderPain();
			m_headerPain.SetMessageReceiver(onMessageReceived);
			m_hierarchyPain = new HierarchyPain();
			m_hierarchyPain.SetMessageReceiver(onMessageReceived);
			m_keyFramePain = new KeyFramePain();
			m_keyFramePain.SetMessageReceiver(onMessageReceived);
			m_dataManager = new DataManager();
			m_uss = null;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void OnEnable()
		{
			if(m_uss == null)
			{
				m_uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(cm_ussPath);
			}
			EditorApplication.delayCall += onSceneLoaded;
			EditorApplication.playModeStateChanged += onPlayModeStateChanged;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void OnDisable()
		{
			EditorApplication.delayCall -= onSceneLoaded;	
			EditorApplication.update -= onEditorUpdate;
			EditorApplication.playModeStateChanged -= onPlayModeStateChanged;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void CreateGUI()
		{
			var rootElement = this.rootVisualElement;
			rootElement.Clear();
			
			rootElement.styleSheets.Add(m_uss);
			m_headerPain.CreateVisualElement(m_dataManager);
			m_hierarchyPain.CreateVisualElement(m_dataManager);
			m_keyFramePain.CreateVisualElement(m_dataManager);
			
		
			var top = new TwoPaneSplitView(0, m_property.headerPainHeight, TwoPaneSplitViewOrientation.Vertical);
			rootElement.Add(top);
			top.Add(m_headerPain.RootElement);
			
			var canvas = new TwoPaneSplitView(1, m_property.hierarchyPainWidth, TwoPaneSplitViewOrientation.Horizontal);
			top.Add(canvas);
			
			canvas.Add(m_hierarchyPain.RootElement);
			canvas.Add(m_keyFramePain.RootElement);
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private void onSceneLoaded()
		{
			if(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path != cm_scenePath) return;
			
			var rootObject = GameObject.Find(cm_playerName);
			if(rootObject == null) 
			{
				rootObject = new GameObject(cm_playerName);
			}
			var windowObject = GameObject.Find(cm_windowObjectName);
			var choicesObject = GameObject.Find(cm_choicesObjectName);
			
			m_dataManager.SetGameObject(rootObject, windowObject, choicesObject);
			m_dataManager.Initialize();
			
			// LocalizeStringEventを使用しない場合は不要
			initializeLocalization();
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private void onEditorUpdate()
		{
			if(!EditorApplication.isPlaying) 
			{
				EditorApplication.update -= onEditorUpdate;
				return;
			}
			if(m_dataManager == null) return;
			
			m_deltaTime += (Time.unscaledDeltaTime - m_deltaTime) * 0.1f;
		//	Debug.Log(1f / m_deltaTime);
			
			m_headerPain.Play(m_dataManager);
			m_hierarchyPain.Play(m_dataManager);
			m_keyFramePain.Play(m_dataManager);
			
			if(!m_dataManager.IsPlaying()) 
			{
				EditorApplication.isPlaying = false;
				EditorApplication.update -= onEditorUpdate;
			}
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private void onPlayModeStateChanged(PlayModeStateChange state)
		{
			switch(state)
			{
				case PlayModeStateChange.EnteredPlayMode:
					EditorApplication.delayCall += onPlay;	
					break;
				case PlayModeStateChange.ExitingPlayMode:
					break;
				case PlayModeStateChange.ExitingEditMode:
					m_dataManager.WriteXml(cm_tmpDataPath);
					break;
				case PlayModeStateChange.EnteredEditMode:
					if(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path != cm_scenePath) break;
					
					Application.targetFrameRate = m_tmpFps;
					m_dataManager.SetGameObject(GameObject.Find(cm_playerName),
												GameObject.Find(cm_windowObjectName),
												GameObject.Find(cm_choicesObjectName));
					readFile(cm_tmpDataPath);
					try
					{
						File.Delete(cm_tmpDataPath);
					}
					catch(System.Exception ex)
					{
						Debug.LogError(ex.Message);
					}
					
					m_dataManager.ForceUpdate();
					break;
				default:
					break;
			}
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private void onPlay()
		{
			EditorApplication.delayCall -= onPlay;	
			
			m_tmpFps = Application.targetFrameRate;
			Application.targetFrameRate = cm_fps;
			
			readFile(cm_tmpDataPath);
			m_dataManager.Play();
			
			m_deltaTime = 0f;
			
			EditorApplication.update += onEditorUpdate;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private void readFile(string filePath)
		{
			m_dataManager.Initialize();
			m_dataManager.ReadXml(filePath);
			m_headerPain.RePaint(m_dataManager);
			m_hierarchyPain.RePaint(m_dataManager);
			m_keyFramePain.ScrollReset();
			m_keyFramePain.RePaint(m_dataManager);
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private async void initializeLocalization()
		{
			await LocalizationSettings.InitializationOperation.Task;
			
			if(LocalizationSettings.InitializationOperation.Status != 
				UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
			{
				Debug.LogError("Localization Initialization Failed");
				return;
			}
    
			var locales = LocalizationSettings.AvailableLocales.Locales;
			if((locales != null) && (locales.Count > 0))
			{
				LocalizationSettings.SelectedLocale = locales[0];
			}
			else
			{
				Debug.LogError("No available locales found.");
			}
		}
	}	// Window
}	// AEImitation
