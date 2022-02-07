using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace VeeamTestTask
{
    class Program
    {
        private static int ThreadCount => Environment.ProcessorCount;
        private static readonly ConcurrentBag<Exception> Errors = new();

        static void Main(string[] args)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                Run(args);
                Console.WriteLine($"Success. Duration: {stopwatch.Elapsed}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void Run(string[] args)
        {
            var path = args[0];
            var chunkSize = int.Parse(args[1]);
            using var fileStream = File.OpenRead(path);
            var threads = new List<Thread>(ThreadCount);
            for (var i = 0; i < ThreadCount; i++)
            {
                threads.Add(RunComputeSignatureTask(chunkSize, fileStream));
            }
            threads.WaitAll();
            Errors.ThrowIfNotEmpty();
        }

        private static Thread RunComputeSignatureTask(int chunkSize, FileStream fileStream)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    ComputeSignature(chunkSize, fileStream);
                }
                catch (Exception e)
                {
                    Errors.Add(e);
                }
            });
            thread.Start();
            return thread;
        }

        private static void ComputeSignature(int chunkSize, FileStream fileStream)
        {
            var chunk = new byte[chunkSize];
            using var hashAlgorithm = SHA256.Create();
            while (true)
            {
                var fileChunk = GetFileChunk(fileStream, chunk);
                if (fileChunk == FileChunk.Empty)
                {
                    break;
                }
                var hashValue = hashAlgorithm.ComputeHash(chunk);
                Console.WriteLine($"{fileChunk.Number} - {BitConverter.ToString(hashValue)}");
            }
        }

        private static FileChunk GetFileChunk(FileStream fileStream, byte[] chunk)
        {
            long chunkNumber;
            int byteCount;
            lock (fileStream)
            {
                byteCount = fileStream.Read(chunk, 0, chunk.Length);
                chunkNumber = fileStream.Position / chunk.Length;
            }

            return ReadWholeChunk(chunk.Length, byteCount) ?
                new FileChunk(chunkNumber, chunk) :
                FileChunk.Empty;
        }

        private static bool ReadWholeChunk(int chunkSize, int readByteCount) => 
            chunkSize == readByteCount;
    }
}
