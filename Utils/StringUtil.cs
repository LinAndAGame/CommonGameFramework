using System.Text;

namespace CommonGameFramework.Utils
{
    /// <summary>简易字符串构建辅助（热路径可复用 StringBuilder）。</summary>
    public static class StringUtil
    {
        [ThreadStatic]
        static StringBuilder _builder;

        static StringBuilder SharedBuilder
        {
            get
            {
                if (_builder == null)
                    _builder = new StringBuilder(128);
                return _builder;
            }
        }

        /// <summary>拼接非空片段，以 separator 连接。</summary>
        public static string JoinNonEmpty(string separator, params string[] parts)
        {
            if (parts == null || parts.Length == 0) return string.Empty;

            var sb = SharedBuilder;
            sb.Clear();
            var first = true;
            for (var i = 0; i < parts.Length; i++)
            {
                if (string.IsNullOrEmpty(parts[i])) continue;
                if (first == false)
                    sb.Append(separator);
                sb.Append(parts[i]);
                first = false;
            }

            return sb.ToString();
        }
    }
}
