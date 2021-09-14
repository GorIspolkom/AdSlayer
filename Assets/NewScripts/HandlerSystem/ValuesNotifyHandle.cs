using Clicker.Models;
using System;

namespace Clicker.HandlerSystem
{
    class ValuesNotifyHandle
    {
        private static Notify[] notifies;
        public static void putNotify(Notify notify) => notifies[1] += notify;
        private Values val; 

        public void Swap()
        {
            notifies[0] = notifies[1];
            notifies[1] = new VoidNotify();
        }
        public ValuesNotifyHandle()
        {
            val = new Values();
            notifies = new Notify[2];
            notifies[0] = new VoidNotify();
            notifies[1] = new VoidNotify();
        }
        public ValuesNotifyHandle(Notify notify)
        {
            val = new Values();
            notifies = new Notify[2];
            notifies[0] = notify;
        }

        public void update()
        {
            notifies[0].complite(ref val.profileRef);
            notifies[0] = null;
            Swap();
        }
    }
}
