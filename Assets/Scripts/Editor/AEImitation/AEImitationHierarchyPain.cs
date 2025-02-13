/** 
 *	@file	AEImitationHierarchyPain.cs
 *	@brief	ヒエラルキー表示制御
 *
 *	@author miura
 */
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization.Tables;
using UnityEditor;
using UnityEditor.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using EventData;

namespace AEImitation
{
	/*----------------------------------------------------------------------------------------------------------*/
	/** @class	Window
	 *  @brief  ウィンドウ表示クラス
	 */
	public partial class Window : EditorWindow
	{
		/*----------------------------------------------------------------------------------------------------------*/
		private class HierarchyPain : DataPainBase
		{
			/*----------------------------------------------------------------------------------------------------------*/
			private abstract class Item
			{
				public abstract VisualElement GetElement();
			}	// Item
			/*----------------------------------------------------------------------------------------------------------*/
			private class ParentItem : Item
			{
				/*----------------------------------------------------------------------------------------------------------*/
				private Element									m_element;
				private int										m_id;
				private Action<string>							m_callback;
				/*----------------------------------------------------------------------------------------------------------*/
				public Element									Data { get => m_element; }
				public int										Id { get => m_id; }
				/*----------------------------------------------------------------------------------------------------------*/
				public ParentItem(Element element, int id, Action<string> callback)
				{
					m_element = element;
					m_id = id;
					m_callback = callback;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override VisualElement GetElement()
				{
					var textField = new TextField();
					textField.AddToClassList("element-name-field");
					textField.multiline = false;
					textField.value = m_element.Name;
					textField.RegisterCallback<GeometryChangedEvent>((evt) => 
						{
							var tooltip = string.Format("{0, -15}:{1, 10}", "StartFrame", m_element.StartFrame);
							tooltip += Environment.NewLine;
							tooltip += string.Format("{0, -15}:{1, 10}", "EndFrame", m_element.EndFrame);
							textField.parent.tooltip = tooltip;
						});
					textField.RegisterCallback<FocusOutEvent>((evt) =>
					{
						m_callback(textField.value);
					});
					return textField;
				}
			}	// ParentItem
			/*----------------------------------------------------------------------------------------------------------*/
			/**
			 *	@class	ValueItemBase
			 *  @brief  数値設定フィールド基底クラス
			 */
			private abstract class ValueItemBase : Item
			{
				/*----------------------------------------------------------------------------------------------------------*/
				private Param									m_param;
				private Func<Message, Param, object, object>	m_sendMessageCallback;	
				/*----------------------------------------------------------------------------------------------------------*/
				public void Initialize(Param param, Func<Message, Param, object, object> sendMessageCallback)
				{
					m_param = param;
					m_sendMessageCallback = sendMessageCallback;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				protected Param getParam() => m_param;
				/*----------------------------------------------------------------------------------------------------------*/
				protected object sendMessage(Message message, Param param, object obj)
				{
					return m_sendMessageCallback(message, param, obj);
				}
			}	// ValueItemBase
			/*----------------------------------------------------------------------------------------------------------*/
			private abstract class ValueItem<T, I> : ValueItemBase where T: VisualElement, INotifyValueChanged<I>
			{
				/*----------------------------------------------------------------------------------------------------------*/
				public sealed override VisualElement GetElement()
				{
					var element = getElement();
					var obj = sendMessage(Message.GetValue, getParam(), null);
					if(obj is I)
					{
						element.value = (I)obj;
					}
					element.RegisterValueChangedCallback((evt) =>
					{
						var newValue = evt.newValue;
						if(!checkValue(ref newValue)) 
						{
							element.value = newValue;
						}
						sendMessage(Message.ChangeValue, getParam(), newValue);
					});
					return element;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				protected virtual bool checkValue(ref I value) { return true; }
				/*----------------------------------------------------------------------------------------------------------*/
				protected abstract T getElement();
			}	// ValueItem<T, I> 
			/*----------------------------------------------------------------------------------------------------------*/
			private class Vector2Item : ValueItem<Vector2Field, Vector2>
			{
				/*----------------------------------------------------------------------------------------------------------*/
				protected override Vector2Field getElement()
				{
					var element = new Vector2Field(getParam().Name);
					element.AddToClassList("vector2-field");
					return element;
				}		
			}	// Vector2Item
			/*----------------------------------------------------------------------------------------------------------*/
			private class IntegerItem : ValueItem<IntegerField, int>
			{
				/*----------------------------------------------------------------------------------------------------------*/
				protected override IntegerField getElement()
				{
					var element = new IntegerField(getParam().Name);
					element.AddToClassList("single-value-field");
					element.labelElement.AddToClassList("single-value-field__label");
					return element;
				}		
			}	// IntegerItem
			/*----------------------------------------------------------------------------------------------------------*/
			private class FloatItem : ValueItem<FloatField, float>
			{
				/*----------------------------------------------------------------------------------------------------------*/
				protected override FloatField getElement()
				{
					var element = new FloatField(getParam().Name);
					element.AddToClassList("single-value-field");
					element.labelElement.AddToClassList("single-value-field__label");
					return element;
				}		
			}	// FloatItem
			/*----------------------------------------------------------------------------------------------------------*/
			private class ColorItem : ValueItem<ColorField, Color>
			{
				/*----------------------------------------------------------------------------------------------------------*/
				protected override ColorField getElement()
				{
					var element = new ColorField(getParam().Name);
					element.labelElement.AddToClassList("single-value-field__label");
					return element;
				}		
			}	// ColorItem
			/*----------------------------------------------------------------------------------------------------------*/
			private class AnimatorItem : ValueItemBase
			{
				/*----------------------------------------------------------------------------------------------------------*/
				private AnimatorAsset							m_asset;
				private Action<Message, AnimatorAsset, string>	m_sendMessageCallback;
				/*----------------------------------------------------------------------------------------------------------*/
				public AnimatorItem(AnimatorAsset asset, Action<Message, AnimatorAsset, string> sendMessageCallback)
				{
					m_asset = asset;
					m_sendMessageCallback = sendMessageCallback;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override VisualElement GetElement()
				{
					var element = new VisualElement();
					element.style.flexDirection = FlexDirection.Row;
					
					var conditionList = m_asset.ConditionList.ToList();
					DropdownField dropdownField;
					if(conditionList.Count > 0)
					{
						dropdownField = new DropdownField("Animator", conditionList, m_asset.SelectCondition);
					}
					else
					{
						dropdownField = new DropdownField("Animator");
					}
					dropdownField.labelElement.AddToClassList("animator-field__label");
					dropdownField.RegisterValueChangedCallback((evt) =>
					{
						m_sendMessageCallback(Message.SelectCondition, m_asset, evt.newValue);
					});
					element.Add(dropdownField);
					
					var integerField = new IntegerField();
					integerField.value = (int)sendMessage(Message.GetValue, getParam(), null);
					integerField.RegisterValueChangedCallback((evt) =>
					{
						sendMessage(Message.ChangeValue, getParam(), evt.newValue);
					});
					element.Add(integerField);
					
					return element;
				}
			}	// AnimatorItem
			/*----------------------------------------------------------------------------------------------------------*/
			private class LabelItem : ValueItemBase
			{
				/*----------------------------------------------------------------------------------------------------------*/
				private SharedTableDataAsset									m_asset;
				/*----------------------------------------------------------------------------------------------------------*/
				public LabelItem(SharedTableDataAsset asset)
				{
					m_asset = asset;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override VisualElement GetElement()
				{
					var list = m_asset.KeyList.ToList();
					var value = (string)sendMessage(Message.GetValue, getParam(), null);
					int index = list.IndexOf(value);
					if(index < 0) index = 0;
					var element = new DropdownField("Text", list, index);
					element.labelElement.AddToClassList("single-value-field__label");
					element.RegisterValueChangedCallback((evt) =>
					{
						sendMessage(Message.ChangeValue, getParam(), evt.newValue);
					});
					return element;
				}		
			}	// LabelItem
			/*----------------------------------------------------------------------------------------------------------*/
			private class VolumeItem : FloatItem
			{
				/*----------------------------------------------------------------------------------------------------------*/
				protected override bool checkValue(ref float value)
				{ 
					var tmp = value;
					value = Mathf.Clamp(value, 0f, 1f); 
					return (value == tmp) ? true : false;
				}
			}	// VolumeItem
			/*----------------------------------------------------------------------------------------------------------*/
			private class ChoicesItem : ValueItemBase
			{
				/*----------------------------------------------------------------------------------------------------------*/
				private SharedTableDataAsset									m_asset;
				/*----------------------------------------------------------------------------------------------------------*/
				public ChoicesItem(SharedTableDataAsset asset)
				{
					m_asset = asset;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override VisualElement GetElement()
				{
					var element = new VisualElement();
					element.style.flexDirection = FlexDirection.Row;
					
					var list = m_asset.KeyList.ToList();
					var value = (ChoicesParam.Data)sendMessage(Message.GetValue, getParam(), null);
					int index = list.IndexOf(value.label);
					if(index < 0) index = 0;
					var dropdownField = new DropdownField("Text", list, index);
					dropdownField.labelElement.AddToClassList("single-value-field__label");
					dropdownField.RegisterValueChangedCallback((evt) =>
					{
						value.label = evt.newValue;
						sendMessage(Message.ChangeValue, getParam(), value);
					});
					element.Add(dropdownField);
					
					foreach(var child in dropdownField.hierarchy.Children())
					{
						if(child is Label) continue;
						child.AddToClassList("choices-lable__input");
						break;
					}
					
					var flagsField = new EnumFlagsField(value.flags);
					flagsField.RegisterValueChangedCallback((evt) =>
					{
						value.flags = (EventFlags)evt.newValue;
						sendMessage(Message.ChangeValue, getParam(), value);
					});
					element.Add(flagsField);
					
					return element;
				}		
			}	// ChoicesItem
			/*----------------------------------------------------------------------------------------------------------*/
			private class BoolItem : ValueItemBase
			{
				/*----------------------------------------------------------------------------------------------------------*/
				public override VisualElement GetElement()
				{
					var element = new Toggle(getParam().Name);
					element.labelElement.AddToClassList("single-value-field__label");
					element.value = (bool)sendMessage(Message.GetValue, getParam(), null);
					element.RegisterValueChangedCallback((evt) =>
					{
						sendMessage(Message.ChangeValue, getParam(), evt.newValue);
					});
					return element;
				}
			}	// BoolItem
			/*----------------------------------------------------------------------------------------------------------*/
			/** 
			 *	@class	ObjectItem
			 *  @brief  オブジェクトフィールド
			 */
			private class ObjectItem : Item
			{
				/*----------------------------------------------------------------------------------------------------------*/
				private Type									m_type;
				private string									m_label;
				private UnityEngine.Object						m_defaultValue;
				private Action<object>							m_sendMessageCallback;
				/*----------------------------------------------------------------------------------------------------------*/
				public ObjectItem(Type type, string label)
				{
					m_type = type;
					m_label = label;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public void Initialize(UnityEngine.Object defaultValue, Action<object> sendMessageCallback)
				{
					m_defaultValue = defaultValue;
					m_sendMessageCallback = sendMessageCallback;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override VisualElement GetElement()
				{
					var element = new ObjectField(m_label);
					element.AddToClassList("asset-value-field");
					element.labelElement.AddToClassList("single-value-field__label");
					
					element.RegisterValueChangedCallback((evt) =>
					{
						m_sendMessageCallback(evt.newValue);
					});
					element.objectType = m_type;
					element.value = m_defaultValue;
					return element;
				}
			}	// ObjectItem
			/*----------------------------------------------------------------------------------------------------------*/
			private abstract class SettingItem : Item
			{
				/*----------------------------------------------------------------------------------------------------------*/
				private Setting								m_setting;
				private Action<Message, Setting, object>	m_sendMessageCallback;	
				/*----------------------------------------------------------------------------------------------------------*/
				public void Initialize(Setting settng, Action<Message, Setting, object> sendMessageCallback)
				{
					m_setting = settng;
					m_sendMessageCallback = sendMessageCallback;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				protected void sendMessage(object value)
				{
					m_sendMessageCallback(Message.ApplySetting, m_setting, value);
				}
				/*----------------------------------------------------------------------------------------------------------*/
				protected T getSetting<T>()	where T : Setting
				{
					return (T)m_setting;
				}
			}	// SettingItem
			/*----------------------------------------------------------------------------------------------------------*/
			private class FlagItem : SettingItem
			{
				/*----------------------------------------------------------------------------------------------------------*/
				public override VisualElement GetElement()
				{
					var element = new EnumFlagsField("Flag", getSetting<FlagSetting>().Value);
					element.AddToClassList("flag-field");
					element.labelElement.AddToClassList("single-value-field__label");
					element.RegisterValueChangedCallback((evt) =>
					{
						sendMessage(evt.newValue);
					});
					return element;
				}
			}	// FlagItem
			/*----------------------------------------------------------------------------------------------------------*/
			private class LoopItem : SettingItem
			{
				/*----------------------------------------------------------------------------------------------------------*/
				public override VisualElement GetElement()
				{
					var element = new Toggle("Loop");
					element.labelElement.AddToClassList("single-value-field__label");
					element.value = getSetting<LoopSetting>().Value;
					element.RegisterValueChangedCallback((evt) =>
					{
						sendMessage(evt.newValue);
					});
					return element;
				}
			}	// LoopItem
			/*----------------------------------------------------------------------------------------------------------*/
			private class CustomTreeView : TreeView
			{
				/*----------------------------------------------------------------------------------------------------------*/
				private class Controller : DefaultTreeViewController<Item>
				{
					/*----------------------------------------------------------------------------------------------------------*/
					private bool 								m_isEnable;
					/*----------------------------------------------------------------------------------------------------------*/
					public bool									IsEnable { get => m_isEnable; }
					/*----------------------------------------------------------------------------------------------------------*/
					public Controller()
					{
						m_isEnable = true;
					}
					/*----------------------------------------------------------------------------------------------------------*/
					public void EnableExpand(bool isEnable)
					{
						m_isEnable = isEnable;
					}
					/*----------------------------------------------------------------------------------------------------------*/
				    public override bool CanChangeExpandedState(int id)
				    {
				        return m_isEnable;
				    }
				}	// Controller
				/*----------------------------------------------------------------------------------------------------------*/
				private Controller							m_controller;
				/*----------------------------------------------------------------------------------------------------------*/
				public bool									IsEnableExpand { get => m_controller.IsEnable; }
				/*----------------------------------------------------------------------------------------------------------*/
				public CustomTreeView()
				{
					m_controller = (Controller)CreateViewController();
					SetViewController(m_controller);
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public void EnableExpand(bool isEnable)
				{
					m_controller.EnableExpand(isEnable);
				}
				/*----------------------------------------------------------------------------------------------------------*/
			    protected override CollectionViewController CreateViewController()
			    {
			        return new Controller();
			    }
			}	// CustomTreeView
			/*----------------------------------------------------------------------------------------------------------*/
			private List<ParentItem>							m_parentList;
			private ScrollView									m_scrollView;
			private bool										m_sync;
			// unity version 6000.0.37f1時点でhandleDropが実行されなかった場合
			// TreeViewReorderableDragAndDropController.DragCleanupが呼ばれず
			// m_TreeView.schedule.Execute(m_ExpandDropItemCallback).Every(10L);の処理が残り続けるため
			// 同じIDを使用しているとitemExpandedChangedが勝手に呼ばれる。
			private static int									sm_id = 0;
			/*----------------------------------------------------------------------------------------------------------*/
			/**
			 *	@brief コンストラクタ
			 */
			public HierarchyPain()
			{
				m_parentList = new List<ParentItem>();
				m_scrollView = null;
				m_sync = false;
			}
			/*----------------------------------------------------------------------------------------------------------*/		
			/**
			 *	@brief Unity再生時の処理
			 *
			 *	@param dataManager	データ
			 */
			public override void Play(IDataManager dataManager)
			{
			
			}
			/*----------------------------------------------------------------------------------------------------------*/
			/**
			 *	@brief EventData.Elementの追加
			 *
			 *	@param element		追加要素
			 */
			public override void AddElement(Element element)
			{
				var root = (TreeView)bodyElement;
				var parentId = sm_id;
				var parentItem = new ParentItem(element, parentId,
					(name)=>
					{
						sendMessage(Message.ChangeElementName, element, name);	
					});
				m_parentList.Add(parentItem);
				root.AddItem(new TreeViewItemData<Item>(sm_id++, parentItem));
				foreach(var param in element.ParamList)
				{
					ValueItemBase item;
					switch(param.Type)
					{
						case ParamType.Position:
						case ParamType.Scale:
							item = new Vector2Item();
							break;
						case ParamType.Rotation:
							item = new FloatItem();
							break;
						case ParamType.Color:
							item = new ColorItem();
							break;
						case ParamType.Animator:
							{
								var asset = element.AssetList.FirstOrDefault((p) => p.Type == AssetType.Animator) as AnimatorAsset;
								if(asset == null) continue;
								item = new AnimatorItem(asset,
									(message, asset, value) =>
									{
										sendMessage(message, asset, value);	
									});
							}
							break;
						case ParamType.TextLabel:
						case ParamType.Choices:
							{
								var asset = element.AssetList.FirstOrDefault(
										(p) => p.Type == AssetType.SharedTableData) as SharedTableDataAsset;
								if(asset == null) continue;
								item = (param.Type == ParamType.TextLabel) ? new LabelItem(asset) : new ChoicesItem(asset);
							}
							break;
						case ParamType.Volume:
							item = new VolumeItem();
							break;
						case ParamType.Raycast:
							item = new BoolItem();
							break;
						default:
							continue;
					}
					item.Initialize(param, 
						(message, param, value) => 
						{
							return sendMessage(message, param, value);
						});
					root.AddItem(new TreeViewItemData<Item>(sm_id++, item), parentId);
				}
				foreach(var asset in element.AssetList)
				{
					ObjectItem item;
					switch(asset.Type)
					{
						case AssetType.Image:
							item = new ObjectItem(typeof(Sprite), "Source Image");
							break;
						case AssetType.Animator:
							item = new ObjectItem(typeof(RuntimeAnimatorController), "Controller");
							break;
						case AssetType.SharedTableData:
							item = new ObjectItem(typeof(SharedTableData), "TextTable");
							break;
						case AssetType.Audio:
							item = new ObjectItem(typeof(AudioClip), "Audio");
							break;
						default:
							continue;
					}
					item.Initialize(asset.Object,
						(value) => 
						{
							sendMessage(Message.SetAsset, element, asset.Type, value);
						});
					root.AddItem(new TreeViewItemData<Item>(sm_id++, item), parentId);
				}
				foreach(var setting in element.SettingList)
				{
					SettingItem item;
					switch(setting.Type)
					{
						case SettingType.Flags:
							item = new FlagItem();
							break;
						case SettingType.Loop:
							item = new LoopItem();
							break;
						default:
							continue;
					}
					item.Initialize(setting,
						(message, setting, value) => 
						{
							sendMessage(message, setting, value);
						});
					root.AddItem(new TreeViewItemData<Item>(sm_id++, item), parentId);
				}
				
				if(element.IsExpand)
				{
					root.ExpandItem(parentId);
				}
				else
				{
					root.CollapseItem(parentId);
				}
			}
			/*----------------------------------------------------------------------------------------------------------*/		
			public override void SyncScroll(float scrollOffset)
			{
				if(m_scrollView == null) return;
				m_scrollView.scrollOffset = new Vector2(m_scrollView.scrollOffset.x, scrollOffset);
				m_sync = true;
			}
			/*----------------------------------------------------------------------------------------------------------*/
			protected override VisualElement createBodyElement()
			{
				var element = new CustomTreeView();
				element.fixedItemHeight = cm_itemHeight;
				element.itemExpandedChanged += itemExpandedChanged;
				element.canStartDrag += startDragItem;
				element.setupDragAndDrop += setUpDragItem;
				element.dragAndDropUpdate += dragUpdate;
				element.handleDrop += dropItem;
				element.RegisterCallback<KeyDownEvent>(
					(evt) =>
					{
						if(evt.keyCode == KeyCode.Delete)
						{
							deleteSelectItem();
							evt.StopPropagation();
						}
					});
				element.RegisterCallback<DragExitedEvent>((evt) => 
					{
						element.EnableExpand(true);
						sendMessage(Message.RePaint, this);
					});
				
				m_scrollView = null;
				foreach(var child in element.hierarchy.Children())
				{
					if(!(child is ScrollView)) continue;
					m_scrollView = (ScrollView)child;
					m_scrollView.mode = ScrollViewMode.VerticalAndHorizontal;
					m_scrollView.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
					m_scrollView.horizontalScrollerVisibility  = ScrollerVisibility.AlwaysVisible;
					m_scrollView.verticalScroller.valueChanged += 
						(evt) =>
						{
							if(!m_sync)
							{
								sendMessage(Message.SyncScroll, this, evt);	
							}
							m_sync = false;
						};
					break;
				}
				
				return element;
			}
			/*----------------------------------------------------------------------------------------------------------*/			
			protected override void createBodyVisualElement(IDataManager dataManager)
			{
				bodyElement.Clear();
				
				var root = (TreeView)bodyElement;
				root.SetRootItems(new List<TreeViewItemData<Item>>());
				
				m_parentList.Clear();
				foreach(var element in dataManager.ElementList)
				{
					AddElement(element);
				}
				root.makeItem = () => 
				{	
					var element = new VisualElement();
					element.AddToClassList("treeveiw-item-node");
					return element;
				};
				root.bindItem = (VisualElement element, int index) =>
				{
					element.Clear();
					var item = root.GetItemDataForIndex<Item>(index).GetElement();
					item.AddToClassList("treeveiw-item-text");
					element.Add(item);
				};
				root.RegisterCallback<MouseDownEvent>((evt) => 
				{
					if(evt.button == (int)MouseButton.RightMouse)
					{
						showContextMenu(evt.mousePosition);
						evt.StopPropagation();
					}
				});		
			}
			/*----------------------------------------------------------------------------------------------------------*/
			protected override Vector2 getScrollOffset()
			{
				return m_scrollView.scrollOffset;
			}
			/*----------------------------------------------------------------------------------------------------------*/
			protected override void setScrollOffset(Vector2 scrollOffset)
			{
				m_scrollView.scrollOffset = scrollOffset;
			}
			/*----------------------------------------------------------------------------------------------------------*/
			private void itemExpandedChanged(TreeViewExpansionChangedArgs args)
			{
				var target = m_parentList.Find((item) => item.Id == args.id);
				if(target == null) return;
				sendMessage(Message.ExpandElement, target.Data, args.isExpanded);
			}
			/*----------------------------------------------------------------------------------------------------------*/
			/**
			 *	@brief ドラッグ有効・無効制御
			 *	
			 *	@param args					対象ドラッグ情報
			 *	@return						true:ドラッグ有効
			 */	
			private bool startDragItem(CanStartDragArgs args)
			{
				return (m_parentList.Find((item) => item.Id == args.id) != null) ? true : false;
			}
			/*----------------------------------------------------------------------------------------------------------*/
			/**
			 *	@brief ドラッグ初期化
			 *	
			 *	@param args					対象ドラッグ情報
			 *	@return						ドラッグデータ
			 */	
			private StartDragArgs setUpDragItem(SetupDragAndDropArgs args)
			{
				((CustomTreeView)bodyElement).EnableExpand(false);
				var data = new StartDragArgs(args.startDragArgs.title, DragVisualMode.Move);
				data.SetGenericData("sourceId", args.selectedIds.FirstOrDefault());
				return data;
			}
			/*----------------------------------------------------------------------------------------------------------*/
			/**
			 *	@brief ドラッグ更新処理
			 *	
			 *	@param args					対象ドラッグ情報
			 *	@return						Move:ドロップ有効	Rejected:ドロップ無効
			 */	
			private DragVisualMode dragUpdate(HandleDragAndDropArgs args)
			{
				return (args.parentId == -1) ? DragVisualMode.Move : DragVisualMode.Rejected;
			}
			/*----------------------------------------------------------------------------------------------------------*/
			/**
			 *	@brief ドロップ処理
			 *	
			 *	@param args					対象ドラッグ情報
			 *	@return						Rejected:ドロップ無効
			 */	
			private DragVisualMode dropItem(HandleDragAndDropArgs args)
			{
				((CustomTreeView)bodyElement).EnableExpand(true);
				
				if(args.parentId != -1) return DragVisualMode.Rejected;
				
				int sourceId = (int)args.dragAndDropData.GetGenericData("sourceId");
				int count = m_parentList.Count;
				int sourceIndex = count;
				int targetIndex = count;
				for(int i = 0, treeIndex = 0; i < count; ++i, ++treeIndex)
				{
					if(m_parentList[i].Id == sourceId) sourceIndex = i;
					else if(treeIndex == args.insertAtIndex) targetIndex = i;
					if(m_parentList[i].Data.IsExpand)
					{
						treeIndex += (m_parentList[i].Data.ParamList.Count + m_parentList[i].Data.AssetList.Count + 
							m_parentList[i].Data.SettingList.Count);
					}
				}
				sendMessage(Message.MoveElementPriority, sourceIndex, targetIndex);
				
				return DragVisualMode.Rejected;
			}
			/*----------------------------------------------------------------------------------------------------------*/
			/**
			 *	@brief ポップアップメニュー表示
			 *
			 *	@param mousePosition		マウス座標
			 */			
			private void showContextMenu(Vector2 mousePosition)
			{
				var menu = new GenericMenu();

				foreach(var dataInfo in Window.sm_dataInfo) 
				{
					menu.AddItem(new GUIContent(dataInfo.text), false, () => { sendMessage(Message.AddItem, dataInfo.type); } );
				}
				menu.AddSeparator("");
				menu.AddItem(new GUIContent("Delete"), false, () => { deleteSelectItem(); });

				menu.DropDown(new Rect(mousePosition.x, mousePosition.y, 0, 0));
			}
			/*----------------------------------------------------------------------------------------------------------*/
			private void deleteSelectItem()
			{
				var items = ((CustomTreeView)bodyElement).GetSelectedItems<Item>();
				foreach(var item in items)
				{
					if(item.data is ParentItem)
					{
						sendMessage(Message.DeleteItem, ((ParentItem)item.data).Data); 
					}
				}
			}
		}	// HierarchyPain
	}	// Window
}	// AEImitation
