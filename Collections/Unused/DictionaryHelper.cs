//namespace JointCode.Common.Collections
//{
//    public static class DictionaryHelper
//    {
//        /// <summary>
//        /// This method is used by the dictionary implementations to adapt
//        /// a given size to a prime number (or at least to some number that's
//        /// not easily divided).
//        /// </summary>
//        /// <param name="value">The original length of buckets, which is a prime number.</param>
//        /// <returns>Another prime number that bigger than the original length.</returns>
//        public static int AdaptSize(int value)
//        {
//            if (value <= 31)
//                return 31;

//            if ((value % 2) == 0)
//                value--;

//            checked
//            {
//                while (true)
//                {
//                    value += 2;

//                    if (value % 3 == 0)
//                        continue;

//                    if (value % 5 == 0)
//                        continue;

//                    if (value % 7 == 0)
//                        continue;

//                    if (value % 11 == 0)
//                        continue;

//                    if (value % 13 == 0)
//                        continue;

//                    if (value % 17 == 0)
//                        continue;

//                    if (value % 19 == 0)
//                        continue;

//                    if (value % 23 == 0)
//                        continue;

//                    if (value % 29 == 0)
//                        continue;

//                    if (value % 31 == 0)
//                        continue;

//                    return value;
//                }
//            }
//        }
//    }
//}
