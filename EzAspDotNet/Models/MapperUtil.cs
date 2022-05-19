using AutoMapper;
using EzAspDotNet.Exception;
using Serilog;

namespace EzAspDotNet.Models
{
    public class MapperUtil
    {
        private static MapperConfiguration _configuration;

        private static IMapper _mapper;

        public static IMapper Mapper => _mapper;

        public static bool Initialize(MapperConfiguration configure)
        {
            try
            {
                _configuration = configure;
                _configuration.AssertConfigurationIsValid();
                _mapper = _configuration.CreateMapper();
                return true;
            }
            catch (System.Exception ex)
            {
                Log.Error($"MapperUtil::Initialize() <Desc:exception occurred while dto mapper initializing> <Message:{ex.Message}>");
                ex.ExceptionLog();
            }
            return false;
        }

        public static TDestination Map<TDestination>(object source)
        {
            return _mapper.Map<TDestination>(source);
        }
        public static TDestination Map<TSource, TDestination>(TSource source)
        {
            return _mapper.Map<TSource, TDestination>(source);
        }
        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return _mapper.Map<TSource, TDestination>(source, destination);
        }
    }
}
