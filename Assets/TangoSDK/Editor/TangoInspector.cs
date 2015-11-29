﻿//-----------------------------------------------------------------------
// <copyright file="TangoInspector.cs" company="Google">
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
using System.Collections;
using UnityEditor;
using UnityEngine;
using Tango;

/// <summary>
/// Custom editor for the TangoApplication.
/// </summary>
[CustomEditor(typeof(TangoApplication))]
public class TangoInspector : Editor
{
    private TangoApplication m_tangoApplication;

    /// <summary>
    /// Raises the inspector GUI event.
    /// </summary>
    public override void OnInspectorGUI()
    {
        m_tangoApplication.m_autoConnectToService = EditorGUILayout.Toggle("Auto-connect to Service",
                                                                           m_tangoApplication.m_autoConnectToService);
        EditorGUILayout.Space();

        _DrawMotionTrackingOptions(m_tangoApplication);
        _DrawDepthOptions(m_tangoApplication);
        _DrawVideoOverlayOptions(m_tangoApplication);
        _DrawDevelopmentOptions(m_tangoApplication);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(m_tangoApplication);
        }
    }

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    private void OnEnable()
    {
        m_tangoApplication = (TangoApplication)target;
    }

    /// <summary>
    /// Draw motion tracking options.
    /// </summary>
    /// <param name="tangoApplication">Tango application.</param>
    private void _DrawMotionTrackingOptions(TangoApplication tangoApplication)
    {
        tangoApplication.m_enableMotionTracking = EditorGUILayout.Toggle("Enable Motion Tracking", 
                                                                         tangoApplication.m_enableMotionTracking);
        if (tangoApplication.m_enableMotionTracking)
        {
            EditorGUI.indentLevel++;
            tangoApplication.m_motionTrackingAutoReset = EditorGUILayout.Toggle("Auto Reset", 
                                                                                tangoApplication.m_motionTrackingAutoReset);

            tangoApplication.m_enableADFLoading = EditorGUILayout.Toggle("Load ADF",
                                                                         tangoApplication.m_enableADFLoading);
            tangoApplication.m_enableAreaLearning = EditorGUILayout.Toggle("Area Learning", 
                                                                           tangoApplication.m_enableAreaLearning);

            tangoApplication.m_enableCloudADF = EditorGUILayout.Toggle("Cloud ADF", tangoApplication.m_enableCloudADF);
            if (tangoApplication.m_enableCloudADF)
            {
                tangoApplication.m_cloudApiKey = EditorGUILayout.TextField("Cloud API Key", tangoApplication.m_cloudApiKey);
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
    }

    /// <summary>
    /// Draw depth options.
    /// </summary>
    /// <param name="tangoApplication">Tango application.</param>
    private void _DrawDepthOptions(TangoApplication tangoApplication)
    {
        tangoApplication.m_enableDepth = EditorGUILayout.Toggle("Enable Depth", tangoApplication.m_enableDepth);
        EditorGUILayout.Space();
    }

    /// <summary>
    /// Draw video overlay options.
    /// </summary>
    /// <param name="tangoApplication">Tango application.</param>
    private void _DrawVideoOverlayOptions(TangoApplication tangoApplication)
    {
        tangoApplication.m_enableVideoOverlay = EditorGUILayout.Toggle("Enable Video Overlay", 
                                                                       tangoApplication.m_enableVideoOverlay);
        if (tangoApplication.m_enableVideoOverlay)
        {
            EditorGUI.indentLevel++;
            tangoApplication.m_useExperimentalVideoOverlay = EditorGUILayout.Toggle("GPU Accelerated (Experimental)", 
                                                                                    tangoApplication.m_useExperimentalVideoOverlay);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
    }

    /// <summary>
    /// Draws development options.
    /// 
    /// These should only be set while in development.
    /// </summary>
    /// <param name="tangoApplication">Tango application.</param>
    private void _DrawDevelopmentOptions(TangoApplication tangoApplication)
    {
        GUILayout.Label("Development Options (Disable these before publishing)", GUILayout.ExpandWidth(true));
        EditorGUI.indentLevel++;
        tangoApplication.m_allowOutOfDateTangoAPI = EditorGUILayout.Toggle("Allow out of date API",
                                                                           m_tangoApplication.m_allowOutOfDateTangoAPI);
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
    }
}
