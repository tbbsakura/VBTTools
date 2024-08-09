using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using B83.Win32;

using System.IO;
using System.Threading.Tasks;
using VRM;

using UniGLTF;
using UniHumanoid;
using VRMShaders;
using System.Security.Cryptography;

using OgLikeVMT;
using SakuraScript.VBTTool;

public class VBTTools_FileDragAndDrop : MonoBehaviour
{
    private bool m_loadAsync = true;
    private bool m_loading = false;
    private Text m_topText;

    public EVMC4U.ExternalReceiver m_exrec;
    private UniHumanoid.HumanPoseTransfer m_target = null; // ロードされたVRMモデルのHumanPoseTransfer
    public VBTToolsSample m_sampleProject;

    void OnEnable ()
    {
        if ( m_exrec == null ) { 
            Debug.LogError( "EVMC4U.ExternalReceiver not speficied" );
            return;
        }
        if ( m_sampleProject == null ) { 
            Debug.LogError( "VBTToolsSample not speficied" );
            return;
        }

        if ( m_target != null ) m_exrec.Model = m_target.gameObject;

        // must be installed on the main thread to get the right thread id.
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnFiles;
        m_topText = GameObject.Find("TopText").GetComponent<Text>();

        /* buildせずにテストするときはドラッグ＆ドロップが効かないのでコードで読む場合、 true にする  */
        #if true 
        if ( m_sampleProject != null && m_sampleProject._animationTarget == null ) {
            const char separatorChar = '/';
            string modelFilepath = "Assets/SakuraShop_tbb/VRM_CC0/HairSample_Male.vrm"; //CC0 model
            modelFilepath = modelFilepath.Replace( separatorChar, System.IO.Path.DirectorySeparatorChar );
            //modelFilepath = "Z:\\VR\\_VRM\\fumifumi\\3c6.0_noshoe_.vrm";
            LoadModel(modelFilepath);
        }
        #endif
    }
    void OnDisable()
    {
        UnityDragAndDropHook.UninstallHook();
    }

    void OnFiles(List<string> aFiles, POINT aPos)
    {
        if ( m_loading ) return; // ignore
        LoadModel(aFiles[0].ToString()); // load only 1st file
    }
    
    void OnLoaded(RuntimeGltfInstance loaded)
    {
        var root = loaded.gameObject;

        root.transform.SetParent(transform, false);// このcomponentがattachされているオブジェクトを親に設定する
        root.transform.position.Set(-1,1,3); // 位置調整
        m_topText.text = "Showing Meshes...";
        foreach (var spring in root.GetComponentsInChildren<VRMSpringBone>())
        {
            spring.Setup();
            spring.gameObject.SetActive(false);
        }

        loaded.ShowMeshes(); // メッシュ表示
        var lah = loaded.GetComponent<VRMLookAtHead>();
        if (lah !=null)
        {
            lah.UpdateType = UpdateType.LateUpdate;
        }

        m_target = root.AddComponent<UniHumanoid.HumanPoseTransfer>();
        if ( m_target != null ) 
        {
            var animator = m_target.GetComponent<Animator>();
            if (animator != null)
            {
                if ( m_exrec is not null && m_target is not null && m_target.gameObject is not null ) m_exrec.Model = m_target.gameObject;


                if ( m_sampleProject is not null ) {
                    var sensorTemplateL = GameObject.Find("/origLeftHand/ControllerSensorL");
                    var sensorTemplateR = GameObject.Find("/origRightHand/ControllerSensorR");

                    m_sampleProject._animationTarget = animator;
                    m_sampleProject.SetHandler();

                    var leftsensor = new GameObject("LeftSensor");
                    leftsensor.transform.parent = animator.GetBoneTransform( HumanBodyBones.LeftHand );
                    leftsensor.transform.localPosition = sensorTemplateL.transform.localPosition;
                    leftsensor.transform.localRotation = sensorTemplateL.transform.localRotation;
                    m_sampleProject._vbtHandPosTrack._transformVirtualLController = leftsensor.transform;
                    var sl = GameObject.Find("/origLeftHand/ControllerSensorL/Sphere");
                    if (sl) {
                        sl.transform.parent = leftsensor.transform;
                        sl.transform.localPosition = Vector3.zero;
                        m_sampleProject._sphereL = sl.transform;
                    }

                    var rightsensor = new GameObject("RightSensor");
                    rightsensor.transform.parent = animator.GetBoneTransform( HumanBodyBones.RightHand );
                    rightsensor.transform.localPosition = sensorTemplateR.transform.localPosition;
                    rightsensor.transform.localRotation = sensorTemplateR.transform.localRotation;
                    m_sampleProject._vbtHandPosTrack._transformVirtualRController = rightsensor.transform;
                    var sr = GameObject.Find("/origLeftHand/ControllerSensorR/Sphere");
                    if (sr) {
                        sr.transform.parent = rightsensor.transform;
                        sr.transform.localPosition = Vector3.zero;
                        m_sampleProject._sphereR = sr.transform;
                    }

                }
            }
        }
        if (m_topText != null) m_topText.text = "VRM loaded";
    }

    private async void LoadModel(string path)
    {
        var ext = Path.GetExtension(path).ToLower();
        if ( ext == ".vrm" ) 
        {
            if ( m_topText != null ) m_topText.text = "Loading VRM...";
            if (m_target != null) // unload
            {
                GameObject.Destroy(m_target.gameObject);
                m_target = null;
            }
            using ( var data = new GlbFileParser(path).Parse() ) 
            {
                var vrm = new VRMData(data);
                using ( var context = new VRMImporterContext(vrm) ) 
                {
                    var loaded = default(RuntimeGltfInstance);
                    m_loading = true;
                    if (m_loadAsync)
                    {
                        try {
                            loaded = await context.LoadAsync(new VRMShaders.RuntimeOnlyAwaitCaller());
                        } catch(System.Exception e) {
                            System.Type exceptionType = e.GetType();
                            if (m_topText != null) m_topText.text = "Load failed. : " + e.Message;
                            m_loading = false;
                            return;
                        }
                    }
                    else
                    {
                        try {
                            loaded = context.Load();
                        } catch(System.AggregateException ae1) { // 2層Aggregateが帰ってきたりするので
                            foreach ( var eInner in ae1.InnerExceptions ) 
                            {
                                if (eInner.GetType() == typeof(System.AggregateException) ) 
                                {
                                    System.AggregateException ae2 = (System.AggregateException)eInner;
                                    foreach ( var eInner2 in ae2.InnerExceptions ) 
                                    {
                                        if (m_topText != null) m_topText.text = "Load failed. : " + eInner2.Message;
                                        m_loading = false;
                                        return;
                                    }
                                }
                                else {
                                    if (m_topText != null) m_topText.text = "Load failed. : " + eInner.Message;
                                    m_loading = false;
                                    return;
                                }
                            }
                            m_loading = false;
                            return;
                        } catch (System.Exception e) {
                            if (m_topText != null) m_topText.text = "Load failed. : " + e.Message;
                            m_loading = false;
                            return;
                        }
                    }
                    OnLoaded(loaded);
                }
            }
        }
        else {
            if (m_topText != null) m_topText.text = "Dropped is not .vrm file.";
        }
        m_loading = false;
    }
}

