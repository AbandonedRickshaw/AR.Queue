using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StatefulQueue
{
    public interface IStatefulQueue<TQueueContext, TQueueItem, TBody, TStateInfo>
        where TQueueItem : class, IQueueItem<TBody, TStateInfo>
        where TQueueContext : IQueueDbContext<TQueueItem, TBody, TStateInfo>
    {
        public int MaxAttempts { get; set; }
        public ValueTask<TQueueItem> AddItemAsync(TQueueItem item);
        public ValueTask<TQueueItem> AddItemAsync(TBody data, string lockId = null);
        public ValueTask<IEnumerable<TQueueItem>> GetExclusiveBatchAsync(string lockId, int maxItemsReturned = int.MaxValue);
        public ValueTask<IEnumerable<TQueueItem>> GetExclusiveBatchAsync2(string lockId, int maxItemsReturned = int.MaxValue);
        public ValueTask<TQueueItem> GetExclusiveItemAsync(string lockId, long queueItemID);
        public ValueTask<int> ReleaseItemsAsync(Expression<Func<TQueueItem, bool>> filter = null);
        public ValueTask<TQueueItem> UpdateItemAsync(string lockId, TQueueItem item);
    }
}
