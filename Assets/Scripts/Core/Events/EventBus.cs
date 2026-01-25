using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSS.Core
{
    /// <summary>
    /// 이벤트 버스 - Pub/Sub 패턴 구현
    /// Event Bus - Pub/Sub pattern implementation
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();

        /// <summary>
        /// 이벤트 구독
        /// Subscribe to an event
        /// </summary>
        public static void Subscribe<T>(Action<T> handler) where T : IGameEvent
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
        /// Unsubscribe from an event
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var type = typeof(T);

            if (_subscribers.TryGetValue(type, out var handlers))
            {
                handlers.Remove(handler);
            }
        }

        /// <summary>
        /// 이벤트 발행
        /// Publish an event
        /// </summary>
        public static void Publish<T>(T gameEvent) where T : IGameEvent
        {
            var type = typeof(T);

            if (_subscribers.TryGetValue(type, out var handlers))
            {
                // 복사본으로 순회 (순회 중 구독 해제 대비)
                // Iterate over copy to handle unsubscription during iteration
                var handlersCopy = new List<Delegate>(handlers);

                foreach (var handler in handlersCopy)
                {
                    try
                    {
                        ((Action<T>)handler)?.Invoke(gameEvent);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EventBus] Error handling event {type.Name}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 특정 이벤트 타입의 모든 구독 해제
        /// Clear all subscriptions for a specific event type
        /// </summary>
        public static void Clear<T>() where T : IGameEvent
        {
            var type = typeof(T);

            if (_subscribers.ContainsKey(type))
            {
                _subscribers[type].Clear();
            }
        }

        /// <summary>
        /// 모든 구독 해제
        /// Clear all subscriptions
        /// </summary>
        public static void ClearAll()
        {
            _subscribers.Clear();
        }
    }

    /// <summary>
    /// 게임 이벤트 마커 인터페이스
    /// Game event marker interface
    /// </summary>
    public interface IGameEvent
    {
    }
}
