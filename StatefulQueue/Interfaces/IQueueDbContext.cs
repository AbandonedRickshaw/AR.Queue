using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StatefulQueue
{
    public interface IQueueDbContext<TQueueItem, TBody, TStateInfo>
        where TQueueItem : class, IQueueItem<TBody, TStateInfo>
    {
        DbSet<TQueueItem> QueueItems { get; set; }
    }
}
