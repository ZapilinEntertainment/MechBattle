using System.Linq;
using System.Collections.Generic;

namespace ZE.MechBattle
{
    public interface ITrimmableBuffer<UserKey, NodeKey> where NodeKey : unmanaged
    {
        IReadOnlyDictionary<UserKey, int> UserToPathId { get; }
        IReadOnlyDictionary<int, PathData<NodeKey>> Paths { get; }
        void RemovePath(int pathId);
    }

    public interface IBufferTrimController
    {
        void Trim(int limit);
    }

    public class BufferTrimController<UserKey, NodeKey> : IBufferTrimController where NodeKey : unmanaged
    {
        private readonly Dictionary<int, int> _usersCount = new();
        private readonly ITrimmableBuffer<UserKey, NodeKey> _buffer;

        public BufferTrimController(ITrimmableBuffer<UserKey, NodeKey> buffer)
        {
            _buffer = buffer;
        }

        public void Trim(int limit)
        {
            foreach (var userToElementKvp in _buffer.UserToPathId)
            {
                _usersCount[userToElementKvp.Value]++;
            }

            var candidatesToRemove = _buffer.Paths
                .Where(p => !_usersCount.ContainsKey(p.Key))
                .OrderByDescending(p => p.Value.LastUseTime)
                .Select(p => p.Key);

            var count = _buffer.Paths.Count;
            foreach (var pathId in candidatesToRemove)
            {
                _buffer.RemovePath(pathId);
                count--;
                if (count <= limit)
                    break;
            }

            _usersCount.Clear();
        }
    }
}
