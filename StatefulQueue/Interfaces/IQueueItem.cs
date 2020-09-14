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
    public interface IQueueItem<TBody, TStateInfo>
        :IEntityTypeConfiguration<IQueueItem<TBody, TStateInfo>>
    {
        public long QueueItemID { get; set; }           // Metadata: unique row id in queue
        public string LockID { get; set; }              // Metadata: used for multiple queue readers
        public DateTimeOffset? LockDate { get; set; }   // Metadata: when last locked
        public int Attempts { get; set; }               // Metadata: number of times attempted
        public TStateInfo StateInfo { get; set; }       // state info for queued item
        public TBody Body { get; set; }                 // item being queued
    }

    public class QueueItemBody
    {
        public QueueItemBody() { }
        public QueueItemBody(int id, string description)
        {
            ID = id;
            Description = description;
        }
        public int ID { get; }
        public string Description { get; }
    }

    public class QueueItemStateInfo
    {
        public QueueItemStateInfo() { }
        public QueueItemStateInfo(int code, string message)
        {
            Code = code;
            Message = message;
        }

        public int Code { get; }
        public string Message { get; }
    }

}
