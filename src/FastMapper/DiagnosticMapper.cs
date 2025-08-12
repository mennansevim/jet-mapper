using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace FastMapper
{
    /// <summary>
    /// Basit diagnostic ve profiling API
    /// </summary>
    public static class DiagnosticMapper
    {
        private static readonly ConcurrentDictionary<long, MappingMetrics> _metricsCache = new();
        private static bool _diagnosticsEnabled = true;

        /// <summary>
        /// Mapping metrikleri
        /// </summary>
        public class MappingMetrics
        {
            public long TotalCalls { get; set; }
            public long SuccessfulCalls { get; set; }
            public long FailedCalls { get; set; }
            public TimeSpan TotalTime { get; set; }
            public TimeSpan AverageTime { get; set; }
            public DateTime LastCallTime { get; set; }
        }

        /// <summary>
        /// Mapping çağrısını kaydet
        /// </summary>
        public static void RecordMapping<TSource, TTarget>(TimeSpan duration, bool success, Exception error = null)
        {
            if (!_diagnosticsEnabled) return;

            var key = GetTypeKey(typeof(TSource), typeof(TTarget));
            var metrics = _metricsCache.GetOrAdd(key, _ => new MappingMetrics());

            lock (metrics)
            {
                metrics.TotalCalls++;
                metrics.TotalTime += duration;

                if (success)
                {
                    metrics.SuccessfulCalls++;
                }
                else
                {
                    metrics.FailedCalls++;
                }

                if (metrics.TotalCalls == 1)
                {
                    metrics.AverageTime = duration;
                }
                else
                {
                    metrics.AverageTime = TimeSpan.FromTicks(metrics.TotalTime.Ticks / metrics.TotalCalls);
                }

                metrics.LastCallTime = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Metrikleri al
        /// </summary>
        public static MappingMetrics GetMetrics<TSource, TTarget>()
        {
            var key = GetTypeKey(typeof(TSource), typeof(TTarget));
            return _metricsCache.TryGetValue(key, out var metrics) ? metrics : new MappingMetrics();
        }

        /// <summary>
        /// Diagnostic'leri etkinleştir/devre dışı bırak
        /// </summary>
        public static void SetDiagnosticsEnabled(bool enabled)
        {
            _diagnosticsEnabled = enabled;
        }

        /// <summary>
        /// Tüm cache'leri temizle
        /// </summary>
        public static void ClearAllCaches()
        {
            _metricsCache.Clear();
        }

        /// <summary>
        /// Type key oluştur
        /// </summary>
        private static long GetTypeKey(Type sourceType, Type targetType)
        {
            return ((long)sourceType.GetHashCode() << 32) | (uint)targetType.GetHashCode();
        }
    }
} 