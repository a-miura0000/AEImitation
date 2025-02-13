/** 
 *	@file	EventDataImageReflector.cs
 *	@brief	イベントデータをgameObjectに反映
 *
 *	@author miura
 */
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using EventData;

public class EventDataImageReflector : EventDataReflectorBase<ImageElement>
{
	/*----------------------------------------------------------------------------------------------------------*/
	private Image					m_image;
	private Animator				m_animator;
	private int						m_transitionNo;
	private string					m_selectCondition;
	/*----------------------------------------------------------------------------------------------------------*/
	protected override void awake()
	{
		gameObject.name = "image";
		m_image = gameObject.GetComponent<Image>();
		if(m_image == null)
		{
			m_image = gameObject.AddComponent<Image>();
			m_image.color = Color.white;
			RectTransform rectTransform = m_image.GetComponent<RectTransform>();
			rectTransform.sizeDelta = Vector2.zero;
			rectTransform.anchoredPosition = Vector2.zero; 
			rectTransform.localScale = Vector3.one;
		}
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected override object setAsset(Asset asset)
	{
		object data = null;
#if UNITY_EDITOR
		if(asset.Type == AssetType.Image)
		{
			var spriteName = ((ImageAsset)asset).Name;
			Sprite[] sprites = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(asset.Path).OfType<Sprite>().ToArray();
			foreach(var sprite in sprites)
			{
				if(sprite.name == spriteName)
				{
					data = sprite;
					setSprite(sprite);
					break;
				}
			}
		}
		else if(asset.Type == AssetType.Animator)
		{
			var animator = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(asset.Path);
			setAnimator(animator);
			data = animator;
		}
#else
				
#endif
		return data;
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected override void setAsset(AssetType type, UnityEngine.Object obj)
	{
		if(type == AssetType.Image)
		{
			setSprite((Sprite)obj);
		}
		else if(type == AssetType.Animator)
		{
			setAnimator(obj);
		}
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected override void initializePlay()
	{
		m_transitionNo = -1;
		if(m_animator != null)
		{
			foreach(var asset in getElement().AssetList)
			{
				if(asset.Type == AssetType.Animator)
				{
					m_selectCondition = ((AnimatorAsset)asset).SelectCondition;
					break;
				}
			}
		}
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected override bool reflectData(object elementData)
	{
		if(!(elementData is ImageElement.Data)) return false;
		var data = (ImageElement.Data)elementData;
		
		var transform = (RectTransform)gameObject.transform;
		transform.localPosition = data.position;
		transform.localRotation = Quaternion.Euler(0f, 0f, data.rotation);
		transform.localScale = new Vector3(data.scale.x, data.scale.y, 1f);
		
		m_image.color = data.color;
		m_image.raycastTarget = data.raycastTarget;
		
		if(data.transitionNo != m_transitionNo)
		{
			m_transitionNo = data.transitionNo;
			if(m_animator != null)
			{
				if(!string.IsNullOrEmpty(m_selectCondition))
				{
					m_animator.SetInteger(m_selectCondition, m_transitionNo);
				}
			}
		}
		return true;
	}
	/*----------------------------------------------------------------------------------------------------------*/
	private void setSprite(Sprite sprite)
	{
		m_image.sprite = sprite;
		((RectTransform)gameObject.transform).sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);
	}
	/*----------------------------------------------------------------------------------------------------------*/
	private void setAnimator(UnityEngine.Object obj)
	{
		var controller = obj as RuntimeAnimatorController;
		if(controller == null) return;
		if(m_animator == null)
		{
			m_animator = gameObject.AddComponent<Animator>();
		}
		m_animator.runtimeAnimatorController = controller;
		// todo
		//animator.cullingMode = AnimatorCullingMode.CullCompletely;
		//animator.enabled = false;
	}
}	// EventDataImageReflector
