using System;

namespace FTACoreSL.Util
{
    public class KeyCreator
    {
        public static string NewKey()
        {
            return Guid.NewGuid().ToString();
        }
    }
}