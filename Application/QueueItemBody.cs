using System;
using System.Collections.Generic;
using System.Text;

namespace EF.Playground
{
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
}
