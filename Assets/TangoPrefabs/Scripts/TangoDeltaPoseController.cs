//-----------------------------------------------------------------------
// <copyright file="TangoDeltaPoseController.cs" company="Google">
//
// Copyright 2015 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections;
using UnityEngine;
using Tango;

/// <summary>
/// This is a more advanced movement controller based on the poses returned
/// from the Tango service.
/// 
/// This updates the position with deltas, so movement can be done using a
/// CharacterController, or physics, or anything else that wants deltas.
/// </summary>
public class TangoDeltaPoseController : MonoBehaviour, ITangoLifecycle, ITangoPose
{
    /// <summary>
    /// The change in time since the last pose update.
    /// </summary>
    [HideInInspector]
    public float m_poseDeltaTime;

    /// <summary>
    /// Total number of poses ever received by this controller.
    /// </summary>
    [HideInInspector]
    public int m_poseCount;

    /// <summary>
    /// The most recent pose status received.
    /// </summary>
    [HideInInspector]
    public TangoEnums.TangoPoseStatusType m_poseStatus;

    /// <summary>
    /// If set, the Area Description used for relocalization.
    /// </summary>
    [HideInInspector]
    public AreaDescription m_areaDescription;

    /// <summary>
    /// The most recent pose timestamp received.
    /// </summary>
    [HideInInspector]
    public float m_poseTimestamp;

    /// <summary>
    /// The most recent Tango rotation.
    /// 
    /// This is different from the pose's rotation because it takes into
    /// account teleporting and the clutch.
    /// </summary>
    [HideInInspector]
    public Vector3 m_tangoPosition;
    
    /// <summary>
    /// The most recent Tango position.
    /// 
    /// This is different from the pose's position because it takes into
    /// account teleporting and the clutch.
    /// </summary>
    [HideInInspector]
    public Quaternion m_tangoRotation;

    /// <summary>
    /// If set, use the character controller to move the object.
    /// </summary>
    public bool m_characterMotion;

    /// <summary>
    /// If set, display a Clutch UI via OnGUI.
    /// </summary>
    public bool m_enableClutchUI;

    /// <summary>
    /// If set, this contoller will use the Device with respect Area Description frame pose.
    /// </summary>
    public bool m_useAreaDescriptionPose;

    /// <summary>
    /// The TangoApplication being listened to.
    /// </summary>
    private TangoApplication m_tangoApplication;

    /// <summary>
    /// The previous Tango's position.
    /// 
    /// This is different from the pose's position because it takes into
    /// account teleporting and the clutch.
    /// </summary>
    private Vector3 m_prevTangoPosition;

    /// <summary>
    /// The previous Tango's rotation.
    /// 
    /// This is different from the pose's rotation because it takes into
    /// account teleporting and the clutch.
    /// </summary>
    private Quaternion m_prevTangoRotation;

    /// <summary>
    /// If the clutch is active.
    /// 
    /// When the clutch is active, the Tango device can be moved and rotated
    /// without the controller actually moving.
    /// </summary>
    /// <value><c>true</c> if clutch active; otherwise, <c>false</c>.</value>
    public bool ClutchActive
    {
        get
        {
            return m_clutchActive;
        }

        set
        {
            if (m_clutchActive && !value)
            {
                SetPose(transform.position, transform.rotation);
            }
            m_clutchActive = value;
        }
    }

    /// <summary>
    /// Internal data about the clutch.
    /// </summary>
    private bool m_clutchActive;

    /// <summary>
    /// Internal CharacterController used for motion.
    /// </summary>
    private CharacterController m_characterController;

    // We use couple of matrix transformation to convert the pose from Tango coordinate
    // frame to Unity coordinate frame.
    // The full equation is:
    //     Matrix4x4 uwTuc = uwTss * ssTd * dTuc;
    //
    // uwTuc: Unity camera with respect to Unity world, this is the desired matrix.
    // uwTss: Constant matrix converting start of service frame to Unity world frame.
    // ssTd: Device frame with repect to start of service frame, this matrix denotes the 
    //       pose transform we get from pose callback.
    // dTuc: Constant matrix converting Unity world frame frame to device frame.
    //
    // Please see the coordinate system section online for more information:
    //     https://developers.google.com/project-tango/overview/coordinate-systems

    /// <summary>
    /// Matrix that transforms from Start of Service to the Unity World.
    /// </summary>
    private Matrix4x4 m_uwTss;

    /// <summary>
    /// Matrix that transforms from the Unity Camera to Device.
    /// </summary>
    private Matrix4x4 m_dTuc;

    /// <summary>
    /// Matrix that transforms from the Unity Camera to the Unity World.
    /// 
    /// Needed to calculate offsets.
    /// </summary>
    private Matrix4x4 m_uwTuc;

    /// <summary>
    /// Matrix that transforms the Unity World taking into account offsets from calls
    /// to <c>SetPose</c>.
    /// </summary>
    private Matrix4x4 m_uwOffsetTuw;

    /// <summary>
    /// Gets the unity world offset which can be then multiplied to any transform to apply this offset.
    /// </summary>
    /// <value>The unity world offset.</value>
    public Matrix4x4 UnityWorldOffset
    {
        get
        {
            return m_uwOffsetTuw;
        }
    }

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    public void Awake()
    {
        // Constant matrix converting start of service frame to Unity world frame.
        m_uwTss = new Matrix4x4();
        m_uwTss.SetColumn(0, new Vector4(1.0f, 0.0f, 0.0f, 0.0f));
        m_uwTss.SetColumn(1, new Vector4(0.0f, 0.0f, 1.0f, 0.0f));
        m_uwTss.SetColumn(2, new Vector4(0.0f, 1.0f, 0.0f, 0.0f));
        m_uwTss.SetColumn(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));

        // Constant matrix converting Unity world frame frame to device frame.
        m_dTuc = new Matrix4x4();
        m_dTuc.SetColumn(0, new Vector4(1.0f, 0.0f, 0.0f, 0.0f));
        m_dTuc.SetColumn(1, new Vector4(0.0f, 1.0f, 0.0f, 0.0f));
        m_dTuc.SetColumn(2, new Vector4(0.0f, 0.0f, -1.0f, 0.0f));
        m_dTuc.SetColumn(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));

        m_poseDeltaTime = -1.0f;
        m_poseTimestamp = -1.0f;
        m_poseCount = -1;
        m_poseStatus = TangoEnums.TangoPoseStatusType.NA;
        m_prevTangoRotation = m_tangoRotation = Quaternion.identity;
        m_prevTangoPosition = m_tangoPosition = Vector3.zero;

        m_uwTuc = Matrix4x4.identity;
        m_uwOffsetTuw = Matrix4x4.identity;
    }

    /// <summary>
    /// Start is called on the frame when a script is enabled.
    /// </summary>
    public void Start()
    {
        m_tangoApplication = FindObjectOfType<TangoApplication>();
        m_characterController = GetComponent<CharacterController>();
        
        if (m_tangoApplication != null)
        {
            m_tangoApplication.Register(this);
            if (AndroidHelper.IsTangoCorePresent())
            {
                // Request Tango permissions
                m_tangoApplication.RequestPermissions();
            }
            else
            {
                // If no Tango Core is present let's tell the user to install it!
                StartCoroutine(_InformUserNoTangoCore());
            }
        }
        else
        {
            Debug.Log("No Tango Manager found in scene.");
        }

        SetPose(transform.position, transform.rotation);
    }

    /// <summary>
    /// Update is called every frame.
    /// </summary>
    public void Update()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (m_tangoApplication != null)
            {
                m_tangoApplication.Shutdown();
            }

            // This is a temporary fix for a lifecycle issue where calling
            // Application.Quit() here, and restarting the application immediately,
            // results in a hard crash.
            AndroidHelper.AndroidQuit();
        }
        #else
        Vector3 tempPosition = transform.position;
        Quaternion tempRotation = transform.rotation;
        PoseProvider.GetMouseEmulation(ref tempPosition, ref tempRotation);
        transform.rotation = tempRotation;
        transform.position = tempPosition;
        #endif
    }

    /// <summary>
    /// OnGUI is called for rendering and handling GUI events.
    /// </summary>
    public void OnGUI()
    {
        if (!m_enableClutchUI)
        {
            return;
        }

        bool buttonState = GUI.RepeatButton(new Rect(10, 500, 200, 200), "<size=40>CLUTCH</size>");

        // OnGUI is called multiple times per frame.  Make sure to only care about the last one.
        if (Event.current.type == EventType.Repaint)
        {
            ClutchActive = buttonState;
        }
    }
    
    /// <summary>
    /// Unity callback when application is paused.
    /// </summary>
    /// <param name="pauseStatus">The pauseStatus as reported by Unity.</param>
    public void OnApplicationPause(bool pauseStatus)
    {
        m_poseDeltaTime = -1.0f;
        m_poseTimestamp = -1.0f;
        m_poseCount = -1;
        m_poseStatus = TangoEnums.TangoPoseStatusType.NA;
    }

    /// <summary>
    /// OnTangoPoseAvailable is called from Tango when a new Pose is available.
    /// </summary>
    /// <param name="pose">The new Tango pose.</param>
    public void OnTangoPoseAvailable(TangoPoseData pose)
    {
        // Get out of here if the pose is null
        if (pose == null)
        {
            Debug.Log("TangoPoseDate is null.");
            return;
        }

        // Only interested in pose updates relative to the start of service pose.
        if (!m_useAreaDescriptionPose)
        {
            if (pose.framePair.baseFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_START_OF_SERVICE
                && pose.framePair.targetFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE)
            {
                _UpdateTransformationFromPose(pose);
            }
        }
        else
        {
            if (pose.framePair.baseFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_AREA_DESCRIPTION
                && pose.framePair.targetFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE)
            {
                _UpdateTransformationFromPose(pose);
            }
        }
    }

    /// <summary>
    /// Sets the pose on this component.  Future Tango pose updates will move relative to this pose.
    /// </summary>
    /// <param name="pos">New position.</param>
    /// <param name="quat">New rotation.</param>
    public void SetPose(Vector3 pos, Quaternion quat)
    {
        Quaternion uwQuc = Quaternion.LookRotation(m_uwTuc.GetColumn(2), m_uwTuc.GetColumn(1));
        Vector3 eulerAngles = quat.eulerAngles;
        eulerAngles.x = uwQuc.eulerAngles.x;
        eulerAngles.z = uwQuc.eulerAngles.z;
        quat.eulerAngles = eulerAngles;

        m_uwOffsetTuw = Matrix4x4.TRS(pos, quat, Vector3.one) * m_uwTuc.inverse;

        m_prevTangoPosition = m_tangoPosition = pos;
        m_prevTangoRotation = m_tangoRotation = quat;
        m_characterController.transform.position = pos;
        m_characterController.transform.rotation = quat;
    }

    /// <summary>
    /// This is called when the permission granting process is finished.
    /// </summary>
    /// <param name="permissionsGranted"><c>true</c> if permissions were granted, otherwise <c>false</c>.</param>
    public void OnTangoPermissions(bool permissionsGranted)
    {
        if (permissionsGranted)
        {
            if (m_tangoApplication.m_enableADFLoading && m_areaDescription != null)
            {
                m_tangoApplication.Startup(m_areaDescription);
            }
            else
            {
                m_tangoApplication.Startup(null);
            }
        }
        else
        {
            AndroidHelper.ShowAndroidToastMessage("Motion Tracking Permissions Needed", true);
        }
    }
    
    /// <summary>
    /// This is called when succesfully connected to the Tango service.
    /// </summary>
    public void OnTangoServiceConnected()
    {
    }
    
    /// <summary>
    /// This is called when disconnected from the Tango service.
    /// </summary>
    public void OnTangoServiceDisconnected()
    {
    }

    /// <summary>
    /// Set controller's transformation based on received pose.
    /// </summary>
    /// <param name="pose">Received Tango pose data.</param>
    private void _UpdateTransformationFromPose(TangoPoseData pose)
    {
        // Remember the previous position, so you can do delta motion
        m_prevTangoPosition = m_tangoPosition;
        m_prevTangoRotation = m_tangoRotation;

        // The callback pose is for device with respect to start of service pose.
        if (pose.status_code == TangoEnums.TangoPoseStatusType.TANGO_POSE_VALID)
        {
            // Construct matrix for the start of service with respect to device from the pose.
            Vector3 posePosition = new Vector3((float)pose.translation[0],
                                               (float)pose.translation[1],
                                               (float)pose.translation[2]);
            Quaternion poseRotation = new Quaternion((float)pose.orientation[0],
                                                     (float)pose.orientation[1],
                                                     (float)pose.orientation[2],
                                                     (float)pose.orientation[3]);
            Matrix4x4 ssTd = Matrix4x4.TRS(posePosition, poseRotation, Vector3.one);

            // Calculate matrix for the camera in the Unity world, taking into account offsets.
            m_uwTuc = m_uwTss * ssTd * m_dTuc;
            Matrix4x4 uwOffsetTuc = m_uwOffsetTuw * m_uwTuc;

            // Extract final position, rotation.
            m_tangoPosition = uwOffsetTuc.GetColumn(3);
            m_tangoRotation = Quaternion.LookRotation(uwOffsetTuc.GetColumn(2), uwOffsetTuc.GetColumn(1));

            // Other pose data -- Pose count gets reset if pose status just became valid.
            if (pose.status_code != m_poseStatus)
            {
                m_poseCount = 0;
            }
            m_poseCount++;

            // Other pose data -- Pose delta time.
            m_poseDeltaTime = (float)pose.timestamp - m_poseTimestamp;
            m_poseTimestamp = (float)pose.timestamp;
        }
        m_poseStatus = pose.status_code;

        if (m_clutchActive)
        {
            // When clutching, preserve position.
            m_tangoPosition = m_prevTangoPosition;

            // When clutching, preserve yaw, keep changes in pitch, roll.
            Vector3 rotationAngles = m_tangoRotation.eulerAngles;
            rotationAngles.y = m_prevTangoRotation.eulerAngles.y;
            m_tangoRotation.eulerAngles = rotationAngles;
        }

        // Calculate final position and rotation deltas and apply them.
        Vector3 deltaPosition = m_tangoPosition - m_prevTangoPosition;
        Quaternion deltaRotation = m_tangoRotation * Quaternion.Inverse(m_prevTangoRotation);

        if (m_characterMotion)
        {
            m_characterController.Move(deltaPosition);
            transform.rotation = deltaRotation * transform.rotation;
        }
        else
        {
            transform.position = transform.position + deltaPosition;
            transform.rotation = deltaRotation * transform.rotation;
        }
    }

    /// <summary>
    /// Internal callback when no Tango core is present.
    /// </summary>
    /// <returns>Coroutine IEnumerator.</returns>
    private IEnumerator _InformUserNoTangoCore()
    {
        AndroidHelper.ShowAndroidToastMessage("Please install Tango Core", false);
        yield return new WaitForSeconds(2.0f);
        Application.Quit();
    }
}
