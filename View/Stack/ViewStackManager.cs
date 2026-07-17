using System.Collections.Generic;

namespace CommonGameFramework.View
{
    /// <summary>
    /// 按 ViewLayer 分区的视图栈；日常请走 ViewFlowController，本类供进阶调试。
    /// </summary>
    public class ViewStackManager
    {
        readonly Dictionary<ViewLayer, Stack<BaseView>> _stacks = new Dictionary<ViewLayer, Stack<BaseView>>();

        Stack<BaseView> GetStack(ViewLayer layer)
        {
            if (layer == null) layer = DefaultViewLayer.Instance;
            if (_stacks.TryGetValue(layer, out var stack) == false)
            {
                stack = new Stack<BaseView>();
                _stacks[layer] = stack;
            }

            return stack;
        }

        public BaseView GetTop(ViewLayer layer)
        {
            var stack = GetStack(layer);
            return stack.Count > 0 ? stack.Peek() : null;
        }

        public bool Contains(ViewLayer layer, BaseView view)
        {
            if (view == null) return false;
            foreach (var item in GetStack(layer))
            {
                if (item == view) return true;
            }

            return false;
        }

        public void Push(ViewLayer layer, BaseView view)
        {
            if (view == null) return;
            GetStack(layer).Push(view);
        }

        public BaseView Pop(ViewLayer layer)
        {
            var stack = GetStack(layer);
            return stack.Count > 0 ? stack.Pop() : null;
        }

        /// <summary>从该层栈中移除指定 view（不要求在栈顶）。</summary>
        public bool Remove(ViewLayer layer, BaseView view)
        {
            if (view == null) return false;

            var stack = GetStack(layer);
            if (stack.Count == 0) return false;

            var buffer = new Stack<BaseView>(stack.Count);
            var removed = false;
            while (stack.Count > 0)
            {
                var item = stack.Pop();
                if (item == view)
                {
                    removed = true;
                    continue;
                }

                buffer.Push(item);
            }

            while (buffer.Count > 0)
                stack.Push(buffer.Pop());

            return removed;
        }

        public void Replace(ViewLayer layer, BaseView view)
        {
            var stack = GetStack(layer);
            stack.Clear();
            if (view != null) stack.Push(view);
        }

        public void Clear(ViewLayer layer) => GetStack(layer).Clear();
    }
}
