using StatefulQueue.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Concurrent;

namespace StatefulQueue
{
    public class Queue<TBody, TStateInfo>
        : IStatefulQueue<QueueDbContext<TBody, TStateInfo>, QueueItem<TBody, TStateInfo>, TBody, TStateInfo>

    {
        private QueueDbContext<TBody, TStateInfo> _dbContext;
        private ILogger _logger { get; set; }

        public Queue(QueueDbContext<TBody, TStateInfo> dbContext, ILogger logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException("dbContext");
            _logger = logger ?? throw new ArgumentNullException("logger");
        }

        public int MaxAttempts { get; set; }

        public async ValueTask<QueueItem<TBody, TStateInfo>> AddItemAsync(QueueItem<TBody, TStateInfo> item)
        {
            // ensure no id has been set
            item.QueueItemID = default;

            // If an item lock is specified, lock it before it even gets to the database
            if (!string.IsNullOrWhiteSpace(item.LockID))
            {
                item.LockDate = DateTimeOffset.Now;
                item.LockID = item.LockID;
            }

            // attach queueItem as a new entity to be inserted
            var entry = _dbContext.Attach(item);

            // do the insertion
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            // return the item
            return item;
        }

        public ValueTask<QueueItem<TBody, TStateInfo>> AddItemAsync(TBody body, string lockId = null)
        {
            return AddItemAsync(item: new QueueItem<TBody, TStateInfo>() { Body = body, LockID = lockId });
        }

        public async ValueTask<IEnumerable<QueueItem<TBody, TStateInfo>>> GetExclusiveBatchAsync(string lockId, int maxItemsReturned = int.MaxValue)
        {
            if (string.IsNullOrWhiteSpace(lockId)) throw new ArgumentException("lockId must not be empty, and must contain at least one non-whitespace character.");

            try
            {
                // release any previous locks first
                await ReleaseItemsAsync(e => e.LockID == lockId && e.Attempts < MaxAttempts);

                await _dbContext.QueueItems
                    .Where(e => (e.LockID == null || e.LockID == lockId) && e.Attempts < MaxAttempts)
                    .ForEachAsync((e) =>
                    {
                        e.Attempts += 1;
                        e.LockID = lockId;
                        e.LockDate = DateTimeOffset.Now;
                    }).ConfigureAwait(false);

                await _dbContext.SaveChangesAsync().ConfigureAwait(false);

                return await _dbContext.QueueItems
                    .Where(e => e.LockID == lockId)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// This method is slower, but more reliable since it locks rows one at a time
        /// </summary>
        /// <param name="lockId"></param>
        /// <param name="maxItemsReturned"></param>
        /// <returns></returns>
        public async ValueTask<IEnumerable<QueueItem<TBody, TStateInfo>>> GetExclusiveBatchAsync2(string lockId, int maxItemsReturned = int.MaxValue)
        {
            if (string.IsNullOrWhiteSpace(lockId)) throw new ArgumentException("lockId must not be empty, and must contain at least one non-whitespace character.");

            try
            {
                // release any previous locks first
                await ReleaseItemsAsync(e => e.LockID == lockId && e.Attempts < 3);

                var items = new ConcurrentBag<QueueItem<TBody, TStateInfo>>();

                var ids = await _dbContext.QueueItems
                    .AsNoTracking()
                    .Where(e => (e.LockID == null) && e.Attempts < 3)
                    .Select(e => e.QueueItemID)
                    .Take(maxItemsReturned)
                    .ToListAsync().ConfigureAwait(false);

                foreach (var id in ids)
                {
                    try
                    {
                        items.Add(await GetExclusiveItemAsync(lockId, id).ConfigureAwait(false));
                    }
                    catch (Exception ex) { Console.WriteLine($"Missed {id} {ex.Message}"); }
                }

                return items.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async ValueTask<QueueItem<TBody, TStateInfo>> GetExclusiveItemAsync(string lockId, long queueItemID)
        {
            if (string.IsNullOrWhiteSpace(lockId)) throw new ArgumentException("lockId must not be empty, and must contain at least one non-whitespace character.");

            try
            {
                // create a new item with only the id set.  This is what we're looking for
                var item = new QueueItem<TBody, TStateInfo>() { QueueItemID = queueItemID };

                // get the change tracker entry if there is one, otherwise attach this new item to the change tracker
                var entry = _dbContext.ChangeTracker.Entries<QueueItem<TBody, TStateInfo>>().Where(e => e.Entity.QueueItemID == queueItemID).FirstOrDefault()
                    ?? _dbContext.QueueItems.Attach(item);

                // set the lock properties for the item
                entry.Entity.LockID = lockId;
                entry.Entity.LockDate = DateTimeOffset.Now;

                // save it
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);

                // return it
                return item;
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogError($"Queue item {queueItemID} could not be locked exclusively.  Either the item doesn't exist or it is already exclusively locked.");
                throw;
            }
        }

        public async ValueTask<int> ReleaseItemsAsync(Expression<Func<QueueItem<TBody, TStateInfo>, bool>> filter = null)
        {
            if (filter == null)
                filter = e => 1 == 1;
            await _dbContext.QueueItems
                .Where(filter)
                .ForEachAsync((e) =>
                {
                    e.LockID = null;
                    e.LockDate = null;
                }).ConfigureAwait(false);

            return await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public ValueTask<QueueItem<TBody, TStateInfo>> UpdateItemAsync(string lockId, QueueItem<TBody, TStateInfo> item)
        {
            throw new NotImplementedException();
        }


        //public async ValueTask<TItem> InsertItem(TItem item, long lockId = 0)
        //{

        //    var retval = await _dbContext.AddAsync(item).ConfigureAwait(false);
        //    await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        //    return retval.Entity;
        //}

        //public void UpdateItemState(long rowID, TState state)
        //{

        //}

        //public ICollection<TItem> GetBatch(int lockId, int maxRows = int.MaxValue)
        //{

        //}

        //public void ReleaseBatch(int lockId)
        //{
        //    if (lockId == 0) throw new ArgumentException("lockId must not be 0.");

        //}

        //public ValueTask<SalesTransaction> AddItem(SalesTransaction item, Func<SalesTransactionQueueData, string> dataSerializer = null, Func<string, string> stateInfoSerializer = null)
        //{

        //    var sb = new StringBuilder("INSERT INTO ")
        //}

        //public ValueTask<SalesTransaction> AddItem(SalesTransactionQueueData data, string lockId = null, Func<SalesTransactionQueueData, string> dataSerializer = null)
        //{
        //}


    }

}
