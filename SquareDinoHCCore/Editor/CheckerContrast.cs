using UnityEditor;
using UnityEngine;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace SquareDino.ChackerContrast
{
    class CheckerContrast : EditorWindow
    {
        private bool isHaveAsset;
        
        #if UNITY_POST_PROCESSING_STACK_V2
            private PostProcessProfile profile;
            private PostProcessLayer postProcessingLayer;
        #endif

        private static bool haveAsset;
    
        [MenuItem("Window/SquareDino/CheckerContrast")]
        static void Init()
        {
            CheckerContrast window = (CheckerContrast) GetWindow(typeof(CheckerContrast));
        }

        private void OnGUI()
        {
#if UNITY_POST_PROCESSING_STACK_V2
            if (profile != null)
                isHaveAsset = true;
            else
                isHaveAsset = false;
            
            GUI.enabled = !isHaveAsset;
            
                if (GUILayout.Button("Create profile to PostProcessing"))
                {
                    if (profile == null)
                    {                    
                        profile = ScriptableObject.CreateInstance<PostProcessProfile>();
                        AssetDatabase.CreateAsset(profile, "Assets/CheckerContrastProfile.asset");
                        //EditorUtility.FocusProjectWindow();
                        //Selection.activeObject = profile;
                        
                        var granding = new ColorGrading();
                        granding.saturation.overrideState = true;
                        granding.saturation.value = -100;
                        
                        var grandingLinq = profile.AddSettings(granding);
                        grandingLinq.enabled.value = true;
                        EditorUtility.SetDirty(profile);
                        AssetDatabase.SaveAssets();
                    }  
                }
            
            GUI.enabled = true;

            GUI.enabled = isHaveAsset;
            
            if (GUILayout.Button("Enable contrast"))
            {
                var camera = Camera.main;

                if (go != null)
                {
                    go.transform.position = camera.transform.position;
                }
            
                if (camera != null)
                {    
                    go = new GameObject("PostProcessing Volume");
                    var boxCollier = go.AddComponent<BoxCollider>();
                    boxCollier.size = new Vector3(1, 1, 1);
                    
                    go.transform.position = camera.transform.position;
                    postProcessingLayer = camera.gameObject.AddComponent<PostProcessLayer>();
                    postProcessingLayer.volumeLayer = ~0;
                    var layer = go.AddComponent<PostProcessVolume>();
                    layer.blendDistance = 1000;
                    layer.profile = profile;
                }
            }

            if (GUILayout.Button("Disable contrast"))
            {
                DestroyImmediate(go);
                DestroyImmediate(postProcessingLayer);
            }
#endif
        }
    }
}