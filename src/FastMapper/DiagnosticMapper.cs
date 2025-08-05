using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace FastMapper
{
    /// <summary>
    /// Diagnostic ve Profiling API - Mapping metriklerini kaydeder ve analiz eder
    /// </summary>
    public static class DiagnosticMapper
    {
        private static readonly ConcurrentDictionary<long, MappingMetrics> _metricsCache = new();
        private static readonly ConcurrentDictionary<string, PerformanceProfile> _performanceProfiles = new();
        private static readonly ConcurrentQueue<MappingEvent> _eventLog = new();
        private static readonly object _lockObject = new object();
        private static bool _diagnosticsEnabled = true;
        private static int _maxEventLogSize = 10000;

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
            public TimeSpan MinTime { get; set; }
            public TimeSpan MaxTime { get; set; }
            public DateTime LastCallTime { get; set; }
            public DateTime FirstCallTime { get; set; }
            public Dictionary<string, object> CustomMetrics { get; set; } = new();
            public List<Exception> RecentErrors { get; set; } = new();
            public int ErrorCount { get; set; }
        }

        /// <summary>
        /// Performance profili
        /// </summary>
        public class PerformanceProfile
        {
            public string ProfileName { get; set; }
            public DateTime CreatedAt { get; set; }
            public TimeSpan Duration { get; set; }
            public long TotalMappings { get; set; }
            public long SuccessfulMappings { get; set; }
            public long FailedMappings { get; set; }
            public TimeSpan AverageMappingTime { get; set; }
            public TimeSpan TotalMappingTime { get; set; }
            public Dictionary<string, MappingMetrics> TypeMetrics { get; set; } = new();
            public List<PerformanceSnapshot> Snapshots { get; set; } = new();
            public Dictionary<string, object> CustomData { get; set; } = new();
        }

        /// <summary>
        /// Performance snapshot
        /// </summary>
        public class PerformanceSnapshot
        {
            public DateTime Timestamp { get; set; }
            public long TotalMappings { get; set; }
            public TimeSpan AverageTime { get; set; }
            public long MemoryUsage { get; set; }
            public int ThreadCount { get; set; }
            public Dictionary<string, object> Metrics { get; set; } = new();
        }

        /// <summary>
        /// Mapping event
        /// </summary>
        public class MappingEvent
        {
            public DateTime Timestamp { get; set; }
            public string EventType { get; set; }
            public Type SourceType { get; set; }
            public Type TargetType { get; set; }
            public TimeSpan Duration { get; set; }
            public bool Success { get; set; }
            public Exception Error { get; set; }
            public Dictionary<string, object> Metadata { get; set; } = new();
        }

        /// <summary>
        /// Diagnostic raporu
        /// </summary>
        public class DiagnosticReport
        {
            public DateTime GeneratedAt { get; set; }
            public TimeSpan ReportPeriod { get; set; }
            public Dictionary<string, MappingMetrics> AllMetrics { get; set; } = new();
            public List<PerformanceProfile> PerformanceProfiles { get; set; } = new();
            public List<MappingEvent> RecentEvents { get; set; } = new();
            public DiagnosticSummary Summary { get; set; }
            public List<PerformanceRecommendation> Recommendations { get; set; } = new();
        }

        /// <summary>
        /// Diagnostic özeti
        /// </summary>
        public class DiagnosticSummary
        {
            public long TotalMappings { get; set; }
            public long SuccessfulMappings { get; set; }
            public long FailedMappings { get; set; }
            public double SuccessRate { get; set; }
            public TimeSpan AverageMappingTime { get; set; }
            public TimeSpan TotalMappingTime { get; set; }
            public int UniqueTypePairs { get; set; }
            public string MostUsedMapping { get; set; }
            public string SlowestMapping { get; set; }
            public string FastestMapping { get; set; }
        }

        /// <summary>
        /// Performance önerisi
        /// </summary>
        public class PerformanceRecommendation
        {
            public string Category { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Impact { get; set; }
            public string Suggestion { get; set; }
            public double EstimatedImprovement { get; set; }
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
                    metrics.ErrorCount++;
                    if (error != null && metrics.RecentErrors.Count < 10)
                    {
                        metrics.RecentErrors.Add(error);
                    }
                }

                // Zaman istatistikleri
                if (metrics.TotalCalls == 1)
                {
                    metrics.FirstCallTime = DateTime.UtcNow;
                    metrics.MinTime = duration;
                    metrics.MaxTime = duration;
                }
                else
                {
                    if (duration < metrics.MinTime) metrics.MinTime = duration;
                    if (duration > metrics.MaxTime) metrics.MaxTime = duration;
                }

                metrics.AverageTime = TimeSpan.FromTicks(metrics.TotalTime.Ticks / metrics.TotalCalls);
                metrics.LastCallTime = DateTime.UtcNow;

                // Event log'a kaydet
                LogMappingEvent(typeof(TSource), typeof(TTarget), duration, success, error);
            }
        }

        /// <summary>
        /// Performance profili başlat
        /// </summary>
        public static PerformanceProfile StartPerformanceProfile(string profileName)
        {
            var profile = new PerformanceProfile
            {
                ProfileName = profileName,
                CreatedAt = DateTime.UtcNow
            };

            _performanceProfiles[profileName] = profile;
            return profile;
        }

        /// <summary>
        /// Performance profili bitir
        /// </summary>
        public static PerformanceProfile EndPerformanceProfile(string profileName)
        {
            if (_performanceProfiles.TryGetValue(profileName, out var profile))
            {
                profile.Duration = DateTime.UtcNow - profile.CreatedAt;
                
                // Tüm metrikleri topla
                foreach (var metrics in _metricsCache.Values)
                {
                    lock (metrics)
                    {
                        if (metrics.LastCallTime >= profile.CreatedAt)
                        {
                            profile.TotalMappings += metrics.TotalCalls;
                            profile.SuccessfulMappings += metrics.SuccessfulCalls;
                            profile.FailedMappings += metrics.FailedCalls;
                            profile.TotalMappingTime += metrics.TotalTime;
                        }
                    }
                }

                if (profile.TotalMappings > 0)
                {
                    profile.AverageMappingTime = TimeSpan.FromTicks(profile.TotalMappingTime.Ticks / profile.TotalMappings);
                }

                // Snapshot oluştur
                var snapshot = new PerformanceSnapshot
                {
                    Timestamp = DateTime.UtcNow,
                    TotalMappings = profile.TotalMappings,
                    AverageTime = profile.AverageMappingTime,
                    MemoryUsage = GC.GetTotalMemory(false),
                    ThreadCount = Thread.CurrentThread.ManagedThreadId
                };

                profile.Snapshots.Add(snapshot);
            }

            return profile;
        }

        /// <summary>
        /// Diagnostic raporu oluştur
        /// </summary>
        public static DiagnosticReport GenerateDiagnosticReport(TimeSpan? period = null)
        {
            var report = new DiagnosticReport
            {
                GeneratedAt = DateTime.UtcNow,
                ReportPeriod = period ?? TimeSpan.FromHours(1)
            };

            var cutoffTime = DateTime.UtcNow - report.ReportPeriod;

            // Tüm metrikleri topla
            foreach (var kvp in _metricsCache)
            {
                var metrics = kvp.Value;
                lock (metrics)
                {
                    if (metrics.LastCallTime >= cutoffTime)
                    {
                        var typeKey = GetTypeKeyString(kvp.Key);
                        report.AllMetrics[typeKey] = metrics;
                    }
                }
            }

            // Performance profillerini topla
            report.PerformanceProfiles = _performanceProfiles.Values
                .Where(p => p.CreatedAt >= cutoffTime)
                .ToList();

            // Son event'leri topla
            report.RecentEvents = _eventLog
                .Where(e => e.Timestamp >= cutoffTime)
                .OrderByDescending(e => e.Timestamp)
                .Take(100)
                .ToList();

            // Özet hesapla
            CalculateDiagnosticSummary(report);

            // Öneriler oluştur
            GenerateRecommendations(report);

            return report;
        }

        /// <summary>
        /// Diagnostic özeti hesapla
        /// </summary>
        private static void CalculateDiagnosticSummary(DiagnosticReport report)
        {
            var summary = new DiagnosticSummary();

            foreach (var metrics in report.AllMetrics.Values)
            {
                summary.TotalMappings += metrics.TotalCalls;
                summary.SuccessfulMappings += metrics.SuccessfulCalls;
                summary.FailedMappings += metrics.FailedCalls;
                summary.TotalMappingTime += metrics.TotalTime;
            }

            summary.UniqueTypePairs = report.AllMetrics.Count;
            summary.SuccessRate = summary.TotalMappings > 0 ? (double)summary.SuccessfulMappings / summary.TotalMappings : 0;
            summary.AverageMappingTime = summary.TotalMappings > 0 ? 
                TimeSpan.FromTicks(summary.TotalMappingTime.Ticks / summary.TotalMappings) : TimeSpan.Zero;

            // En çok kullanılan mapping
            var mostUsed = report.AllMetrics.OrderByDescending(m => m.Value.TotalCalls).FirstOrDefault();
            summary.MostUsedMapping = mostUsed.Key;

            // En yavaş mapping
            var slowest = report.AllMetrics.OrderByDescending(m => m.Value.AverageTime).FirstOrDefault();
            summary.SlowestMapping = slowest.Key;

            // En hızlı mapping
            var fastest = report.AllMetrics.OrderBy(m => m.Value.AverageTime).FirstOrDefault();
            summary.FastestMapping = fastest.Key;

            report.Summary = summary;
        }

        /// <summary>
        /// Performance önerileri oluştur
        /// </summary>
        private static void GenerateRecommendations(DiagnosticReport report)
        {
            var recommendations = new List<PerformanceRecommendation>();

            // Başarı oranı düşükse
            if (report.Summary.SuccessRate < 0.95)
            {
                recommendations.Add(new PerformanceRecommendation
                {
                    Category = "Error Handling",
                    Title = "Yüksek Hata Oranı",
                    Description = $"Mapping başarı oranı %{report.Summary.SuccessRate:P1}",
                    Impact = "High",
                    Suggestion = "Mapping konfigürasyonlarını gözden geçirin ve hata durumlarını ele alın",
                    EstimatedImprovement = 0.15
                });
            }

            // Ortalama süre yüksekse
            if (report.Summary.AverageMappingTime > TimeSpan.FromMilliseconds(10))
            {
                recommendations.Add(new PerformanceRecommendation
                {
                    Category = "Performance",
                    Title = "Yavaş Mapping",
                    Description = $"Ortalama mapping süresi {report.Summary.AverageMappingTime.TotalMilliseconds:F2}ms",
                    Impact = "Medium",
                    Suggestion = "Mapping cache'ini optimize edin ve gereksiz dönüşümleri azaltın",
                    EstimatedImprovement = 0.25
                });
            }

            // Çok fazla farklı tip çifti varsa
            if (report.Summary.UniqueTypePairs > 50)
            {
                recommendations.Add(new PerformanceRecommendation
                {
                    Category = "Memory",
                    Title = "Çok Fazla Tip Çifti",
                    Description = $"{report.Summary.UniqueTypePairs} farklı tip çifti cache'lenmiş",
                    Impact = "Low",
                    Suggestion = "Cache boyutunu kontrol edin ve gereksiz mapping'leri temizleyin",
                    EstimatedImprovement = 0.05
                });
            }

            report.Recommendations = recommendations;
        }

        /// <summary>
        /// Mapping event'ini logla
        /// </summary>
        private static void LogMappingEvent(Type sourceType, Type targetType, TimeSpan duration, bool success, Exception error)
        {
            var mappingEvent = new MappingEvent
            {
                Timestamp = DateTime.UtcNow,
                EventType = success ? "MappingSuccess" : "MappingError",
                SourceType = sourceType,
                TargetType = targetType,
                Duration = duration,
                Success = success,
                Error = error
            };

            _eventLog.Enqueue(mappingEvent);

            // Event log boyutunu kontrol et
            while (_eventLog.Count > _maxEventLogSize)
            {
                _eventLog.TryDequeue(out _);
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
        /// Tüm metrikleri al
        /// </summary>
        public static Dictionary<string, MappingMetrics> GetAllMetrics()
        {
            var result = new Dictionary<string, MappingMetrics>();
            foreach (var kvp in _metricsCache)
            {
                result[GetTypeKeyString(kvp.Key)] = kvp.Value;
            }
            return result;
        }

        /// <summary>
        /// Performance profili al
        /// </summary>
        public static PerformanceProfile GetPerformanceProfile(string profileName)
        {
            return _performanceProfiles.TryGetValue(profileName, out var profile) ? profile : null;
        }

        /// <summary>
        /// Tüm performance profillerini al
        /// </summary>
        public static List<PerformanceProfile> GetAllPerformanceProfiles()
        {
            return _performanceProfiles.Values.ToList();
        }

        /// <summary>
        /// Son event'leri al
        /// </summary>
        public static List<MappingEvent> GetRecentEvents(int count = 100)
        {
            return _eventLog.Take(count).ToList();
        }

        /// <summary>
        /// Diagnostic'leri etkinleştir/devre dışı bırak
        /// </summary>
        public static void SetDiagnosticsEnabled(bool enabled)
        {
            _diagnosticsEnabled = enabled;
        }

        /// <summary>
        /// Event log boyutunu ayarla
        /// </summary>
        public static void SetMaxEventLogSize(int size)
        {
            _maxEventLogSize = size;
        }

        /// <summary>
        /// Tüm cache'leri temizle
        /// </summary>
        public static void ClearAllCaches()
        {
            _metricsCache.Clear();
            _performanceProfiles.Clear();
            
            while (_eventLog.TryDequeue(out _)) { }
        }

        /// <summary>
        /// Belirli bir tip çiftinin metriklerini temizle
        /// </summary>
        public static void ClearMetrics<TSource, TTarget>()
        {
            var key = GetTypeKey(typeof(TSource), typeof(TTarget));
            _metricsCache.TryRemove(key, out _);
        }

        /// <summary>
        /// Performance profili sil
        /// </summary>
        public static bool DeletePerformanceProfile(string profileName)
        {
            return _performanceProfiles.TryRemove(profileName, out _);
        }

        /// <summary>
        /// Diagnostic istatistiklerini al
        /// </summary>
        public static DiagnosticStatistics GetDiagnosticStatistics()
        {
            return new DiagnosticStatistics
            {
                TotalMetrics = _metricsCache.Count,
                TotalProfiles = _performanceProfiles.Count,
                EventLogSize = _eventLog.Count,
                DiagnosticsEnabled = _diagnosticsEnabled
            };
        }

        /// <summary>
        /// Diagnostic istatistikleri
        /// </summary>
        public class DiagnosticStatistics
        {
            public int TotalMetrics { get; set; }
            public int TotalProfiles { get; set; }
            public int EventLogSize { get; set; }
            public bool DiagnosticsEnabled { get; set; }
        }

        /// <summary>
        /// Type key oluştur
        /// </summary>
        private static long GetTypeKey(Type sourceType, Type targetType)
        {
            return ((long)sourceType.GetHashCode() << 32) | (uint)targetType.GetHashCode();
        }

        /// <summary>
        /// Type key string'e çevir
        /// </summary>
        private static string GetTypeKeyString(long key)
        {
            var sourceHash = (int)(key >> 32);
            var targetHash = (int)(key & 0xFFFFFFFF);
            return $"{sourceHash:X8}->{targetHash:X8}";
        }
    }
} 