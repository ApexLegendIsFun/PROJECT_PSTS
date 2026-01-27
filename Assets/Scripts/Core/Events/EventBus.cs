// Core/Events/EventBus.cs
// 이벤트 버스 시스템 - Pub/Sub 패턴

using System;
using System.Collections.Generic;

namespace ProjectSS.Core.Events
{
    /// <summary>
    /// 중앙 이벤트 버스 - 게임 전역 이벤트 관리
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        /// <summary>
        /// 이벤트 구독
        /// </summary>
        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (!_subscribers.ContainsKey(type))
            {
                _subscribers[type] = new List<Delegate>();
            }
            _subscribers[type].Add(handler);
        }

        /// <summary>
        /// 이벤트 구독 해제
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (_subscribers.ContainsKey(type))
            {
                _subscribers[type].Remove(handler);
            }
        }

        /// <summary>
        /// 이벤트 발행
        /// </summary>
        public static void Publish<T>(T eventData) where T : struct
        {
            var type = typeof(T);
            if (_subscribers.ContainsKey(type))
            {
                foreach (var handler in _subscribers[type].ToArray())
                {
                    try
                    {
                        ((Action<T>)handler)?.Invoke(eventData);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError($"[EventBus] Error handling event {type.Name}: {e.Message}\n{e.StackTrace}");
                    }
                }
            }
        }

        /// <summary>
        /// 모든 구독 해제 (씬 전환 시 호출)
        /// </summary>
        public static void Clear()
        {
            _subscribers.Clear();
        }

        /// <summary>
        /// 특정 이벤트 타입의 모든 구독 해제
        /// </summary>
        public static void Clear<T>() where T : struct
        {
            var type = typeof(T);
            if (_subscribers.ContainsKey(type))
            {
                _subscribers[type].Clear();
            }
        }
    }
}
