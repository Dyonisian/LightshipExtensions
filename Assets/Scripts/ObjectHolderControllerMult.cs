using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;

namespace Niantic.ARDK.Templates
{
    public class ObjectHolderControllerMult : ObjectHolderController
    {
        public List<GameObject> ObjectHolders;      


        void Start()
        {
            ARSessionFactory.SessionInitialized += OnSessionInitialized;
            if (Cursor != null) Cursor.SetActive(false);
            foreach(GameObject go in ObjectHolders)
            {
                go.SetActive(false);
            }
        }

        private void OnSessionInitialized(AnyARSessionInitializedArgs args)
        {
            ARSessionFactory.SessionInitialized -= OnSessionInitialized;
            _session = args.Session;
        }
    }
}
