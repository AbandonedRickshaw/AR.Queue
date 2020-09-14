using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StatefulQueue.Models
{
    public class QueueItem<TBody, TStateInfo>
        : IQueueItem<TBody, TStateInfo>
        , IEntityTypeConfiguration<IQueueItem<TBody, TStateInfo>>

    {
        [Key]
        public long QueueItemID { get; set; }

        [MaxLength(50)]
        public string LockID { get; set; }

        public DateTimeOffset? LockDate { get; set; }

        public int Attempts { get; set; }

        public TStateInfo StateInfo { get; set; }

        public TBody Body { get; set; }

        public void Configure(EntityTypeBuilder<IQueueItem<TBody, TStateInfo>> builder)
        {
            {
                builder.Property(e => e.Attempts)
                    .HasDefaultValue(0);

                builder.Property(e => e.Body)
                    .HasConversion(new JsonSerializerValueConverter<TBody>().ValueConverter);

                builder.Property(e => e.StateInfo)
                    .HasConversion(new JsonSerializerValueConverter<TStateInfo>().ValueConverter);

                builder.HasIndex(e => new { e.LockID, e.Attempts })
                    .HasName("IX_QueueItem_LockID_Attempts")
                    .IncludeProperties(e => new { e.QueueItemID, e.LockDate, e.Body, e.StateInfo });

            }
        }
    }
}
