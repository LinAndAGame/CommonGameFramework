namespace CommonGameFramework.DragPlace
{
    /// <summary>
    /// Instantiate 拖拽副本期间关掉自动注册，避免副本上的 Source/Place/Controller 加入正式会话。
    /// </summary>
    internal static class DragCloneGuard
    {
        static int _depth;

        public static bool IsCloning
        {
            get { return _depth > 0; }
        }

        // 进入克隆实例化。
        public static void Enter()
        {
            _depth++;
        }

        // 离开克隆实例化。
        public static void Exit()
        {
            if (_depth > 0)
            {
                _depth--;
            }
        }
    }
}
