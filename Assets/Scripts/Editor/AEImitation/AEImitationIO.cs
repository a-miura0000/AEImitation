/** 
 *	@file	AEImitationIO.cs
 *	@brief	IO
 *
 *	@author miura
 */
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Xml;
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
		private partial class DataManager : Manager, IDataManager
		{
			/*----------------------------------------------------------------------------------------------------------*/
			/** 
			 *	@class	Reader
			 *  @brief  読み込み基底クラス
			 */
			private abstract class Reader
			{
				public abstract void Read(in XmlReader reader);
				public abstract object Flush();
				public abstract void Merge(object data);
			}	// Reader
			/*----------------------------------------------------------------------------------------------------------*/
			/** 
			 *	@class	PlayDataReader
			 *  @brief  PlayData読み込み
			 */
			private class PlayDataReader : Reader
			{
				/*----------------------------------------------------------------------------------------------------------*/
				private EditorData			m_playData = new EditorData();
				/*----------------------------------------------------------------------------------------------------------*/
				public override void Read(in XmlReader reader)
				{
					switch(reader.Name)
					{
						case cm_xmlTagTotalFrames:
							m_playData.TotalFrames = convertValue<int>(reader.ReadElementContentAsString());
							break;
						default:
							break;
					}
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override object Flush()
				{
					return m_playData;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override void Merge(object data)
				{
					if(data is Element)
					{
						m_playData.AddElement((Element)data);
					}
					else if(data is Comment)
					{
						m_playData.AddComment((Comment)data);
					}
				}
			}	// PlayDataReader
			/*----------------------------------------------------------------------------------------------------------*/
			/** 
			 *	@class	ElementReader
			 *  @brief  エレメント情報読み込み
			 */
			private class ElementReader : Reader
			{
				/*----------------------------------------------------------------------------------------------------------*/
				private Element				m_element = null;
				/*----------------------------------------------------------------------------------------------------------*/
				public override void Read(in XmlReader reader)
				{
					switch(reader.Name)
					{
						case cm_xmlTagElementType:
							switch((ElementType)convertValue<int>(reader.ReadElementContentAsString()))
							{
								case ElementType.Image:
									m_element = new ImageElement();
									break;
								case ElementType.Panel:
									m_element = new PanelElement();
									break;
								case ElementType.Text:
									m_element = new TextElement();
									break;
								case ElementType.Choices:
									m_element = new ChoicesElement();
									break;
								case ElementType.Sound:
									m_element = new SoundElement();
									break;
								case ElementType.SoundOneShot:
									m_element = new SoundOneShotElement();
									break;
								default:
									break;	
							}
							break;
						case cm_xmlTagElementName:
							if(m_element == null) break;
							m_element.Name = reader.ReadElementContentAsString();
							break;
						case cm_xmlTagStartFrame:
							if(m_element == null) break;
							m_element.StartFrame = convertValue<int>(reader.ReadElementContentAsString());
							break;
						case cm_xmlTagEndFrame:
							if(m_element == null) break;
							m_element.EndFrame = convertValue<int>(reader.ReadElementContentAsString());
							break;
						case cm_xmlTagExpand:
							if(m_element == null) break;
							m_element.IsExpand = convertValue<bool>(reader.ReadElementContentAsString());
							break;
						default:
							break;
					}
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override object Flush()
				{
					return m_element;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override void Merge(object data)
				{
					if(data is Param)
					{
						m_element.SetParam((Param)data);
					}
					else if(data is Asset)
					{
						m_element.SetAsset((Asset)data);
					}
					else if(data is Setting)
					{
						m_element.SetSetting((Setting)data);
					}
				}
			}	// ElementReader
			/*----------------------------------------------------------------------------------------------------------*/
			/** 
			 *	@class	ParamReader
			 *  @brief  パラメータ情報読み込み
			 */
			private class ParamReader : Reader
			{
				/*----------------------------------------------------------------------------------------------------------*/
				private Param				m_param;
				/*----------------------------------------------------------------------------------------------------------*/
				public override void Read(in XmlReader reader)
				{
					switch(reader.Name)
					{
						case cm_xmlTagParamType:
							switch((ParamType)convertValue<int>(reader.ReadElementContentAsString()))
							{
								case ParamType.Position:
									m_param = new Vector2Param(ParamType.Position);
									break;
								case ParamType.Rotation:
									m_param = new FloatParam(ParamType.Rotation);
									break;
								case ParamType.Scale:
									m_param = new ScaleParam();
									break;
								case ParamType.Color:
									m_param = new ColorParam();
									break;
								case ParamType.Animator:
									m_param = new AnimatorParam();
									break;
								case ParamType.TextLabel:
									m_param = new LabelParam();
									break;
								case ParamType.Volume:
									m_param = new VolumeParam();
									break;
								case ParamType.Choices:
									m_param = new ChoicesParam();
									break;
								case ParamType.Raycast:
									m_param = new BoolParam(ParamType.Raycast);
									break;
								default:
									break;
							}
							break;
						case cm_xmlTagSegment:
							if(m_param == null) break;
							m_param.Segment = convertValue<int>(reader.ReadElementContentAsString());
							break;
						default:
							break;
					}
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override object Flush()
				{
					return m_param;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override void Merge(object data)
				{
					if(!(data is List<KeyFrameBase>)) return;
					m_param.SetKeyFrameList((List<KeyFrameBase>)data);
				}
			}	// ParamReader
			/*----------------------------------------------------------------------------------------------------------*/
			/** 
			 *	@class	KeyFramesReader
			 *  @brief  キーフレーム情報読み込み
			 */
			private class KeyFramesReader : Reader
			{
				/*----------------------------------------------------------------------------------------------------------*/
				private List<KeyFrameBase>	m_keyFrameList = new List<KeyFrameBase>();
				/*----------------------------------------------------------------------------------------------------------*/
				public override void Read(in XmlReader reader)
				{
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override object Flush()
				{
					m_keyFrameList.Sort((a, b) => { return a.Frame - b.Frame; });
					return m_keyFrameList;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override void Merge(object data)
				{
					if(!(data is KeyFrameBase)) return;
					m_keyFrameList.Add((KeyFrameBase)data);
				}
			}	// KeyFramesReader
			/*----------------------------------------------------------------------------------------------------------*/
			/** 
			 *	@class	KeyReader
			 *  @brief  キーフレーム情報読み込み
			 */
			private class KeyReader : Reader
			{
				/*----------------------------------------------------------------------------------------------------------*/
				private ParamType			m_type;
				private int					m_frame;
				private Vector2				m_vec2;
				private float				m_float;
				private Color				m_color;
				private int					m_int;
				private string				m_string;
				private uint				m_uint;
				private bool				m_bool;
				/*----------------------------------------------------------------------------------------------------------*/
				public override void Read(in XmlReader reader)
				{
					switch(reader.Name)
					{
						case cm_xmlTagKeyType:
							m_type = (ParamType)convertValue<int>(reader.ReadElementContentAsString());
							break;
						case cm_xmlTagFrame:
							m_frame = convertValue<int>(reader.ReadElementContentAsString());
							break;
						case cm_xmlTagX:
							m_vec2.x = convertValue<float>(reader.ReadElementContentAsString());
							break;
						case cm_xmlTagY:
							m_vec2.y = convertValue<float>(reader.ReadElementContentAsString());
							break;
						case cm_xmlTagRot:
						case cm_xmlTagVolume:
							m_float = convertValue<float>(reader.ReadElementContentAsString());
							break;
						case cm_xmlTagColR:
							m_color.r = convertValue<float>(reader.ReadElementContentAsString());
							break;
						case cm_xmlTagColG:
							m_color.g = convertValue<float>(reader.ReadElementContentAsString());
							break;
						case cm_xmlTagColB:
							m_color.b = convertValue<float>(reader.ReadElementContentAsString());
							break;
						case cm_xmlTagColA:
							m_color.a = convertValue<float>(reader.ReadElementContentAsString());
							break;
						case cm_xmlTagTransitionNo:
							m_int = convertValue<int>(reader.ReadElementContentAsString());
							break;
						case cm_xmlTagString:
							m_string = reader.ReadElementContentAsString();
							break;
						case cm_xmlTagUInteger:
							m_uint = convertValue<uint>(reader.ReadElementContentAsString());
							break;
						case cm_xmlTagBool:
							m_bool = convertValue<bool>(reader.ReadElementContentAsString());
							break;
						default:
							break;
					}
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override object Flush()
				{
					switch(m_type)
					{
						case ParamType.Position:
						case ParamType.Scale:
							return new KeyFrame<Vector2>(m_vec2, m_frame);
						case ParamType.Rotation:
						case ParamType.Volume:
							return new KeyFrame<float>(m_float, m_frame);
						case ParamType.Color:
							return new KeyFrame<Color>(m_color, m_frame);
						case ParamType.Animator:
							return new KeyFrame<int>(m_int, m_frame);
						case ParamType.TextLabel:
							return new KeyFrame<string>(m_string, m_frame);
						case ParamType.Choices:
							return new KeyFrame<ChoicesParam.Data>(
								new ChoicesParam.Data(m_string, (EventFlags)m_uint), m_frame);
						case ParamType.Raycast:
							return new KeyFrame<bool>(m_bool, m_frame);
						default:
							break;
					}
					return null;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override void Merge(object data)
				{
				}
			}	// KeyReader
			/*----------------------------------------------------------------------------------------------------------*/
			/** 
			 *	@class	AssetReader
			 *  @brief  Asset情報読み込み
			 */
			private class AssetReader : Reader
			{
				/*----------------------------------------------------------------------------------------------------------*/
				private Asset				m_asset;
				/*----------------------------------------------------------------------------------------------------------*/
				public override void Read(in XmlReader reader)
				{
					switch(reader.Name)
					{
						case cm_xmlTagAssetType:
							switch((AssetType)convertValue<int>(reader.ReadElementContentAsString()))
							{
								case AssetType.Image:
									m_asset = new ImageAsset();
									break;
								case AssetType.Animator:
									m_asset = new AnimatorAsset();
									break;
								case AssetType.SharedTableData:
									m_asset = new SharedTableDataAsset();
									break;
								case AssetType.Audio:
									m_asset = new AudioAsset();
									break;
								default:
									break;
							}
							break;
						case cm_xmlTagPath:
							if(m_asset == null) break;
							m_asset.Path = reader.ReadElementContentAsString();
							break;
						case cm_xmlTagSpriteName:
							if(m_asset == null) break;
							((ImageAsset)m_asset).Name = reader.ReadElementContentAsString();
							break;
						case cm_xmlTagAnimatorCondition:
							if(m_asset == null) break;
							((AnimatorAsset)m_asset).SelectCondition = reader.ReadElementContentAsString();
							break;
						default:
							break;
					}
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override object Flush()
				{
					return m_asset;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override void Merge(object data)
				{
				}
			}	// AssetReader
			/*----------------------------------------------------------------------------------------------------------*/
			/** 
			 *	@class	SettingReader
			 *  @brief  設定読み込み
			 */
			private class SettingReader : Reader
			{
				/*----------------------------------------------------------------------------------------------------------*/
				private Setting				m_setting;
				/*----------------------------------------------------------------------------------------------------------*/
				public override void Read(in XmlReader reader)
				{
					switch(reader.Name)
					{
						case cm_xmlTagSettingType:
							switch((SettingType)convertValue<int>(reader.ReadElementContentAsString()))
							{
								case SettingType.Flags:
									m_setting = new FlagSetting();
									break;
								case SettingType.Loop:
									m_setting = new LoopSetting();
									break;
								default:
									break;
							}
							break;
						case cm_xmlTagUInteger:
							if(m_setting == null) break;
							((FlagSetting)m_setting).Value = (EventFlags)convertValue<uint>(reader.ReadElementContentAsString());
							break;
						case cm_xmlTagBool:
							if(m_setting == null) break;
							((LoopSetting)m_setting).Value = convertValue<bool>(reader.ReadElementContentAsString());
							break;
						default:
							break;
					}
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override object Flush()
				{
					return m_setting;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override void Merge(object data)
				{
				}
			}	// SettingReader
			/*----------------------------------------------------------------------------------------------------------*/
			/** 
			 *	@class	CommentReader
			 *  @brief  コメント読み込み
			 */
			private class CommentReader : Reader
			{
				/*----------------------------------------------------------------------------------------------------------*/
				private Comment				m_comment;
				/*----------------------------------------------------------------------------------------------------------*/
				public CommentReader()
				{
					m_comment = new Comment();
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override void Read(in XmlReader reader)
				{
					switch(reader.Name)
					{
						case cm_xmlTagFrame:
							m_comment.Frame = convertValue<int>(reader.ReadElementContentAsString());
							break;
						case cm_xmlTagString:
							m_comment.Text = reader.ReadElementContentAsString();
							break;
						default:
							break;
					}
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override object Flush()
				{
					return m_comment;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override void Merge(object data)
				{
				}
			}	// CommentReader
			/*----------------------------------------------------------------------------------------------------------*/
			private const int				cm_xmlDataVersion = 0x100;
			private const string			cm_xmlTagRoot = "AEImitation";
			private const string			cm_xmlTagVer = "Version";		
			private const string			cm_xmlTagPlayData = "PlayData";	
			private const string			cm_xmlTagTotalFrames = "TotalFrames";	
			private const string			cm_xmlTagElement = "Element";	
			private const string			cm_xmlTagElementType = "Type";
			private const string			cm_xmlTagElementName = "Name";
			private const string			cm_xmlTagStartFrame = "StartFrame";
			private const string			cm_xmlTagEndFrame = "EndFrame";
			private const string			cm_xmlTagExpand = "IsExpand";
			private const string			cm_xmlTagParam = "Param";
			private const string			cm_xmlTagParamType = "Type";
			private const string			cm_xmlTagSegment = "Segment";
			private const string			cm_xmlTagKeyFrames = "KeyFrames";
			private const string			cm_xmlTagKey = "Key";
			private const string			cm_xmlTagKeyType = "Type";
			private const string			cm_xmlTagFrame = "Frame";
			private const string			cm_xmlTagX = "X";
			private const string			cm_xmlTagY = "Y";
			private const string			cm_xmlTagRot = "Rotation";
			private const string			cm_xmlTagColR = "R";
			private const string			cm_xmlTagColG = "G";
			private const string			cm_xmlTagColB = "B";
			private const string			cm_xmlTagColA = "A";
			private const string			cm_xmlTagTransitionNo = "TransitionNo";
			private const string			cm_xmlTagString = "String";
			private const string			cm_xmlTagVolume = "Volume";
			private const string			cm_xmlTagUInteger = "cm_xmlTagUInteger";
			private const string			cm_xmlTagBool = "Bool";
			private const string			cm_xmlTagAsset = "Asset";
			private const string			cm_xmlTagAssetType = "Type";
			private const string			cm_xmlTagPath = "Path";
			private const string			cm_xmlTagSpriteName = "SpriteName";
			private const string			cm_xmlTagAnimatorCondition = "Condition";
			private const string			cm_xmlTagSetting = "Setting";
			private const string			cm_xmlTagSettingType = "Type";
			private const string			cm_xmlTagComment = "Comment";	
			/*----------------------------------------------------------------------------------------------------------*/
			public void WriteXml(string filePath)
			{
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = true;
				
				using(XmlWriter writer = XmlWriter.Create(filePath, settings))
				{
					writer.WriteStartDocument();
					writer.WriteStartElement(cm_xmlTagRoot);
					writer.WriteElementString(cm_xmlTagVer, cm_xmlDataVersion.ToString());
					writePlayData(writer, this.data);
					writer.WriteEndElement();
					writer.WriteEndDocument();
				}
			}
			/*----------------------------------------------------------------------------------------------------------*/
			public void ReadXml(string filePath)
			{
				XmlReaderSettings settings = new XmlReaderSettings();
				using(XmlReader reader = XmlReader.Create(filePath, settings))
				{
					var stack = new Stack<Reader>();
					while(reader.Read())
					{
						switch(reader.NodeType)
						{
							case XmlNodeType.Element:
								switch(reader.Name)
								{
									case cm_xmlTagVer:
										// versionチェック
										if(convertValue<int>(reader.ReadElementContentAsString()) != cm_xmlDataVersion)
										{
											Debug.LogError("version is different");
											goto END;
										}
										break;
									case cm_xmlTagPlayData:
										stack.Push(new PlayDataReader());
										break;
									case cm_xmlTagElement:
										stack.Push(new ElementReader());
										break;
									case cm_xmlTagParam:
										stack.Push(new ParamReader());
										break;
									case cm_xmlTagKeyFrames:
										stack.Push(new KeyFramesReader());
										break;
									case cm_xmlTagKey:
										stack.Push(new KeyReader());
										break;
									case cm_xmlTagAsset:
										stack.Push(new AssetReader());
										break;
									case cm_xmlTagSetting:
										stack.Push(new SettingReader());
										break;
									case cm_xmlTagComment:
										stack.Push(new CommentReader());
										break;
									default:
										if(stack.Count == 0) break;
										stack.Peek().Read(reader);
										break;
								}
								break;
							case XmlNodeType.EndElement:
								switch(reader.Name)
								{
									case cm_xmlTagPlayData:
									case cm_xmlTagElement:
									case cm_xmlTagParam:
									case cm_xmlTagKeyFrames:
									case cm_xmlTagKey:
									case cm_xmlTagAsset:
									case cm_xmlTagSetting:
									case cm_xmlTagComment:
										var data = stack.Peek().Flush();
										stack.Pop();
										if(stack.Count > 0)
										{
											stack.Peek().Merge(data);
										}
										else if(reader.Name == cm_xmlTagPlayData)
										{
											this.data = (EditorData)data;
											foreach(var element in this.data.ElementList)
											{
												AddObject(element);
											}
										}
										break;
									default:
										break;
								}
								break;
							default:
								break;
						}
					}
					END:;
				}
			}
			/*----------------------------------------------------------------------------------------------------------*/
			private void writePlayData(XmlWriter writer, EditorData data)
			{
				writer.WriteStartElement(cm_xmlTagPlayData);
				writer.WriteElementString(cm_xmlTagTotalFrames, data.TotalFrames.ToString());
				foreach(var element in data.ElementList)
				{
					writeElement(writer, element);
				}
				foreach(var comment in data.CommentList)
				{
					writeComment(writer, comment);
				}
				writer.WriteEndElement();
			}
			/*----------------------------------------------------------------------------------------------------------*/
			/**
			 * @brief Element情報出力
			 *
			 * @param writer		xmlwriter
			 * @param element		出力エレメント
			 */
			private void writeElement(XmlWriter writer, Element element)
			{
				writer.WriteStartElement(cm_xmlTagElement);
				writer.WriteElementString(cm_xmlTagElementType, ((int)element.Type).ToString());
				writer.WriteElementString(cm_xmlTagElementName, element.Name);
				writer.WriteElementString(cm_xmlTagStartFrame, element.StartFrame.ToString());
				writer.WriteElementString(cm_xmlTagEndFrame, element.EndFrame.ToString());
				writer.WriteElementString(cm_xmlTagExpand, element.IsExpand.ToString());
				foreach(var param in element.ParamList)
				{
					writeParam(writer, param);
				}
				foreach(var asset in element.AssetList)
				{
					writeAsset(writer, asset);
				}
				foreach(var setting in element.SettingList)
				{
					writeSetting(writer, setting);
				}
				writer.WriteEndElement();
			}
			/*----------------------------------------------------------------------------------------------------------*/
			private void writeParam(XmlWriter writer, Param param)
			{
				writer.WriteStartElement(cm_xmlTagParam);
				writer.WriteElementString(cm_xmlTagParamType, ((int)param.Type).ToString());
				writer.WriteElementString(cm_xmlTagSegment, param.Segment.ToString());
				// KeyFrames
				writer.WriteStartElement(cm_xmlTagKeyFrames);
				foreach(var keyFrame in param.KeyFrameList)
				{
					writeKeyFrame(writer, keyFrame, param.Type);
				}
				// 出力形式の調整
				if(param.KeyFrameList.Count == 0)
				{
					writer.WriteString("");
				}
				writer.WriteEndElement();
				
				writer.WriteEndElement();
			}
			/*----------------------------------------------------------------------------------------------------------*/
			private void writeKeyFrame(XmlWriter writer, KeyFrameBase keyFrame, ParamType type)
			{
				writer.WriteStartElement(cm_xmlTagKey);
				writer.WriteElementString(cm_xmlTagKeyType, ((int)type).ToString());
				writer.WriteElementString(cm_xmlTagFrame, keyFrame.Frame.ToString());
				switch(type)
				{
					case ParamType.Position:
					case ParamType.Scale:
						{
							var tmp = (KeyFrame<Vector2>)keyFrame;
							writer.WriteElementString(cm_xmlTagX, tmp.Value.x.ToString());
							writer.WriteElementString(cm_xmlTagY, tmp.Value.y.ToString());
						}
						break;
					case ParamType.Rotation:
						writer.WriteElementString(cm_xmlTagRot, ((KeyFrame<float>)keyFrame).Value.ToString());
						break;
					case ParamType.Color:
						{
							var color = ((KeyFrame<Color>)keyFrame).Value;
							writer.WriteElementString(cm_xmlTagColR, color.r.ToString());
							writer.WriteElementString(cm_xmlTagColG, color.g.ToString());
							writer.WriteElementString(cm_xmlTagColB, color.b.ToString());
							writer.WriteElementString(cm_xmlTagColA, color.a.ToString());
						}
						break;
					case ParamType.Animator:
						writer.WriteElementString(cm_xmlTagTransitionNo, ((KeyFrame<int>)keyFrame).Value.ToString());
						break;
					case ParamType.TextLabel:
						writer.WriteElementString(cm_xmlTagString, ((KeyFrame<string>)keyFrame).Value);
						break;
					case ParamType.Volume:
						writer.WriteElementString(cm_xmlTagVolume, ((KeyFrame<float>)keyFrame).Value.ToString());
						break;
					case ParamType.Choices:
						{
							var value = ((KeyFrame<ChoicesParam.Data>)keyFrame).Value;
							writer.WriteElementString(cm_xmlTagString, value.label);
							writer.WriteElementString(cm_xmlTagUInteger, ((uint)value.flags).ToString());
						}
						break;
					case ParamType.Raycast:
						writer.WriteElementString(cm_xmlTagBool, ((KeyFrame<bool>)keyFrame).Value.ToString());
						break;
					default:
						break;
				}
				
				writer.WriteEndElement();
			}
			/*----------------------------------------------------------------------------------------------------------*/
			/**
			 * @brief Asse情報出力
			 *
			 * @param writer		xmlwriter
			 * @param asset			出力アセット
			 */
			private void writeAsset(XmlWriter writer, Asset asset)
			{
				writer.WriteStartElement(cm_xmlTagAsset);
				writer.WriteElementString(cm_xmlTagAssetType, ((int)asset.Type).ToString());
				writer.WriteElementString(cm_xmlTagPath, asset.Path);
				switch(asset.Type)
				{
					case AssetType.Image:
						writer.WriteElementString(cm_xmlTagSpriteName, ((ImageAsset)asset).Name);
						break;
					case AssetType.Animator:
						writer.WriteElementString(cm_xmlTagAnimatorCondition, ((AnimatorAsset)asset).SelectCondition);
						break;
					default:
						break;
				}
				writer.WriteEndElement();
			}
			/*----------------------------------------------------------------------------------------------------------*/
			/**
			 * @brief 設定出力
			 *
			 * @param writer		xmlwriter
			 * @param setting		設定
			 */
			private void writeSetting(XmlWriter writer, Setting setting)
			{
				writer.WriteStartElement(cm_xmlTagSetting);
				writer.WriteElementString(cm_xmlTagSettingType, ((int)setting.Type).ToString());
				switch(setting.Type)
				{
					case SettingType.Flags:
						writer.WriteElementString(cm_xmlTagUInteger, ((uint)((FlagSetting)setting).Value).ToString());
						break;
					case SettingType.Loop:
						writer.WriteElementString(cm_xmlTagBool, ((LoopSetting)setting).Value.ToString());
						break;
					default:
						break;
				}
				writer.WriteEndElement();
			}
			/*----------------------------------------------------------------------------------------------------------*/
			/**
			 * @brief コメント出力
			 *
			 * @param writer		xmlwriter
			 * @param asset			出力コメント
			 */
			private void writeComment(XmlWriter writer, Comment comment)
			{
				writer.WriteStartElement(cm_xmlTagComment);
				writer.WriteElementString(cm_xmlTagFrame, comment.Frame.ToString());
				writer.WriteElementString(cm_xmlTagString, comment.Text);
				writer.WriteEndElement();
			}
			/*----------------------------------------------------------------------------------------------------------*/
			private static T convertValue<T>(string valueString) where T : struct
			{
				try
				{
					T value = (T)Convert.ChangeType(valueString, typeof(T));
					return value;
				}
				catch(FormatException)
				{
					Debug.LogError("Invalid String:" + valueString);
					return default(T);
				}
				catch(InvalidCastException)
				{
					Debug.LogError("Cannot cast " + valueString + " to " + typeof(T).ToString());
					return default(T);
				}
			}
		}	// DataManager
	}	// Window
}	// AEImitation
