using System.Collections.Generic;
using System.Threading;

namespace CrossinformTask.Core
{
    public class TripletsFinder
    {
        /// <summary>
        /// Выполняет поиск и вычисление количества вхождений триплетов в text
        /// и записывает результат в словарь result в формате {триплет} : {количество вхождений} 
        /// </summary>
        /// <param name="text">Текст для поиска триплетов</param>
        /// <param name="result">Словарь для результатов поиска</param>
        public static void FindInString(string text, Dictionary<string, int> result)
        {
            if (string.IsNullOrEmpty(text) || result == null || text.Length < 3) 
                return;

            for (var i = 0; i < text.Length - 2; i++)
            {
                var triplet = new string(new[] { text[i], text[i + 1], text[i + 2] });
                result[triplet] = result.ContainsKey(triplet) ? result[triplet] + 1 : 1;
            }
        }
    }
}