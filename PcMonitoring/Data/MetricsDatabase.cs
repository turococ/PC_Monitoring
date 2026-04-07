using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace PcMonitoring.Data
{
    public class MetricRecord
    {
        public DateTime Timestamp { get; set; }
        public float? CpuLoad { get; set; }
        public float? CpuTemp { get; set; }
        public float? GpuLoad { get; set; }
        public float? GpuTemp { get; set; }
        public float? RamUsedPercent { get; set; }
    }

    public class MetricsDatabase : IDisposable
    {
        private readonly string _dbPath;
        private SqliteConnection? _connection;

        public MetricsDatabase()
        {
            _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "metrics.db");
            Initialize();
        }

        private void Initialize()
        {
            _connection = new SqliteConnection($"Data Source={_dbPath}");
            _connection.Open();

            var createTable = @"
                CREATE TABLE IF NOT EXISTS HardwareMetrics (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp TEXT NOT NULL,
                    CpuLoad REAL,
                    CpuTemp REAL,
                    GpuLoad REAL,
                    GpuTemp REAL,
                    RamUsedPercent REAL
                )";

            using var cmd = new SqliteCommand(createTable, _connection);
            cmd.ExecuteNonQuery();

            // Индекс для ускорения запросов по времени
            var createIndex = @"
                CREATE INDEX IF NOT EXISTS IX_HardwareMetrics_Timestamp 
                ON HardwareMetrics(Timestamp)";

            using var indexCmd = new SqliteCommand(createIndex, _connection);
            indexCmd.ExecuteNonQuery();

            // Автоочистка данных старше 30 дней
            CleanupOldData();
        }

        public void InsertMetric(float? cpuLoad, float? cpuTemp, float? gpuLoad, float? gpuTemp, float? ramUsedPercent)
        {
            if (_connection == null) return;

            var insert = @"
                INSERT INTO HardwareMetrics (Timestamp, CpuLoad, CpuTemp, GpuLoad, GpuTemp, RamUsedPercent)
                VALUES ($timestamp, $cpuLoad, $cpuTemp, $gpuLoad, $gpuTemp, $ramUsedPercent)";

            using var cmd = new SqliteCommand(insert, _connection);
            cmd.Parameters.AddWithValue("$timestamp", DateTime.UtcNow.ToString("O"));
            cmd.Parameters.AddWithValue("$cpuLoad", cpuLoad ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$cpuTemp", cpuTemp ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$gpuLoad", gpuLoad ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$gpuTemp", gpuTemp ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$ramUsedPercent", ramUsedPercent ?? (object)DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public List<MetricRecord> GetMetrics(TimeSpan timeRange)
        {
            if (_connection == null) return new List<MetricRecord>();

            var since = DateTime.UtcNow - timeRange;
            var select = @"
                SELECT Timestamp, CpuLoad, CpuTemp, GpuLoad, GpuTemp, RamUsedPercent
                FROM HardwareMetrics
                WHERE Timestamp >= $since
                ORDER BY Timestamp ASC";

            var results = new List<MetricRecord>();

            using var cmd = new SqliteCommand(select, _connection);
            cmd.Parameters.AddWithValue("$since", since.ToString("O"));

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new MetricRecord
                {
                    Timestamp = DateTime.Parse(reader.GetString(0)),
                    CpuLoad = reader.IsDBNull(1) ? null : reader.GetFloat(1),
                    CpuTemp = reader.IsDBNull(2) ? null : reader.GetFloat(2),
                    GpuLoad = reader.IsDBNull(3) ? null : reader.GetFloat(3),
                    GpuTemp = reader.IsDBNull(4) ? null : reader.GetFloat(4),
                    RamUsedPercent = reader.IsDBNull(5) ? null : reader.GetFloat(5)
                });
            }

            return results;
        }

        public (float min, float max, float avg) GetStats(string column, TimeSpan timeRange)
        {
            if (_connection == null) return (0, 0, 0);

            var since = DateTime.UtcNow - timeRange;
            var select = $@"
                SELECT MIN({column}) as minVal, MAX({column}) as maxVal, AVG({column}) as avgVal
                FROM HardwareMetrics
                WHERE Timestamp >= $since AND {column} IS NOT NULL";

            using var cmd = new SqliteCommand(select, _connection);
            cmd.Parameters.AddWithValue("$since", since.ToString("O"));

            using var reader = cmd.ExecuteReader();
            if (reader.Read() && !reader.IsDBNull(0))
            {
                return (
                    reader.IsDBNull(0) ? 0 : reader.GetFloat(0),
                    reader.IsDBNull(1) ? 0 : reader.GetFloat(1),
                    reader.IsDBNull(2) ? 0 : reader.GetFloat(2)
                );
            }

            return (0, 0, 0);
        }

        private void CleanupOldData()
        {
            if (_connection == null) return;

            var cutoff = DateTime.UtcNow.AddDays(-30).ToString("O");
            var delete = @"DELETE FROM HardwareMetrics WHERE Timestamp < $cutoff";

            using var cmd = new SqliteCommand(delete, _connection);
            cmd.Parameters.AddWithValue("$cutoff", cutoff);
            cmd.ExecuteNonQuery();
        }

        public List<MetricRecord> GetRecentMetrics(int count)
        {
            if (_connection == null) return new List<MetricRecord>();

            var select = @"
                SELECT Timestamp, CpuLoad, CpuTemp, GpuLoad, GpuTemp, RamUsedPercent
                FROM HardwareMetrics
                ORDER BY Timestamp DESC
                LIMIT $limit";

            var results = new List<MetricRecord>();

            using var cmd = new SqliteCommand(select, _connection);
            cmd.Parameters.AddWithValue("$limit", count);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new MetricRecord
                {
                    Timestamp = DateTime.Parse(reader.GetString(0)),
                    CpuLoad = reader.IsDBNull(1) ? null : reader.GetFloat(1),
                    CpuTemp = reader.IsDBNull(2) ? null : reader.GetFloat(2),
                    GpuLoad = reader.IsDBNull(3) ? null : reader.GetFloat(3),
                    GpuTemp = reader.IsDBNull(4) ? null : reader.GetFloat(4),
                    RamUsedPercent = reader.IsDBNull(5) ? null : reader.GetFloat(5)
                });
            }

            results.Reverse();
            return results;
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
