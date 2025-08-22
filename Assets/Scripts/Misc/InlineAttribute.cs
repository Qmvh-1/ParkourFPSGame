using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace Stubblefield.Utility
{
    public class InlineAttribute : PropertyAttribute { }
    
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InlineAttribute))]
    public class InlineDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new();
            property.Next(enterChildren: true);
            int depth = property.depth;
            do
            {
                if (property.depth != depth) break;
                root.Add(new PropertyField(property));
            } while (property.NextVisible(enterChildren: false));
            property.Reset();
            return root;
        }
    }
    #endif
}