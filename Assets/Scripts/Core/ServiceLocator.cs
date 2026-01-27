// Core/ServiceLocator.cs
// 서비스 로케이터 패턴 - 의존성 주입 컨테이너

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSS.Core
{
    /// <summary>
    /// 서비스 로케이터 - 전역 서비스 관리
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();
        private static bool _isInitialized = false;

        /// <summary>
        /// 서비스 등록
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Service {type.Name} already registered. Overwriting.");
            }
            _services[type] = service;
            Debug.Log($"[ServiceLocator] Registered: {type.Name}");
        }

        /// <summary>
        /// 서비스 등록 해제
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                _services.Remove(type);
                Debug.Log($"[ServiceLocator] Unregistered: {type.Name}");
            }
        }

        /// <summary>
        /// 서비스 가져오기
        /// </summary>
        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return service as T;
            }

            Debug.LogError($"[ServiceLocator] Service {type.Name} not found!");
            return null;
        }

        /// <summary>
        /// 서비스 가져오기 (null 허용)
        /// </summary>
        public static T TryGet<T>() where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return service as T;
            }
            return null;
        }

        /// <summary>
        /// 서비스 존재 여부 확인
        /// </summary>
        public static bool Has<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// 모든 서비스 해제
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
            _isInitialized = false;
            Debug.Log("[ServiceLocator] All services cleared.");
        }

        /// <summary>
        /// 초기화 상태
        /// </summary>
        public static bool IsInitialized
        {
            get => _isInitialized;
            set => _isInitialized = value;
        }
    }
}
