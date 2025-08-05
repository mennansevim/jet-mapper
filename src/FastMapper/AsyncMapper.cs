using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading;
using System.Text;

namespace FastMapper
{
    /// <summary>
    /// Async Mapping ve Snapshot/Restore özellikleri
    /// </summary>
    public static class AsyncMapper
    {
        private static readonly ConcurrentDictionary<long, object> _asyncMapperCache = new();
        private static readonly ConcurrentDictionary<string, object> _snapshotCache = new();
        private static readonly ConcurrentDictionary<Type, PropertySnapshotInfo[]> _snapshotPropertyCache = new();

        /// <summary>
        /// Asenkron mapping sonucu
        /// </summary>
        public class AsyncMappingResult<T>
        {
            public List<T> Results { get; set; } = new();
            public TimeSpan TotalTime { get; set; }
            public TimeSpan AverageTime { get; set; }
            public int SuccessCount { get; set; }
            public int ErrorCount { get; set; }
            public List<MappingError> Errors { get; set; } = new();
            public Dictionary<string, object> Metrics { get; set; } = new();
        }

        /// <summary>
        /// Mapping hatası
        /// </summary>
        public class MappingError
        {
            public int Index { get; set; }
            public object Source { get; set; }
            public Exception Exception { get; set; }
            public string ErrorMessage { get; set; }
        }

        /// <summary>
        /// Snapshot bilgisi
        /// </summary>
        public class SnapshotInfo
        {
            public string Id { get; set; }
            public DateTime CreatedAt { get; set; }
            public Type ObjectType { get; set; }
            public string SerializedData { get; set; }
            public Dictionary<string, object> Metadata { get; set; } = new();
            public int SizeInBytes { get; set; }
        }

        /// <summary>
        /// Property snapshot bilgisi
        /// </summary>
        private class PropertySnapshotInfo
        {
            public string PropertyName { get; set; }
            public Type PropertyType { get; set; }
            public Func<object, object> Getter { get; set; }
            public Action<object, object> Setter { get; set; }
            public bool IsSerializable { get; set; }
        }

        /// <summary>
        /// Asenkron liste mapping
        /// </summary>
        public static async Task<AsyncMappingResult<TTarget>> MapAsync<TSource, TTarget>(
            IEnumerable<TSource> sources, 
            int maxConcurrency = 4)
            where TTarget : new()
        {
            var startTime = DateTime.UtcNow;
            var result = new AsyncMappingResult<TTarget>();
            var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
            var tasks = new List<Task<TTarget>>();
            var errors = new List<MappingError>();

            var sourceList = sources.ToList();
            var totalCount = sourceList.Count;

            for (int i = 0; i < sourceList.Count; i++)
            {
                var index = i;
                var source = sourceList[i];

                var task = Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        return source.FastMapTo<TTarget>();
                    }
                    catch (Exception ex)
                    {
                        lock (errors)
                        {
                            errors.Add(new MappingError
                            {
                                Index = index,
                                Source = source,
                                Exception = ex,
                                ErrorMessage = ex.Message
                            });
                        }
                        return default(TTarget);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                tasks.Add(task);
            }

            try
            {
                var results = await Task.WhenAll(tasks);
                result.Results.AddRange(results.Where(r => r != null));
                result.SuccessCount = result.Results.Count;
                result.ErrorCount = errors.Count;
                result.Errors = errors;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new MappingError
                {
                    Index = -1,
                    Exception = ex,
                    ErrorMessage = $"Genel mapping hatası: {ex.Message}"
                });
            }

            result.TotalTime = DateTime.UtcNow - startTime;
            result.AverageTime = TimeSpan.FromTicks(result.TotalTime.Ticks / Math.Max(totalCount, 1));
            result.Metrics["TotalItems"] = totalCount;
            result.Metrics["ConcurrencyLevel"] = maxConcurrency;
            result.Metrics["SuccessRate"] = (double)result.SuccessCount / totalCount;

            return result;
        }

        /// <summary>
        /// Asenkron mapping with progress reporting
        /// </summary>
        public static async Task<AsyncMappingResult<TTarget>> MapAsync<TSource, TTarget>(
            IEnumerable<TSource> sources,
            IProgress<MappingProgress> progress,
            int maxConcurrency = 4)
            where TTarget : new()
        {
            var startTime = DateTime.UtcNow;
            var result = new AsyncMappingResult<TTarget>();
            var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
            var sourceList = sources.ToList();
            var totalCount = sourceList.Count;
            var completedCount = 0;
            var lockObject = new object();

            var tasks = sourceList.Select(async (source, index) =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var mapped = source.FastMapTo<TTarget>();
                    
                    lock (lockObject)
                    {
                        completedCount++;
                        progress?.Report(new MappingProgress
                        {
                            CompletedCount = completedCount,
                            TotalCount = totalCount,
                            Percentage = (double)completedCount / totalCount * 100,
                            CurrentItem = source,
                            CurrentIndex = index
                        });
                    }

                    return mapped;
                }
                catch (Exception ex)
                {
                    lock (result.Errors)
                    {
                        result.Errors.Add(new MappingError
                        {
                            Index = index,
                            Source = source,
                            Exception = ex,
                            ErrorMessage = ex.Message
                        });
                    }
                    return default(TTarget);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            try
            {
                var results = await Task.WhenAll(tasks);
                result.Results.AddRange(results.Where(r => r != null));
                result.SuccessCount = result.Results.Count;
                result.ErrorCount = result.Errors.Count;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new MappingError
                {
                    Index = -1,
                    Exception = ex,
                    ErrorMessage = $"Genel mapping hatası: {ex.Message}"
                });
            }

            result.TotalTime = DateTime.UtcNow - startTime;
            result.AverageTime = TimeSpan.FromTicks(result.TotalTime.Ticks / Math.Max(totalCount, 1));

            return result;
        }

        /// <summary>
        /// Mapping progress bilgisi
        /// </summary>
        public class MappingProgress
        {
            public int CompletedCount { get; set; }
            public int TotalCount { get; set; }
            public double Percentage { get; set; }
            public object CurrentItem { get; set; }
            public int CurrentIndex { get; set; }
        }

        /// <summary>
        /// Nesne snapshot'ı oluştur
        /// </summary>
        public static SnapshotInfo CreateSnapshot<T>(T obj, string id = null)
        {
            if (obj == null)
                return null;

            var snapshot = new SnapshotInfo
            {
                Id = id ?? Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                ObjectType = typeof(T)
            };

            try
            {
                // JSON serialization (simplified for .NET Standard 2.0)
                snapshot.SerializedData = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

                snapshot.SizeInBytes = System.Text.Encoding.UTF8.GetByteCount(snapshot.SerializedData);
                snapshot.Metadata["SerializationMethod"] = "JSON";
                snapshot.Metadata["ObjectType"] = typeof(T).FullName;

                // Cache'e kaydet
                _snapshotCache[snapshot.Id] = snapshot;

                return snapshot;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Snapshot oluşturulamadı: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Snapshot'tan nesneyi geri yükle
        /// </summary>
        public static T RestoreFromSnapshot<T>(string snapshotId)
        {
            if (!_snapshotCache.TryGetValue(snapshotId, out var snapshotObj))
                throw new KeyNotFoundException($"Snapshot bulunamadı: {snapshotId}");

            var snapshot = (SnapshotInfo)snapshotObj;

            try
            {
                if (typeof(T) != snapshot.ObjectType)
                {
                    throw new InvalidOperationException(
                        $"Tip uyumsuzluğu: Beklenen {typeof(T).Name}, Snapshot'ta {snapshot.ObjectType.Name}");
                }

                var restored = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(snapshot.SerializedData);

                return restored;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Snapshot'tan geri yükleme başarısız: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Snapshot'ı sil
        /// </summary>
        public static bool DeleteSnapshot(string snapshotId)
        {
            return _snapshotCache.TryRemove(snapshotId, out _);
        }

        /// <summary>
        /// Tüm snapshot'ları listele
        /// </summary>
        public static List<SnapshotInfo> ListSnapshots()
        {
            return _snapshotCache.Values.Cast<SnapshotInfo>().ToList();
        }

        /// <summary>
        /// Snapshot istatistiklerini al
        /// </summary>
        public static SnapshotStatistics GetSnapshotStatistics()
        {
            var snapshots = ListSnapshots();
            var totalSize = snapshots.Sum(s => s.SizeInBytes);
            var averageSize = snapshots.Any() ? snapshots.Average(s => s.SizeInBytes) : 0;

            return new SnapshotStatistics
            {
                TotalSnapshots = snapshots.Count,
                TotalSizeInBytes = totalSize,
                AverageSizeInBytes = (long)averageSize,
                OldestSnapshot = snapshots.OrderBy(s => s.CreatedAt).FirstOrDefault(),
                NewestSnapshot = snapshots.OrderByDescending(s => s.CreatedAt).FirstOrDefault()
            };
        }

        /// <summary>
        /// Snapshot istatistikleri
        /// </summary>
        public class SnapshotStatistics
        {
            public int TotalSnapshots { get; set; }
            public long TotalSizeInBytes { get; set; }
            public long AverageSizeInBytes { get; set; }
            public SnapshotInfo OldestSnapshot { get; set; }
            public SnapshotInfo NewestSnapshot { get; set; }
        }

        /// <summary>
        /// Deep copy snapshot oluştur
        /// </summary>
        public static SnapshotInfo CreateDeepCopySnapshot<T>(T obj, string id = null)
        {
            if (obj == null)
                return null;

            var snapshot = new SnapshotInfo
            {
                Id = id ?? Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                ObjectType = typeof(T)
            };

            try
            {
                // Deep copy için özel serialization
                var settings = new Newtonsoft.Json.JsonSerializerSettings
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                };

                snapshot.SerializedData = Newtonsoft.Json.JsonConvert.SerializeObject(obj, settings);
                snapshot.SizeInBytes = System.Text.Encoding.UTF8.GetByteCount(snapshot.SerializedData);
                snapshot.Metadata["SerializationMethod"] = "DeepCopy";
                snapshot.Metadata["ObjectType"] = typeof(T).FullName;

                _snapshotCache[snapshot.Id] = snapshot;
                return snapshot;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Deep copy snapshot oluşturulamadı: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Partial snapshot oluştur (sadece belirli property'ler)
        /// </summary>
        public static SnapshotInfo CreatePartialSnapshot<T>(T obj, string[] propertyNames, string id = null)
        {
            if (obj == null || propertyNames == null || propertyNames.Length == 0)
                return null;

            var snapshot = new SnapshotInfo
            {
                Id = id ?? Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                ObjectType = typeof(T)
            };

            try
            {
                // Partial object oluştur
                var partialObject = new Dictionary<string, object>();
                var type = typeof(T);
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var prop in properties)
                {
                    if (propertyNames.Contains(prop.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        var value = prop.GetValue(obj);
                        partialObject[prop.Name] = value;
                    }
                }

                snapshot.SerializedData = Newtonsoft.Json.JsonConvert.SerializeObject(partialObject);
                snapshot.SizeInBytes = System.Text.Encoding.UTF8.GetByteCount(snapshot.SerializedData);
                snapshot.Metadata["SerializationMethod"] = "Partial";
                snapshot.Metadata["IncludedProperties"] = string.Join(",", propertyNames);
                snapshot.Metadata["ObjectType"] = typeof(T).FullName;

                _snapshotCache[snapshot.Id] = snapshot;
                return snapshot;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Partial snapshot oluşturulamadı: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Snapshot'ları temizle (eski snapshot'ları sil)
        /// </summary>
        public static int CleanupSnapshots(TimeSpan olderThan)
        {
            var cutoffTime = DateTime.UtcNow - olderThan;
            var snapshotsToRemove = _snapshotCache.Values
                .Cast<SnapshotInfo>()
                .Where(s => s.CreatedAt < cutoffTime)
                .ToList();

            foreach (var snapshot in snapshotsToRemove)
            {
                _snapshotCache.TryRemove(snapshot.Id, out _);
            }

            return snapshotsToRemove.Count;
        }

        /// <summary>
        /// Async mapping cache'ini temizle
        /// </summary>
        public static void ClearAsyncMappingCache()
        {
            _asyncMapperCache.Clear();
        }

        /// <summary>
        /// Snapshot cache'ini temizle
        /// </summary>
        public static void ClearSnapshotCache()
        {
            _snapshotCache.Clear();
            _snapshotPropertyCache.Clear();
        }

        /// <summary>
        /// Async mapping istatistiklerini al
        /// </summary>
        public static (int AsyncMapperCache, int SnapshotCache, int PropertyCache) GetAsyncMappingStats()
        {
            return (_asyncMapperCache.Count, _snapshotCache.Count, _snapshotPropertyCache.Count);
        }
    }
} 