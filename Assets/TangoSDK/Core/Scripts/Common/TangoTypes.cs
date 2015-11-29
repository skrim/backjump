﻿//-----------------------------------------------------------------------
// <copyright file="TangoTypes.cs" company="Google">
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules",
                                                         "SA1649:FileHeaderFileNameDocumentationMustMatchTypeName",
                                                         Justification = "Types file.")]

namespace Tango
{
    /// <summary>
    /// The TangoXYZij struct contains information returned from the depth sensor.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TangoXYZij
    {
        /// <summary>
        /// An integer denoting the version of the structure.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int version;

        /// <summary>
        /// Time of capture of the depth data for this struct (in seconds).
        /// </summary>
        [MarshalAs(UnmanagedType.R8)]
        public double timestamp;

        /// <summary>
        /// The number of points in the xyz array.
        ///
        /// This is variable with result and is returned in (x,y,z) triplets populated (e.g. 2 points populated
        /// returned means 6 floats, or 6*4 bytes used).
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int xyz_count;

        /// <summary>
        /// An array of packed coordinate triplets, x,y,z as floating point values.
        /// 
        /// With the unit in landscape orientation, screen facing the user: +Z points in the direction of the
        /// camera's optical axis, and is measured perpendicular to the plane of the camera. +X points toward the
        /// user's right, and +Y points toward the bottom of the screen. The origin is the focal centre of the color
        /// camera. The output is in units of metres.
        /// </summary>
        [MarshalAs(UnmanagedType.LPArray)]
        public IntPtr xyz;

        /// <summary>
        /// The dimensions of the ij index buffer.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int ij_rows;

        /// <summary>
        /// The dimensions of the ij index buffer.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int ij_cols;

        /// <summary>
        /// A 2D buffer, of size ij_rows x ij_cols in raster ordering that contains the index of a point in the xyz
        /// array that was generated at this "ij" location.
        /// 
        /// A value of -1 denotes there was no corresponding point generated at that position. This buffer can be used
        /// to find neighbouring points in the point cloud.
        /// 
        /// For more information, see our developer overview on depth perception .
        /// </summary>
        public IntPtr ij;

        /// <summary>
        /// TangoImageBuffer is reserved for future use.
        /// </summary>
        public IntPtr color_image;
        
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="Tango.TangoXYZij"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="Tango.TangoXYZij"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("timestamp : {0}\nxyz_count : {1}\nij_rows : {2}\nij_cols : {3}",
                                 timestamp, xyz_count, ij_rows, ij_cols);
        }
    }

    /// <summary>
    /// The TangoEvent structure signals important sensor and tracking events.
    /// 
    /// Each event comes with a timestamp, a type, and a key-value pair describing
    /// the event.  The type is an enumeration which generally classifies the event
    /// type. The key is a text string describing the event.  The description holds
    /// parameters specific to the event.
    ///
    /// Possible descriptions (as "key:value") are:
    /// - "TangoServiceException:X" - The service has encountered an exception, and
    /// a text description is given in X.
    /// - "FisheyeOverExposed:X" - the fisheye image is over exposed with average
    /// pixel value X px.
    /// - "FisheyeUnderExposed:X" - the fisheye image is under exposed with average
    /// pixel value X px.
    /// - "ColorOverExposed:X" - the color image is over exposed with average pixel
    /// value X px.
    /// - "ColorUnderExposed:X" - the color image is under exposed with average
    /// pixel value X px.
    /// - "TooFewFeaturesTracked:X" - too few features were tracked in the fisheye
    /// image.  The number of features tracked is X.
    /// - "Unknown".
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TangoEvent
    {
        /// <summary>
        /// Timestamp, in seconds, of the event.
        /// </summary>
        [MarshalAs(UnmanagedType.R8)]
        public double timestamp;

        /// <summary>
        /// Type of event.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public TangoEnums.TangoEventType type;

        /// <summary>
        /// Description of the event key.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string event_key;

        /// <summary>
        /// Description of the event value.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string event_value;
    }

    /// <summary>
    /// The TangoCoordinateFramePair struct contains a pair of coordinate frames of reference.
    ///
    /// Tango pose data is calculated as a transformation between two frames
    /// of reference (so, for example, you can be asking for the pose of the
    /// device within a learned area).
    ///
    /// This struct is used to specify the desired base and target frames of
    /// reference when requesting pose data.  You can also use it when you have
    /// a TangoPoseData structure returned from the API and want to examine which
    /// frames of reference were used to get that pose.
    ///
    /// For more information, including which coordinate frame pairs are valid,
    /// see our page on
    /// <a href ="/project-tango/overview/frames-of-reference">frames of reference</a>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TangoCoordinateFramePair
    {
        /// <summary>
        /// Base frame of reference to compare against when requesting pose data.
        /// For example, if you have loaded an area and want to find out where the
        /// device is within it, you would use the
        /// <code>TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_AREA_DESCRIPTION</code> frame of reference
        /// as your base.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public TangoEnums.TangoCoordinateFrameType baseFrame;

        /// <summary>
        /// Target frame of reference when requesting pose data, compared to the
        /// base. For example, if you want the device's pose data, use
        /// <code>TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE</code>.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public TangoEnums.TangoCoordinateFrameType targetFrame;
    }

    /// <summary>
    /// The TangoImageBuffer contains information about a byte buffer holding image data.
    /// 
    /// This data is populated by the service when it returns an image.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TangoImageBuffer
    {
        /// <summary>
        /// The width of the image data.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public UInt32 width;

        /// <summary>
        /// The height of the image data.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public UInt32 height;

        /// <summary>
        /// The number of pixels per scanline of the image data.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public UInt32 stride;

        /// <summary>
        /// The timestamp of this image.
        /// </summary>
        [MarshalAs(UnmanagedType.R8)]
        public double timestamp;

        /// <summary>
        /// The frame number of this image.
        /// </summary>
        [MarshalAs(UnmanagedType.I8)]
        public Int64 frame_number;

        /// <summary>
        /// The pixel format of the data.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public TangoEnums.TangoImageFormatType format;

        /// <summary>
        /// Pixels in the format of this image buffer.
        /// </summary>
        public IntPtr data;
    }

    /// <summary>
    /// The TangoCameraIntrinsics struct contains intrinsic parameters for a camera.
    ///
    /// Given a 3D point (X, Y, Z) in camera coordinates, the corresponding
    /// pixel coordinates (x, y) are:
    ///
    /// <code>
    /// x = X / Z * fx * rd / ru + cx
    /// y = X / Z * fy * rd / ru + cy
    /// </code>
    ///
    /// The normalized radial distance ru is given by:
    ///
    /// <code>
    /// ru = sqrt((X^2 + Y^2) / (Z^2))
    /// </code>
    ///
    /// The distorted radial distance rd depends on the distortion model used.
    ///
    /// For <code>TangoCalibrationType.TANGO_CALIBRATION_POLYNOMIAL_3_PARAMETERS</code>, rd is a
    /// polynomial that depends on the 3 distortion coefficients k1, k2 and k3:
    ///
    /// <code>
    /// rd = ru + k1 * ru^3 + k2 * ru^5 + k3 * ru^7
    /// </code>
    ///
    /// For <code>TangoCalibrationType.TANGO_CALIBRATION_EQUIDISTANT</code>, rd depends on the single
    /// distortion coefficient w:
    ///
    /// <code>
    /// rd = 1 / w * arctan(2 * ru * tan(w / 2))
    /// </code>
    ///
    /// For more information, see our page on
    /// <a href ="/project-tango/overview/intrinsics-extrinsics">Camera Intrinsics and Extrinsics</a>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TangoCameraIntrinsics
    {
        /// <summary>
        /// ID of the camera which the intrinsics reference.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public TangoEnums.TangoCameraId camera_id;

        /// <summary>
        /// The type of distortion model used. This determines the meaning of the
        /// distortion coefficients.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public TangoEnums.TangoCalibrationType calibration_type;

        /// <summary>
        /// The width of the image in pixels.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public UInt32 width;

        /// <summary>
        /// The height of the image in pixels.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public UInt32 height;

        /// <summary>
        /// Focal length, x axis, in pixels.
        /// </summary>
        [MarshalAs(UnmanagedType.R8)]
        public double fx;

        /// <summary>
        /// Focal length, y axis, in pixels.
        /// </summary>
        [MarshalAs(UnmanagedType.R8)]
        public double fy;

        /// <summary>
        /// Principal point x coordinate on the image, in pixels.
        /// </summary>
        [MarshalAs(UnmanagedType.R8)]
        public double cx;

        /// <summary>
        /// Principal point y coordinate on the image, in pixels.
        /// </summary>
        [MarshalAs(UnmanagedType.R8)]
        public double cy;

        /// <summary>
        /// Distortion coefficient 0.  Meaning of this value depends on the distortion model specified by
        /// <c>calibration_type</c>.
        /// </summary>
        [MarshalAs(UnmanagedType.R8)]
        public double distortion0;

        /// <summary>
        /// Distortion coefficient 1.  Meaning of this value depends on the distortion model specified by
        /// <c>calibration_type</c>.
        /// </summary>
        [MarshalAs(UnmanagedType.R8)]
        public double distortion1;

        /// <summary>
        /// Distortion coefficient 2.  Meaning of this value depends on the distortion model specified by
        /// <c>calibration_type</c>.
        /// </summary>
        [MarshalAs(UnmanagedType.R8)]
        public double distortion2;

        /// <summary>
        /// Distortion coefficient 3.  Meaning of this value depends on the distortion model specified by
        /// <c>calibration_type</c>.
        /// </summary>
        [MarshalAs(UnmanagedType.R8)]
        public double distortion3;

        /// <summary>
        /// Distortion coefficient 4.  Meaning of this value depends on the distortion model specified by
        /// <c>calibration_type</c>.
        /// </summary>
        [MarshalAs(UnmanagedType.R8)]
        public double distortion4;
    }
    
    /// <summary>
    /// The TangoPoseData struct contains 6DOF pose information.
    /// 
    /// The device pose is given using Android conventions.  See the Android
    /// <a href ="http://developer.android.com/guide/topics/sensors/sensors_overview.html#sensors-coords">Sensor
    /// Overview</a> page for more information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TangoPoseData
    {
        /// <summary>
        /// An integer denoting the version of the structure.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int version;

        /// <summary>
        /// Timestamp of the time that this pose estimate corresponds to.
        /// </summary>
        [MarshalAs(UnmanagedType.R8)]
        public double timestamp;

        /// <summary>
        /// Orientation, as a quaternion, of the pose of the target frame
        /// with reference to the base frame.
        /// Specified as (x,y,z,w) where RotationAngle is in radians:
        /// <code>
        ///   x = RotationAxis.x * sin(RotationAngle / 2)
        ///   y = RotationAxis.y * sin(RotationAngle / 2)
        ///   z = RotationAxis.z * sin(RotationAngle / 2)
        ///   w = cos(RotationAngle / 2)
        /// </code>
        /// </summary>
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.R8)]
        public double[] orientation;

        /// <summary>
        /// Translation, ordered x, y, z, of the pose of the target frame
        /// with reference to the base frame.
        /// </summary>
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = UnmanagedType.R8)]
        public double[] translation;

        /// <summary>
        /// The status of the pose, according to the pose lifecycle.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public TangoEnums.TangoPoseStatusType status_code;

        /// <summary>
        /// The pair of coordinate frames for this pose.
        /// 
        /// We retrieve a pose for a target coordinate frame (such as the Tango device) against a base
        /// coordinate frame (such as a learned area).
        /// </summary>
        [MarshalAs(UnmanagedType.Struct)]
        public TangoCoordinateFramePair framePair;

        /// <summary>
        /// Unused.  Integer levels are determined by service.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int confidence;

        /// <summary>
        /// Unused.  Reserved for metric accuracy.
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float accuracy;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Tango.TangoPoseData"/> class.
        /// </summary>
        public TangoPoseData()
        {
            version = 0;
            timestamp = 0.0;
            orientation = new double[4];
            translation = new double[3];
            status_code = TangoEnums.TangoPoseStatusType.TANGO_POSE_UNKNOWN;
            framePair.baseFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_START_OF_SERVICE;
            framePair.targetFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE;
            confidence = 0;
        }

        /// <summary>
        /// Deep copy from poseToCopy into this.
        /// </summary>
        /// <param name="poseToCopy">Pose to copy.</param>
        public void DeepCopy(TangoPoseData poseToCopy)
        {
            this.version = poseToCopy.version;
            this.timestamp = poseToCopy.timestamp;
            this.status_code = poseToCopy.status_code;
            this.framePair.baseFrame = poseToCopy.framePair.baseFrame;
            this.framePair.targetFrame = poseToCopy.framePair.targetFrame;
            this.confidence = poseToCopy.confidence;
            for (int i = 0; i < 4; ++i)
            {
                this.orientation[i] = poseToCopy.orientation[i];
            }
            for (int i = 0; i < 3; ++i)
            {
                this.translation[i] = poseToCopy.translation[i];
            }
        }
    }

    /// DEPRECATED: Use AreaDescription instead.
    /// <summary>
    /// Unity-side representation of a area description ID and its associated metadata.
    /// 
    /// Used to avoid too many conversions when needing to access the information.
    /// </summary>
    public class UUIDUnityHolder
    {
        /// <summary>
        /// The Metadata for this area description ID.
        /// </summary>
        public Metadata uuidMetaData;

        private UUID uuidObject;
        private string uuidName;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tango.UUIDUnityHolder"/> class.
        /// </summary>
        public UUIDUnityHolder()
        {
            uuidObject = new UUID();
            uuidMetaData = new Metadata();
            uuidObject.data = IntPtr.Zero;
            uuidName = string.Empty;
        }

        /// <summary>
        /// Prepares the UUID meta data by the calling uuidMetaData object's 
        /// method - PopulateMetaDataKeyValues().
        /// </summary>
        public void PrepareUUIDMetaData()
        {
            uuidMetaData.PopulateMetaDataKeyValues();
        }

        /// <summary>
        /// Allocates memory for the IntPtr of the UUID data to be filled out.
        /// Uses Marshal.AllocHGlobal to initialize the IntPtr.
        /// </summary>
        public void AllocateDataBuffer()
        {
            uuidObject.data = Marshal.AllocHGlobal(Common.UUID_LENGTH);
        }

        /// <summary>
        /// Copies the data contained by <c>uuidData</c> into our UUID object
        /// data IntPtr.
        /// </summary>
        /// <param name="uuidData">The data marshalled by the UUID list object for this UUID object.</param>
        public void SetDataUUID(byte[] uuidData)
        {
            if (uuidObject.data == IntPtr.Zero)
            {
                AllocateDataBuffer();
            }
            Marshal.Copy(uuidData, 0, uuidObject.data, Common.UUID_LENGTH);
            SetDataUUID(System.Text.Encoding.UTF8.GetString(uuidData));
        }

        /// <summary>
        /// Copies the data contained by <c>uuidData</c> into our UUID object
        /// data IntPtr.
        /// </summary>
        /// <param name="uuidString">The UTF-8 encoded string for this UUID object.</param>
        public void SetDataUUID(string uuidString)
        {
            uuidName = uuidString;
        }

        /// <summary>
        /// Returns raw IntPtr to UUID data.
        /// </summary>
        /// <returns>The raw data UUID IntPtr.</returns>
        public IntPtr GetRawDataUUID()
        {
            return uuidObject.data;
        }

        /// <summary>
        /// Returns a human readable string in UTF-8 format of the UUID data.
        /// </summary>
        /// <returns>The UTF-8 string for the UUID.</returns>
        public string GetStringDataUUID()
        {
            return uuidName;
        }

        /// <summary>
        /// Determines whether or not the UUID object that we have is valid.
        /// </summary>
        /// <returns><c>true</c> if this instance contains a valid UUID object; otherwise, <c>false</c>.</returns>
        public bool IsObjectValid()
        {
            return uuidObject != null && (uuidObject.data != IntPtr.Zero || !string.IsNullOrEmpty(uuidName));
        }
    }

    /// DEPRECATED: The AreaDescription class now returns UUIDs as a string.
    /// <summary>
    /// The unique id associated with a single area description.
    /// 
    /// Should be 36 characters including dashes and a null terminating character.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class UUID
    {
        [MarshalAs(UnmanagedType.I4)]
        public IntPtr data;
    }

    /// DEPRECATED: Use AreaDescription[] when working with multiple ADFs.
    /// <summary>
    /// List of all UUIDs on device.
    /// </summary>
    public class UUID_list
    {
        private UUIDUnityHolder[] uuids;
        private int count;

        /// <summary>
        /// Count of all Area Description Files (Read only).
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get { return count; }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Tango.UUID_list"/> class.
        /// </summary>
        public UUID_list()
        {
            uuids = null;
        }
        
        /// <summary>
        /// Populates the UUID list.
        /// </summary>
        /// <param name="uuidNames">UUID names.</param>
        public void PopulateUUIDList(string uuidNames)
        {
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            string[] splitNames = uuidNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            uuids = new UUIDUnityHolder[splitNames.Length];
            count = splitNames.Length;
            for (int i = 0; i < count; ++i)
            {
                if (uuids[i] == null)
                {
                    uuids[i] = new Tango.UUIDUnityHolder();
                }

                // Following three calls should be done in the same order always.
                uuids[i].SetDataUUID(System.Text.Encoding.UTF8.GetString(encoder.GetBytes(splitNames[i])));
                PoseProvider.GetAreaDescriptionMetaData(uuids[i]);
                uuids[i].PrepareUUIDMetaData();
            }
        }
        
        /// <summary>
        /// Returns the latest ADF UUID found in the list.
        /// </summary>
        /// <returns>UUIDUnityHolder object that contains the last ADF saved.</returns>
        public UUIDUnityHolder GetLatestADFUUID()
        {
            if (uuids == null || (uuids != null && count <= 0))
            {
                return null;
            }
            return uuids[count - 1];
        }

        /// <summary>
        /// Query specific ADF.
        /// </summary>
        /// <returns>UUIDUnityHolder object that contains the last ADF saved.</returns>
        /// <param name="index">Index.</param>
        public UUIDUnityHolder GetADFAtIndex(int index)
        {
            if (uuids == null || (index < 0 || index >= count))
            {
                return null;
            }
            return uuids[index];
        }

        /// <summary>
        /// Gets the UUID as string.
        /// </summary>
        /// <returns>The UUID as string.</returns>
        /// <param name="index">Index.</param>
        public string GetUUIDAsString(int index)
        {
            if (uuids == null || (index < 0 || index >= count))
            {
                return null;
            }
            return uuids[index].GetStringDataUUID();
        }

        /// <summary>
        /// Determines whether this instance has valid UUID entries.
        /// </summary>
        /// <returns><c>true</c> if this instance has at least one or more UUIDs; otherwise, <c>false</c>.</returns>
        public bool HasEntries()
        {
            return count > 0;
        }
    }
    
    /// DEPRECATED: Use AreaDescription.Metadata instead.
    /// <summary>
    /// UUID Metadata list.
    /// </summary>
    public class Metadata
    {
        public IntPtr meta_data_pointer;
        private Dictionary<string, string> m_keyValuePairs = new Dictionary<string, string>();
       
        /// <summary>
        /// Populates the meta data key values pairs.
        /// </summary>
        public void PopulateMetaDataKeyValues()
        {
            PoseProvider.PopulateAreaDescriptionMetaDataKeyValues(meta_data_pointer, ref m_keyValuePairs);
        }

        /// <summary>
        /// Returns the dictionary object with the Metadata's Key Value pairs.
        /// PopulateMetaDataKeyValues() should be called before calling this.
        /// </summary>
        /// <returns>The meta data key values.</returns>
        public Dictionary<string, string> GetMetaDataKeyValues()
        {
            return m_keyValuePairs;
        }
    }

    /// <summary>
    /// The TangoUnityImageData contains information about a byte buffer holding image data.
    /// </summary>
    public class TangoUnityImageData
    {
        /// <summary>
        /// The width of the image data.
        /// </summary>
        public UInt32 width;
        
        /// <summary>
        /// The height of the image data.
        /// </summary>
        public UInt32 height;
        
        /// <summary>
        /// The number of pixels per scanline of the image data.
        /// </summary>
        public UInt32 stride;
        
        /// <summary>
        /// The timestamp of this image.
        /// </summary>
        public double timestamp;
        
        /// <summary>
        /// The frame number of this image.
        /// </summary>
        public Int64 frame_number;
        
        /// <summary>
        /// Pixel format of the data.
        /// </summary>
        public TangoEnums.TangoImageFormatType format;
        
        /// <summary>
        /// Pixels in the format of this image buffer.
        /// </summary>
        public byte[] data;
    }

    /// <summary>
    /// Like TangoXYZij, but more Unity friendly.
    /// </summary>
    public class TangoUnityDepth
    {
        /// <summary>
        /// Max point array size is currently defined by the largest single mesh
        /// supported by Unity. This array is multiplied by 3 to account for the
        /// x/y/z components.
        /// </summary>
        public static readonly int MAX_POINTS_ARRAY_SIZE = Common.UNITY_MAX_SUPPORTED_VERTS_PER_MESH * 3;
        
        /// <summary>
        /// Max IJ array size is currently defined by the largest single mesh
        /// supported by Unity. This number is multiplied by 2 to account for the
        /// i/j components.
        /// </summary>
        public static readonly int MAX_IJ_ARRAY_SIZE = Common.UNITY_MAX_SUPPORTED_VERTS_PER_MESH * 2;

        /// <summary>
        /// 
        /// An integer denoting the version of the structure.
        /// </summary>
        public int m_version;

        /// <summary>
        /// The number of points in the m_points array.
        /// 
        /// Because the points array always contains 3D points, this is m_points.Count / 3.
        /// </summary>
        public int m_pointCount;

        /// <summary>
        /// An array of packed coordinate triplets, x,y,z as floating point values.
        /// </summary>
        public float[] m_points;

        /// <summary>
        /// Time of capture of the depth data for this struct (in seconds).
        /// </summary>
        public double m_timestamp;

        /// <summary>
        /// The dimensions of the ij index buffer.
        /// </summary>
        public int m_ijRows;

        /// <summary>
        /// The dimensions of the ij index buffer.
        /// </summary>
        public int m_ijColumns;

        /// <summary>
        /// A 2D buffer, of size ij_rows x ij_cols in raster ordering that contains
        /// the index of a point in the xyz array that was generated at this "ij"
        /// location.
        /// 
        /// A value of -1 denotes there was no corresponding point generated at that position. This buffer can be used
        /// to find neighbouring points in the point cloud.
        /// 
        /// For more information, see our
        /// <a href ="/project-tango/overview/depth-perception#xyzij">developer
        /// overview on depth perception</a>.
        /// </summary>
        public int[] m_ij;

        /// <summary>
        /// Initializes an empty instance of the <see cref="Tango.TangoUnityDepth"/> class, with no points.
        /// </summary>
        public TangoUnityDepth()
        {
            m_points = new float[MAX_POINTS_ARRAY_SIZE];
            m_ij = new int[MAX_IJ_ARRAY_SIZE];
            m_version = -1;
            m_timestamp = 0.0;
            m_pointCount = m_ijRows = m_ijColumns = 0;
        }
    }
}
