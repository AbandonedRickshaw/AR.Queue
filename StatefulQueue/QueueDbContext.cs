using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using StatefulQueue.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StatefulQueue
{
    public partial class QueueDbContext<TBody, TStateInfo>
        : DbContext
        , IQueueDbContext<QueueItem<TBody, TStateInfo>, TBody, TStateInfo>
    {
        private readonly ILoggerFactory _loggerFactory;
        private ILogger<QueueDbContext<TBody, TStateInfo>> _logger;


        public QueueDbContext(DbContextOptions<QueueDbContext<TBody, TStateInfo>> options)
            : this(options, NullLoggerFactory.Instance) { }

        public QueueDbContext(DbContextOptions<QueueDbContext<TBody, TStateInfo>> options, ILoggerFactory loggerFactory)
            : base(options)
        {
            _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
            _loggerFactory.CreateLogger<QueueDbContext<TBody, TStateInfo>>();
        }

        public DbSet<QueueItem<TBody, TStateInfo>> QueueItems { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration<QueueItem<TBody, TStateInfo>>();
        }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbUpdateException dbuex)
            {
                _logger.LogError(dbuex.Message);
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    try
                    {
                        var content = new List<string>() { dbuex.Message };
                        var inner = dbuex.InnerException;
                        string innermostException = dbuex.Message;
                        while (inner != null)
                        {
                            innermostException = inner.Message;
                            _logger.LogError(inner.Message);
                            inner = inner.InnerException;
                        }
                        _logger.LogDebug("See below for affected entry or entries.");

                        int entryCount = dbuex.Entries.Count;
                        for (int i = 0; i < entryCount; i++)
                        {
                            var entry = dbuex.Entries[i];

                            var propLines = new List<string>();
                            foreach (var prop in entry.GetPrimaryKeys())
                            {
                                propLines.Add($"Key value(s) of affected entry {i + 1} of {entryCount}: {prop.Name} = {entry.Property(prop.Name).CurrentValue}");
                            }

                            _logger.LogDebug($"{string.Join(',', propLines)}");
                        }
                    }
                    catch { }
                }
                throw;
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    var content = new List<string>() { ex.Message };
                    var inner = ex.InnerException;
                    string innermostException = ex.Message;
                    while (inner != null)
                    {
                        innermostException = inner.Message;
                        _logger.LogError(inner.Message);
                        inner = inner.InnerException;
                    }
                }
                else
                {
                    _logger.LogError(ex.ToString());
                }
                throw;
            }
        }

        public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException dbuex)
            {
                _logger.LogError(dbuex.Message);
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    try
                    {
                        var content = new List<string>() { dbuex.Message };
                        var inner = dbuex.InnerException;
                        string innermostException = dbuex.Message;
                        while (inner != null)
                        {
                            innermostException = inner.Message;
                            _logger.LogError(inner.Message);
                            inner = inner.InnerException;
                        }
                        _logger.LogDebug("See below for affected entry or entries.");

                        int entryCount = dbuex.Entries.Count;
                        for (int i = 0; i < entryCount; i++)
                        {
                            var entry = dbuex.Entries[i];

                            var propLines = new List<string>();
                            foreach (var prop in entry.GetPrimaryKeys())
                            {
                                propLines.Add($"Key value(s) of affected entry {i + 1} of {entryCount}: {prop.Name} = {entry.Property(prop.Name).CurrentValue}");
                            }

                            _logger.LogDebug($"{string.Join(',', propLines)}");
                        }
                    }
                    catch { }
                }
                throw;
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    var content = new List<string>() { ex.Message };
                    var inner = ex.InnerException;
                    string innermostException = ex.Message;
                    while (inner != null)
                    {
                        innermostException = inner.Message;
                        _logger.LogError(inner.Message);
                        inner = inner.InnerException;
                    }
                }
                else
                {
                    _logger.LogError(ex.Message);
                }
                throw;
            }
        }

    }


}
