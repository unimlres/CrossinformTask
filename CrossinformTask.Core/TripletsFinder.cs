using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
        /// <param name="cancellationToken">Токен отмены</param>
        public static async Task FindInStringAsync(string text, Dictionary<string, int> result,
            CancellationToken cancellationToken = default)
            => await Task.Factory.StartNew(() => FindInString(text, result, cancellationToken), cancellationToken);
       
        /// <summary>
        /// Выполняет поиск и вычисление количества вхождений триплетов в text
        /// и записывает результат в словарь result в формате {триплет} : {количество вхождений} 
        /// </summary>
        /// <param name="text">Текст для поиска триплетов</param>
        /// <param name="result">Словарь для результатов поиска</param>
        public static void FindInString(string text, Dictionary<string, int> result)
            => FindInString(text, result, default);

        private static void FindInString(string text, Dictionary<string, int> result,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(text) || result == null || text.Length < 3) 
                return;

            for (var i = 0; i < text.Length - 2; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var triplet = new string(new[] { text[i], text[i + 1], text[i + 2] });
                result[triplet] = result.ContainsKey(triplet) ? result[triplet] + 1 : 1;
            }
        }

        /// <summary>
        /// Выполняет поиск и вычисление количества вхождений триплетов в файле path
        /// и записывает результат в словарь result в формате {триплет} : {количество вхождений}
        /// <para>
        /// Считывает содержимое всего файла в память. Для больших файлов используйте FindInBigFileAsync.
        /// </para>
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <param name="result">Словарь для результатов поиска</param>
        /// <param name="cancellationToken">Токен отмены</param>
        public static async Task FindInFileAsync(string path, Dictionary<string, int> result,
            CancellationToken cancellationToken = default)
        {
            var text = await File.ReadAllTextAsync(path, cancellationToken);
            await FindInStringAsync(text, result, cancellationToken);
        }

        /// <summary>
        /// Выполняет поиск и вычисление количества вхождений триплетов в файле path
        /// и записывает результат в словарь result в формате {триплет} : {количество вхождений}
        /// <para>
        /// Считывает содержимое всего файла в память. Для больших файлов используйте FindInBigFile.
        /// </para>
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <param name="result">Словарь для результатов поиска</param>
        public static void FindInFile(string path, Dictionary<string, int> result)
        {
            var text = File.ReadAllText(path);
            FindInString(text, result);
        }


        /// <summary>
        /// Выполняет поиск и вычисление количества вхождений триплетов в файле path
        /// и записывает результат в словарь result в формате {триплет} : {количество вхождений}
        /// <para>
        /// Считыает файл блоками равными размеру буфера, что уменьшает расход памяти, но
        /// требует дополнительных вычислений. Для небольших файлов используйте FindInFileAsync.
        /// </para>
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <param name="result">Словарь для результатов поиска</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <param name="bufferSize">Размер буфера - максимальное количество символов, загружаемых в память</param>
        public static async Task FindInBigFileAsync(string path, Dictionary<string, int> result,
            CancellationToken cancellationToken = default, int bufferSize = 16384)
        {
            using var fs = new StreamReader(path);
            var buffer = new char[bufferSize];
            var last = ""; // Буфер для поиска между блоками. При поиске только в блоках пропускаются два триплета
            var first = ""; // Например, возьмем 2 блока : 1234|5678, тогда - {345,456} будут пропущены 

            while (!fs.EndOfStream)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var count = await fs.ReadBlockAsync(buffer, 0, bufferSize);

                if (count < 3)
                    break;

                // Последний блок может быть меньше буфера
                var text = new string(buffer,0, count);

                first = new string(new[] { buffer[0], buffer[1] });
                await FindInStringAsync(text, result, cancellationToken); // Длинная операция - асинхронно
                FindInString(last + first, result, cancellationToken); // Поиск между блоками - короткая операция, поэтому синхронно
                last = new string(new[] { buffer[count - 2], buffer[count - 1] });
            }
        }

        /// <summary>
        /// Выполняет поиск и вычисление количества вхождений триплетов в файле path
        /// и записывает результат в словарь result в формате {триплет} : {количество вхождений}
        /// <para>
        /// Считыает файл блоками равными размеру буфера, что уменьшает расход памяти, но
        /// требует дополнительных вычислений. Для небольших файлов используйте FindInFile.
        /// </para>
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <param name="result">Словарь для результатов поиска</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <param name="bufferSize">Размер буфера - максимальное количество символов, загружаемых в память</param>
        public static void FindInBigFile(string path, Dictionary<string, int> result, int bufferSize = 16384)
        {
            using var fs = new StreamReader(path);
            var buffer = new char[bufferSize];
            var last = ""; // Буфер для поиска между блоками. При поиске только в блоках пропускаются два триплета
            var first = ""; // Например, возьмем 2 блока : 1234|5678, тогда - {345,456} будут пропущены

            while (!fs.EndOfStream)
            {
                var count = fs.ReadBlock(buffer, 0, bufferSize);

                if (count < 3)
                    break;

                // Последний блок может быть меньше буфера
                var text = new string(buffer, 0, count);

                first = new string(new[] { buffer[0], buffer[1] });
                FindInString(text, result);
                FindInString(last + first, result);
                last = new string(new[] { buffer[count - 2], buffer[count - 1] });
            }
        }
    }
}