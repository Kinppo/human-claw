using UnityEditor;
using UnityEngine;

public enum BodyPartName
{
    Body,
    RightArm,
    LeftArm,
    RightFoot,
    LeftFoot,
}

public class BodyPart : MonoBehaviour
{
    public BodyPartName bodyPartName;
    public Transform bone;
    public SpriteRenderer sprite;
    [HideInInspector] public Transform childBone;

    private void Update()
    {
    }

    #region Editor

#if UNITY_EDITOR

    [CustomEditor(typeof(BodyPart))]
    public class PlatformEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var bodyPart = (BodyPart) target;
            switch (bodyPart.bodyPartName)
            {
                case BodyPartName.RightFoot:
                    bodyPart.childBone =
                        (Transform) EditorGUILayout.ObjectField("Child Bone", bodyPart.childBone, typeof(Transform));
                    break;
                case BodyPartName.LeftFoot:
                    bodyPart.childBone =
                        (Transform) EditorGUILayout.ObjectField("Child Bone", bodyPart.childBone, typeof(Transform));
                    break;
            }
            
            // GUILayout.Space(10);
            // GUILayout.BeginHorizontal();
            // if (GUILayout.Button("Log Out"))
            //     print("Yes");
            // GUILayout.EndHorizontal();

            if (GUI.changed) EditorUtility.SetDirty(bodyPart);
        }
    }
#endif

    #endregion
}