using System;

namespace Xamarin.Helper.Tools
{
    public class TimeHelper
    {
        /// <summary>
        /// �õ���ǰʱ�����1970.1.1������
        /// </summary>
        static public long CurrentSystemTimeMillis
        {
            get
            {
                TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                long millis = (long)ts.TotalMilliseconds;
                return millis;
            }
        }
    }
}
