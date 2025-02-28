using System.Collections.Generic;
using System.Linq;

namespace PCAB_Debugger2_GUI
{
    public static class SearchList
    {
        /// <summary>
        /// Binary search with the sunday algorithm
        /// </summary>
        /// <param name="target">Target list</param>
        /// <param name="pattern">Search vlue byte array</param>
        /// <returns>Match position (-1 if not found)</returns>
        public static int SearchBytesSundayIndexOf(List<byte> target, byte[] pattern)
        {
            return SearchBytesSundayIndexOf(target, pattern, 0);
        }

        /// <summary>
        /// Binary search with the sunday algorithm
        /// </summary>
        /// <param name="target">Target list</param>
        /// <param name="pattern">Search vlue byte array</param>
        /// <returns>Match position (-1 if not found)</returns>
        public static int SearchBytesSundayLastIndexOf(List<byte> target, byte[] pattern)
        {
            return SearchBytesSundayLastIndexOf(target, pattern, target.Count - 1);
        }


        /// <summary>
        /// Binary search with the sunday algorithm
        /// </summary>
        /// <param name="target">Target list</param>
        /// <param name="pattern">Search vlue byte array</param>
        /// <param name="index">Start position</param>
        /// <returns>Match position (-1 if not found)</returns>
        public static int SearchBytesSundayIndexOf(List<byte> target, byte[] pattern, int index)
        {
            int patLen = pattern.Length;
            int txtLen = target.Count;

            if (txtLen < patLen) return -1;
            txtLen -= patLen;

            Dictionary<byte, int> bmTable = MakeSundayTable(pattern);

            int patIdx = 0;
            while (index <= txtLen)
            {
                patIdx = patLen; // search position
                for (; patIdx > 0; --patIdx)
                {
                    if (target[index + patIdx - 1] != pattern[patIdx - 1])
                    {
                        break;
                    }
                }

                if (patIdx == 0)
                {   // All matched.
                    return index;
                }

                if (index == txtLen)
                {   // not found.
                    break;
                }

                if (bmTable.ContainsKey(target[index + patLen]))
                {
                    index += bmTable[target[index + patLen]];
                }
                else
                {
                    index += patLen + 1;
                }
            }

            return -1;
        }


        /// <summary>
        /// Binary search with the sunday algorithm
        /// </summary>
        /// <param name="target">Target list</param>
        /// <param name="pattern">Search vlue byte array</param>
        /// <param name="index">Start position</param>
        /// <returns>Match position (-1 if not found)</returns>
        public static int SearchBytesSundayLastIndexOf(List<byte> target, byte[] pattern, int index)
        {
            if (target.Count < pattern.Length) return -1;
            List<byte> targRev = new List<byte>(target);
            List<byte> patRev = pattern.ToList();
            targRev.Reverse();
            patRev.Reverse();
            int num = SearchBytesSundayIndexOf(targRev, patRev.ToArray(), target.Count - index - 1);
            if(num<0) return -1;
            return target.Count - num - patRev.Count;
        }


        /// <summary>
        /// Create shift table.
        /// </summary>
        /// <param name="pattern">Search vlue byte array</param>
        /// <returns>A dictionary where keys are bytes and values ​​are shift.</returns>
        private static Dictionary<byte, int> MakeSundayTable(byte[] pattern)
        {
            var result = new Dictionary<byte, int>();
            var counter = pattern.Length;
            var i = 0;
            while (counter > 0)
            {
                result[pattern[i++]] = counter--;
            }
            return result;
        }
    }
}
