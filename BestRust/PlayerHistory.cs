namespace BestRust
{
    internal class PlayerHistory
    {
        public DataPoint[] data;

        public HistoryStats calculateStats()
        {
            var stats = new HistoryStats();

            foreach (var item in data)
            {
                stats.avg += item.attributes.value;

                if (stats.min > item.attributes.min) stats.min = item.attributes.min;

                if (stats.max < item.attributes.max) stats.max = item.attributes.max;
            }

            stats.avg /= data.Length;

            int lowPop = stats.avg / 5;

            foreach (var item in data)
            {
                if (item.attributes.value < lowPop) stats.timelowPop++;
            }

            return stats;
        }
    }

    internal class HistoryStats
    {
        public int avg = 0;
        public int max = 0, min = 10000;
        public int timelowPop = 0;
    }

    internal struct DataPoint
    {
        public string type;
        public DataAttribute attributes;
    }

    internal struct DataAttribute
    {
        public DateTime timestamp;
        public int max, value, min;
    }
}
