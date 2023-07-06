using AutoMapper;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Wombat;
using Wombat.Infrastructure;

namespace Wombat.Web.Infrastructure
{
    public static partial class Extention
    {
        /// <summary>
        /// 使用AutoMapper自动映射拥有MapAttribute的类
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configure">自定义配置</param>
        public static IServiceCollection AddAutoMapper(this IServiceCollection services, Action<IMapperConfigurationExpression> configure = null)
        {
            List<(Type from, Type[] targets)> maps = new List<(Type from, Type[] targets)>();

            maps.AddRange((IEnumerable<(Type from, Type[] targets)>)AssemblyLoader.GetAssemblyList().ToArray().Where(x => x.GetCustomAttribute<MapAttribute>() != null)
                .Select(x => (x, x.GetCustomAttribute<MapAttribute>().TargetTypes)));

            var configuration = new MapperConfiguration(cfg =>
            {
                maps.ForEach(aMap =>
                {
                    aMap.targets.ToList().ForEach(aTarget =>
                    {
                        cfg.CreateMap(aMap.from, aTarget).IgnoreAllNonExisting(aMap.from, aTarget).ReverseMap();
                    });
                });

                cfg.AddMaps(AssemblyLoader.GetAssemblyList());

                //自定义映射
                configure?.Invoke(cfg);
            });

#if DEBUG
            //只在Debug时检查配置
            configuration.AssertConfigurationIsValid();
#endif
            services.AddSingleton(configuration.CreateMapper());

            return services;
        }

        /// <summary>
        /// 忽略所有不匹配的属性。
        /// </summary>
        /// <param name="expression">配置表达式</param>
        /// <param name="from">源类型</param>
        /// <param name="to">目标类型</param>
        /// <returns></returns>
        public static IMappingExpression IgnoreAllNonExisting(this IMappingExpression expression, Type from, Type to)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance;
            to.GetProperties(flags).Where(x => from.GetProperty(x.Name, flags) == null).ForEach(aProperty =>
            {
                expression.ForMember(aProperty.Name, opt => opt.Ignore());
            });

            return expression;
        }

    }
}
