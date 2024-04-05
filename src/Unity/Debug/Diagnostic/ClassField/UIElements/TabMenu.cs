using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace AnotherECS.Debug.Diagnostic.UIElements
{
    public class TabMenu : VisualElement
    {
        public static readonly string ussClassName = "tabmenu-field";
        public static readonly string tabUssClassName = ussClassName + "__tab";
        public static readonly string contentUssClassName = ussClassName + "__content";

        private int _selectIndex = -1;
        private int _tabsCount = 0;

        public int TabsCount => _tabsCount;

        public VisualElement[] Tabs
        {
            get => this.Q("tabmenu-tab").Children().ToArray();
            set
            {
                var container = this.Q("tabmenu-tab");
                container.Clear();
                for (int i = 0; i < value.Length; ++i)
                {
                    container.Add(value[i]);
                    int index = i;
                    value[i].RegisterCallback<ClickEvent>((p) => OnTabClick(index));
                }
                EnableTab(_selectIndex);
                _tabsCount = value.Length;
            }
        }

        public VisualElement[] Contents
        {
            get => this.Q("tabmenu-content").Children().ToArray();
            set
            {
                var container = this.Q("tabmenu-content");
                container.Clear();
                for (int i = 0; i < value.Length; ++i)
                {
                    container.Add(value[i]);
                }
                EnableTab(_selectIndex);
            }
        }

        public int SelectIndex
        {
            get => _selectIndex;
            set
            {
                value = Math.Clamp(value, 0, this.Q("tabmenu-tab").childCount);
                if (_selectIndex != value)
                {
                    _selectIndex = value;
                    EnableTab(_selectIndex);
                }
            }
        }

        public TabMenu()
        {
            base.focusable = true;
            base.tabIndex = 0;
            base.delegatesFocus = true;
            AddToClassList(ussClassName);
            var tabContainer = new VisualElement
            {
                name = "tabmenu-tab",
                focusable = true,
                tabIndex = -1,
            };
            tabContainer.AddToClassList(tabUssClassName);
            Add(tabContainer);

            var contentContainer = new VisualElement
            {
                name = "tabmenu-content",
                focusable = true,
                tabIndex = -1,
            };
            contentContainer.AddToClassList(contentUssClassName);
            Add(contentContainer);
        }

        public void SetTabsAsString(IEnumerable<string> labels)
        {
            Tabs = labels.Select(p => new Button { text = p }).ToArray();
        }

        private void EnableTab(int index)
        {
            var container = this.Q("tabmenu-content");
            for(int i = 0; i < container.childCount; ++i)
            {
                container.ElementAt(i).style.display = (index == i) ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void OnTabClick(int index)
        {
            EnableTab(index);
        }
    }
}