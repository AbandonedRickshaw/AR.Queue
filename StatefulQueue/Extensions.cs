using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatefulQueue
{
    public static class Extensions
    {
        /// <summary>
        /// This is just a simpler version that can be used when the data model itself implements IEntityTypeConfiguration<typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="modelBuilder"></param>
        /// <returns></returns>
        public static ModelBuilder ApplyConfiguration<T>(this ModelBuilder modelBuilder)
            where T : class, IEntityTypeConfiguration<T>, new()
        {
            return modelBuilder.ApplyConfiguration(new T());
        }

        public static IReadOnlyList<IProperty> GetPrimaryKeys(this EntityEntry entry)
        {
            return entry.Metadata.FindPrimaryKey()
                         .Properties
                         .ToArray();
        }
    }
}
