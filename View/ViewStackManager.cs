using System.Collections.Generic;

namespace CardMaster.Framework.View
{
    /// <summary>
    /// 按层管理 View 栈，支持 Push / Pop / Replace。
    /// </summary>
    public class ViewStackManager
    {
        readonly Dictionary<UILayer, Stack<BaseView>> _stacks = new Dictionary<UILayer, Stack<BaseView>>();

        Stack<BaseView> GetStack(UILayer layer)
        {
            if (_stacks.TryGetValue(layer, out var stack) == false)
            {
                stack = new Stack<BaseView>();
                _stacks[layer] = stack;
            }
            return stack;
        }

        public BaseView GetTop(UILayer layer)
        {
            var stack = GetStack(layer);
            return stack.Count > 0 ? stack.Peek() : null;
        }

        public void Push(UILayer layer, BaseView view)
        {
            if (view == null) return;
            GetStack(layer).Push(view);
        }

        public BaseView Pop(UILayer layer)
        {
            var stack = GetStack(layer);
            return stack.Count > 0 ? stack.Pop() : null;
        }

        public void Replace(UILayer layer, BaseView view)
        {
            var stack = GetStack(layer);
            if (stack.Count > 0) stack.Pop();
            if (view != null) stack.Push(view);
        }

        public void Clear(UILayer layer) => GetStack(layer).Clear();
    }
}
