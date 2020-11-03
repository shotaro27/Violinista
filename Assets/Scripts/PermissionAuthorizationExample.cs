using UnityEngine;
using System.Collections;
using System;

#if UNITY_ANDROID && UNITY_2018_3_OR_NEWER
using UnityEngine.Android;
#endif

namespace PermissionAuthorizationTest
{
    public class PermissionAuthorizationExample : MonoBehaviour
    {
        IEnumerator Start()
        {
#if UNITY_IOS && UNITY_2018_1_OR_NEWER
            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
                yield return RequestUserAuthorization(UserAuthorization.Microphone);
            if (Application.HasUserAuthorization(UserAuthorization.Microphone))
                findMicrophones();

#elif UNITY_ANDROID && UNITY_2018_3_OR_NEWER
            if (!Permission.HasUserAuthorizedPermission (Permission.Microphone))
                yield return RequestUserPermission (Permission.Microphone);
            if (Permission.HasUserAuthorizedPermission (Permission.Microphone))
                findMicrophones();
#endif

            yield break;
        }

#if (UNITY_IOS && UNITY_2018_1_OR_NEWER) || (UNITY_ANDROID && UNITY_2018_3_OR_NEWER)
        bool isRequesting;

        IEnumerator OnApplicationFocus(bool hasFocus)
        {
            yield return null;
            if (isRequesting && hasFocus)
                isRequesting = false;
        }

#if UNITY_IOS
        IEnumerator RequestUserAuthorization(UserAuthorization mode)
        {
            isRequesting = true;
            yield return Application.RequestUserAuthorization(mode);
            float timeElapsed = 0;
            while (isRequesting)
            {
                if (timeElapsed > 0.5f)
                {
                    isRequesting = false;
                    yield break;
                }
                timeElapsed += Time.deltaTime;

                yield return null;
            }
            yield break;
        }
#elif UNITY_ANDROID
        IEnumerator RequestUserPermission(string permission)
        {
            isRequesting = true;
            Permission.RequestUserPermission(permission);
            float timeElapsed = 0;
            while (isRequesting)
            {
                if (timeElapsed > 0.5f){
                    isRequesting = false;
                    yield break;
                }
                timeElapsed += Time.deltaTime;

                yield return null;
            }
            yield break;
        }
#endif
#endif

        void findMicrophones()
        {
            foreach (var device in Microphone.devices)
            {
                Debug.Log("Name: " + device);
            }
        }
    }
}