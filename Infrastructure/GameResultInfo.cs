using System;
using System.Runtime.Serialization;

namespace CVARC.Infrastructure
{
    [DataContract]
    [Serializable]
    public class GameResultInfo
    {
        [DataMember]
        public PlayerInfo[] Players { get; set; }
        [DataMember]
        public string LogFileName { get; set; }
        [DataMember]
        public string Tag { get; set; }
        [DataMember]
        public string Subtag { get; set; }
        [DataMember]
        public string PushPassword { get; set; }
    }
}