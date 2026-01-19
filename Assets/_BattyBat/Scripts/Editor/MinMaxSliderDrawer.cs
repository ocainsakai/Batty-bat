using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
public class MinMaxSliderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 1. Chỉ hoạt động với Vector2
        if (property.propertyType != SerializedPropertyType.Vector2)
        {
            EditorGUI.LabelField(position, label, "Use MinMaxSlider on Vector2 only!");
            return;
        }

        MinMaxSliderAttribute attr = attribute as MinMaxSliderAttribute;
        
        // Lấy giá trị hiện tại
        Vector2 range = property.vector2Value;
        float minVal = range.x;
        float maxVal = range.y;

        // 2. Vẽ cái Nhãn (Label) trước và lấy phần diện tích còn lại để vẽ thanh trượt
        // Hàm này tự động căn lề thẳng tắp với các biến khác trong Unity
        Rect controlRect = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // 3. Tính toán kích thước các thành phần con
        float fieldWidth = 40f; // Chiều rộng ô nhập số (40px là vừa đẹp)
        float spacing = 5f;     // Khoảng cách giữa các ô

        // Vị trí ô Min (Bên trái cùng của vùng điều khiển)
        Rect minFieldRect = new Rect(controlRect.x, controlRect.y, fieldWidth, controlRect.height);
        
        // Vị trí ô Max (Bên phải cùng)
        Rect maxFieldRect = new Rect(controlRect.xMax - fieldWidth, controlRect.y, fieldWidth, controlRect.height);
        
        // Vị trí thanh Slider (Nằm giữa 2 ô số)
        Rect sliderRect = new Rect(
            minFieldRect.xMax + spacing, 
            controlRect.y, 
            maxFieldRect.xMin - minFieldRect.xMax - spacing, 
            controlRect.height
        );

        // 4. Vẽ giao diện và xử lý logic
        
        // -- Vẽ ô nhập số Min --
        // Cho phép người dùng gõ số chính xác vào
        minVal = EditorGUI.FloatField(minFieldRect, float.Parse(minVal.ToString("F2"))); 
        
        // -- Vẽ ô nhập số Max --
        maxVal = EditorGUI.FloatField(maxFieldRect, float.Parse(maxVal.ToString("F2")));

        // Giới hạn giá trị nhập tay để không vượt quá giới hạn attribute
        // Ví dụ: Không cho nhập Min thấp hơn -5 nếu giới hạn là -5
        minVal = Mathf.Clamp(minVal, attr.Min, maxVal); // Min không được lớn hơn Max
        maxVal = Mathf.Clamp(maxVal, minVal, attr.Max); // Max không được nhỏ hơn Min

        // -- Vẽ thanh Slider --
        EditorGUI.MinMaxSlider(sliderRect, ref minVal, ref maxVal, attr.Min, attr.Max);

        // 5. Lưu giá trị ngược lại vào biến
        property.vector2Value = new Vector2(minVal, maxVal);
    }
}