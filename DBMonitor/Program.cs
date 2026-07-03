using DBMonitor;
using Microsoft.Data.SqlClient;
using StackExchange.Redis;

namespace SqlServerMonitor
{
    class Program
    {
        /*private const string SqlConnectionStringData =
            "Data Source=localhost;Initial Catalog=FusionCacheDemo;Integrated Security=False;User Id=sa;Password=Damian2202@@;TrustServerCertificate=true;";*/

        private const string SqlConnectionString =
            "Data Source=localhost;Initial Catalog=TowbookDev;Integrated Security=False;User Id=sa;Password=Damian2202@@;TrustServerCertificate=true;";

        const string RedisConnectionString =
            "localhost:6379,allowAdmin=true";

        const string DatabaseName = "TowbookDev";

        static async Task Main()
        {
            var redis = ConnectionMultiplexer.Connect(RedisConnectionString);

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            long sqlBaseline;
            long redisBaseline;

            using (var conn = new SqlConnection(SqlConnectionString))
            {
                await conn.OpenAsync();

                sqlBaseline = await GetTotalExecutions(conn);

                var server = redis.GetServers().First();

                redisBaseline =
                    GetRedisLong(server, "Stats", "total_commands_processed");
            }

            /*await using var metricsConn =
                new SqlConnection(SqlConnectionStringData);

            await metricsConn.OpenAsync();*/

            while (true)
            {
                try
                {
                    using var conn = new SqlConnection(SqlConnectionString);

                    await conn.OpenAsync();

                    var sqlMetrics =
                        await GetSqlMetrics(conn);

                    sqlMetrics.QueriesSinceStart =
                        sqlMetrics.TotalExecutions - sqlBaseline;

                    var redisMetrics =
                        GetRedisMetrics(redis);

                    redisMetrics.CommandsSinceStart =
                        redisMetrics.TotalCommandsProcessed - redisBaseline;

                    CsvMetricsLogger.Log(sqlMetrics, redisMetrics);

                    //await SaveMetrics(metricsConn, sqlMetrics, redisMetrics);

                    Console.Clear();
                    DrawDashboard(sqlMetrics, redisMetrics);
                }
                catch (Exception ex)
                {
                    Console.Clear();
                    Console.WriteLine(ex);
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        static async Task<SqlMetrics> GetSqlMetrics(SqlConnection conn)
        {
            var metrics = new SqlMetrics();

            metrics.ActiveConnections =
                await ExecuteInt(conn, @"
            SELECT COUNT(*)
            FROM sys.dm_exec_sessions
            WHERE database_id = DB_ID(@db)
            AND is_user_process = 1");

            metrics.SleepingConnections =
                await ExecuteInt(conn, @"
            SELECT COUNT(*)
            FROM sys.dm_exec_sessions
            WHERE database_id = DB_ID(@db)
            AND is_user_process = 1
            AND status = 'sleeping'");

            metrics.ActiveRequests =
                await ExecuteInt(conn, @"
            SELECT COUNT(*)
            FROM sys.dm_exec_requests
            WHERE database_id = DB_ID(@db)");

            metrics.BlockingSessions =
                await ExecuteInt(conn, @"
            SELECT COUNT(*)
            FROM sys.dm_exec_requests
            WHERE blocking_session_id <> 0");

            metrics.LongRunningQueries =
                await ExecuteInt(conn, @"
            SELECT COUNT(*)
            FROM sys.dm_exec_requests
            WHERE database_id = DB_ID(@db)
            AND total_elapsed_time > 30000");

            metrics.TotalExecutions =
                await GetTotalExecutions(conn);

            metrics.DatabaseSizeMB =
                await GetDatabaseSize(conn);

            metrics.LogPercentUsed =
                await GetLogUsage(conn);

            metrics.SqlMemoryMB =
                await GetSqlMemory(conn);

            metrics.SqlCpuPercent =
                await GetSqlCpu(conn);

            metrics.Applications =
                await GetTopApplications(conn);

            metrics.Waits =
                await GetTopWaits(conn);

            return metrics;
        }

        static async Task<long> GetTotalExecutions(SqlConnection conn)
        {
            using var cmd = new SqlCommand(@"
                SELECT cntr_value
                FROM sys.dm_os_performance_counters
                WHERE counter_name = 'Batch Requests/sec'", conn);

            var result = await cmd.ExecuteScalarAsync();

            return Convert.ToInt64(result);
        }

        static async Task<double> GetDatabaseSize(SqlConnection conn)
        {
            using var cmd = new SqlCommand(@"
        SELECT
            SUM(size) * 8.0 / 1024
        FROM TowbookDev.sys.database_files", conn);

            var result = await cmd.ExecuteScalarAsync();

            return Convert.ToDouble(result);
        }

        static async Task<double> GetLogUsage(SqlConnection conn)
        {
            using var cmd = new SqlCommand(@"
        SELECT used_log_space_in_percent
        FROM sys.dm_db_log_space_usage", conn);

            var result = await cmd.ExecuteScalarAsync();

            return Convert.ToDouble(result);
        }

        static async Task<long> GetSqlMemory(SqlConnection conn)
        {
            using var cmd = new SqlCommand(@"
        SELECT physical_memory_in_use_kb / 1024
        FROM sys.dm_os_process_memory", conn);

            var result = await cmd.ExecuteScalarAsync();

            return Convert.ToInt64(result);
        }

        static async Task<int> GetSqlCpu(SqlConnection conn)
        {
            using var cmd = new SqlCommand(@"
        SELECT TOP (1)
            CAST(record AS xml).value(
                '(./Record/SchedulerMonitorEvent/SystemHealth/ProcessUtilization)[1]',
                'int'
            )
        FROM sys.dm_os_ring_buffers
        WHERE ring_buffer_type = N'RING_BUFFER_SCHEDULER_MONITOR'
        ORDER BY [timestamp] DESC", conn);

            var result = await cmd.ExecuteScalarAsync();

            return result == null
                ? 0
                : Convert.ToInt32(result);
        }

        static async Task<List<TopApplication>> GetTopApplications(
    SqlConnection conn)
        {
            var list = new List<TopApplication>();

            using var cmd = new SqlCommand(@"
        SELECT TOP 10
            ISNULL(program_name,'Unknown'),
            COUNT(*)
        FROM sys.dm_exec_sessions
        WHERE database_id = DB_ID(@db)
        AND is_user_process = 1
        GROUP BY program_name
        ORDER BY COUNT(*) DESC", conn);

            cmd.Parameters.AddWithValue("@db", DatabaseName);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new TopApplication
                {
                    ProgramName = reader.GetString(0),
                    Connections = reader.GetInt32(1)
                });
            }

            return list;
        }

        static async Task<List<WaitStat>> GetTopWaits(
    SqlConnection conn)
        {
            var waits = new List<WaitStat>();

            using var cmd = new SqlCommand(@"
        SELECT TOP 5
            wait_type,
            wait_time_ms
        FROM sys.dm_os_wait_stats
        WHERE wait_type NOT LIKE 'SLEEP%'
        ORDER BY wait_time_ms DESC", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                waits.Add(new WaitStat
                {
                    WaitType = reader.GetString(0),
                    WaitMs = Convert.ToInt64(reader[1])
                });
            }

            return waits;
        }

        static async Task<int> ExecuteInt(
    SqlConnection conn,
    string sql)
        {
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@db", DatabaseName);

            var result = await cmd.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }

        static RedisMetrics GetRedisMetrics(ConnectionMultiplexer redis)
        {
            var metrics = new RedisMetrics();

            try
            {
                var server = redis.GetServers().First();

                metrics.ConnectedClients =
                    (int)GetRedisLong(server, "Clients", "connected_clients");

                metrics.BlockedClients =
                    (int)GetRedisLong(server, "Clients", "blocked_clients");

                metrics.TotalCommandsProcessed =
                    GetRedisLong(server, "Stats", "total_commands_processed");

                metrics.OpsPerSecond =
                    (int)GetRedisLong(server, "Stats", "instantaneous_ops_per_sec");

                metrics.UsedMemory =
                    GetRedisLong(server, "Memory", "used_memory");

                metrics.PeakMemory =
                    GetRedisLong(server, "Memory", "used_memory_peak");

                metrics.Hits =
                    GetRedisLong(server, "Stats", "keyspace_hits");

                metrics.Misses =
                    GetRedisLong(server, "Stats", "keyspace_misses");

                metrics.ExpiredKeys =
                    GetRedisLong(server, "Stats", "expired_keys");

                metrics.EvictedKeys =
                    GetRedisLong(server, "Stats", "evicted_keys");
            }
            catch
            {
                // Redis unavailable
            }

            return metrics;
        }

        static long GetRedisLong(
    IServer server,
    string section,
    string key)
        {
            try
            {
                var info = server.Info(section);

                foreach (var group in info)
                {
                    foreach (var pair in group)
                    {
                        if (pair.Key.Equals(
                            key,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            if (long.TryParse(
                                pair.Value,
                                out var value))
                            {
                                return value;
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            return 0;
        }

        static void DrawDashboard(
    SqlMetrics sql,
    RedisMetrics redis)
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine(
                "==========================================================================");

            Console.WriteLine(
                $" DATABASE MONITOR  |  {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            Console.WriteLine(
                "==========================================================================");

            Console.ResetColor();

            DrawSqlSection(sql);

            Console.WriteLine();

            DrawRedisSection(redis);

            /*Console.WriteLine();

            DrawWarnings(sql, redis);*/

            Console.WriteLine();
            Console.WriteLine("Refreshing every 5 seconds...");
        }

        static void DrawSqlSection(SqlMetrics sql)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine("SQL SERVER (TowbookDev)");
            Console.WriteLine("-----------------------");

            Console.ResetColor();

            Console.WriteLine(
                $"Active Connections      : {sql.ActiveConnections:N0}");

            Console.WriteLine(
                $"Sleeping Connections    : {sql.SleepingConnections:N0}");

            Console.WriteLine(
                $"Executing Requests      : {sql.ActiveRequests:N0}");

            Console.WriteLine(
                $"Blocking Sessions       : {sql.BlockingSessions:N0}");

            Console.WriteLine(
                $"Long Running Queries    : {sql.LongRunningQueries:N0}");

            Console.WriteLine(
                $"Queries Since Start     : {sql.QueriesSinceStart:N0}");

            Console.WriteLine(
                $"Database Size           : {sql.DatabaseSizeMB:N2} MB");

            Console.WriteLine(
                $"Log Usage               : {sql.LogPercentUsed:N2}%");

            Console.WriteLine(
                $"SQL Memory              : {sql.SqlMemoryMB:N0} MB");

            Console.WriteLine(
                $"SQL CPU                 : {sql.SqlCpuPercent}%");

            /*Console.WriteLine();

            Console.WriteLine("Top Applications");

            foreach (var app in sql.Applications)
            {
                Console.WriteLine(
                    $"  {app.ProgramName,-40} {app.Connections,6}");
            }

            Console.WriteLine();

            Console.WriteLine("Top Waits");

            foreach (var wait in sql.Waits)
            {
                Console.WriteLine(
                    $"  {wait.WaitType,-35} {wait.WaitMs,12:N0} ms");
            }*/
        }

        static void DrawRedisSection(
    RedisMetrics redis)
        {
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("REDIS");
            Console.WriteLine("-----");

            Console.ResetColor();

            Console.WriteLine(
                $"Connected Clients       : {redis.ConnectedClients:N0}");

            Console.WriteLine(
                $"Blocked Clients         : {redis.BlockedClients:N0}");

            Console.WriteLine(
                $"Operations / Second     : {redis.OpsPerSecond:N0}");

            Console.WriteLine(
                $"Requests Since Start    : {redis.CommandsSinceStart:N0}");

            Console.WriteLine(
                $"Memory Used             : {FormatBytes(redis.UsedMemory)}");

            Console.WriteLine(
                $"Peak Memory             : {FormatBytes(redis.PeakMemory)}");

            Console.WriteLine(
                $"Cache Hit Rate          : {redis.HitRate:N2}%");

            Console.WriteLine(
                $"Expired Keys            : {redis.ExpiredKeys:N0}");

            Console.WriteLine(
                $"Evicted Keys            : {redis.EvictedKeys:N0}");
        }

        static string FormatBytes(long bytes)
        {
            string[] suffixes =
            [
                "B",
        "KB",
        "MB",
        "GB",
        "TB"
            ];

            double value = bytes;

            int suffix = 0;

            while (value >= 1024 &&
                   suffix < suffixes.Length - 1)
            {
                value /= 1024;
                suffix++;
            }

            return $"{value:N2} {suffixes[suffix]}";
        }
    }

    public class SqlMetrics
    {
        public int ActiveConnections { get; set; }
        public int SleepingConnections { get; set; }
        public int ActiveRequests { get; set; }
        public int BlockingSessions { get; set; }

        public long TotalExecutions { get; set; }
        public long QueriesSinceStart { get; set; }

        public double DatabaseSizeMB { get; set; }

        public double LogPercentUsed { get; set; }

        public long SqlMemoryMB { get; set; }

        public int SqlCpuPercent { get; set; }

        public int LongRunningQueries { get; set; }

        public List<TopApplication> Applications { get; set; } = [];

        public List<WaitStat> Waits { get; set; } = [];
    }

    public class TopApplication
    {
        public string ProgramName { get; set; } = "";
        public int Connections { get; set; }
    }

    public class WaitStat
    {
        public string WaitType { get; set; } = "";
        public long WaitMs { get; set; }
    }

    public class RedisMetrics
    {
        public int ConnectedClients { get; set; }
        public int BlockedClients { get; set; }

        public long TotalCommandsProcessed { get; set; }
        public long CommandsSinceStart { get; set; }

        public int OpsPerSecond { get; set; }

        public long UsedMemory { get; set; }
        public long PeakMemory { get; set; }

        public long Hits { get; set; }
        public long Misses { get; set; }

        public long ExpiredKeys { get; set; }
        public long EvictedKeys { get; set; }

        public double HitRate =>
            Hits + Misses == 0
                ? 0
                : (double)Hits /
                  (Hits + Misses) * 100.0;
    }
}