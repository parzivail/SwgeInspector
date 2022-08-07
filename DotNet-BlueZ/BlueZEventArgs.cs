using System;

namespace HashtagChris.DotNetBlueZ
{
    public class BlueZEventArgs : EventArgs
    {
        public BlueZEventArgs(bool isStateChange = true)
        {
            IsStateChange = isStateChange;
        }

        public bool IsStateChange { get; }
    }
}