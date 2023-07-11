using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Majordomo.Common
{
    public enum MDPCommand
    {
        Kill = 0x00,
        Ready = 0x01,
        Request = 0x02,
        Reply = 0x03,
        Heartbeat = 0x04,
        Disconnect = 0x05
    }
}
