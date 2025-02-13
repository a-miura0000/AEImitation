/** 
 *	@file	EventData.cs
 *	@brief	イベントデータ
 *
 *	@author miura
 */
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Tables;
using System.Collections.Generic;
using System.Linq;

namespace EventData 
{
	/*----------------------------------------------------------------------------------------------------------*/
	public enum ElementType
	{
		Image			= 0x0000,
		Panel			= 0x0001,
		Text			= 0x0100,
		Choices			= 0x0101,
		Sound			= 0x0200,
		SoundOneShot	= 0x0201,
	}	// ElementType
	/*----------------------------------------------------------------------------------------------------------*/
	public enum ParamType
	{
		Position = 0,
		Rotation,
		Scale,
		Color,
		Animator,
		TextLabel,
		Volume,
		Choices,
		Raycast,
	}	// ParamType
	/*----------------------------------------------------------------------------------------------------------*/
	public enum AssetType
	{
		Image = 0,
		Animator,
		SharedTableData,
		Audio,
	}	// AssetType
	/*----------------------------------------------------------------------------------------------------------*/
	public enum SettingType
	{
		Flags = 0,
		Loop,
	}	// SettingType
	/*----------------------------------------------------------------------------------------------------------*/
	[System.Flags]
	public enum EventFlags : uint
	{
		FlagNone = 0x00000000,
		
		Flag00 = 0x00000001,
		Flag01 = 0x00000002,
		Flag02 = 0x00000004,
		Flag03 = 0x00000008,
		Flag04 = 0x00000010,
		Flag05 = 0x00000020,
		Flag06 = 0x00000040,
		Flag07 = 0x00000080,
		Flag08 = 0x00000100,
		Flag09 = 0x00000200,
		
		FlagAll = 0xFFFFFFFF,
	}	// EventFlags
	/*----------------------------------------------------------------------------------------------------------*/
	/** 
	 *	@class	Setting
	 *  @brief  設定管理基底クラス
	 */
	public abstract class Setting
	{
		/*----------------------------------------------------------------------------------------------------------*/
		private SettingType					m_type;
		/*----------------------------------------------------------------------------------------------------------*/
		public SettingType					Type { get => m_type; }
		/*----------------------------------------------------------------------------------------------------------*/
		public Setting(SettingType type)
		{
			m_type = type;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public abstract void Apply(object value);
	}	// Setting
	/*----------------------------------------------------------------------------------------------------------*/
	public abstract class SettingBase<T> : Setting where T : struct
	{
		/*----------------------------------------------------------------------------------------------------------*/
		private T							m_value;
		/*----------------------------------------------------------------------------------------------------------*/
		public T							Value { get => m_value; set => m_value = value; }
		/*----------------------------------------------------------------------------------------------------------*/
		public SettingBase(T defaultValue, SettingType type) : base(type)
		{
			m_value = defaultValue;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public override void Apply(object value)
		{
			if(!(value is T)) return;
			m_value = (T)value;
		}
	}	// SettingBase<T>
	/*----------------------------------------------------------------------------------------------------------*/
	/** 
	 *	@class	FlagSetting
	 *  @brief  Flag設定クラス
	 */
	public class FlagSetting : SettingBase<EventFlags>
	{
		/*----------------------------------------------------------------------------------------------------------*/
		public FlagSetting() : base(EventFlags.FlagNone, SettingType.Flags)
		{
		}
	}	// FlagSetting
	/*----------------------------------------------------------------------------------------------------------*/
	/** 
	 *	@class	LoopSetting
	 *  @brief  Loop設定クラス
	 */
	public class LoopSetting : SettingBase<bool>
	{
		/*----------------------------------------------------------------------------------------------------------*/
		public LoopSetting() : base(false, SettingType.Loop)
		{
		}
	}	// LoopSetting
	/*----------------------------------------------------------------------------------------------------------*/
	/** 
	 *	@class	Asset
	 *  @brief  コンポーネント管理基底クラス
	 */
	public abstract class Asset
	{
		/*----------------------------------------------------------------------------------------------------------*/
		private AssetType					m_type;
		private string						m_path;
#if UNITY_EDITOR
		private UnityEngine.Object			m_object;
#endif
		/*----------------------------------------------------------------------------------------------------------*/
		public AssetType					Type { get => m_type; }
		public string						Path { get => m_path; set => m_path = value; }
#if UNITY_EDITOR
		public UnityEngine.Object			Object { get => m_object; set => m_object = value; }		
#endif
		/*----------------------------------------------------------------------------------------------------------*/
		public Asset(AssetType type)
		{
			m_type = type;
			m_path = null;
#if UNITY_EDITOR
			m_object = null;
#endif
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public abstract void Set(object data);
	}	// Asset
	/*----------------------------------------------------------------------------------------------------------*/
	/** 
	 *	@class	ImageAsset
	 *  @brief  Imageコンポーネント管理クラス
	 */
	public class ImageAsset : Asset
	{
		/*----------------------------------------------------------------------------------------------------------*/
		private string						m_name;
		/*----------------------------------------------------------------------------------------------------------*/
		public string						Name { get => m_name; set => m_name = value; }
		/*----------------------------------------------------------------------------------------------------------*/
		public ImageAsset() : base(AssetType.Image)
		{
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public override void Set(object data)
		{
			var sprite = data as Sprite;
			if(sprite == null) return;
			
			m_name = sprite.name;
#if UNITY_EDITOR
			Object = sprite;
#endif
		}
	}	// ImageAsset
	/*----------------------------------------------------------------------------------------------------------*/
	/** 
	 *	@class	AnimatorAsset
	 *  @brief  Animatorコンポーネント管理クラス
	 */
	public class AnimatorAsset : Asset
	{
		/*----------------------------------------------------------------------------------------------------------*/
		private string						m_selectCondition;
		public string						SelectCondition { get => m_selectCondition; set => m_selectCondition = value; }
#if UNITY_EDITOR
		private List<string>				m_conditionList;
		public IReadOnlyList<string>		ConditionList { get => m_conditionList.AsReadOnly(); }
#endif
		/*----------------------------------------------------------------------------------------------------------*/
		public AnimatorAsset() : base(AssetType.Animator)
		{
			m_selectCondition = "";
#if UNITY_EDITOR
			m_conditionList = new List<string>();
#endif
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public override void Set(object data)
		{
#if UNITY_EDITOR
			var controller = data as RuntimeAnimatorController;
			if(controller == null) return;
			
			Object = controller;
			
			m_conditionList = new List<string>();
			var animatorController = controller as UnityEditor.Animations.AnimatorController;
			if(animatorController == null) return;
			foreach(var layer in animatorController.layers)
			{
				foreach(var state in layer.stateMachine.states)
				{
					foreach(var transition in state.state.transitions)
					{
						foreach(var condition in transition.conditions)
						{
							m_conditionList.Add(condition.parameter);
						}
					}
				}
			}
			if(!string.IsNullOrEmpty(m_selectCondition))
			{
				if(m_conditionList.Find((p) => p == m_selectCondition) == null)
				{
					m_selectCondition = "";
				}
			}
			if(string.IsNullOrEmpty(m_selectCondition))
			{
				m_selectCondition = (m_conditionList.Count > 0) ? m_conditionList[0] : "";
			}
#endif	// UNITY_EDITOR
		}
	}	// AnimatorAsset
	/*----------------------------------------------------------------------------------------------------------*/
	/** 
	 *	@class	SharedTableDataAsset
	 *  @brief  SharedTableDataコンポーネント管理クラス
	 */
	public class SharedTableDataAsset : Asset
	{
#if UNITY_EDITOR
		private List<string>				m_keyList;
		public IReadOnlyList<string>		KeyList { get => m_keyList.AsReadOnly(); }
#endif
		/*----------------------------------------------------------------------------------------------------------*/
		public SharedTableDataAsset() : base(AssetType.SharedTableData)
		{
#if UNITY_EDITOR
			m_keyList = new List<string>();
#endif
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public override void Set(object data)
		{
#if UNITY_EDITOR
			var table = data as SharedTableData;
			if(table == null) return;
			
			Object = table;
			
			m_keyList.Clear();
			m_keyList.Add("None");
			foreach(var entry in table.Entries)
			{
				m_keyList.Add(entry.Key);
			}
#endif
		}
	}	// SharedTableDataAsset
	/*----------------------------------------------------------------------------------------------------------*/
	/** 
	 *	@class	AudioAsset
	 *  @brief  Audioコンポーネント管理クラス
	 */
	public class AudioAsset : Asset
	{
		/*----------------------------------------------------------------------------------------------------------*/
		public AudioAsset() : base(AssetType.Audio)
		{
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public override void Set(object data)
		{			
#if UNITY_EDITOR
			Object = (UnityEngine.Object)data;
#endif
		}
	}	// AudioAsset
	/*----------------------------------------------------------------------------------------------------------*/
	/** 
	 *	@class	KeyFrameBase
	 *  @brief  キーフレーム基底クラス
	 */
	public abstract class KeyFrameBase
	{
		/*----------------------------------------------------------------------------------------------------------*/
		private int							m_frame;
		public int							Frame { get => m_frame; }
		/*----------------------------------------------------------------------------------------------------------*/
		public KeyFrameBase(int frame)
		{
			m_frame = frame;
		}
#if UNITY_EDITOR
		/*----------------------------------------------------------------------------------------------------------*/
		public void MoveFrame(int moveFrame)
		{
			m_frame += moveFrame;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public abstract void SetValue(object value);
#endif	// UNITY_EDITOR
	}
	/*----------------------------------------------------------------------------------------------------------*/
	/** 
	 *	@class	KeyFrame
	 *  @brief  キーフレームクラス
	 */
	public class KeyFrame<T> : KeyFrameBase
	{
		/*----------------------------------------------------------------------------------------------------------*/
		public T							m_value;
		/*----------------------------------------------------------------------------------------------------------*/
		public T							Value { get => m_value; }
		/*----------------------------------------------------------------------------------------------------------*/
		public KeyFrame(T value, int frame) : base(frame)
		{
			m_value = value;
		}
#if UNITY_EDITOR
		/*----------------------------------------------------------------------------------------------------------*/
		public override void SetValue(object value)
		{
			if(!(value is T)) return;
			m_value = (T)value;
		}
#endif	// UNITY_EDITOR
	}	// KeyFrame<T>
	/*----------------------------------------------------------------------------------------------------------*/
	public abstract class Param
	{
		/*----------------------------------------------------------------------------------------------------------*/
		private ParamType					m_type;
		private string						m_name;
		private int							m_segment;
		private List<KeyFrameBase>			m_keyFrameList;
		/*----------------------------------------------------------------------------------------------------------*/
		public ParamType					Type { get => m_type; }
		public string						Name { get => m_name; }
		public int							Segment { get => m_segment; set => m_segment = value; }
		public IReadOnlyList<KeyFrameBase>	KeyFrameList { get => m_keyFrameList.AsReadOnly(); }
		/*----------------------------------------------------------------------------------------------------------*/
		public Param(ParamType type)
		{
			m_type = type;
			m_name = m_type.ToString();
			m_segment = -1;
			m_keyFrameList = new List<KeyFrameBase>();
		}
#if UNITY_EDITOR
		/*----------------------------------------------------------------------------------------------------------*/		
		public void SwitchKeyFrame(int frame)
		{
			var keyFrame = m_keyFrameList.Find((p) => p.Frame == frame);
			if(keyFrame == null)
			{
				AddKeyFrame(frame);
			}
			else 
			{
				m_keyFrameList.Remove(keyFrame);
			}
		}
		/*----------------------------------------------------------------------------------------------------------*/		
		public void MoveKeyFrame(int moveFrame)
		{
			foreach(var keyFrame in m_keyFrameList)
			{
				keyFrame.MoveFrame(moveFrame);
			}
		}
		/*----------------------------------------------------------------------------------------------------------*/		
		public bool MoveKeyFrame(int frame, int moveFrame)
		{
			var keyFrame = m_keyFrameList.Find((p) => p.Frame == frame);
			if(keyFrame == null) return false;
			int targetFrame = frame + moveFrame;
			if(m_keyFrameList.Find((p) => p.Frame == targetFrame) != null)
			{
				return false;
			}
			keyFrame.MoveFrame(moveFrame);
			m_keyFrameList.Remove(keyFrame);
			addKeyFrame(keyFrame);
			return true;
		}
		/*----------------------------------------------------------------------------------------------------------*/		
		public void AddKeyFrame(int frame)
		{
			var value = GetValue(frame);
			addKeyFrame(createKeyFrame(frame));
			SetValue(frame, value);
		}
		/*----------------------------------------------------------------------------------------------------------*/		
		public void DeleteKeyFrame(KeyFrameBase keyFrame)
		{
			m_keyFrameList.Remove(keyFrame);
		}
		/*----------------------------------------------------------------------------------------------------------*/	
		public void SetKeyFrameList(List<KeyFrameBase> keyFrameList)
		{
			m_keyFrameList = keyFrameList;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void InsertFrame(int baseFrame, int frames)
		{
			foreach(var keyFrame in m_keyFrameList)
			{
				if(keyFrame.Frame < baseFrame) continue;
				keyFrame.MoveFrame(frames);
			}
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void DeleteFrame(int baseFrame, int frames)
		{
			if(frames <= 0) return;
			
			int count = m_keyFrameList.Count;
			for(int i = count - 1; i >= 0; --i)
			{
				if(m_keyFrameList[i].Frame < baseFrame) break;
				if(m_keyFrameList[i].Frame <= (baseFrame + frames - 1))
				{
					m_keyFrameList.Remove(m_keyFrameList[i]);
					continue;
				}
				m_keyFrameList[i].MoveFrame(-frames);
			}
		}
#endif	// UNITY_EDITOR
		/*----------------------------------------------------------------------------------------------------------*/		
		public object GetValue(int frame)
		{
			KeyFrameBase current = null;
			KeyFrameBase next = null;
			for(int i = 0; i < m_keyFrameList.Count; ++i) 
			{
				if(m_keyFrameList[i].Frame > frame) 
				{
					next = m_keyFrameList[i];
					break;
				}
				current = m_keyFrameList[i];
			}
			return getValue(current, next, frame);
		}
		/*----------------------------------------------------------------------------------------------------------*/	
		/**
		 *	@brief キーフレーム値設定
		 *	
		 *	@param frame				対象フレーム
		 *	@param value				値
		 *	@return						true: 新規キーフレーム追加
		 */		
		public bool SetValue(int frame, object value)
		{
			bool isAdd = false;
			var keyFrame = m_keyFrameList.Find((p) => p.Frame == frame);
			if(keyFrame == null)
			{
				keyFrame = createKeyFrame(frame);
				addKeyFrame(keyFrame);
				isAdd = true;
			}
			keyFrame.SetValue(value);
			
			return isAdd;
		}
		/*----------------------------------------------------------------------------------------------------------*/		
		protected abstract KeyFrameBase createKeyFrame(int frame);
		protected abstract object getValue(KeyFrameBase currentKeyFrame, KeyFrameBase nextKeyFrame, int frame);
		/*----------------------------------------------------------------------------------------------------------*/
		/**
		 *	@brief フレーム昇順でリストに登録
		 *	
		 *	@param keyFrame				対象キーフレーム
		 */	
		private void addKeyFrame(KeyFrameBase keyFrame)
		{
			int insertIndex = ~m_keyFrameList.BinarySearch(keyFrame, 
				Comparer<KeyFrameBase>.Create((a, b) => a.Frame.CompareTo(b.Frame)));
			
			if(insertIndex >= m_keyFrameList.Count)
			{
				m_keyFrameList.Add(keyFrame);
			}
			else 
			{
				m_keyFrameList.Insert(insertIndex, keyFrame);
			}
		}
	}	// Param
	/*----------------------------------------------------------------------------------------------------------*/
	public abstract class ParamBase<T> : Param where T : struct
	{
		/*----------------------------------------------------------------------------------------------------------*/		
		public ParamBase(ParamType type) : base(type)
		{
		}
		/*----------------------------------------------------------------------------------------------------------*/		
		protected sealed override KeyFrameBase createKeyFrame(int frame)
		{
			return new KeyFrame<T>(getDefaultValue(), frame);
		}
		/*----------------------------------------------------------------------------------------------------------*/		
		protected sealed override object getValue(KeyFrameBase currentKeyFrame, KeyFrameBase nextKeyFrame, int frame)
		{
			if(currentKeyFrame == null) return getDefaultValue();
			var current = (KeyFrame<T>)currentKeyFrame;
			if(nextKeyFrame == null) return current.Value;
			var next = (KeyFrame<T>)nextKeyFrame;
			
			float t = (float)(frame - current.Frame) / (float)(next.Frame - current.Frame);
			return getValue(current.Value, next.Value, t);
		}
		/*----------------------------------------------------------------------------------------------------------*/	
		protected virtual T getDefaultValue() { return default(T); }
		/*----------------------------------------------------------------------------------------------------------*/		
		protected abstract object getValue(T currentValue, T nextValue, float t);
	}	// ParamBase
	/*----------------------------------------------------------------------------------------------------------*/
	public class Vector2Param : ParamBase<Vector2>
	{
		/*----------------------------------------------------------------------------------------------------------*/		
		public Vector2Param(ParamType type) : base(type)
		{
		}
		/*----------------------------------------------------------------------------------------------------------*/		
		protected override object getValue(Vector2 currentValue, Vector2 nextValue, float t)
		{
			return (nextValue - currentValue) * t + currentValue;			
		}
	}	// Vector2Param
	/*----------------------------------------------------------------------------------------------------------*/
	public class IntParam : ParamBase<int>
	{
		/*----------------------------------------------------------------------------------------------------------*/		
		public IntParam(ParamType type) : base(type)
		{
		}
		/*----------------------------------------------------------------------------------------------------------*/		
		protected override object getValue(int currentValue, int nextValue, float t)
		{
			return (int)((nextValue - currentValue) * t + 0.5f) + currentValue;
		}
	}	// IntParam
	/*----------------------------------------------------------------------------------------------------------*/
	public class FloatParam : ParamBase<float>
	{
		/*----------------------------------------------------------------------------------------------------------*/		
		public FloatParam(ParamType type) : base(type)
		{
		}
		/*----------------------------------------------------------------------------------------------------------*/		
		protected override object getValue(float currentValue, float nextValue, float t)
		{
			return (nextValue - currentValue) * t + currentValue;
		}
	}	// FloatParam
	/*----------------------------------------------------------------------------------------------------------*/
	public class ScaleParam : Vector2Param
	{
		/*----------------------------------------------------------------------------------------------------------*/		
		public ScaleParam() : base(ParamType.Scale)
		{
		}
		/*----------------------------------------------------------------------------------------------------------*/	
		protected override Vector2 getDefaultValue() 
		{
			return Vector2.one; 
		}
	}	// ScaleParam
	/*----------------------------------------------------------------------------------------------------------*/
	public class ColorParam : ParamBase<Color>
	{
		/*----------------------------------------------------------------------------------------------------------*/		
		public ColorParam() : base(ParamType.Color)
		{
		}
		/*----------------------------------------------------------------------------------------------------------*/	
		protected override Color getDefaultValue() 
		{
			return Color.white; 
		}
		/*----------------------------------------------------------------------------------------------------------*/		
		protected override object getValue(Color currentValue, Color nextValue, float t)
		{
			return new Color(
						nextValue.r - currentValue.r,
						nextValue.g - currentValue.g,
						nextValue.b - currentValue.b,
						nextValue.a - currentValue.a 
					) * t + currentValue;
		}
	}	// ColorParam
	/*----------------------------------------------------------------------------------------------------------*/
	public class BoolParam : Param
	{
		/*----------------------------------------------------------------------------------------------------------*/		
		public BoolParam(ParamType type) : base(type)
		{
		}
		/*----------------------------------------------------------------------------------------------------------*/		
		protected override KeyFrameBase createKeyFrame(int frame)
		{
			return new KeyFrame<bool>(false, frame);
		}
		/*----------------------------------------------------------------------------------------------------------*/		
		protected override object getValue(KeyFrameBase currentKeyFrame, KeyFrameBase nextKeyFrame, int frame)
		{
			return (currentKeyFrame != null) ? ((KeyFrame<bool>)currentKeyFrame).Value : false;
		}
	}	// BoolParam
	/*----------------------------------------------------------------------------------------------------------*/
	public class StringParam : Param
	{
		/*----------------------------------------------------------------------------------------------------------*/		
		public StringParam(ParamType type) : base(type)
		{
		}
		/*----------------------------------------------------------------------------------------------------------*/		
		protected override KeyFrameBase createKeyFrame(int frame)
		{
			return new KeyFrame<string>("", frame);
		}
		/*----------------------------------------------------------------------------------------------------------*/		
		protected override object getValue(KeyFrameBase currentKeyFrame, KeyFrameBase nextKeyFrame, int frame)
		{
			return (currentKeyFrame != null) ? ((KeyFrame<string>)currentKeyFrame).Value : "";
		}
	}	// StringParam
	/*----------------------------------------------------------------------------------------------------------*/
	public class AnimatorParam : IntParam
	{
		/*----------------------------------------------------------------------------------------------------------*/		
		public AnimatorParam() : base(ParamType.Animator)
		{
		}
		/*----------------------------------------------------------------------------------------------------------*/	
		protected override int getDefaultValue() 
		{
			return -1; 
		}
		/*----------------------------------------------------------------------------------------------------------*/		
		protected override object getValue(int currentValue, int nextValue, float t)
		{
			return currentValue;
		}
	}	// AnimatorParam
	/*----------------------------------------------------------------------------------------------------------*/
	public class LabelParam : StringParam
	{
		/*----------------------------------------------------------------------------------------------------------*/		
		public LabelParam() : base(ParamType.TextLabel)
		{
		}
		/*----------------------------------------------------------------------------------------------------------*/		
		protected override object getValue(KeyFrameBase currentKeyFrame, KeyFrameBase nextKeyFrame, int frame)
		{
			if(currentKeyFrame == null) return "";
			return (currentKeyFrame.Frame == frame) ? ((KeyFrame<string>)currentKeyFrame).Value : "";
		}
	}	// LabelParam
	/*----------------------------------------------------------------------------------------------------------*/
	public class VolumeParam : FloatParam
	{
		/*----------------------------------------------------------------------------------------------------------*/		
		public VolumeParam() : base(ParamType.Volume)
		{
		}
		/*----------------------------------------------------------------------------------------------------------*/	
		protected override float getDefaultValue() 
		{
			return 1f; 
		}
	}	// VolumeParam
	/*----------------------------------------------------------------------------------------------------------*/
	public class ChoicesParam : Param
	{
		/*----------------------------------------------------------------------------------------------------------*/		
		public struct Data 
		{
			public string					label;
			public EventFlags				flags;
			
			public Data(string _label, EventFlags _flags)
			{
				label = _label;
				flags = _flags;
			}
			
			private static readonly Data	s_empty = new Data("", EventFlags.FlagNone);
			public static Data				Empty { get => s_empty; }
		}	// Data
		/*----------------------------------------------------------------------------------------------------------*/		
		public ChoicesParam() : base(ParamType.Choices)
		{
		}
		/*----------------------------------------------------------------------------------------------------------*/		
		protected override KeyFrameBase createKeyFrame(int frame)
		{
			return new KeyFrame<Data>(Data.Empty, frame);
		}
		/*----------------------------------------------------------------------------------------------------------*/		
		protected override object getValue(KeyFrameBase currentKeyFrame, KeyFrameBase nextKeyFrame, int frame)
		{
			if(currentKeyFrame == null) return Data.Empty;
			if(currentKeyFrame.Frame == frame) return ((KeyFrame<Data>)currentKeyFrame).Value;
			return Data.Empty;
		}
	}	// ChoicesParam
	/*----------------------------------------------------------------------------------------------------------*/
	public abstract class Element
	{
		/*----------------------------------------------------------------------------------------------------------*/
		/** 
		 *	@struct	FrameData
		 *  @brief  1フレーム分の表示データを格納する構造体
		 */
		public struct FrameData
		{
			public bool						isActive;
			public object					elementData;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private ElementType					m_type;
		private string						m_name;
		private int							m_startFrame;
		private int							m_endFrame;
		private List<Param>					m_paramList;
		private List<Asset>					m_assetList;
		private List<Setting>				m_settingList;
#if UNITY_EDITOR		
		private bool 						m_isExpand;
#endif	// UNITY_EDITOR	
		/*----------------------------------------------------------------------------------------------------------*/
		public ElementType					Type { get => m_type; }
		public string						Name { get => m_name; set => m_name = value; }
		public int							StartFrame { get => m_startFrame; set => m_startFrame = value; }
		public virtual int					EndFrame { get => m_endFrame; set => m_endFrame = value; }
		public IReadOnlyList<Param>			ParamList { get => m_paramList.AsReadOnly(); }
		public IReadOnlyList<Asset>			AssetList { get => m_assetList.AsReadOnly(); }
		public IReadOnlyList<Setting>		SettingList { get => m_settingList.AsReadOnly(); }
		public EventFlags					Flags { get => ((FlagSetting)m_settingList[0]).Value; }
#if UNITY_EDITOR	
		public bool							IsExpand { get => m_isExpand; set => m_isExpand = value; }
#endif	// UNITY_EDITOR	
		/*----------------------------------------------------------------------------------------------------------*/
		public Element(ElementType type)
		{
			m_type = type;
			m_name = type.ToString();
			m_startFrame = 0;
			m_endFrame = 1;
			m_paramList = new List<Param>();
			m_assetList = new List<Asset>();
			m_settingList = new List<Setting>();
#if UNITY_EDITOR
			m_isExpand = true;
#endif

			addSetting(new FlagSetting());
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void SetParam(Param param)
		{
			var type = param.Type;
			var segment = param.Segment;
			int count = m_paramList.Count;
			for(int i = 0; i < count; ++i)
			{
				if(m_paramList[i].Type != type) continue;
				if(m_paramList[i].Segment != segment) continue;
				m_paramList[i] = param;
				break;
			}
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void SetAsset(AssetType assetType, object data)
		{
			var asset = m_assetList.Find((p) => p.Type == assetType);
			if(asset == null) return;
			asset.Set(data);
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void SetAsset(Asset asset)
		{
			var type = asset.Type;
			int count = m_assetList.Count;
			for(int i = 0; i < count; ++i)
			{
				if(m_assetList[i].Type != type) continue;
				m_assetList[i] = asset;
				break;
			}
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void SetSetting(Setting setting)
		{
			var type = setting.Type;
			int count = m_settingList.Count;
			for(int i = 0; i < count; ++i)
			{
				if(m_settingList[i].Type != type) continue;
				m_settingList[i] = setting;
				break;
			}
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public FrameData GetFrameData(int frame)
		{
			FrameData data;
			data.isActive = ((frame >= StartFrame) && (frame < EndFrame)) ? true : false;
			data.elementData = getFrameData(frame);
			
			return data;
		}
#if UNITY_EDITOR
		/*----------------------------------------------------------------------------------------------------------*/
		public void Move(int moveFrame)
		{
			m_startFrame += moveFrame;
			m_endFrame += moveFrame;
			
			foreach(var param in m_paramList) 
			{
				param.MoveKeyFrame(moveFrame);
			}
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void SetAssetPath(AssetType assetType, string path)
		{
			var asset = m_assetList.Find((p) => p.Type == assetType);
			if(asset == null) return;
			asset.Path = path;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void InsertFrame(int baseFrame, int frames)
		{
			if(m_startFrame >= baseFrame) m_startFrame += frames;
			if(m_endFrame > baseFrame) m_endFrame += frames;
			foreach(var param in m_paramList)
			{
				param.InsertFrame(baseFrame, frames);
			}
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void DeleteFrame(int baseFrame, int frames)
		{
			if(frames <= 0) return;
			
			if(m_startFrame >= baseFrame)
			{
				if(m_startFrame >= (baseFrame + frames))
				{
					m_startFrame -= frames;
				}
				else
				{
					m_startFrame = baseFrame;
				}
			}
			if(m_endFrame > baseFrame)
			{
				if(m_endFrame >= (baseFrame + frames))
				{
					m_endFrame -= frames;
				}
				else
				{
					m_endFrame = baseFrame;
				}
			}
			foreach(var param in m_paramList)
			{
				param.DeleteFrame(baseFrame, frames);
			}
		}
#endif	// UNITY_EDITOR
		/*----------------------------------------------------------------------------------------------------------*/
		protected void addParam(Param param)
		{
			var list = m_paramList.Where((p) => p.Type == param.Type).ToList();
			for(int i = 0; i < list.Count; ++i)
			{
				list[i].Segment = i;
			}
			m_paramList.Add(param);
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected void addAsset(Asset asset)
		{
			m_assetList.Add(asset);
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected void addSetting(Setting setting)
		{
			m_settingList.Add(setting);
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected abstract object getFrameData(int frame);
	}	// Element
	/*----------------------------------------------------------------------------------------------------------*/
	public class ImageElement : Element
	{
		/*----------------------------------------------------------------------------------------------------------*/
		public struct Data
		{
			public Vector2					position;
			public float					rotation;
			public Vector2					scale;
			public Color					color;
			public int						transitionNo;
			public bool						raycastTarget;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private Image						m_image;
		private Animator					m_animator;
		/*----------------------------------------------------------------------------------------------------------*/
		public ImageElement() : base(ElementType.Image)
		{
			addParam(new Vector2Param(ParamType.Position));
			addParam(new FloatParam(ParamType.Rotation));
			addParam(new ScaleParam());
			addParam(new ColorParam());
			addParam(new AnimatorParam());
			addParam(new BoolParam(ParamType.Raycast));
			
			addAsset(new ImageAsset());
			addAsset(new AnimatorAsset());
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected override object getFrameData(int frame)
		{
			Data data;
			var list = ParamList;
			data.position = (Vector2)list[0].GetValue(frame);
			data.rotation = (float)list[1].GetValue(frame);
			data.scale = (Vector2)list[2].GetValue(frame);
			data.color = (Color)list[3].GetValue(frame);
			data.transitionNo = (int)list[4].GetValue(frame);
			data.raycastTarget = (bool)list[5].GetValue(frame);
			
			return data;
		}
	}	// ImageElement
	/*----------------------------------------------------------------------------------------------------------*/
	public class PanelElement : Element
	{
		/*----------------------------------------------------------------------------------------------------------*/
		public struct Data
		{
			public Vector2					position;
			public float					rotation;
			public Vector2					scale;
			public Color					color;
			public bool						raycastTarget;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public PanelElement() : base(ElementType.Panel)
		{
			addParam(new Vector2Param(ParamType.Position));
			addParam(new FloatParam(ParamType.Rotation));
			addParam(new ScaleParam());
			addParam(new ColorParam());
			addParam(new BoolParam(ParamType.Raycast));
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected override object getFrameData(int frame)
		{
			Data data;
			var list = ParamList;
			data.position = (Vector2)list[0].GetValue(frame);
			data.rotation = (float)list[1].GetValue(frame);
			data.scale = (Vector2)list[2].GetValue(frame);
			data.color = (Color)list[3].GetValue(frame);
			data.raycastTarget = (bool)list[4].GetValue(frame);
			
			return data;
		}
	}	// PanelElement
	/*----------------------------------------------------------------------------------------------------------*/
	public class TextElement : Element
	{
		/*----------------------------------------------------------------------------------------------------------*/
		public struct Data
		{
			public string					label;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public TextElement() : base(ElementType.Text)
		{
			addParam(new LabelParam());
			
			addAsset(new SharedTableDataAsset());
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected override object getFrameData(int frame)
		{
			Data data;
			data.label = (string)ParamList[0].GetValue(frame);
			return data;
		}
	}	// TextElement
	/*----------------------------------------------------------------------------------------------------------*/
	public class ChoicesElement : Element
	{
		/*----------------------------------------------------------------------------------------------------------*/
		public struct Data
		{
			public List<ChoicesParam.Data>	choiceList;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public ChoicesElement() : base(ElementType.Choices)
		{
			addParam(new ChoicesParam());
			addParam(new ChoicesParam());
			
			addAsset(new SharedTableDataAsset());
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected override object getFrameData(int frame)
		{
			Data data;
			data.choiceList = new List<ChoicesParam.Data>();
			data.choiceList.Add((ChoicesParam.Data)ParamList[0].GetValue(frame));
			data.choiceList.Add((ChoicesParam.Data)ParamList[1].GetValue(frame));
			return data;
		}
	}	// ChoicesElement
	/*----------------------------------------------------------------------------------------------------------*/
	public class SoundElement : Element
	{
		/*----------------------------------------------------------------------------------------------------------*/
		public struct Data
		{
			public float					volume;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public SoundElement() : base(ElementType.Sound)
		{
			addParam(new VolumeParam());
			addAsset(new AudioAsset());
			addSetting(new LoopSetting());
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public bool IsLoop()
		{
			return ((LoopSetting)SettingList[1]).Value;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected override object getFrameData(int frame)
		{
			Data data;
			data.volume = (float)ParamList[0].GetValue(frame);
			return data;
		}
	}	// SoundElement
	/*----------------------------------------------------------------------------------------------------------*/
	public class SoundOneShotElement : Element
	{
		/*----------------------------------------------------------------------------------------------------------*/
		public override int					EndFrame { get => StartFrame + 1; set => base.EndFrame = value; }
		/*----------------------------------------------------------------------------------------------------------*/
		public SoundOneShotElement() : base(ElementType.SoundOneShot)
		{
			addAsset(new AudioAsset());
		}
		/*----------------------------------------------------------------------------------------------------------*/
		protected override object getFrameData(int frame)
		{
			return null;
		}
	}	// SoundOneShotElement
	/*----------------------------------------------------------------------------------------------------------*/	
	public class PlayData
	{
		/*----------------------------------------------------------------------------------------------------------*/
		private int							m_totalFrames;
		private List<Element>				m_elementList;
		/*----------------------------------------------------------------------------------------------------------*/
		public int 							TotalFrames 
											{ 
												get => m_totalFrames; 
#if UNITY_EDITOR
												set => m_totalFrames = value; 
#endif
											}
		public IReadOnlyList<Element>		ElementList { get => m_elementList.AsReadOnly(); }
		/*----------------------------------------------------------------------------------------------------------*/
		public PlayData()
		{
			m_totalFrames = 600;
			m_elementList = new List<Element>();
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public Element AddElement(ElementType elementType)
		{
			Element element = null;
			switch(elementType) 
			{
				case ElementType.Image:
					element = new ImageElement();
					break;
				case ElementType.Panel:
					element = new PanelElement();
					break;
				case ElementType.Text:
					element = new TextElement();
					break;
				case ElementType.Choices:
					element = new ChoicesElement();
					break;
				case ElementType.Sound:
					element = new SoundElement();
					break;
				case ElementType.SoundOneShot:
					element = new SoundOneShotElement();
					break;
				default:
					break;					
			}
			
			m_elementList.Add(element);
			return element;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void AddElement(Element element)
		{
			m_elementList.Add(element);
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void DeleteElement(Element element)
		{
			m_elementList.Remove(element);
		}
#if UNITY_EDITOR
		/*----------------------------------------------------------------------------------------------------------*/
		public void MoveElementPriority(int sourceIndex, int targetIndex)
		{
			if(sourceIndex >= m_elementList.Count) return;
			var element = m_elementList[sourceIndex];
			m_elementList.Remove(element);
			if(targetIndex > sourceIndex) --targetIndex;
			m_elementList.Insert(targetIndex, element);
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void InsertFrame(int baseFrame, int frames)
		{
			m_totalFrames += frames;
			foreach(var element in m_elementList)
			{
				element.InsertFrame(baseFrame, frames);
			}
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void DeleteFrame(int baseFrame, int frames)
		{
			if(frames <= 0) return;
			if(frames >= m_totalFrames) return;
			
			m_totalFrames -= frames;
			foreach(var element in m_elementList)
			{
				element.DeleteFrame(baseFrame, frames);
			}
		}
#endif
	}	// PlayData
}
