using UnityEngine;
#if UNITY_EDITOR
#endif
namespace Core.Utilities
{
    // 1. Định nghĩa cái tên Attribute để dùng trong code
    public class MinMaxSliderAttribute : PropertyAttribute
    {
        public float Min { get; private set; }
        public float Max { get; private set; }

        public MinMaxSliderAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}
#if UNITY_EDITOR
#endif