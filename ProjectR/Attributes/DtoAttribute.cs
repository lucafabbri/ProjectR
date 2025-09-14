using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectR.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class DtoAttribute<TEntity> : Attribute
    {
        public Type EntityType { get; }
        public DtoAttribute()
        {
            EntityType = typeof(TEntity);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class DtoMapperAttribute<TEntity, TMapper> : DtoAttribute<TEntity>
    {
        public Type MapperType { get; }
        public DtoMapperAttribute() : base()
        {
            MapperType = typeof(TMapper);
        }
    }
}
