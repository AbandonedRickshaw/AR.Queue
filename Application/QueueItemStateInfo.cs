using System;
using System.Collections.Generic;
using System.Text;

namespace EF.Playground
{
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
