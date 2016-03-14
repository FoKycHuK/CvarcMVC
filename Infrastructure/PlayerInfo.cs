using System;
using System.Runtime.Serialization;

namespace CVARC.Infrastructure
{
    [DataContract]
    [Serializable]
    public class PlayerInfo
    {
        [DataMember]
        public string CvarcTag { get; set; }
        [DataMember]
        public string NickName { get; set; }
        [DataMember]
        public bool Connected { get; set; }
        [DataMember]
        public int Score { get; set; }
    }
}
