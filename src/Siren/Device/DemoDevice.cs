using System;

namespace PBS.Siren
{
    /*
    The Demo Device is an implementation of the Device interface that allows us to simulate how
    the Automation Domain will interact with real devices.
    */
    public class DemoDevice : IDevice
    {
        public String Name { get; }
        public DemoDevice(String name)
        {
            Name = name;
        }

        public override String ToString()
        {
            return base.ToString() + " Name: " + Name;
        }
    }
}