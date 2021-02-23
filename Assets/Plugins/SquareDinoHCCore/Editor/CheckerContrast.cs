using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine;

#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace SquareDino.ChackerContrast
{
    class CheckerContrast : EditorWindow
    {
        #if UNITY_POST_PROCESSING_STACK_V2
            private static PostProcessProfile profile;
            private static PostProcessLayer postProcessingLayer;
            private static ColorGrading granding;
        #endif
        
        private static bool enable;
        private const string enableKey = "enableKey";
        
        [MenuItem("Window/SquareDino/CheckerContrast")]
        static void Init()
        {
            CheckerContrast window = (CheckerContrast) GetWindow(typeof(CheckerContrast), false, "CheckerContrast", true);
            window.maxSize = new Vector2(300f, 65f);
            window.minSize = window.maxSize;
        }

        private void OnGUI()
        {
#if UNITY_POST_PROCESSING_STACK_V2
            GUILayout.BeginHorizontal("box");
            
            GUI.enabled = !enable;
            
            if (GUILayout.Button("Enable contrast", GUILayout.Height(50)))
            {    
                enable = true;    
                EditorPrefs.SetInt(enableKey, 1);
            }   
            
            GUI.enabled = enable;
            
            if (GUILayout.Button("Disable contrast", GUILayout.Height(50)))
            {
                enable = false;
                EditorPrefs.SetInt(enableKey, 0);
            } 
            
            GUILayout.EndHorizontal();
#endif
        }

        private void OnEnable()
        {
            enable = EditorPrefs.GetInt(enableKey, 0) == 1;
            
            SceneManager.sceneLoaded -= SceneLoaded;
            SceneManager.sceneLoaded += SceneLoaded;
            EditorApplication.playModeStateChanged += TryDisable;
        }

        private void TryDisable(PlayModeStateChange mode)
        {
            if (mode == PlayModeStateChange.ExitingPlayMode)
            {
                Disable();
            }
        }

        private void SceneLoaded(Scene arg0, LoadSceneMode loadSceneMode)
        {
            Disable();
            Enable();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            EditorApplication.playModeStateChanged -= TryDisable;
        }

        private static void Enable()
        {
#if UNITY_POST_PROCESSING_STACK_V2            
            if(!enable) return;
            
            profile = ScriptableObject.CreateInstance<PostProcessProfile>();
            AssetDatabase.CreateAsset(profile, "Assets/CheckerContrastProfile.asset");
            granding = ScriptableObject.CreateInstance<ColorGrading>();
            AssetDatabase.CreateAsset(granding, "Assets/Granding.asset");
            
            granding.saturation.overrideState = true;
            granding.saturation.value = -100;
            profile.AddSettings(granding);
            profile.settings[0].enabled.value = true;
            
            EditorApplication.update += EnableDelay;
#endif
        }

        private static void EnableDelay()
        {
#if UNITY_POST_PROCESSING_STACK_V2            
            var camera = Camera.main;

            var checkerContrastGameObject = new GameObject("PostProcessing Volume");

            var boxCollier = checkerContrastGameObject.AddComponent<BoxCollider>();
            boxCollier.size = Vector3.one;

            checkerContrastGameObject.transform.position = camera.transform.position;
            checkerContrastGameObject.transform.SetParent(camera.transform);
            
            postProcessingLayer = camera.GetComponent<PostProcessLayer>(); 
            
            if(postProcessingLayer == null)
                postProcessingLayer = camera.gameObject.AddComponent<PostProcessLayer>();

            postProcessingLayer.volumeLayer = ~0;
            postProcessingLayer.volumeTrigger = camera.transform;
            
            var layer = checkerContrastGameObject.AddComponent<PostProcessVolume>();
            layer.blendDistance = 1000;
            layer.profile = profile;
            
            EditorApplication.update -= EnableDelay;
#endif
        }

        private static void Disable()
        {
            AssetDatabase.DeleteAsset("Assets/CheckerContrastProfile.asset");
            AssetDatabase.DeleteAsset("Assets/Granding.asset");
        }
    }
}