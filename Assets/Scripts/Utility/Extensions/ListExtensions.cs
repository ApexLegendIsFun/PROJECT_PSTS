using System.Collections.Generic;

namespace ProjectSS.Utility
{
    /// <summary>
    /// List 확장 메서드
    /// List extension methods
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// 리스트를 Fisher-Yates 알고리즘으로 셔플
        /// Shuffle list using Fisher-Yates algorithm
        /// </summary>
        public static void Shuffle<T>(this IList<T> list, System.Random random = null)
        {
            random ??= new System.Random();

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        /// <summary>
        /// 리스트에서 랜덤 요소 반환
        /// Get random element from list
        /// </summary>
        public static T GetRandom<T>(this IList<T> list, System.Random random = null)
        {
            if (list == null || list.Count == 0)
                return default;

            random ??= new System.Random();
            return list[random.Next(list.Count)];
        }

        /// <summary>
        /// 리스트에서 랜덤 요소를 뽑아서 제거 후 반환
        /// Pop random element from list (removes and returns)
        /// </summary>
        public static T PopRandom<T>(this IList<T> list, System.Random random = null)
        {
            if (list == null || list.Count == 0)
                return default;

            random ??= new System.Random();
            int index = random.Next(list.Count);
            T item = list[index];
            list.RemoveAt(index);
            return item;
        }

        /// <summary>
        /// 리스트의 마지막 요소 반환
        /// Get last element of list
        /// </summary>
        public static T Last<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
                return default;

            return list[list.Count - 1];
        }

        /// <summary>
        /// 리스트의 마지막 요소 제거 후 반환
        /// Pop last element from list
        /// </summary>
        public static T PopLast<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
                return default;

            T item = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            return item;
        }
    }
}
