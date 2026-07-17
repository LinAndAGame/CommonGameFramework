using UnityEngine;

namespace CommonGameFramework.Time
{
    /// <summary>
    /// 每帧驱动 Clock.Tick。由 Clock.EnsureDriver 在首次使用时创建到 [Clock] 物体上，无需场景预挂。
    /// </summary>
    public sealed class ClockDriver : MonoBehaviour
    {
        void Update()
        {
            Clock.Tick();
        }
    }
}
