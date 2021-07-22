using System.Collections.Generic;
using SoftGear.Strix.Client.Core.Model.Manager.Filter;
using SoftGear.Strix.Client.Core.Model.Manager.Filter.Builder;

namespace StrixEx {
    public struct RoomFilter {
        public string roomName;
        public int maxPlayer;
        public int mapKey;

        public static RoomFilter Create(string roomName, int capacity, int mapKey) {
            return new RoomFilter() {
                roomName = roomName,
                maxPlayer = capacity,
                mapKey = mapKey
            };
        }

        public readonly ICondition ToCondition() {
            IConditionBuilder builder;

            builder = ConditionBuilder.Builder().Field("capacity").EqualTo(maxPlayer);
            builder.And().Field("key1").EqualTo((double) mapKey);

            return builder.Build();
        }

        public IDictionary<string, object> ToDictionary() {
            return new Dictionary<string, object> {
                {"name", roomName},
                {"capacity", maxPlayer},
                {"password", ""},
                {"state", 0},
                {"isJoinable", true},
                {"key1", (double) mapKey},
            };
        }
    }
}