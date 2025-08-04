using UnityEngine;

public static class SyncBuffer
{
    public struct Snapshot
    {
        public float time;
        public Vector3 position;
    }

    public enum ActionType
    {
        Jump,
        CollectOrb,
        HitObstacle
    }

    public struct PlayerEvent
    {
        public float time;
        public ActionType type;
        public int targetID;
    }

    const int SNAPSHOT_BUFFER_SIZE = 32; //for positions
    const int EVENT_BUFFER_SIZE = 32; // for jump and collisions

    static Snapshot[] snapshots = new Snapshot[SNAPSHOT_BUFFER_SIZE];
    static int snapHead = 0, snapTail = 0;

    static PlayerEvent[] events = new PlayerEvent[EVENT_BUFFER_SIZE];
    static int eventHead = 0, eventTail = 0;

    public static void RecordSnapshot(float time, Vector3 pos)
    {
        snapshots[snapTail] = new Snapshot { time = time, position = pos };
        snapTail = (snapTail + 1) % SNAPSHOT_BUFFER_SIZE;
        if (snapTail == snapHead)
            snapHead = (snapHead + 1) % SNAPSHOT_BUFFER_SIZE; // overwrite oldest
    }

    public static void RecordEvent(PlayerEvent e)
    {
        events[eventTail] = e;
        eventTail = (eventTail + 1) % EVENT_BUFFER_SIZE;
        if (eventTail == eventHead)
            eventHead = (eventHead + 1) % EVENT_BUFFER_SIZE;  // overwrite oldest
    }

    /// <summary>
    /// Returns an interpolated Snapshot at the requested time,
    /// or null if there are fewer than two samples.
    /// </summary>
    public static Snapshot? GetInterpolatedSnapshot(float targetTime)
    {
        int count = (snapTail - snapHead + SNAPSHOT_BUFFER_SIZE) % SNAPSHOT_BUFFER_SIZE;
        if (count < 2)
            return null;

        int idx = snapHead;
        for (int i = 0; i < count; i++)
        {
            var after = snapshots[idx];
            if (after.time >= targetTime)
            {
                int prevIdx = (idx - 1 + SNAPSHOT_BUFFER_SIZE) % SNAPSHOT_BUFFER_SIZE;
                var before = snapshots[prevIdx];

                float span = after.time - before.time;
                float t = span > 0f
                    ? Mathf.Clamp01((targetTime - before.time) / span)
                    : 0f;

                return new Snapshot
                {
                    time = targetTime,
                    position = Vector3.Lerp(before.position, after.position, t)
                };
            }
            idx = (idx + 1) % SNAPSHOT_BUFFER_SIZE;
        }

        // If targetTime is newer than any sample, return the newest sample
        int latestIdx = (snapTail - 1 + SNAPSHOT_BUFFER_SIZE) % SNAPSHOT_BUFFER_SIZE;
        return snapshots[latestIdx];
    }

    /// <summary>
    /// Peek at the next PlayerEvent without dequeuing.
    /// Returns false if the buffer is empty.
    /// </summary>
    public static bool PeekEvent(out PlayerEvent e)
    {
        if (eventHead == eventTail)
        {
            e = default;
            return false;
        }
        e = events[eventHead];
        return true;
    }

    /// <summary>
    /// Dequeue the next PlayerEvent (assumes PeekEvent returned true).
    /// </summary>
    public static PlayerEvent DequeueEvent()
    {
        var e = events[eventHead];
        eventHead = (eventHead + 1) % EVENT_BUFFER_SIZE;
        return e;
    }
}