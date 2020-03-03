using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrossinformTask.Core
{
    public class ParallelTripletsFinder
    {
        #region Private
        private static void FindInString(string text, ConcurrentDictionary<string, int> result,
            CancellationToken cancellationToken, int bufferSize)
        {
            var length = text.Length;

            var blocksCount = (int)Math.Ceiling((length - 2) / (double)(bufferSize - 2));
            Parallel.For(0, blocksCount,
                new ParallelOptions() { CancellationToken = cancellationToken, MaxDegreeOfParallelism = -1 },
                (i, state) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var index = i * (bufferSize - 2); // Каждый следующий блок включает 2 символа из предыдущего
                    var count = i == blocksCount - 1 ? length - index : bufferSize;

                    var buffer = text.AsSpan(index, count);

                    FindInStringLoop(new string(buffer), result, cancellationToken);
                });
        }

        private static void FindInStringLoop(string text, ConcurrentDictionary<string, int> result,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(text) || result == null || text.Length < 3)
                return;

            for (var i = 0; i < text.Length - 2; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var triplet = new string(new[] { text[i], text[i + 1], text[i + 2] });
                result.AddOrUpdate(triplet, 1, (key, value) => value + 1);
            }
        }

        private static void FindInBigFile(string path, ConcurrentDictionary<string, int> result,
            CancellationToken cancellationToken, int bufferSize)
        {
            // TODO: Учесть кодировку 

            using var file = new FileStream(path, FileMode.Open, FileAccess.Read);
            var length = file.Length;

            var tasksCount = (int)Math.Ceiling((length - 2) / (double)(bufferSize - 2));
            Parallel.For(0, tasksCount,
                new ParallelOptions() { CancellationToken = cancellationToken, MaxDegreeOfParallelism = -1 },
                (i, state) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    using var reader = new StreamReader(path);

                    var index = i * (bufferSize - 2);
                    var buffer = new char[bufferSize];
                    
                    reader.DiscardBufferedData();
                    reader.BaseStream.Seek(i == 0 ? 0 : index, SeekOrigin.Begin);
                    var count = reader.ReadBlock(buffer, 0, bufferSize);

                    FindInStringLoop(new string(buffer, 0, count), result, cancellationToken);
                });
        }

        #endregion

        /// <summary>
        /// Выполняет поиск и вычисление количества вхождений триплетов в text
        /// и записывает результат в словарь result в формате {триплет} : {количество вхождений} 
        /// </summary>
        /// <param name="text">Текст для поиска триплетов</param>
        /// <param name="result">Словарь для результатов поиска</param>
        /// <param name="bufferSize">Размер буфера - максимальное число символов из text, передаваемых каждому потоку </param>
        public static void FindInString(string text, ConcurrentDictionary<string, int> result, int bufferSize = 16384)
            => FindInString(text, result, default, bufferSize);

        /// <summary>
        /// Выполняет поиск и вычисление количества вхождений триплетов в text
        /// и записывает результат в словарь result в формате {триплет} : {количество вхождений}
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <param name="result">Словарь для результатов поиска</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <param name="bufferSize">Размер буфера - максимальное число символов из text, передаваемых каждому потоку</param>
        public static async Task FindInStringAsync(string text, ConcurrentDictionary<string, int> result,
            CancellationToken cancellationToken = default, int bufferSize = 16384)
            => await Task.Factory.StartNew(() => FindInString(text, result, cancellationToken, bufferSize), cancellationToken);

        /// <summary>
        /// Выполняет поиск и вычисление количества вхождений триплетов в файле path
        /// и записывает результат в словарь result в формате {триплет} : {количество вхождений}
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <param name="result">Словарь для результатов поиска</param>
        /// <param name="bufferSize">Размер буфера - максимальное число символов из text, передаваемых каждому потоку</param>
        public static void FindInFile(string path, ConcurrentDictionary<string, int> result, int bufferSize = 16384)
        {
            var text = File.ReadAllText(path);
            FindInString(text, result, bufferSize);
        }

        /// <summary>
        /// Выполняет поиск и вычисление количества вхождений триплетов в файле path
        /// и записывает результат в словарь result в формате {триплет} : {количество вхождений}
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <param name="result">Словарь для результатов поиска</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <param name="bufferSize">Размер буфера - максимальное число символов из text, передаваемых каждому потоку</param>
        public static async Task FindInFileAsync(string path, ConcurrentDictionary<string, int> result,
            CancellationToken cancellationToken = default, int bufferSize = 16384)
        {
            var text = await File.ReadAllTextAsync(path, cancellationToken);
            await FindInStringAsync(text, result, cancellationToken, bufferSize);
        }

        /// <summary>
        /// Выполняет поиск и вычисление количества вхождений триплетов в файле path
        /// и записывает результат в словарь result в формате {триплет} : {количество вхождений}
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <param name="result">Словарь для результатов поиска</param>
        /// <param name="bufferSize">Размер буфера - максимальное число символов из text, передаваемых каждому потоку</param>
        public static void FindInBigFile(string path, ConcurrentDictionary<string, int> result, int bufferSize = 16384)
            => FindInBigFile(path, result, default, bufferSize);

        /// <summary>
        /// Выполняет поиск и вычисление количества вхождений триплетов в файле path
        /// и записывает результат в словарь result в формате {триплет} : {количество вхождений}
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <param name="result">Словарь для результатов поиска</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <param name="bufferSize">Размер буфера - максимальное число символов из text, передаваемых каждому потоку</param>
        public static async Task FindInBigFileAsync(string path, ConcurrentDictionary<string, int> result,
            CancellationToken cancellationToken = default, int bufferSize = 16384)
            => await Task.Factory.StartNew(() => FindInBigFile(path, result, cancellationToken, bufferSize), cancellationToken);
    }
}