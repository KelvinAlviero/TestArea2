using UnityEngine;
using UnityEngine.UI;

namespace Extras
{
    [RequireComponent(typeof(Canvas)), RequireComponent(typeof(GraphicRaycaster))]
    public abstract class UIPage : MonoBehaviour
    {
        protected bool isPageDisplayed;
        public bool IsPageDisplayed { get => isPageDisplayed; set => isPageDisplayed = value; }

        protected Canvas canvas;
        public Canvas Canvas => canvas;

        protected GraphicRaycaster graphicRaycaster;
        public GraphicRaycaster GraphicRaycaster => graphicRaycaster;

        private string defaultName;

        public void CacheComponents()
        {
            defaultName = name;

            canvas = GetComponent<Canvas>();
            graphicRaycaster = GetComponent<GraphicRaycaster>();
        }
//----- UIController
        public static class UIController
        {
            public static void HidePage<T>() where T : MonoBehaviour
            {
                // Do nothing - let the calling script handle hiding
            }
            
            public static void ShowPage<T>() where T : MonoBehaviour
            {
                // Do nothing - let the calling script handle hiding
            }
            public static void OnPageOpened(object page) { }
            public static void OnPopupWindowOpened(object page) { }
            public static void OnPageClosed(object page) { }
            public static void OnPopupWindowClosed(object page) { }
        }

//----- Canvas Control ----//
        public void EnableCanvas()
        {
            isPageDisplayed = true;

            canvas.enabled = true;

#if UNITY_EDITOR
            name = string.Format("{0} (Active)", defaultName);
#endif
        }

        public void DisableCanvas()
        {
            isPageDisplayed = false;

            canvas.enabled = false;

#if UNITY_EDITOR
            name = defaultName;
#endif
        }

        public abstract void PlayShowAnimation();
        public abstract void PlayHideAnimation();

        public virtual void Unload()
        {
            isPageDisplayed = false;

            canvas.enabled = false;
        }
    }
}