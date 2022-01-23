using EzAspDotNet.Common;

namespace EzAspDotNet.Code
{
    public class ResultCode : Enumeration
    {
        protected ResultCode(int id, string name) : base(id, name)
        {
        }

        public static ResultCode Success = new(1, "성공");

        public static ResultCode BadRequest = new(2, "잘못된 요청");

        public static ResultCode UnknownException = new(3, "핸들링하지 못하는 예외가 발생했다");

        public static ResultCode HttpError = new(4, "Http오류");

        public static ResultCode NotConnectedMQ = new(5, "MQ에 연결되지 않았습니다");

        public static ResultCode NotImplementedYet = new(6, "미구현 상태입니다");

        public static ResultCode InternalServerError = new(7, "내부적인 서버 오류가 발생했다");
    }
}
