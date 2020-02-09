using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace D2M.Common.Extensions
{
    public static class TaskExtensions
    {
        public static void FireAndForget(this Task sourceTask)
        {
            // Do absolutely nothing, because the task will just fire off here
        }
    }
}
