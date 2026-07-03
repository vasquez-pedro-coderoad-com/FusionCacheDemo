using SqlServerMonitor;
using System.Globalization;
using System.Text;

namespace DBMonitor
{
    public static class CsvMetricsLogger
    {
        private static readonly string timestamp_str = DateTime.UtcNow.ToString(
                "yyyy_MM_dd_HH_mm_ss",
                CultureInfo.InvariantCulture);

        private static readonly string FileName =
            $"database_metrics_{timestamp_str}.csv";

        private static readonly object _lock = new();

        public static void Log(
            SqlMetrics sql,
            RedisMetrics redis)
        {
            lock (_lock)
            {
                bool fileExists =
                    File.Exists(FileName);

                using var writer =
                    new StreamWriter(
                        FileName,
                        append: true,
                        Encoding.UTF8);

                if (!fileExists)
                {
                    writer.WriteLine(GetHeader());
                }

                writer.WriteLine(GetLine(sql, redis));
            }
        }

        private static string GetHeader()
        {
            return string.Join(",",
            [
                "Timestamp",

            "SqlActiveConnections",
            "SqlSleepingConnections",
            "SqlActiveRequests",
            "SqlBlockingSessions",
            "SqlLongRunningQueries",
            "SqlQueriesSinceStart",
            "SqlDatabaseSizeMB",
            "SqlLogPercentUsed",
            "SqlMemoryMB",
            "SqlCpuPercent",

            "RedisConnectedClients",
            "RedisBlockedClients",
            "RedisOpsPerSecond",
            "RedisCommandsSinceStart",
            "RedisUsedMemoryBytes",
            "RedisPeakMemoryBytes",
            "RedisHitRate",
            "RedisExpiredKeys",
            "RedisEvictedKeys"
            ]);
        }

        private static string GetLine(
            SqlMetrics sql,
            RedisMetrics redis)
        {
            return string.Join(",",
            [
                DateTime.UtcNow.ToString(
                "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture),

            sql.ActiveConnections,
            sql.SleepingConnections,
            sql.ActiveRequests,
            sql.BlockingSessions,
            sql.LongRunningQueries,
            sql.QueriesSinceStart,

            sql.DatabaseSizeMB.ToString(
                CultureInfo.InvariantCulture),

            sql.LogPercentUsed.ToString(
                CultureInfo.InvariantCulture),

            sql.SqlMemoryMB,
            sql.SqlCpuPercent,

            redis.ConnectedClients,
            redis.BlockedClients,
            redis.OpsPerSecond,
            redis.CommandsSinceStart,
            redis.UsedMemory,
            redis.PeakMemory,

            redis.HitRate.ToString(
                CultureInfo.InvariantCulture),

            redis.ExpiredKeys,
            redis.EvictedKeys
            ]);
        }
    }
}
