using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Zeus
{
    public class UIVerticalMenu : ToggleGroup
    {
        [SerializeField] private bool _loop;

        private UIMenuItem[] _menuItems;

        private int _menuIndex;
        public int MenuIndex => _menuIndex;

        public IReadOnlyList<UIMenuItem> MenuItems => _menuItems;

        protected override void Start()
        {
            base.Start();

            _menuItems = GetComponentsInChildren<UIMenuItem>();
        }

        public void Initialization()
        {
            if (_menuItems == null) return;

            _menuIndex = 0;
            SwitchToggle(_menuIndex);

            for (int i = 0; i < _menuItems.Length; i++)
            {
                var item = _menuItems[i];
                item.RemoveTempListener();
            }
        }

        public UIMenuItem GetMenuItem(int index)
        {
            if (_menuItems == null || index < 0 || index > _menuItems.Length - 1) return null;

            return _menuItems[index];
        }

        public void SwitchToggle(int index)
        {
            if (_menuItems == null) return;

            if (_loop)
            {
                if (index < 0) index = _menuItems.Length - 1;
                if (index > _menuItems.Length - 1) index = 0;
            }
            else
            {
                if (index < 0) return;
                if (index > _menuItems.Length - 1) return;
            }

            GetMenuItem(_menuIndex).IsOn = false;
            _menuIndex = index;
            GetMenuItem(index).IsOn = true;
        }

        public virtual void OnNavigate(Vector2 value)
        {
            if (value == Vector2.zero) return;

            var index = 0;

            // up
            if (value.y > 0f) index = _menuIndex - 1;
            // down
            else index = _menuIndex + 1;

            SwitchToggle(index);
        }

        public virtual void OnSubmit()
        {
            GetMenuItem(_menuIndex)?.OnSubmit();
        }
    }
}
