using DG.Tweening;
using UnityEngine;

namespace Zeus
{
    public abstract class UIBase : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup _canvasGroup;

        protected void Start() => _OnStart();

        #region Virtual
        public virtual void _OnStart() => Close();
        public virtual void Open() => OpenAnimation();
        public virtual void Close() => CloseAnimation();
        public virtual void OpenAnimation()
        {
            if (_canvasGroup == null) return;

            var tween = _canvasGroup.DOFade(1, 0.1f).Pause();
            tween.timeScale = DOTween.unscaledTimeScale / DOTween.timeScale;
            tween.Play();
        }
        public virtual void CloseAnimation()
        {
            if (_canvasGroup == null) return;

            var tween = _canvasGroup.DOFade(0, 0.1f).Pause();
            tween.timeScale = DOTween.unscaledTimeScale / DOTween.timeScale;
            tween.Play();
        }
        #endregion

        #region Inputs
        public virtual void OnNavigate(Vector2 value) { }
        public virtual void OnSubmit() { }
        public virtual void OnCancel() { }
        #endregion
    }
}
