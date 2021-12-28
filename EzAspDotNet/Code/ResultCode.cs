using EzAspDotNet.Common;

namespace EzAspDotNet.Code
{
    public class ResultCode : Enumeration
    {
        protected ResultCode(int id, string name) : base(id, name)
        {
        }

        public static ResultCode Success = new(1, "성공");
    }
}
