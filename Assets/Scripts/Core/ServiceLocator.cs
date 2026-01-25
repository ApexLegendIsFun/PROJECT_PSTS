using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSS.Core
{
    /// <summary>
    /// 서비스 로케이터 패턴 구현
    /// Service Locator pattern implementation
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private static bool _isInitialized;

        /// <summary>
        /// 서비스 로케이터 초기화
        /// Initialize service locator
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            _services.Clear();
            _isInitialized = true;

            Debug.Log("[ServiceLocator] Initialized");
        }

        /// <summary>
        /// 서비스 등록
        /// Register a service
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);

            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Service {type.Name} already registered. Replacing.");
                _services[type] = service;
            }
            else
            {
                _services.Add(type, service);
                Debug.Log($"[ServiceLocator] Registered: {type.Name}");
            }
        }

        /// <summary>
        /// 서비스 등록 해제
        /// Unregister a service
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
        /// Get a registered service
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
        /// 서비스 가져오기 시도
        /// Try to get a registered service
        /// </summary>
        public static bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var obj))
            {
                service = obj as T;
                return service != null;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// 서비스 존재 여부 확인
        /// Check if a service is registered
        /// </summary>
        public static bool Has<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// 모든 서비스 제거
        /// Clear all services
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
            _isInitialized = false;
            Debug.Log("[ServiceLocator] Cleared all services");
        }
    }
}
