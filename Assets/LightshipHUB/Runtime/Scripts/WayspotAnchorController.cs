using System;
using System.Collections.Generic;
using System.Linq;

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.AR.WayspotAnchors;
using Niantic.ARDK.LocationService;
using Niantic.ARDK.Utilities.Input.Legacy;
using Niantic.ARDK.Utilities;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Niantic.ARDK.Templates
{
    public class WayspotAnchorController : MonoBehaviour
    {
        public ObjectHolderControllerMult OHcontroller;
        public Text StatusLog;
        public Text LocalizationStatus;

        private WayspotAnchorService _wayspotAnchorService;
        private IARSession _arSession;
        private LocalizationState _localizationState;

        private readonly Dictionary<Guid, GameObject> _wayspotAnchorGameObjects =
        new Dictionary<Guid, GameObject>();

        /// <summary>
        /// Keeps track of object types by anchor id so they can be saved and loaded accurately
        /// </summary>
        private Dictionary<Guid, int> _objectTypeByAnchorId = new Dictionary<Guid, int>();
        /// <summary>
        /// Keeps track of object rotations by anchor id so they can be saved and loaded with correct rotations
        /// </summary>
        private Dictionary<Guid, SerializableTypes.SQuaternion> _objectRotationByAnchorId = new Dictionary<Guid, SerializableTypes.SQuaternion>();

        bool _isPlacingObject = false;
        /// <summary>
        /// Object Id of the object currently being placed
        /// </summary>
        int _placeObjectId = 0;

        [SerializeField]
        FileReadWrite _fileReadWrite;

        /// <summary>
        /// This can just be an empty gameobject
        /// </summary>
        [SerializeField]
        GameObject _lookAtObjectPrefab;
        [SerializeField]
        DirectionalArrow _directionalArrow;

        public UnityAction<LocalizationState> OnLocalizationStateUpdated;

        private void Awake()
        {
            StatusLog.text = "Initializing Session.";
        }

        private void OnEnable()
        {
            ARSessionFactory.SessionInitialized += HandleSessionInitialized;
        }

        private void Update()
        {
            //Restarts wayspot anchor service if localization fails so you don't have to constantly restart the app while trying to localize
            if (_wayspotAnchorService.LocalizationState == LocalizationState.Failed)
            {
                _wayspotAnchorService.Restart();
            }
            if (_wayspotAnchorService == null)
            {
                return;
            }
            //Get the pose where you tap on the screen
            var success = (TryGetTouchInput(out Matrix4x4 localPose) && _isPlacingObject);
            if (_wayspotAnchorService.LocalizationState == LocalizationState.Localized)
            {
                if (_localizationState != _wayspotAnchorService.LocalizationState)
                {
                    Debug.Log("Auto load anchors");
                    LoadWayspotAnchors();
                }
                if (success) //Check is screen tap was a valid tap
                {
                    ValidateObjectId();
                    _isPlacingObject = false;
                    PlaceAnchor(localPose); //Create the Wayspot Anchor and place the GameObject                    
                }
            }
            else
            {
                if (success) //Check is screen tap was a valid tap
                {
                    StatusLog.text = "Must localize before placing anchor.";
                }
                if (_localizationState != _wayspotAnchorService.LocalizationState)
                {
                    _wayspotAnchorGameObjects.Values.ToList().ForEach(a => a.SetActive(false));
                }

            }
            if (_localizationState != _wayspotAnchorService.LocalizationState)
            {
                OnLocalizationStateUpdated?.Invoke(_wayspotAnchorService.LocalizationState);
            }
            LocalizationStatus.text = _wayspotAnchorService.LocalizationState.ToString();
            _localizationState = _wayspotAnchorService.LocalizationState;
        }

        private void OnDisable()
        {
            ARSessionFactory.SessionInitialized -= HandleSessionInitialized;
        }

        private void OnDestroy()
        {
            _wayspotAnchorService.Dispose();
        }

        /// Saves all of the existing wayspot anchors
        public void SaveWayspotAnchors()
        {
            if (_wayspotAnchorGameObjects.Count > 0)
            {
                var wayspotAnchors = _wayspotAnchorService.GetAllWayspotAnchors();
                var payloads = wayspotAnchors.Select(a => a.Payload);
                WayspotAnchorDataUtility.SaveLocalPayloads(payloads.ToArray());
                _fileReadWrite.SaveAnchorGameObjectDataToPlayerPrefs(_objectTypeByAnchorId);
                foreach (var kvp in _wayspotAnchorGameObjects)
                {
                    if (!_objectRotationByAnchorId.ContainsKey(kvp.Key))
                        _objectRotationByAnchorId.Add(kvp.Key, kvp.Value.GetComponentInChildren<LookAtTarget>().transform.rotation);
                    else
                        _objectRotationByAnchorId[kvp.Key] = kvp.Value.GetComponentInChildren<LookAtTarget>().transform.rotation;
                }
                _fileReadWrite.SaveAnchorRotationsToPlayerPrefs(_objectRotationByAnchorId);
            }
            else
            {
                WayspotAnchorDataUtility.SaveLocalPayloads(Array.Empty<WayspotAnchorPayload>());
            }

            StatusLog.text = "Saved Wayspot Anchors.";
        }

        /// Loads all of the saved wayspot anchors
        public void LoadWayspotAnchors()
        {
            if (_wayspotAnchorService.LocalizationState != LocalizationState.Localized)
            {
                StatusLog.text = "Must localize before loading anchors.";
                return;
            }
            _fileReadWrite.LoadAnchorGameObjectDataFromPlayerPrefs(out _objectTypeByAnchorId);
            _fileReadWrite.LoadAnchorRotationDataFromPlayerPrefs(out _objectRotationByAnchorId);

            var payloads = WayspotAnchorDataUtility.LoadLocalPayloads();
            if (payloads.Length > 0)
            {
                var wayspotAnchors = _wayspotAnchorService.RestoreWayspotAnchors(payloads);
                CreateAnchorGameObjects(wayspotAnchors);
                StatusLog.text = "Loaded Wayspot Anchors.";
            }
            else
            {
                StatusLog.text = "No anchors to load.";
            }
        }

        /// Clears all of the active wayspot anchors
        public void ClearAnchorGameObjects()
        {
            if (_wayspotAnchorGameObjects.Count == 0)
            {
                StatusLog.text = "No anchors to clear.";
                return;
            }

            foreach (var anchor in _wayspotAnchorGameObjects)
            {
                Destroy(anchor.Value);
            }

            _wayspotAnchorService.DestroyWayspotAnchors(_wayspotAnchorGameObjects.Keys.ToArray());
            _wayspotAnchorGameObjects.Clear();
            _objectTypeByAnchorId.Clear();
            _objectRotationByAnchorId.Clear();
            StatusLog.text = "Cleared Wayspot Anchors.";
        }

        /// Pauses the AR Session
        public void PauseARSession()
        {
            if (_arSession.State == ARSessionState.Running)
            {
                _arSession.Pause();
                StatusLog.text = $"AR Session Paused.";
            }
            else
            {
                StatusLog.text = $"Cannot pause AR Session.";
            }
        }

        /// Resumes the AR Session
        public void ResumeARSession()
        {
            if (_arSession.State == ARSessionState.Paused)
            {
                _arSession.Run(_arSession.Configuration);
                StatusLog.text = $"AR Session Resumed.";
            }
            else
            {
                StatusLog.text = $"Cannot resume AR Session.";
            }
        }

        private void HandleSessionInitialized(AnyARSessionInitializedArgs anyARSessionInitializedArgs)
        {
            StatusLog.text = "Running Session...";
            _arSession = anyARSessionInitializedArgs.Session;
            _arSession.Ran += HandleSessionRan;
        }

        private void HandleSessionRan(ARSessionRanArgs arSessionRanArgs)
        {
            _arSession.Ran -= HandleSessionRan;
            _wayspotAnchorService = CreateWayspotAnchorService();
            StatusLog.text = "Session Initialized.";
        }
        /// <summary>
        /// This function will have to be modified if you update the Lightship ARDK version
        /// </summary>
        private void PlaceAnchor(Matrix4x4 localPose)
        {
            _wayspotAnchorService.CreateWayspotAnchors(CreateAnchorGameObjects, localPose);
            // Alternatively, you can make this method async and create wayspot anchors using await:
            // var wayspotAnchors = await _wayspotAnchorService.CreateWayspotAnchorsAsync(localPose);
            StatusLog.text = "Anchor placed.";
        }

        /// <summary>
        /// This function will have to be modified if you update the Lightship ARDK version
        /// </summary>
        private WayspotAnchorService CreateWayspotAnchorService()
        {
            var wayspotAnchorsConfiguration = WayspotAnchorsConfigurationFactory.Create();

            var locationService = LocationServiceFactory.Create(_arSession.RuntimeEnvironment);
            locationService.Start();

            var wayspotAnchorService = new WayspotAnchorService(_arSession, locationService, wayspotAnchorsConfiguration);
            return wayspotAnchorService;
        }

        private void CreateAnchorGameObjects(IWayspotAnchor[] wayspotAnchors)
        {
            foreach (var wayspotAnchor in wayspotAnchors)
            {
                if (_wayspotAnchorGameObjects.ContainsKey(wayspotAnchor.ID))
                {
                    continue;
                }
                var id = wayspotAnchor.ID;
                int objectType;
                if (_objectTypeByAnchorId.ContainsKey(id))
                {
                    objectType = _objectTypeByAnchorId[id];
                }
                else
                {
                    objectType = _placeObjectId;
                    _objectTypeByAnchorId.Add(id, objectType);
                }
                var anchor = Instantiate(OHcontroller.ObjectHolders[objectType]);
                anchor.SetActive(false);
                anchor.name = $"Anchor {id}";
                _wayspotAnchorGameObjects.Add(id, anchor);
                wayspotAnchor.TrackingStateUpdated += HandleWayspotAnchorTrackingUpdated;

                LookAtTarget lookAtTarget = anchor.GetComponentInChildren<LookAtTarget>();
                if (_objectRotationByAnchorId.ContainsKey(id))
                {
                    lookAtTarget.transform.rotation = _objectRotationByAnchorId[id];
                }
                else
                {
                    //TODO - This code doesn't scale well as a new lookAtObjectPrefab is spawned for each object placement. 
                    GameObject newLookAtObject = Instantiate(_lookAtObjectPrefab);
                    newLookAtObject.transform.position = Camera.main.transform.position;
                    lookAtTarget._target = newLookAtObject.transform;
                    lookAtTarget._isRotatingEachUpdate = true;
                    lookAtTarget._isRotateYAxisOnly = true;
                    if (objectType == 2 || objectType == 6)
                    {
                        lookAtTarget._offset = 0.2f;
                    }
                }
                _directionalArrow.SetTarget(anchor, new Vector3(), false);
            }
        }
        private void HandleWayspotAnchorTrackingUpdated(WayspotAnchorResolvedArgs wayspotAnchorResolvedArgs)
        {
            var anchor = _wayspotAnchorGameObjects[wayspotAnchorResolvedArgs.ID].transform;
            anchor.position = wayspotAnchorResolvedArgs.Position;
            anchor.rotation = wayspotAnchorResolvedArgs.Rotation;
            anchor.gameObject.SetActive(true);
        }
        private bool TryGetTouchInput(out Matrix4x4 localPose)
        {
            if (_arSession == null || PlatformAgnosticInput.touchCount <= 0)
            {
                localPose = Matrix4x4.zero;
                return false;
            }
            var touch = PlatformAgnosticInput.GetTouch(0);
            if (touch.IsTouchOverUIObject())
            {
                localPose = Matrix4x4.zero;
                return false;
            }

            if (touch.phase != TouchPhase.Began)
            {
                localPose = Matrix4x4.zero;
                return false;
            }

            var currentFrame = _arSession.CurrentFrame;
            if (currentFrame == null)
            {
                localPose = Matrix4x4.zero;
                return false;
            }
                        
            var results = currentFrame.HitTest
            (
                OHcontroller.Camera.pixelWidth,
                OHcontroller.Camera.pixelHeight,
                touch.position,
                ARHitTestResultType.ExistingPlane
            );
            if (_placeObjectId == 2 || _placeObjectId == 6)
            {
                results = currentFrame.HitTest(
                OHcontroller.Camera.pixelWidth,
                OHcontroller.Camera.pixelHeight,
                touch.position,
                ARHitTestResultType.EstimatedVerticalPlane
            );
            }
            int count = results.Count;
            if (count <= 0)
            {
                localPose = Matrix4x4.zero;
                return false;
            }
            var result = results[0];
            localPose = result.WorldTransform;
            return true;
        }
        /// <summary>
        /// This function checks for a raycast collision to a detected plane from the center of the camera viewport without first having to detect a touch
        /// </summary>
        private bool TryGetGazeInput(out Matrix4x4 localPose)
        {
            if (_arSession == null)
            {
                localPose = Matrix4x4.zero;
                return false;
            }
            var currentFrame = _arSession.CurrentFrame;
            if (currentFrame == null)
            {
                localPose = Matrix4x4.zero;
                return false;
            }
            var results = currentFrame.HitTest
            (
                OHcontroller.Camera.pixelWidth,
                OHcontroller.Camera.pixelHeight,
                new Vector2(0, 0),
                ARHitTestResultType.ExistingPlane
            );

            int count = results.Count;
            if (count <= 0)
            {
                localPose = Matrix4x4.zero;
                return false;
            }

            var result = results[0];
            localPose = result.WorldTransform;
            return true;
        }
        public void StartPlacingObject()
        {
            _isPlacingObject = true;
        }
        public void ChangePlaceObjectId(string id)
        {
            int value;
            if (int.TryParse(id, out value))
                _placeObjectId = value;
        }
        void ValidateObjectId()
        {
            if (_placeObjectId < 0)
                _placeObjectId = 0;
            if (_placeObjectId > OHcontroller.ObjectHolders.Count - 1)
            {
                _placeObjectId = OHcontroller.ObjectHolders.Count - 1;
            }
        }

    }
}
