using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Niantic.ARDK.AR;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.Utilities.Input.Legacy;
using Niantic.ARDK.AR.ARSessionEventArgs;
using System;
using UnityEngine.Events;

namespace Niantic.ARDK.Templates
{
    public class PlacementController : MonoBehaviour
    {
        public ObjectHolderControllerMult OHcontroller;

        bool _isPlacingObject = false;
        /// <summary>
        /// Object Id of the object currently being placed
        /// </summary>
        int _placeObjectId = 0;
        private IARSession _arSession;

        /// <summary>
        /// This can just be an empty gameobject
        /// </summary>
        [SerializeField]
        GameObject _lookAtObjectPrefab;
        [SerializeField]
        DirectionalArrow _directionalArrow;
        void Update()
        {
            if (PlatformAgnosticInput.touchCount <= 0) return;

            var touch = PlatformAgnosticInput.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (!touch.IsTouchOverUIObject() || _arSession == null)
                    TouchBegan(touch);
            }
        }
        /// <summary>
        /// This function checks for a raycast collision to a detected plane from the center of the camera viewport without first having to detect a touch
        /// </summary>
        private bool TryGetGazeInput(out Matrix4x4 localPose)
        {
            var currentFrame = OHcontroller.Session.CurrentFrame;
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
        private void OnEnable()
        {
            ARSessionFactory.SessionInitialized += HandleSessionInitialized;
        }
        private void OnDisable()
        {
            ARSessionFactory.SessionInitialized -= HandleSessionInitialized;
        }
        private void HandleSessionInitialized(AnyARSessionInitializedArgs anyARSessionInitializedArgs)
        {
            _arSession = anyARSessionInitializedArgs.Session;
            _arSession.Ran += HandleSessionRan;
        }

        private void HandleSessionRan(ARSessionRanArgs args)
        {
        }

        private void TouchBegan(Touch touch)
        {
            var currentFrame = _arSession.CurrentFrame;
            if (currentFrame == null) return;

            if (OHcontroller.Camera == null) return;            
            
            var hitTestResults = currentFrame.HitTest(
                OHcontroller.Camera.pixelWidth,
                OHcontroller.Camera.pixelHeight,
                touch.position,
                ARHitTestResultType.EstimatedHorizontalPlane
            );
            if (_placeObjectId == 2 || _placeObjectId == 6)
            {
                hitTestResults = currentFrame.HitTest(
                OHcontroller.Camera.pixelWidth,
                OHcontroller.Camera.pixelHeight,
                touch.position,
                ARHitTestResultType.EstimatedVerticalPlane
            );
            }
            if (hitTestResults.Count <= 0) return;

            var position = hitTestResults[0].WorldTransform.ToPosition();
            if (!_isPlacingObject)
                return;

            ValidateObjectId();
            GameObject obj;
            int objectType = _placeObjectId;
            obj = Instantiate(OHcontroller.ObjectHolders[objectType]);
            obj.SetActive(true);
            LookAtTarget lookAtTarget = obj.GetComponentInChildren<LookAtTarget>();
            GameObject newLookAtObject = Instantiate(_lookAtObjectPrefab);
            newLookAtObject.transform.position = Camera.main.transform.position;
            lookAtTarget._target = newLookAtObject.transform;
            lookAtTarget._isRotatingEachUpdate = true;
            lookAtTarget._isRotateYAxisOnly = true;
            if (objectType == 2 || objectType == 6)
            {
                lookAtTarget._offset = 0.2f;
            }
            _isPlacingObject = false;
            obj.SetActive(true);
            obj.transform.position = position;
            _directionalArrow.SetTarget(obj, new Vector3(), false);
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
